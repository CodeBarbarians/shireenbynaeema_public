namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("CmsPage")]
    [EntityDisplayName("CMS Page")]
    public class CmsPage : Auditable, IIdentifiable
    {
        [Key]
        public Guid Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
