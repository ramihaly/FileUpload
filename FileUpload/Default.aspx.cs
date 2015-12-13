using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace WebApplication1
{
	public partial class _Default : Page
	{
		private void Initialize()
		{
			ServicePointManager.ServerCertificateValidationCallback = this.AcceptAllCertifications;
			this.UploadCompletedMessage.Text = "";
			this.UploadCompletedMessage.Style.Add(HtmlTextWriterStyle.Color, "black");
			this.GetMetadataBtn.Visible = false;
		}

		public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			this.Initialize();
			GetContainerList_Rest();
		}


		protected void UploadButton_Click(object sender, EventArgs e)
		{
			if (!this.FileUploadControl.HasFile)
			{
				Debug.WriteLine("Empty file");
				return;
			}

			try
			{
				var filename = Path.GetFileName(this.FileUploadControl.FileName);

				// POST to /entities endpoint
				var fileId = PostFileEntity(filename);
				if (string.IsNullOrEmpty(fileId))
				{
					Debug.WriteLine("Error occurred. Stopped");
					return;
				}

				var fileVersionId = PostFileVersionEntity(fileId, this.Keywords.Text);
				HelperSas.UploadFile(fileId, fileVersionId, this.FileUploadControl.FileContent);
				//var status = PutBlob_Rest(filename, this.FileUploadControl.FileBytes);

				// POST to /upload endpoint
				//var status = PostUpload(contentUrl, byteString);
				//if (status == HttpStatusCode.Created)
				//{
				//	this.UploadCompletedMessage.Text = "File uploaded successfully";
				//	this.UploadCompletedMessage.Style.Add(HtmlTextWriterStyle.Color, "green");
				//	this.GetMetadataBtn.Visible = true;
				//	this.GetMetadataBtn.CommandArgument = fileId;
				//}
				//else if (status == HttpStatusCode.InternalServerError)
				//{
				//	this.UploadCompletedMessage.Text = "File upload failed! :(";
				//	this.UploadCompletedMessage.Style.Add(HtmlTextWriterStyle.Color, "red");
				//}

				this.Keywords.Text = "";
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Could not upload file: " + ex.Message);
				this.UploadCompletedMessage.Text = "File upload failed! :(";
				this.UploadCompletedMessage.Style.Add(HtmlTextWriterStyle.Color, "red");
			}
		}

		private static void GetContainerList_Rest()
		{
			var request = Helper.CreateRESTRequest("GET", "?comp=list");
			var response = request.GetResponse() as HttpWebResponse;

			if (response == null)
			{
				return;
			}

			using (var stream = response.GetResponseStream())
			{
				if (stream == null)
				{
					return;
				}

				var reader = new StreamReader(stream, Encoding.UTF8);
				var responseString = reader.ReadToEnd();
			}
		}

		private static HttpStatusCode GetEntity(string entity, string id)
		{
			using (var client = new WebClient())
			{
				client.Headers[HttpRequestHeader.ContentType] = "application/json";
				client.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
				client.Headers[HttpRequestHeader.UserAgent] = "Fiddler";

				try
				{
					client.DownloadString(Helper.GetEntityByKeyEndpoint(entity, id));
				} catch (Exception e) {
					if (e.Message.Contains("NotFound")) {
						return HttpStatusCode.NotFound;
					}
				}

				return HttpStatusCode.OK;
			}
		}

		private static string PostFileEntity(string filename)
		{
			using (var client = new WebClient())
			{
				client.Headers[HttpRequestHeader.ContentType] = "application/json";
				client.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
				client.Headers[HttpRequestHeader.UserAgent] = "Fiddler";

				var values = new Dictionary<string, string>();
				values["Id"] = Guid.NewGuid().ToString();
				values["Name"] = filename;
				var jsonString = JsonConvert.SerializeObject(values);

				try
				{
					var response = client.UploadString(Helper.GetEndpoint("Files"), jsonString);
					Debug.WriteLine(response);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception " + e.Message + " occurred when attempting to POST to ENTITIES endpoint");
					return string.Empty;
				}

				return values["Id"];
			}
		}

		private static string PostFileVersionEntity(string fileId, string keywords)
		{
			using (var client = new WebClient())
			{
				client.Headers[HttpRequestHeader.ContentType] = "application/json";
				client.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
				client.Headers[HttpRequestHeader.UserAgent] = "Fiddler";

				var fileVersionId = Guid.NewGuid().ToString();
				var fileVersionExists = HttpStatusCode.NotFound != GetEntity("FileVersions", fileVersionId);
				if (fileVersionExists)
				{
					return fileVersionId;
				}

				var values = new Dictionary<string, string>();
				values["Id"] = fileVersionId;
				values["ContentType"] = "myType";
				values["FileId"] = fileId;
				values["BlobUri"] = BlobUri.GetUri(AzureCredentials.StorageAccountName, fileId, values["Id"]).ToString();
				values["Keywords"] = keywords;
				var jsonString = JsonConvert.SerializeObject(values);
				try
				{
					var response = client.UploadString(Helper.GetEndpoint("FileVersions"), jsonString);
					Debug.WriteLine(response);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception " + e.Message + " occurred when attempting to POST to ENTITIES endpoint");
					return string.Empty;
				}

				return values["Id"];
			}
		}

		protected void GetMetadataBtn_Click(object sender, EventArgs e)
		{
			LinkButton btn = (LinkButton)sender;
			var fileId = btn.CommandArgument;
			var url = Helper.GetEntityByKeyEndpoint("Files", fileId);
			Debug.WriteLine(url);
			Response.Redirect(url);
		}
	}
}