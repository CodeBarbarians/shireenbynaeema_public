namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("Customer")]
[EntityDisplayName("Customer")]
public class Customer : SoftDeletableAuditable, IIdentifiable
{
    public Customer()
    {
        Phone = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Phone { get; set; }

    public int OrderCount { get; set; }

    public decimal TotalSpent { get; set; }

    public ICollection<Address> Addresses { get; set; } = [];

    public ICollection<Order> Orders { get; set; } = [];

    public ICollection<Cart> Carts { get; set; } = [];
}
}