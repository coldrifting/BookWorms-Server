using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record ReviewDTO(string ReviewerName, string ReviewerRole, UserIcon ReviewerIcon, double StarRating, string ReviewText)
{
    public static ReviewDTO From(Review r)
    {
        return new(
            r.Reviewer!.Name,
            r.Reviewer.Roles.First(),
            r.Reviewer.UserIcon,
            r.StarRating, 
            r.ReviewText ?? "");
    }
}

public record ReviewRequestDTO(double StarRating, string ReviewText);