using System;
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
        protected void Page_Load(object sender, EventArgs e)
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
                Debug.WriteLine("File name: " + filename);
                var byteString = Encoding.UTF8.GetString(this.FileUploadControl.FileBytes);
                //var fileContentStream = this.FileUploadControl.FileContent;
                Debug.WriteLine("Keywords: " + this.Keywords.Text);

                // POST to /entities endpoint
                PostEntity(filename, this.Keywords.Text);

                // POST to /upload endpoint
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not upload file: " + ex.Message);
            }
        }

        private static void PutBlob_Rest(string filename, string byteString)
        {
            // REST API call
            var headers = new SortedList<string, string>
                {
                    { "x-ms-blob-type", "BlockBlob" }
                };
            var request = Helper.CreateRESTRequest("PUT", "/public/" + filename, byteString, headers);
            var response = request.GetResponse() as HttpWebResponse;

            if (response != null && response.StatusCode == HttpStatusCode.Created)
            {
                Debug.WriteLine("Blob created");
            }

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

        private static void PostEntity(string filename, string keywords)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
                client.Headers[HttpRequestHeader.UserAgent] = "Fiddler";

                var values = new NameValueCollection();
                values["Id"] = Guid.NewGuid().ToString();
                values["Name"] = filename;
                values["ContentUrl"] = AzureCredentials.Endpoint + "/public/" + filename;

                var keywordsSplit = keywords.Split(';');
                var keywordsJson = new StringBuilder();
                keywordsJson.Append("[");
                for (var i = 0; i < keywordsSplit.Length - 1; ++i)
                {
                    keywordsJson.Append(@"""" + keywordsSplit[i] + @"""" + ", ");
                }
                keywordsJson.Append(@"""" + keywordsSplit[keywordsSplit.Length - 1] + @"""" + "]");
                var jsonString = @"{""Id"": """ + values["Id"] + @""", ""Name"": """ + values["Name"] + @""", ""Keywords"": " + keywordsJson + @", ""References"": [] }";
                var response = client.UploadString("http://dam-ent-20151113.azurewebsites.net/digitalassets/entities/7fccfc4a5a10457f89d0ce5ca33c0f58/Files", jsonString);
                Debug.WriteLine(response);
            }
        }


    }
}