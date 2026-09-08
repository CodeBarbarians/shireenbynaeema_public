namespace Domain
{
    using SharedServices;

    public class Category_AddEdit
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowInNav { get; set; } = true;
}

public class CategoryListRequest : ListRequest
{
    public string? Search { get; set; }
}

public class Category_Listing
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool ShowInNav { get; set; }
    public int ProductCount { get; set; }
    }
}
