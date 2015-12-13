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
    public class Helper
    {
        public static string GetEndpoint(string entities)
        {
            return AzureCredentials.EntitiesHost + "/" + entities;
        }

        public static string GetEntityByKeyEndpoint(string entities, string key)
        {
            return AzureCredentials.EntitiesHost + "/" + entities + "(" + key + ")";
        }

        public static WebRequest CreateRESTRequest(
            string method,
            string resource,
            byte[] requestBody = null,
            SortedList<string, string> headers = null,
            string ifMatch = "", string md5 = "")
        {
            byte[] byteArray = null;
            var now = DateTime.UtcNow;
            var uri = AzureCredentials.BlobEndpoint + resource;
            var request = WebRequest.Create(uri);
            request.Method = method;
            request.ContentLength = 0;
            request.Headers.Add("x-ms-date", now.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            request.Headers.Add("x-ms-version", "2014-02-14");

            //if there are additional headers required, they will be passed in to here,
            //add them to the list of request headers
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            //if there is a requestBody, add a header for the Accept-Charset and set the content length
            if (requestBody != null)
            {
                //request.Headers.Add("Accept-Charset", "UTF-8");

                byteArray = requestBody; //Encoding.UTF8.GetBytes(requestBody);
                request.ContentLength = byteArray.Length;
            }

            //add the authorization header 
            request.Headers.Add("Authorization", AuthorizationHeader(method, now, request, ifMatch, md5));

            //now set the body in the request object 
            if (requestBody != null)
            {
                request.GetRequestStream().Write(byteArray, 0, byteArray.Length);
            }

            return request;
        }

        public static string GetCanonicalizedResource(Uri address, string accountName)
        {
            StringBuilder str = new StringBuilder();
            StringBuilder builder = new StringBuilder("/");
            builder.Append(accountName);     //this is testsnapshots
            builder.Append(address.AbsolutePath);  //this is "/" because for getting a list of containers 
            str.Append(builder.ToString());

            var values2 = new NameValueCollection();
            //address.Query is ?comp=list
            //this ends up with a namevaluecollection with 1 entry having key=comp, value=list 
            //it will have more entries if you have more query parameters
            var values = System.Web.HttpUtility.ParseQueryString(address.Query);
            foreach (string str2 in values.Keys)
            {
                var list = new ArrayList(values.GetValues(str2));
                list.Sort();
                StringBuilder builder2 = new StringBuilder();
                foreach (object obj2 in list)
                {
                    if (builder2.Length > 0)
                    {
                        builder2.Append(",");
                    }
                    builder2.Append(obj2.ToString());
                }
                values2.Add((str2 == null) ? str2 : str2.ToLowerInvariant(), builder2.ToString());
            }

            var list2 = new ArrayList(values2.AllKeys);
            list2.Sort();
            foreach (string str3 in list2)
            {
                StringBuilder builder3 = new StringBuilder(string.Empty);
                builder3.Append(str3);
                builder3.Append(":");
                builder3.Append(values2[str3]);
                str.Append("\n");
                str.Append(builder3.ToString());
            }
            return str.ToString();
        }

        public static string GetCanonicalizedHeaders(WebRequest request)
        {
            ArrayList headerNameList = new ArrayList();
            StringBuilder sb = new StringBuilder();

            //retrieve any headers starting with x-ms-, put them in a list and sort them by value.
            foreach (string headerName in request.Headers.Keys)
            {
                if (headerName.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
                {
                    headerNameList.Add(headerName.ToLowerInvariant());
                }
            }
            headerNameList.Sort();

            //create the string that will be the in the right format
            foreach (string headerName in headerNameList)
            {
                StringBuilder builder = new StringBuilder(headerName);
                string separator = ":";
                //get the value for each header, strip out \r\n if found, append it with the key
                foreach (string headerValue in GetHeaderValues(request.Headers, headerName))
                {
                    string trimmedValue = headerValue.Replace("\r\n", String.Empty);
                    builder.Append(separator);
                    builder.Append(trimmedValue);
                    //set this to a comma; this will only be used 
                    //if there are multiple values for one of the headers
                    separator = ",";
                }
                sb.Append(builder.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public static ArrayList GetHeaderValues(NameValueCollection headers, string headerName)
        {
            ArrayList list = new ArrayList();
            string[] values = headers.GetValues(headerName);
            if (values != null)
            {
                foreach (string str in values)
                {
                    list.Add(str.TrimStart(null));
                }
            }
            return list;
        }

        public static string AuthorizationHeader(string method, DateTime now, WebRequest request, string ifMatch = "", string md5 = "")
        {
            //this is the raw representation of the message signature 
            var messageSignature = String.Format("{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                method,
                (method == "GET" || method == "HEAD") ? String.Empty : request.ContentLength.ToString(),
                ifMatch,
                GetCanonicalizedHeaders(request),
                GetCanonicalizedResource(request.RequestUri, AzureCredentials.StorageAccountName),
                md5
                );

            //now turn it into a byte array
            byte[] SignatureBytes = System.Text.Encoding.UTF8.GetBytes(messageSignature);

            //create the HMACSHA256 version of the storage key
            System.Security.Cryptography.HMACSHA256 SHA256 =
                new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(AzureCredentials.StorageAccountKey));

            //Compute the hash of the SignatureBytes and convert it to a base64 string.
            string signature = Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));

            //this is the actual header that will be added to the list of request headers
            string AuthorizationHeader = "SharedKey " + AzureCredentials.StorageAccountName
                + ":" + Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));

            return AuthorizationHeader;
        }
    }
}
