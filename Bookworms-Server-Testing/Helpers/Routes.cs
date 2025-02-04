namespace BookwormsServerTesting.Helpers;

public static class Routes
{
    public static class Children
    {
        public static string All =>
            "/children/all";
        public static string Add(string childName) =>
            $"/children/add?childName={childName}";
        public static string Edit(string childId) =>
            $"/children/{childId}/edit";
        public static string Remove(string childId) =>
            $"/children/{childId}/remove";
    }

    public static class Bookshelves
    {
        public static string All(string childId) =>
            $"/children/{childId}/shelves";
        public static string Details(string childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/details";
        public static string Add(string childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/add";
        public static string Rename(string childId, string bookshelfName, string newName) =>
            $"/children/{childId}/shelves/{bookshelfName}/rename?newName={newName}";
        public static string Delete(string childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/delete";
        public static string Clear(string childId, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfName}/clear";
        public static string Insert(string childId, string bookshelfName, string bookId) =>
            $"/children/{childId}/shelves/{bookshelfName}/insert?bookId={bookId}";
        public static string Remove(string childId, string bookshelfName, string bookId) =>
            $"/children/{childId}/shelves/{bookshelfName}/remove?bookId={bookId}";
    }

    public static string Search(
        string? query = null,
        string? title = null,
        string? author = null,
        List<string>? subjects = null,
        double? ratingMin = null,
        int? levelMin = null,
        int? levelMax = null
        )
    {
        const string baseUrl = "/search";

        List<String> parameters = new List<string>();
        
        if (query is not null)
        {
            parameters.Add($"query={query}");
        }
        
        if (title is not null)
        {
            parameters.Add($"title={title}");
        }
        
        if (author is not null)
        {
            parameters.Add($"author={author}");
        }
        
        if (subjects is not null)
        {
            foreach (var subject in subjects)
            {
                parameters.Add($"subjects={subject}");
            }
        }
        
        if (ratingMin is not null)
        {
            parameters.Add($"ratingMin={ratingMin}");
        }
        
        if (levelMin is not null)
        {
            parameters.Add($"levelMin={levelMin}");
        }
        
        if (levelMax is not null)
        {
            parameters.Add($"levelMax={levelMax}");
        }

        string parametersString = String.Join('&', parameters);
        
        return $"{baseUrl}?{parametersString}";
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
        public static string Remove(string bookId) => 
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
        public static string Delete => 
            "/user/delete";
        public static string DeleteParam(string username) => 
            $"/user/delete?username={username}";
    }
}