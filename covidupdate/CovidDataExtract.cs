using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace covidupdate
{
    public class CovidDataExtract
    {
        public const string Url = "https://msss.gouv.qc.ca/professionnels/statistiques/documents/covid19/";
        public const string HospitalizationFileName = "hospitalizationdata.csv";

        [FunctionName("CovidDataExtract")]
        public async Task Run([TimerTrigger("0 0 12 * * *"
#if DEBUG
                , RunOnStartup = true
#endif
            )]
            TimerInfo myTimer, ILogger log)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Url);

            var stream = await client.GetStreamAsync("COVID19_Qc_RapportINSPQ_HospitalisationsSelonStatutVaccinalEtAge.csv");

            var blobServiceClient = new BlobServiceClient(GetCustomConnectionString("AzureBlobConnectionString"));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("public-covid");

            await blobContainerClient.DeleteBlobIfExistsAsync(HospitalizationFileName);

            await blobContainerClient.UploadBlobAsync(HospitalizationFileName, stream);
            log.LogInformation($"Update Hospitalization Data function executed at: {DateTime.Now}");
        }


        public static string GetCustomConnectionString(string name)
        {
            string conStr = System.Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(conStr)) // Azure Functions App Service naming convention
                conStr = System.Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}", EnvironmentVariableTarget.Process);
            return conStr;
        }
    }
}