namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("Address")]
    [EntityDisplayName("Address")]
    public class Address : Auditable, IIdentifiable
    {
        public Address()
        {
            Label = string.Empty;
            Street = string.Empty;
            City = string.Empty;
            State = string.Empty;
            ZipCode = string.Empty;
            Country = string.Empty;
        }

        [Key]
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public string Label { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }

        public bool IsDefault { get; set; }
    }
}