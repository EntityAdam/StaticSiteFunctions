using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace StaticSiteFunctions
{
    public static class HttpRequestExtensions
    {
        public static async Task<HttpResponseValidationWrapper<T>> GetValidationModelAsync<T>(this HttpRequest request)
        {
            return await ((request.ContentType.StartsWith("application/x-www-form-urlencoded"))
                ? GetFormAsync<T>(request)
                : GetBodyAsync<T>(request));
        }

        public static async Task<HttpResponseValidationWrapper<T>> GetFormAsync<T>(this HttpRequest request)
        {
            var instance = new HttpResponseValidationWrapper<T>();
            var formCollection = await request.ReadFormAsync();
            instance.Value = CopyThingy.Populate<T>(formCollection);

            var results = new List<ValidationResult>();
            instance.IsValid = Validator.TryValidateObject(instance.Value, new ValidationContext(instance.Value, null, null), results, true);
            instance.ValidationResults = results;
            return instance;
        }

        public static async Task<HttpResponseValidationWrapper<T>> GetBodyAsync<T>(this HttpRequest request)
        {
            var body = new HttpResponseValidationWrapper<T>();
            var bodyString = await request.ReadAsStringAsync();
            body.Value = JsonConvert.DeserializeObject<T>(bodyString);

            var results = new List<ValidationResult>();
            body.IsValid = Validator.TryValidateObject(body.Value, new ValidationContext(body.Value, null, null), results, true);
            body.ValidationResults = results;
            return body;
        }
    }
}