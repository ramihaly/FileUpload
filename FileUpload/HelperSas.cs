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
        public static string GetToken()
        {
            var storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", AzureCredentials.StorageAccountName, AzureCredentials.StorageAccountKey);
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
            blobPermissions.SharedAccessPolicies.Add("mypolicy", new SharedAccessBlobPolicy()
            {
                // To ensure SAS is valid immediately, don’t set the start time.
                // This way, you can avoid failures caused by small clock differences.
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15),
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Add
            });

            // The public access setting explicitly specifies that 
            // the container is private, so that it can't be accessed anonymously.
            blobPermissions.PublicAccess = BlobContainerPublicAccessType.Off;

            // Set the new stored access policy on the container.
            container.SetPermissions(blobPermissions);

            // Get the shared access signature token to share with users.
            return container.GetSharedAccessSignature(new SharedAccessBlobPolicy(), "mypolicy");
        }

        public static void UploadFile(Stream fileStream)
        {
            var token = GetToken();
            var uri = new Uri(string.Format("https://{0}.blob.core.windows.net/private/{1}", AzureCredentials.StorageAccountName, Guid.NewGuid().ToString()));

            // Create credentials with the SAS token. The SAS token was created in previous example.
            var credentials = new StorageCredentials(token);

            // Create a new blob.
            var blob = new CloudBlockBlob(uri, credentials);

            blob.UploadFromStream(fileStream);
        }
    }
}
