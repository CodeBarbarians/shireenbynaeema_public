namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    public class BlogPostService : Service<BlogPost>, IBlogPostService
    {
        private readonly DatabaseContext db;
        private readonly IResponse resp;

        public BlogPostService(IRepository<BlogPost> repository, IResponse response, DatabaseContext db)
            : base(repository, response)
        {
            this.db = db;
            this.resp = response;
        }

        public async Task<IResponse> ListPublished(int skip, int take)
        {
            var query = db.BlogPosts.AsNoTracking()
                .Where(b => b.IsPublished && !b.IsDeleted)
                .OrderByDescending(b => b.PublishedOn);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(skip).Take(take)
                .Select(b => new BlogPost_Listing
                {
                    Id = b.Id, Title = b.Title, Slug = b.Slug,
                    Excerpt = b.Excerpt, CoverImageUrl = b.CoverImageUrl,
                    Author = b.Author, PublishedOn = b.PublishedOn,
                    ViewCount = b.ViewCount
                }).ToListAsync();

            resp.IsSuccess = true;
            resp.Data = new { entities = items, totalCount };
            return resp;
        }

        public async Task<IResponse> GetBySlug(string slug)
        {
            var post = await db.BlogPosts.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished && !b.IsDeleted);
            if (post == null)
            {
                resp.IsSuccess = false;
                resp.Message = "Blog post not found";
                return resp;
            }

            await db.BlogPosts
                .Where(b => b.Id == post.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.ViewCount, b => b.ViewCount + 1));

            post.ViewCount++;

            resp.IsSuccess = true;
            resp.Data = post;
            return resp;
        }

        public async Task<IResponse> ListAll(ListRequest request)
        {
            var query = db.BlogPosts.AsNoTracking().Where(b => !b.IsDeleted);
            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(b => b.CreatedOn)
                .Skip(request.Skip).Take(request.Take)
                .Select(b => new BlogPost_Listing
                {
                    Id = b.Id, Title = b.Title, Slug = b.Slug,
                    Excerpt = b.Excerpt, CoverImageUrl = b.CoverImageUrl,
                    Author = b.Author, IsPublished = b.IsPublished,
                    PublishedOn = b.PublishedOn, ViewCount = b.ViewCount
                }).ToListAsync();

            resp.IsSuccess = true;
            resp.Data = items.ToListResponse(request, totalCount);
            return resp;
        }
    }
}
