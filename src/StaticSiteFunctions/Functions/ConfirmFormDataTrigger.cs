using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using StaticSiteFunctions.Models;
using System.Threading.Tasks;

namespace StaticSiteFunctions
{
    public static class ConfirmFormDataTrigger
    {
        [FunctionName("confirm")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "confirm")] HttpRequest req,
            [Table("AwaitConfirmationFormData")] CloudTable formDataTable,
            [Queue("confirmed-formdata")] IAsyncCollector<ContactFormEntity> formDataQueue,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("ConfirmFormDataTrigger: HTTP trigger fired");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("sites_configuration.json", optional: false, reloadOnChange: true)
                .Build();

            string id = req.Query["id"];
            string hostname = req.Query["hostname"];

            if (string.IsNullOrWhiteSpace(hostname))
            {
                log.LogInformation($"ConfirmFormDataTrigger: Invalid Partition Key: '{hostname}'");
                return new BadRequestObjectResult("The confirmation link is either invalid.");
            }

            var siteConfig = config.GetSection(hostname);
            log.LogInformation($"ConfirmFormDataTrigger: Received Partition Key: '{hostname}' and ID '{id}'");

            if (string.IsNullOrWhiteSpace(id))
            {
                log.LogInformation($"ConfirmFormDataTrigger: Invalid ID: '{id}'");

                //TODO: Redirect to Error URL
                return new BadRequestObjectResult("The confirmation link is either invalid.");
            }

            var retrieve = TableOperation.Retrieve<ContactFormEntity>(hostname, id);
            var result = await formDataTable.ExecuteAsync(retrieve);
            if (result.Result == null)
            {
                log.LogInformation($"ConfirmFormDataTrigger: Invalid ID '{id}'");

                var errorUrl = siteConfig.GetSection("ConfirmationErrorUrl").Value;
                if (string.IsNullOrWhiteSpace(errorUrl))
                {
                    //TODO: Redirect to Error URL
                    return new BadRequestObjectResult("The confirmation link is either invalid or expired, please resubmit.");
                }
                return new RedirectResult(errorUrl);
            }

            var item = (ContactFormEntity)result.Result;
            log.LogInformation($"ConfirmFormDataTrigger: {item.Id} confirmed");
            await formDataQueue.AddAsync(item);

            var deleteOperation = TableOperation.Delete(item);
            await formDataTable.ExecuteAsync(deleteOperation);
            log.LogInformation($"ConfirmFormDataTrigger: {item.Id} removed");

            var successUrl = siteConfig.GetSection("ConfirmationSuccessUrl").Value;
            if (string.IsNullOrWhiteSpace(successUrl))
            {
                return new OkObjectResult($"Confirmed {item.Id}");
            }

            return new RedirectResult(successUrl);
        }
    }
}