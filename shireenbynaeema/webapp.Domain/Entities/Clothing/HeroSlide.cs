namespace Domain
{
    public class HeroSlide
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ImageUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = "/collections/all";
        public string ButtonText { get; set; } = "Shop Now";
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
