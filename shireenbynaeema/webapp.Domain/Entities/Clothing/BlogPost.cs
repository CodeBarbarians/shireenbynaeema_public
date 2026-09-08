namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("BlogPost")]
    [EntityDisplayName("BlogPost")]
    public class BlogPost : SoftDeletableAuditable, IIdentifiable
    {
        [Key]
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Excerpt { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string CoverImageUrl { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public long? PublishedOn { get; set; }

        public int ViewCount { get; set; }
    }
}
