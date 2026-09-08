namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("Setting")]
    [EntityDisplayName("Setting")]
    public class Setting : Auditable, IIdentifiable
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
