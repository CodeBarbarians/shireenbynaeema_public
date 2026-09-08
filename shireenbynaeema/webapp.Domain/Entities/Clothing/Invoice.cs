namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("Invoice")]
[EntityDisplayName("Invoice")]
public class Invoice : SoftDeletableAuditable, IIdentifiable
{
    public Invoice()
    {
        InvoiceNumber = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public string InvoiceNumber { get; set; }

    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public decimal Amount { get; set; }

    public decimal Tax { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime? DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    public string? PaymentIntentId { get; set; }
}
}