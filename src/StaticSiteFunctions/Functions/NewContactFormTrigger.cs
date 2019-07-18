using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StaticSiteFunctions
{
    public static class NewContactFormTrigger
    {
        //TODO: Validate configuration
        [FunctionName("contactForm")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "contactForm")] HttpRequest req,
            [Queue("unconfirmed-formdata")] IAsyncCollector<ContactFormModel> formQueue,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("NewContactFormTrigger: HTTP trigger fired");

            var formValidationModel = await req.GetValidationModelAsync<ContactFormModel>();
            if (!formValidationModel.IsValid)
            {
                log.LogInformation("NewContactFormTrigger: Received invalid form data");
                return new BadRequestObjectResult(formValidationModel.ValidationResults);
            }

            log.LogInformation($"NewContactFormTrigger: Received valid form data");

            var data = formValidationModel.Value;

            //Try set the hostname
            try
            {
                data.Hostname = GetHost(req);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }


            var confirmTemplate = Path.Combine(context.FunctionAppDirectory, $"Templates/{data.Hostname.ToLowerInvariant()}/confirm_template.html");
            var contactTemplate = Path.Combine(context.FunctionAppDirectory, $"Templates/{data.Hostname.ToLowerInvariant()}/contact_template.html");

            if (!File.Exists(confirmTemplate) || !File.Exists(contactTemplate))
            {
                log.LogInformation($"NewContactFormTrigger: {data.Hostname} is not configured properly. Please try again later.");
                return new BadRequestObjectResult(formValidationModel.ValidationResults);
            }

            await formQueue.AddAsync(data);

            return new OkObjectResult("success");
        }

        private static string GetHost(HttpRequest req)
        {
            //for local debugging only
            //if (req.Host.Host == "localhost")
            //{
            //    return "entityadam.com";
            //}

            var origin = GetUriIgnoreException(req.Headers["Origin"].ToString());
            var referer = GetUriIgnoreException(req.Headers["Referer"].ToString());
            if (origin != null)
            {
                return origin.Host;
            }
            if (referer != null)
            {
                return referer.Host;
            }
            else
            {
                throw new ArgumentException(nameof(req));
            }
        }

        private static Uri GetUriIgnoreException(string uri)
        {
            try
            {
                return new Uri(uri);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}