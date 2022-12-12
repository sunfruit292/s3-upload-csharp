using System;
using System.IO;
using Amazon.S3;
using DotNetCore.AWS.Services;
using Microsoft.AspNetCore.Http;

namespace DotNetCore.AWS.Common
{
	public class DocumentStore : IDocumentStore
	{
		private readonly IAws3Services _aws3Services;
		private readonly string _noFile = "NoSuchKey";

		public DocumentStore(IAppConfiguration appConfiguration)
		{
			_aws3Services = new Aws3Services(appConfiguration.AwsAccessKey, appConfiguration.AwsSecretAccessKey, appConfiguration.Region, appConfiguration.BucketName);
		}

		public byte[] DownloadDocument(string documentName)
		{
			try
			{
				return _aws3Services.DownloadFileAsync(documentName).Result;
			}
			catch (AmazonS3Exception ex)
			{
				if (ex.ErrorCode.Contains(_noFile))
					throw new FileNotFoundException(string.Format("The document '{0}' is not found", documentName));
				else
					throw;

			}
			catch (Exception)
			{
				throw;
			}
		}

		public bool UploadDocument(IFormFile file)
		{
			return _aws3Services.UploadFileAsync(file).Result;
		}

		public bool DeleteDocument(string fileName, string versionId = "")
		{
			return _aws3Services.DeleteFileAsync(fileName).Result;
		}
	}
}
