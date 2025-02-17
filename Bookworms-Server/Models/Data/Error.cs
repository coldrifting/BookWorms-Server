namespace BookwormsServer.Models.Data;

public record ErrorResponse(string Error, string Description)
{
    // Prefab error classes for consistent errors
    public static ErrorResponse Unauthorized => new("Unauthorized", 
        "Valid token required for this route");
    
    public static ErrorResponse Forbidden => new("Forbidden", 
        "User is not permitted to access this route");
    
    public static ErrorResponse UsernameAlreadyExists => new("Invalid Credentials", 
        "The specified Username already exists");
    
    public static ErrorResponse LoginFailure => new("Invalid Login Credentials", 
        "Incorrect username and/or password");
    
    public static ErrorResponse ReviewNotFound => new("Review Not Found", 
        "The user has not reviewed the specified book");
    
    public static ErrorResponse BookNotFound => new("Book Not Found", 
        "Unable to find the specified book");
    
    public static ErrorResponse BookIdInvalid => new("Book Id Invalid", 
        "Unable to find a book matching the given id");

    public static ErrorResponse BookCoverNotFound => new("Book Cover Not Found",
        "Unable to find the requested book cover on external API server");

    public static ErrorResponse DuplicateDifficultyRating => new("Difficulty rating already exists",
        "A child can only rate a book's difficulty once");

    public static ErrorResponse ChildNotFound => new("Child Not Found",
       "Unable to find a child with the given name under the logged in parent");

    public static ErrorResponse UserNotFound => new("User Not Found",
       "Unable to find a user matching the given username");

    public static ErrorResponse UserNotAdmin => new("User is not an Administrator",
       "User must be an administrator to access this route");

    public static ErrorResponse UserNotParent => new("User is not a Parent",
       "User must be a parent to access this route");

    public static ErrorResponse UserNotTeacher => new("User is not a Teacher",
       "User must be a teacher to access this route");

    public static ErrorResponse BookshelfNotFound => new("Bookshelf Not Found",
       "Unable to find a bookshelf with the given name for the currently selected child");

    public static ErrorResponse BookshelfAlreadyExists => new("Bookshelf Already Exists",
       "Unable to add a bookshelf with the given name as one already exists under the selected child");

    public static ErrorResponse BookshelfBookNotFound => new("Bookshelf Book Not Found",
       "Unable to find a book with the given id in this bookshelf");

    public static ErrorResponse ClassroomNotFound => new("Classroom Not Found", 
       "Unable to find a class with the given classroom code");

    public static ErrorResponse InvalidIconIndex => new("Invalid Icon Index", 
       "The requested icon index is not valid");
}