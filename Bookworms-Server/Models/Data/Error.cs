namespace BookwormsServer.Models.Data;

public class ErrorDTO(string error, string description)
{
    public override bool Equals(object? other)
    {
        if (other is ErrorDTO otherError)
        {
            return Error == otherError.Error && Description == otherError.Description;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Error, Description);
    }

    public string Error { get; } = error;
    public string Description { get; } = description;

    // Prefab error classes for consistent errors
    public static ErrorDTO Unauthorized => new("Unauthorized", "Valid token required for this route");
    public static ErrorDTO Forbidden => new("Forbidden", "User is not permitted to perform this action");
    
    public static ErrorDTO UsernameAlreadyExists => new("Invalid Credentials", "The specified Username already exists");
    public static ErrorDTO LoginFailure => new("Invalid Login Credentials", "Incorrect username and/or password");
    public static ErrorDTO UsernameNotFound => new("Username Not Found", "Unable to find the given username");
    
    public static ErrorDTO BookNotFound => new("Book Not Found", "Unable to find a book matching the given id");

    public static ErrorDTO BookCoverNotFound => new("Book Cover Not Found",
        "Unable to find the requested book cover on external API server");
    
    public static ErrorDTO ReviewNotFound => new("Review Not Found", "Unable to find a review matching the given id");

    public static ErrorDTO ReviewAlreadyExists =>
        new("Review Already Exists", "Unable to create a new review as it already exists");

   public static ErrorDTO ReviewStarRatingOrTextRequired => new("Invalid Review Data", "Star Rating or Review Text required");

   public static ErrorDTO ChildNotFound => new("Child Not Found",
       "Unable to find a child with the given name under the logged in parent");
   public static ErrorDTO ChildAlreadyExists => new("Child Already Exists",
       "Unable to create a child with the same name under the same parent");
   
   public static ErrorDTO UserNotParent => new("User is not a Parent",
       "The requested operation is invalid because the logged in user is not a parent.");
   
   public static ErrorDTO UserNotTeacher => new("User is not a Teacher",
       "The requested operation is invalid because the logged in user is not a teacher.");
}