﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Collections.Specialized;
using System.Collections;

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
                //var fileId = PostEntity(filename, this.Keywords.Text);

                HelperSas.UploadFile(this.FileUploadControl.FileContent);
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

        // REST API call
        private static HttpStatusCode PutBlob_Rest(string filename, byte[] byteArray)
        {

            var headers = new SortedList<string, string>
                {
                    { "x-ms-blob-type", "BlockBlob" }
                };
            var request = Helper.CreateRESTRequest("PUT", "/public/" + filename, byteArray, headers);
            var response = request.GetResponse() as HttpWebResponse;

            if (response == null)
            {
                return HttpStatusCode.InternalServerError;
            }

            return response.StatusCode;
        }

        private static void GetEntity(string Id)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
                client.Headers[HttpRequestHeader.UserAgent] = "Fiddler";

                client.DownloadString(AzureCredentials.EntitiesEndpoint + "(" + Id + ")");
            }
        }

        private static string PostEntity(string filename, string keywords)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
                client.Headers[HttpRequestHeader.UserAgent] = "Fiddler";

                var values = new NameValueCollection();
                values["Id"] = Guid.NewGuid().ToString();
                values["Name"] = filename;
                values["ContentUrl"] = AzureCredentials.BlobEndpoint + "/public/" + filename;

                var keywordsSplit = keywords.IndexOf(";", StringComparison.InvariantCulture) > -1 ? keywords.Split(';') : new string[] { keywords };
                var keywordsJson = new StringBuilder();
                keywordsJson.Append("[");
                for (var i = 0; i < keywordsSplit.Length - 1; ++i)
                {
                    keywordsJson.Append(@"""" + keywordsSplit[i] + @"""" + ", ");
                }
                keywordsJson.Append(@"""" + keywordsSplit[keywordsSplit.Length - 1] + @"""" + "]");
                var jsonString = @"{""Id"": """ + values["Id"] + @""", ""Name"": """ + values["Name"] + @""", ""Keywords"": " + keywordsJson + @", ""References"": [] }";
                try
                {
                    var response = client.UploadString(AzureCredentials.EntitiesEndpoint, jsonString);
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

        //private static HttpStatusCode PostUpload(string contentUrl, string byteString)
        //{
        //	using (var client = new WebClient())
        //	{
        //		client.Headers[HttpRequestHeader.ContentType] = "application/json";
        //		client.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
        //		client.Headers[HttpRequestHeader.UserAgent] = "Fiddler";

        //		var jsonString = @"{""ContentUrl"": """ + contentUrl + @""", ""Content"": """ + byteString + @""" }";

        //		try
        //		{
        //			var response = client.UploadString(AzureCredentials.UploadEndpoint, jsonString);
        //			Debug.WriteLine(response);
        //		}
        //		catch (Exception e)
        //		{
        //			Debug.WriteLine("Exception " + e.Message + " occurred when attempting to POST to UPLOAD endpoint");
        //			return HttpStatusCode.InternalServerError;
        //		}

        //		return HttpStatusCode.Created;
        //	}
        //}

        protected void GetMetadataBtn_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            var fileId = btn.CommandArgument;
            var url = AzureCredentials.EntitiesEndpoint + "(" + fileId + ")";
            Debug.WriteLine(url);
            Response.Redirect(url);
        }
    }
}