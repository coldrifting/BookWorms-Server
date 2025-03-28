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

    public static ErrorResponse DuplicateDifficultyRating => new("Difficulty Rating Already Exists",
        "A child can only rate a book's difficulty once");

    public static ErrorResponse StarRatingRequired => new("Star Rating Required",
        "A star rating is required when inserting into a Completed bookshelf");

    public static ErrorResponse ChildNotFound => new("Child Not Found",
       "Unable to find a child matching the requested id");

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

   public static ErrorResponse BookshelfNameReserved(string name) => new("Bookshelf Name Reserved",
       $"The name \"{name}\" is reserved and cannot be used for custom bookshelves");

   public static ErrorResponse BookshelfBookNotFound => new("Bookshelf Book Not Found",
       "Unable to find a book with the given id in this bookshelf");
   
   public static ErrorResponse ClassroomNotFound => new("Classroom Not Found", 
       "Unable to find a class with the given classroom code");
   
   public static ErrorResponse BookAlreadyInClass => new("Book Already In Class", 
       "The requested book is already part of this classroom's reading list");

   public static ErrorResponse ChildAlreadyInClass => new("Child Already In Class",
       "The child is already a member of this classroom");

   public static ErrorResponse ChildNotInClass => new("Child Not In Class",
       "The child is not a member of this classroom");

   public static ErrorResponse ClassroomAlreadyExists => new("Classroom Already Exists",
       "The classroom already exists for this teacher");

   public static ErrorResponse ClassroomAnnouncementNotFound => new("Classroom Announcement Not Found",
       "Unable to find a classroom announcement with the specified Id");
   
    public static ErrorResponse GoalNotFound => new("Goal Not Found",
        "Could not find the requested goal");

    public static ErrorResponse GoalTypeInvalid => new("Goal Type Invalid",
        "The provided goal type is not valid for this route");

    public static ErrorResponse GoalEditInfoInvalid => new("Goal Edit Info Invalid",
        "Provided values can not be 0");
}