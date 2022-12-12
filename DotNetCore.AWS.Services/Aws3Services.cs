using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace DotNetCore.AWS.Services
{
	public class Aws3Services : IAws3Services
	{
		private readonly string _bucketName;
		private readonly IAmazonS3 _awsS3Client;

		public Aws3Services(string awsAccessKeyId, string awsSecretAccessKey,string region, string bucketName)
		{
			_bucketName = bucketName;
			_awsS3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey,  RegionEndpoint.GetBySystemName(region));
		}

		public async Task<byte[]> DownloadFileAsync(string file)
		{
			MemoryStream ms = null;

			try
			{
				using (var response = await _awsS3Client.GetObjectAsync(AwsS3Request.S3GetObjectRequest(_bucketName, file)))
				{
					if (response.HttpStatusCode == HttpStatusCode.OK)
					{
						using (ms = new MemoryStream())
						{
							await response.ResponseStream.CopyToAsync(ms);
						}
					}
				}

				if (ms is null || ms.ToArray().Length < 1)
					throw new FileNotFoundException(string.Format("The document '{0}' is not found", file));

				return ms.ToArray();
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<bool> UploadFileAsync(IFormFile file)
		{
			try
			{
				using (var newMemoryStream = new MemoryStream())
				{
					file.CopyTo(newMemoryStream);

					var uploadRequest = new TransferUtilityUploadRequest
					{
						InputStream = newMemoryStream,
						Key = file.FileName,
						BucketName = _bucketName+@"/assets/css",
						ContentType = file.ContentType,
						
					};

					var fileTransferUtility = new TransferUtility(_awsS3Client);

					await fileTransferUtility.UploadAsync(uploadRequest);

					return true;
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<bool> DeleteFileAsync(string fileName, string versionId = "")
		{
			try
			{
				if (!IsFileExists(fileName, versionId))
					throw new FileNotFoundException(string.Format("The document '{0}' is not found", fileName));

				if (string.IsNullOrEmpty(versionId))
				{
					await DeleteFile(fileName, versionId);

					return true;
				}

				var listVersionsRequest = new ListVersionsRequest { BucketName = _bucketName, Prefix = fileName };

				var listVersionsResponse = _awsS3Client.ListVersionsAsync(listVersionsRequest).Result;

				foreach (S3ObjectVersion versionIDs in listVersionsResponse.Versions)
				{
					if (versionIDs.IsDeleteMarker)
					{
						await DeleteFile(fileName, versionIDs.VersionId);
					}
				}

				return true;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private async Task DeleteFile(string fileName, string versionId)
		{
			DeleteObjectRequest request = new DeleteObjectRequest
			{
				BucketName = _bucketName,
				Key = fileName
			};

			if (!string.IsNullOrEmpty(versionId))
				request.VersionId = versionId;

			await _awsS3Client.DeleteObjectAsync(request);
		}

		public bool IsFileExists(string fileName, string versionId)
		{
			try
			{
				GetObjectMetadataRequest request = new GetObjectMetadataRequest()
				{
					BucketName = _bucketName,
					Key = fileName,
					VersionId = !string.IsNullOrEmpty(versionId) ? versionId : null
				};

				var response = _awsS3Client.GetObjectMetadataAsync(request).Result;

				return true;
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null && ex.InnerException is AmazonS3Exception awsEx)
				{
					if (string.Equals(awsEx.ErrorCode, "NoSuchBucket"))
						return false;

					else if (string.Equals(awsEx.ErrorCode, "NotFound"))
						return false;
				}

				throw;
			}
		}
	}
}
