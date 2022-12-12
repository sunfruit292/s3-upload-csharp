using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.S3.Model;

namespace DotNetCore.AWS.Services
{
	static class AwsS3Request
	{
		public static GetObjectRequest S3GetObjectRequest(string bucketName, string file)
		{
			return new GetObjectRequest
			{
				BucketName = bucketName,
				Key = file
			};
		}

		public static PutObjectRequest S3PutObjectRequest(string bucketName, string fileName, string contentType, Stream content)
		{
			return new PutObjectRequest
			{
				BucketName = bucketName,
				Key = fileName,
				InputStream = content,
				ContentType = contentType
			};
		}
	}
}
