namespace BookwormsServer.Models.Data;

public class BookshelfResponse(string name, List<BookResponsePreview> books)
{
    public string Name { get; } = name;
    public List<BookResponsePreview> Books { get; } = books;

    public override bool Equals(object? other)
    {
        if (other is not BookshelfResponse otherBookshelfPreview)
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
        return HashCode.Combine(Name, Books);
    }
}