namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("Category")]
    [EntityDisplayName("Category")]
    public class Category : SoftDeletableAuditable, IIdentifiable
    {
        public Category()
        {
            Name = string.Empty;
            Slug = string.Empty;
            Description = string.Empty;
            ImageUrl = string.Empty;
        }

        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public Guid? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public Category? Parent { get; set; }

        public string ImageUrl { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool ShowInNav { get; set; } = true;

        public ICollection<Product> Products { get; set; } = [];

        public ICollection<Category> Children { get; set; } = [];

        public ICollection<SizeChart> SizeCharts { get; set; } = [];

        public ICollection<CategoryImage> Images { get; set; } = [];
    }
}