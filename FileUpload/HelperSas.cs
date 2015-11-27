
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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace WebApplication1
{
	public class HelperSas
	{
		public static string PolicyName = "read-write";

		public static string GetToken(Uri blobUri)
		{
			var blobComponents = BlobUri.Parse(blobUri) as BlobUri;
			var storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", blobComponents.Account, AzureCredentials.StorageAccountKey);
			var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

			// Create the blob client object.
			var blobClient = storageAccount.CreateCloudBlobClient();

			// Get a reference to the container for which shared access signature will be created.
			var container = blobClient.GetContainerReference("private");
			container.CreateIfNotExists();

			// Get the current permissions for the blob container.
			var blobPermissions = container.GetPermissions();

			// Clear the container's shared access policies to avoid naming conflicts.
			blobPermissions.SharedAccessPolicies.Clear();

			// The new shared access policy provides read/write access to the container for 24 hours.
			blobPermissions.SharedAccessPolicies.Add(HelperSas.PolicyName, new SharedAccessBlobPolicy()
			{
				// To ensure SAS is valid immediately, don’t set the start time.
				// This way, you can avoid failures caused by small clock differences.
				SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15),
				Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write
			});

			// The public access setting explicitly specifies that 
			// the container is private, so that it can't be accessed anonymously.
			blobPermissions.PublicAccess = BlobContainerPublicAccessType.Off;

			// Set the new stored access policy on the container.
			container.SetPermissions(blobPermissions);

			HelperSas.CalculateContainerBytes(container);

			// Get the shared access signature token to share with users.
			return container.GetSharedAccessSignature(new SharedAccessBlobPolicy(), HelperSas.PolicyName);
		}

		public static void UploadFile(string fileId, string fileVersionId, Stream fileStream)
		{
			var uri = BlobUri.GetUri(AzureCredentials.StorageAccountName, fileId, fileVersionId);
			var token = GetToken(uri);

			// Create credentials with the SAS token. The SAS token was created in previous example.
			var credentials = new StorageCredentials(token);

			// Create a new blob.
			var blob = new CloudBlockBlob(uri, credentials);

			blob.UploadFromStream(fileStream);
		}

		private static void CalculateContainerBytes(CloudBlobContainer container)
		{
			var containerSizeInBytes = 48 + Guid.NewGuid().ToString().Length * 2;
			foreach (var metadata in container.Metadata)
			{
				containerSizeInBytes += 3 + metadata.Key.Length + metadata.Value.Length;
			}

			containerSizeInBytes += container.GetPermissions().SharedAccessPolicies.Count * 512;

			Debug.WriteLine("Container " + container.Name + " size (kb): " + containerSizeInBytes / 1024.0);
		}
	}
}
