using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record ReviewDTO(string ReviewerFirstName, string ReviewerLastName, string ReviewerRole, UserIcon ReviewerIcon, DateTime ReviewDate, double StarRating, string ReviewText)
{
    public static ReviewDTO From(Review r)
    {
        return new(
            r.Reviewer!.FirstName,
            r.Reviewer.LastName,
            r.Reviewer.Roles.First(),
            r.Reviewer.UserIcon,
            r.ReviewDate,
            r.StarRating, 
            r.ReviewText ?? "");
    }
}

public record ReviewAddOrUpdateRequestDTO(double StarRating, string? ReviewText);
