using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNetCore.AWS.Common
{
	public interface IDocumentStore
	{
		byte[] DownloadDocument(string documentName);

		bool UploadDocument(IFormFile file);

		bool DeleteDocument(string fileName, string versionId = "");
	}
}
