using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System.IO;
using System.Threading.Tasks;

namespace StaticSiteFunctions.Functions
{
    public static class SendConfirmedEmail
    {
        [FunctionName("SendConfirmedEmail")]
        public static async Task Run(
            [QueueTrigger("confirmed-formdata")] ContactFormModel formData,
            [SendGrid] IAsyncCollector<SendGridMessage> emailCollector,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"SendConfirmedEmail: Queue trigger fired");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("sites_configuration.json", optional: true, reloadOnChange: true)
                .Build();

            var html = await GetEmailContent(context, config, formData);
            var message = BuildMessage(config, formData, html);

            log.LogInformation($"SendConfirmedEmail: Sending confirmation email");
            await emailCollector.AddAsync(message);
        }

        private static async Task<string> GetEmailContent(ExecutionContext context, IConfigurationRoot config, ContactFormModel data)
        {
            var file = Path.Combine(context.FunctionAppDirectory, $"Templates/{data.Hostname.ToLowerInvariant()}/contact_template.html");
            var html = await File.ReadAllTextAsync(file);

            var siteConfig = config.GetSection(data.Hostname);

            return html
                .Replace("{{FromDisplayName}}", siteConfig.GetSection("fromDisplayName").Value)
                .Replace("{{Hostname}}", siteConfig.GetSection("hostname").Value)
                .Replace("{{Name}}", data.Name)
                .Replace("{{EmailAddress}}", data.EmailAddress)
                .Replace("{{Message}}", data.Message);
        }

        private static SendGridMessage BuildMessage(IConfigurationRoot config, ContactFormModel data, string body)
        {
            var siteConfig = config.GetSection(data.Hostname);

            var message = new SendGridMessage();
            message.AddTo(
                data.EmailAddress,
                $"{data.Name}");
            message.AddContent("text/html", body);
            message.SetFrom(
                siteConfig.GetSection("fromAddress").Value,
                siteConfig.GetSection("fromDisplayName").Value);
            message.SetSubject(siteConfig.GetSection("ConfirmedSubject").Value);
            return message;
        }
    }
}