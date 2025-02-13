using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public class BookshelfPreviewResponseDTO(string name, BookshelfBookPreviewDTO[] books)
{
    public string Name { get; } = name;
    public BookshelfBookPreviewDTO[] Books { get; } = books;

    public override bool Equals(object? other)
    {
        if (other is not BookshelfPreviewResponseDTO otherBookshelfPreview)
        {
            return false;
        }
        
        if (Books.Length != otherBookshelfPreview.Books.Length)
        {
            return false;
        }

        if (Name != otherBookshelfPreview.Name)
        {
            return false;
        }

        for (int i = 0; i < Books.Length; i++)
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

    public static BookshelfPreviewResponseDTO From(string name, IEnumerable<ChildBookshelfBook> books)
    {
        List<BookshelfBookPreviewDTO> bookPreviews = [];
        foreach (var bookshelfBook in books)
        {
            if (bookshelfBook.Book != null)
            {
                bookPreviews.Add(BookshelfBookPreviewDTO.From(bookshelfBook.Book));
            }
        }

        return new(name, bookPreviews.ToArray());
    }

    public static BookshelfPreviewResponseDTO From(string name, IEnumerable<Book> books)
    {
        List<BookshelfBookPreviewDTO> bookPreviews = [];
        bookPreviews.AddRange(books.Select(BookshelfBookPreviewDTO.From));

        return new(name, bookPreviews.ToArray());
    }
}

public record BookshelfBookPreviewDTO(string BookId, string Title, List<string> Authors)
{
    public static BookshelfBookPreviewDTO From(Book book)
    {
        return new(book.BookId, book.Title, book.Authors);
    }
}