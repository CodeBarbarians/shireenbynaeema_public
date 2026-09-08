namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class RatingService : IRatingService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public RatingService(DatabaseContext db, IResponse response) { this.db = db; this.resp = response; }

    public async Task<IResponse> GetByProduct(Guid productId)
    {
        var ratings = await db.Ratings.AsNoTracking().Where(r => r.ProductId == productId)
            .Include(r => r.User).OrderByDescending(r => r.CreatedOn)
            .Select(r => new ReviewDto
            {
                Id = r.Id, UserName = r.User != null ? r.User.DisplayName : "",
                Score = r.Score, Comment = r.Comment, CreatedOn = r.CreatedOn ?? 0
            }).ToListAsync();

        resp.IsSuccess = true;
        resp.Data = ratings;
        return resp;
    }

    public async Task<IResponse> Create(Guid userId, ReviewCreateRequest request)
    {
        var rating = new Rating
        {
            Id = Guid.NewGuid(), ProductId = request.ProductId, UserId = userId,
            Score = request.Score, Comment = request.Comment,
            CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        db.Ratings.Add(rating);

        var product = await db.Products.FindAsync(request.ProductId);
        if (product != null)
        {
            var ratings = await db.Ratings.Where(r => r.ProductId == request.ProductId).ToListAsync();
            ratings.Add(rating);
            product.Rating = (decimal)ratings.Average(r => r.Score);
            product.ReviewCount = ratings.Count;
        }

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        return resp;
    }
}
}