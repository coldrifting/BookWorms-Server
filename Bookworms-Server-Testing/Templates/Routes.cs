namespace BookwormsServerTesting.Templates;

public abstract class Routes
{
    public static class Children
    {
        public static string All =>
            "/children/all";
        public static string Add(string childName) =>
            $"/children/add?childName={childName}";
        public static string Edit(Guid childId) =>
            $"/children/{childId}/edit";
        public static string Remove(Guid childId) =>
            $"/children/{childId}/remove";
    }

    public static class Bookshelves
    {
        public static string All(Guid childId) =>
            $"/children/{childId}/shelves";
        public static string Details(Guid childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/details";
        public static string Add(Guid childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/add";
        public static string Rename(Guid childId, string bookshelfName, string newName) =>
            $"/children/{childId}/shelves/{bookshelfName}/rename?newName={newName}";
        public static string Delete(Guid childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/delete";
        public static string Clear(Guid childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/clear";
        public static string Insert(Guid childId, string bookshelfName, string bookId) =>
            $"/children/{childId}/shelves/{bookshelfName}/insert?bookId={bookId}";
        public static string Remove(Guid childId, string bookshelfName, string bookId) =>
            $"/children/{childId}/shelves/{bookshelfName}/remove?bookId={bookId}";
    }

    public static class Search
    {
        public static string Title(string title) =>
            $"/search/title?query={title}";
    }

    public static class Books
    {
        public static string Details(string bookId) => 
            $"/books/{bookId}/details";
        public static string Cover (string bookId) =>
            $"/books/{bookId}/cover";
        public static string CoverBatch =>
            "/books/covers";
    }

    public static class Reviews
    {
        public static string All(string bookId) => 
            $"/books/{bookId}/reviews";
        public static string AllParam(string bookId, int start, int max) => 
            $"/books/{bookId}/reviews?start={start}&max={max}";
        public static string Edit(string bookId) => 
            $"/books/{bookId}/review";
    }

    public static class User
    {
        public static string All =>
            "/user/all";
        public static string Details  => 
            "/user/details";
        public static string Login => 
            "/user/login";
        public static string Register => 
            "/user/register";
    }
}