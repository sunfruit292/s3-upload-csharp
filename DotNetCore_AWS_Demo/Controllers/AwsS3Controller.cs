using System;
using System.IO;
using System.Net;
using DotNetCore.AWS.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCore_AWS_Demo.Controllers
{
	[ApiController]
	[Route("documents")]
	public class AwsS3Controller : ControllerBase
	{
		private IDocumentStore _documentStore;
		private readonly IAppConfiguration _appConfiguration;

		public AwsS3Controller(IAppConfiguration appConfiguration)
		{
			_appConfiguration = appConfiguration;
		}

		[HttpGet("{documentName}")]
		public IActionResult GetDocumentFromS3(string documentName)
		{
			try
			{
				if (string.IsNullOrEmpty(documentName))
					return ReturnMessage("The 'documentName' parameter is required", (int)HttpStatusCode.BadRequest);

				_documentStore = new DocumentStore(_appConfiguration);

				var document = _documentStore.DownloadDocument(documentName);

				return File(document, "application/octet-stream", documentName);
			}
			catch (Exception ex)
			{
				return ValidateException(ex);
			}
		}

		[HttpPost]
		public IActionResult UploadDocumentToS3(IFormFile file)
		{
			try
			{
				if (file is null || file.Length <= 0)
					return ReturnMessage("file is required to upload", (int)HttpStatusCode.BadRequest);

				_documentStore = new DocumentStore(_appConfiguration);

				var result = _documentStore.UploadDocument(file);

				return ReturnMessage(string.Empty, (int)HttpStatusCode.Created);
			}
			catch (Exception ex)
			{
				return ReturnMessage(ex.Message, (int)HttpStatusCode.InternalServerError);
			}
		}

		[HttpDelete("{documentName}")]
		public IActionResult DeletetDocumentFromS3(string documentName)
		{
			try
			{
				if (string.IsNullOrEmpty(documentName))
					return ReturnMessage("The 'documentName' parameter is required", (int)HttpStatusCode.BadRequest);

				_documentStore = new DocumentStore(_appConfiguration);

				var result = _documentStore.DeleteDocument(documentName);

				return ReturnMessage(string.Format("The document '{0}' is deleted successfully", documentName));
			}
			catch (Exception ex)
			{
				return ValidateException(ex);
			}
		}

		[HttpDelete("{documentName}/{versionId}")]
		public IActionResult DeletetDocumentFromS3(string documentName, string versionId)
		{
			try
			{
				if (string.IsNullOrEmpty(documentName))
					return ReturnMessage("The 'documentName' parameter is required", (int)HttpStatusCode.BadRequest);

				if (string.IsNullOrEmpty(versionId))
					return ReturnMessage("The 'versionId' parameter is required", (int)HttpStatusCode.BadRequest);

				_documentStore = new DocumentStore(_appConfiguration);

				var result = _documentStore.DeleteDocument(documentName, versionId);

				return ReturnMessage(string.Format("The document '{0}' is deleted successfully", documentName));
			}
			catch (Exception ex)
			{
				return ValidateException(ex);
			}
		}

		private IActionResult ReturnMessage(string message, int? statusCode = null)
		{
			return new ContentResult()
			{
				Content = message,
				ContentType = "application/json",
				StatusCode = statusCode
			};
		}

		private IActionResult ValidateException(Exception ex)
		{
			if (ex.InnerException != null && ex.InnerException is FileNotFoundException)
				return ReturnMessage(ex.Message, (int)HttpStatusCode.NotFound);

			return ReturnMessage(ex.Message, (int)HttpStatusCode.InternalServerError);
		}
	}
}
