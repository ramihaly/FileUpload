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

			using (Stream stream = response.GetResponseStream())
			{
				var reader = new StreamReader(stream, Encoding.UTF8);
				var responseString = reader.ReadToEnd();
			}
		}


		protected void UploadButton_Click(object sender, EventArgs e)
		{
			if (!FileUploadControl.HasFile)
			{
				Debug.WriteLine("Empty file");
				return;
			}

			try
			{
				string filename = Path.GetFileName(FileUploadControl.FileName);
				Debug.WriteLine("File name: " + FileUploadControl.FileName.ToString());
				var byteString = System.Text.Encoding.UTF8.GetString(FileUploadControl.FileBytes);
				var stream = FileUploadControl.FileContent;
				//Debug.WriteLine("File bytes: " + byteString);
				Debug.WriteLine("Keywords: " + Keywords.Text);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Could not upload file: " + ex.Message);
			}
		}
	}
}