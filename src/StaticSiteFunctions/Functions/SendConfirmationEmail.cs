using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using StaticSiteFunctions.Models;
using System.IO;
using System.Threading.Tasks;

//Install-Package Microsoft.Azure.WebJobs.Extensions.SendGrid -Version 3.0.0
namespace StaticSiteFunctions
{
    public static class SendConfirmationEmail
    {
        [FunctionName("SendConfirmationEmail")]
        public static async Task Run(
            [QueueTrigger("unconfirmed-formdata")] ContactFormModel formData,
            [SendGrid] IAsyncCollector<SendGridMessage> emailCollector,
            [Table("AwaitConfirmationFormData")] IAsyncCollector<ContactFormEntity> formDataTable,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"SendConfirmationEmail: Queue trigger fired");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("sites_configuration.json", optional: true, reloadOnChange: true)
                .Build();

            var html = await GetEmailContent(context, config, formData);
            var message = BuildMessage(config, formData, html);

            log.LogInformation($"SendConfirmationEmail: Sending confirmation email");
            await emailCollector.AddAsync(message);

            log.LogInformation($"SendConfirmationEmail: Awaiting confirmation");
            var entity = new ContactFormEntity(formData);
            await formDataTable.AddAsync(entity);
        }

        private static async Task<string> GetEmailContent(ExecutionContext context, IConfiguration config, ContactFormModel data)
        {
            var file = Path.Combine(context.FunctionAppDirectory, $"Templates/{data.Hostname.ToLowerInvariant()}/confirm_template.html");
            var html = await File.ReadAllTextAsync(file);

            var endpoint = GetEndpoint(config, data);

            return html
                .Replace("{{name}}", $"{data.Name}")
                .Replace("{{endpoint}}", endpoint)
                .Replace("{{hostname}}", data.Hostname)
                .Replace("{{identifier}}", data.Id.ToString());
        }

        private static string GetEndpoint(IConfiguration config, ContactFormModel data)
        {
            var endpoint = config.GetSection(data.Hostname).GetSection("Endpoint").Value;
            return endpoint.Replace("{{identifier}}", data.Id.ToString());
        }

        private static SendGridMessage BuildMessage(IConfiguration config, ContactFormModel data, string body)
        {
            var message = new SendGridMessage();
            message.AddTo(data.EmailAddress, $"{data.Name}");
            message.AddContent("text/html", body);
            message.SetFrom(
                config.GetSection(data.Hostname).GetSection("fromAddress").Value,
                config.GetSection(data.Hostname).GetSection("fromDisplayName").Value);
            message.SetSubject(config.GetSection(data.Hostname).GetSection("confirmationSubject").Value);
            return message;
        }
    }
}