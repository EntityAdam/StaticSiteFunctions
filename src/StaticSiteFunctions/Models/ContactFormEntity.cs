using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace StaticSiteFunctions.Models
{
    public class ContactFormEntity : TableEntity
    {
        public ContactFormEntity()
        {
        }

        public ContactFormEntity(ContactFormModel formModel)
        {
            Id = formModel.Id;
            Hostname = formModel.Hostname;
            Name = formModel.Name;
            EmailAddress = formModel.EmailAddress;
            Phone = formModel.Phone;
            Message = formModel.Message;

            base.PartitionKey = Hostname;
            base.RowKey = Id.ToString();
        }

        public Guid Id { get; set; }
        public string Hostname { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
    }
}