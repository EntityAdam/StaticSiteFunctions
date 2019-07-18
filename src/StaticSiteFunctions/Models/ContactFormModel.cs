using System;
using System.ComponentModel.DataAnnotations;

namespace StaticSiteFunctions
{
    public class ContactFormModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Hostname { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public string Phone { get; set; }

        [StringLength(2048, MinimumLength = 10)]
        public string Message { get; set; }
    }
}