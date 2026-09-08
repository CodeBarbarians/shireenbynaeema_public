namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using SharedServices;

    [Table("CategoryImage")]
    [EntityDisplayName("CategoryImage")]
    public class CategoryImage : Auditable, IIdentifiable
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [JsonIgnore]
        public Category? Category { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string AltText { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsPrimary { get; set; }
    }
}
