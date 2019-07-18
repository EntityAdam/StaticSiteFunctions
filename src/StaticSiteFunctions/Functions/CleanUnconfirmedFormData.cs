using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using StaticSiteFunctions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSiteFunctions
{
    public static class CleanUnconfirmedFormData
    {
        //Runs every hour
        //"0 0 * * * *"

        //Runs every minute
        //"0 * * * * *"
        [FunctionName("CleanUnconfirmedFormData")]
        public static async Task Run(
            [TimerTrigger("0 0 * * * *")]TimerInfo myTimer,
            [Table("AwaitConfirmationFormData")] CloudTable formDataTable,
            ILogger log)
        {
            log.LogInformation($"CleanUnconfirmedFormData: Checking for timestamps older than {DateTimeOffset.Now.AddDays(-1)}");

            TableQuery<ContactFormEntity> rangeQuery = new TableQuery<ContactFormEntity>().Where(
                TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTimeOffset.Now.AddDays(-1)));

            var result = await formDataTable.ExecuteQuerySegmentedAsync(rangeQuery, null);

            TableBatchOperation deleteBatch = new TableBatchOperation();

            foreach (var item in result)
            {
                log.LogInformation($"CleanUnconfirmedFormData: Queue deletion of {item.Id} with timestamp {item.Timestamp}");
                var deleteOperation = TableOperation.Delete(item);
                deleteBatch.Add(deleteOperation);
            }

            if (deleteBatch.Count() > 0)
            {
                log.LogInformation($"CleanUnconfirmedFormData: Deleting {deleteBatch.Count()} items");
                await formDataTable.ExecuteBatchAsync(deleteBatch);
            }
            else
            {
                log.LogInformation($"CleanUnconfirmedFormData: No entries deleted");
            }
        }
    }
}