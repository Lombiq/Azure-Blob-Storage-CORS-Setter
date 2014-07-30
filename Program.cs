using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            new SimpleAzureBlobCorsSetter(
                new Uri(""),
                new StorageCredentials("", ""))
            .SetBlobCorsGetAnyOrigin(false);

        }
    }


    // Taken directly from: http://blog.codingoutloud.com/2014/02/21/stupid-azure-trick-6-a-cors-toggler-command-line-tool-for-windows-azure-blobs/
    public class SimpleAzureBlobCorsSetter
    {
        public CloudBlobClient CloudBlobClient { get; private set; }

        public SimpleAzureBlobCorsSetter(Uri blobServiceUri, StorageCredentials storageCredentials)
        {
            this.CloudBlobClient = new CloudBlobClient(blobServiceUri, storageCredentials);
        }

        public SimpleAzureBlobCorsSetter(CloudBlobClient blobClient)
        {
            this.CloudBlobClient = blobClient;
        }

        /// <summary>
        /// Set Blob Service CORS settings for specified Windows Azure Storage Account.
        /// Either sets to a hard-coded set of values (see below) or clears of all CORS settings.
        ///
        /// Does not check for any existing CORS settings, but clobbers with the CORS settings
        /// to allow HTTP GET access from any origin. Non-CORS settings are left intact.
        ///
        /// Most useful for scenarios where a file is published in Blob Storage for read-access
        /// by any client.
        ///
        /// Can also be useful in conjunction with Valet Key Pattern-style limited access, as
        /// might be useful with a mobile application.
        /// </summary>
        /// <param name="clear">if true, clears all CORS setting, else allows GET from any origin</param>
        public void SetBlobCorsGetAnyOrigin(bool clear)
        {
            // http://msdn.microsoft.com/en-us/library/windowsazure/dn535601.aspx
            var corsGetAnyOriginRule = new CorsRule();
            corsGetAnyOriginRule.AllowedOrigins.Add("*"); // allow access to any client
            corsGetAnyOriginRule.AllowedMethods = CorsHttpMethods.Get; // only CORS-enable http GET
            corsGetAnyOriginRule.ExposedHeaders.Add("*"); // let client see any header we've configured
            corsGetAnyOriginRule.AllowedHeaders.Add("*"); // let clients request any header they can think of
            corsGetAnyOriginRule.MaxAgeInSeconds = (int)TimeSpan.FromHours(10).TotalSeconds; // clients are safe to cache CORS config for up to this long

            var blobServiceProperties = this.CloudBlobClient.GetServiceProperties();
            if (clear)
            {
                blobServiceProperties.Cors.CorsRules.Clear();
            }
            else
            {
                blobServiceProperties.Cors.CorsRules.Clear(); // replace current property set
                blobServiceProperties.Cors.CorsRules.Add(corsGetAnyOriginRule);
            }
            this.CloudBlobClient.SetServiceProperties(blobServiceProperties);
        }

        public void DumpCurrentProperties()
        {
            var blobServiceProperties = this.CloudBlobClient.GetServiceProperties();
            var blobPropertiesStringified = StringifyProperties(blobServiceProperties);
            Console.WriteLine("Current Properties:\n{0}", blobPropertiesStringified);
        }

        internal string StringifyProperties(ServiceProperties serviceProperties)
        {
            // JsonConvert.SerializeObject(serviceProperties) for whole object graph or
            // JsonConvert.SerializeObject(serviceProperties.Cors) for just CORS
            return Newtonsoft.Json.JsonConvert.SerializeObject(serviceProperties, Formatting.Indented);
        }
    }
}
