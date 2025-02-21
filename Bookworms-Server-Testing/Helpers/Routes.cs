using BookwormsServer.Models.Data;

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
        public static string Details(string childId, BookshelfType bookshelfType, string bookshelfName) =>
            $"/children/{childId}/shelves/{bookshelfType}/{bookshelfName}/details";
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
        public static string InsertCompleted(string childId, string bookshelfName, string bookId, double starRating) =>
            $"/children/{childId}/shelves/{bookshelfName}/insert?bookId={bookId}&starRating={starRating}";
        public static string Remove(string childId, string bookshelfName, string bookId) =>
            $"/children/{childId}/shelves/{bookshelfName}/remove?bookId={bookId}";
    }

    public static class Classrooms
    {
        // Parents (Children)
        public static string All(string childId) =>
            $"/children/{childId}/classrooms/all";
        public static string Join(string childId, string classCode) =>
            $"/children/{childId}/classrooms/{classCode}/join";
        public static string Leave(string childId, string classCode) =>
            $"/children/{childId}/classrooms/{classCode}/leave";
        
        // Teachers
        public const string Details = "/homeroom/details";
        public static string Create(string className) =>
            $"/homeroom/create?className={className}";
        public static string Rename(string newClassName) => 
            $"/homeroom/rename?newClassName={newClassName}";
        public const string Delete = "/homeroom/delete";

        public static string BookshelfCreate(string bookshelfName) =>
            $"/homeroom/shelves/{bookshelfName}/create";

        public static string BookshelfRename(string bookshelfName, string newName) =>
            $"/homeroom/shelves/{bookshelfName}/rename?newBookshelfName={newName}";

        public static string BookshelfDelete(string bookshelfName) =>
            $"/homeroom/shelves/{bookshelfName}/delete";

        public static string BookshelfInsertBook(string bookshelfName, string bookId) =>
            $"/homeroom/shelves/{bookshelfName}/insert?bookId={bookId}";

        public static string BookshelfRemoveBook(string bookshelfName, string bookId) =>
            $"/homeroom/shelves/{bookshelfName}/remove?bookId={bookId}";
    }

    public static class ClassGoals
    {
        public const string All = "/homeroom/goals";
        public const string Add = "/homeroom/goals/add";
        public static string Edit(string goalId) => $"/homeroom/goals/{goalId}/edit";
        public static string Delete(string goalId) => $"/homeroom/goals/{goalId}/delete";
        public static string Details(string goalId) => $"/homeroom/goals/{goalId}/details";
        public static string DetailsAll(string goalId) => $"/homeroom/goals/{goalId}/details/all";
        
        public static string UpdateLog(string childId, string classCode, string goalId) =>
            $"/children/{childId}/classrooms/{classCode}/goals/{goalId}/update";
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
        public static string DetailsExtended(string bookId) => 
            $"/books/{bookId}/details/all";
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

    public static string RateDifficulty(string bookId) =>
        $"/books/{bookId}/rate-difficulty";
    
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