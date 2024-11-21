using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record ReviewDTO(string ReviewerUsername, string ReviewerName, double StarRating, string ReviewText)
{
    public static ReviewDTO From(Review r)
    {
        return new ReviewDTO(r.Reviewer!.Username, r.Reviewer.Name, r.StarRating, r.ReviewText ?? "");
    }
}