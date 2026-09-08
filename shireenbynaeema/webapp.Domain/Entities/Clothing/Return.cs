namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("Return")]
    [EntityDisplayName("Return")]
    public class Return : SoftDeletableAuditable, IIdentifiable
    {
        public Return()
        {
            Reason = string.Empty;
            Notes = string.Empty;
            BankAccount = string.Empty;
            BankName = string.Empty;
            AccountHolderName = string.Empty;
            Attachments = string.Empty;
        }

        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; } = "Requested";

        public decimal? RefundAmount { get; set; }

        public string? RefundPaymentId { get; set; }

        public string BankAccount { get; set; }

        public string BankName { get; set; }

        public string AccountHolderName { get; set; }

        public string Attachments { get; set; }

        public DateTime? RequestedOn { get; set; }

        public DateTime? ProcessedOn { get; set; }

        public string Notes { get; set; }

        public ICollection<ReturnItem> Items { get; set; } = [];
    }
}