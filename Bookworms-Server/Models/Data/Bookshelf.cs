namespace BookwormsServer.Models.Data;

public enum BookshelfType
{
    Completed,
    InProgress,
    Custom,
    Classroom
} 

public class BookshelfResponse(BookshelfType type, string name, List<BookResponse> books, List<BookCompletionData>? completions = null)
{
    public BookshelfType Type { get; } = type;
    public string Name { get; } = name;
    public List<BookResponse> Books { get; } = books;
    public List<BookCompletionData>? Completions { get; set; } = completions;

    public override bool Equals(object? other)
    {
        if (other is not BookshelfResponse otherBookshelfPreview)
        {
            return false;
        }

        if (Type != otherBookshelfPreview.Type)
        {
            return false;
        }
        
        if (Books.Count != otherBookshelfPreview.Books.Count)
        {
            return false;
        }

        if (Name != otherBookshelfPreview.Name)
        {
            return false;
        }

        for (int i = 0; i < Books.Count; i++)
        {
            if (Books[i] != otherBookshelfPreview.Books[i])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Name, Books);
    }
}