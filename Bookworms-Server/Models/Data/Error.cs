namespace BookwormsServer.Models.Data;

public record ErrorDTO(string Error, string Description)
{
    // Prefab error classes for consistent errors
    public static ErrorDTO Unauthorized => new("Unauthorized", 
        "Valid token required for this route");
    
    public static ErrorDTO Forbidden => new("Forbidden", 
        "User is not permitted to access this route");
    
    public static ErrorDTO UsernameAlreadyExists => new("Invalid Credentials", 
        "The specified Username already exists");
    
    public static ErrorDTO LoginFailure => new("Invalid Login Credentials", 
        "Incorrect username and/or password");
    
    public static ErrorDTO ReviewNotFound => new("Review Not Found", 
        "The user has not reviewed the specified book");
    
    public static ErrorDTO BookNotFound => new("Book Not Found", 
        "Unable to find the specified book");
    
    public static ErrorDTO BookIdInvalid => new("Book Id Invalid", 
        "Unable to find a book matching the given id");

    public static ErrorDTO BookCoverNotFound => new("Book Cover Not Found",
        "Unable to find the requested book cover on external API server");

   public static ErrorDTO ChildNotFound => new("Child Not Found",
       "Unable to find a child with the given name under the logged in parent");
   
   public static ErrorDTO UserNotFound => new("User Not Found",
       "Unable to find a user matching the given username");
   
   public static ErrorDTO UserNotAdmin => new("User is not an Administrator",
       "User must be an administrator to access this route");
   
   public static ErrorDTO UserNotParent => new("User is not a Parent",
       "User must be a parent to access this route");
   
   public static ErrorDTO UserNotTeacher => new("User is not a Teacher",
       "User must be a teacher to access this route");

   public static ErrorDTO BookshelfNotFound => new("Bookshelf Not Found",
       "Unable to find a bookshelf with the given name for the currently selected child");

   public static ErrorDTO BookshelfAlreadyExists => new("Bookshelf Already Exists",
       "Unable to add a bookshelf with the given name as one already exists under the selected child");

   public static ErrorDTO BookshelfBookNotFound => new("Bookshelf Book Not Found",
       "Unable to find a book with the given id in this bookshelf");
   
   public static ErrorDTO ClassroomNotFound => new("Classroom Not Found", 
       "Unable to find a class with the given classroom code");

   public static ErrorDTO InvalidIconIndex => new("Invalid Icon Index", 
       "The requested icon index is not valid");
}