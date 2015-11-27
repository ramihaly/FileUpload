using System;

namespace WebApplication1
{
	public class BlobUri
	{

		public string Account { get; private set; }
		public string Container;
		public string Blob;

		public static Uri GetUri(string account, string container, string blob)
		{
			return new Uri(string.Format("https://{0}.blob.core.windows.net/{1}/{2}", account, container, blob));
		}

		public static BlobUri Parse(Uri blobUri)
		{
			var components = blobUri.AbsolutePath.Split('/');
			var result = new BlobUri()
			{
				Account = blobUri.Host.Split('.')[0],
				Container = components[1],
				Blob = components[2]
			};

			return result;
		}
	}
}