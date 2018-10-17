using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WebJob1
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("videoqueue")] Input Input, TextWriter log)
        {
            Console.WriteLine("processing....");
            Console.WriteLine(Input.ResultsLocation);
            Console.WriteLine(Input.VideoUrl);
            VideoIndexer.ProcessVideoIndexing(Input.VideoUrl);
        }

        public static void PutInBlob(string text)
        {
            Console.WriteLine("starting to put in blob");
            string connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName=gcffiles;AccountKey=AQc0j8PIIClvetXowEMxcJ5eyY2In7cFV0OveiK3BhNFirrUvirUxzyb1tLZ4VKcEzLVjAQ/Nfn8IuWbTzCd/w==;EndpointSuffix=core.windows.net");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = cloudBlobClient.GetContainerReference("translation");
            string key = DateTime.UtcNow.ToString("yyyy-MM-dd-HH:mm:ss");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(key);
            blockBlob.UploadText(text);
            Console.WriteLine("finished");
        }
    }
}

/*
 * to test
 * 
 {"videoUrl": "https://topcs.blob.core.windows.net/public/Overview-of-the-Microsoft-AI-School_high.mp4",
"ResultsLocation": "anywhere"}
 */
