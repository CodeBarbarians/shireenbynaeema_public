namespace Server
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogPostService blogPostService;

        public BlogController(IBlogPostService blogPostService) { this.blogPostService = blogPostService; }

        [HttpGet("Published")]
        public async Task<ActionResult> GetPublished([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var response = await blogPostService.ListPublished(skip, take);
            return Ok(response);
        }

        [HttpGet("BySlug/{slug}")]
        public async Task<ActionResult> GetBySlug(string slug)
        {
            var response = await blogPostService.GetBySlug(slug);
            return Ok(response);
        }

        [HttpPost("Admin/List")]
        [Authorize]
        public async Task<ActionResult> ListAdmin(ListRequest request)
        {
            var response = await blogPostService.ListAll(request);
            return Ok(response);
        }

        [HttpPost("Admin/Add")]
        [Authorize]
        public async Task<ActionResult> Add(BlogPost_AddEdit request)
        {
            var post = new BlogPost
            {
                Id = request.Id ?? Guid.NewGuid(),
                Title = request.Title,
                Slug = request.Slug,
                Excerpt = request.Excerpt,
                Content = request.Content,
                CoverImageUrl = request.CoverImageUrl,
                Author = request.Author,
                IsPublished = request.IsPublished,
                PublishedOn = request.IsPublished ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() : null
            };

            if (request.Id.HasValue)
            {
                var response = await blogPostService.UpdateAsync(new SaveRequest<BlogPost> { EntityId = request.Id, Entity = post });
                return Ok(response);
            }
            else
            {
                var response = await blogPostService.AddAsync(new SaveRequest<BlogPost> { Entity = post });
                return Ok(response);
            }
        }

        [HttpDelete("Admin/Delete/{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            var response = await blogPostService.DeleteAsync(id);
            return Ok(response);
        }
    }
}
