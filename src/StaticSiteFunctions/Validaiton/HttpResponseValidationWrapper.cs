using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaticSiteFunctions
{
    public class HttpResponseValidationWrapper<T>
    {
        public bool IsValid { get; set; }
        public T Value { get; set; }
        public IEnumerable<ValidationResult> ValidationResults { get; set; }
    }
}