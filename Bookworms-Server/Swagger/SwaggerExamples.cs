using System.Diagnostics.CodeAnalysis;
using BookwormsServer.Models.Data;
using Swashbuckle.AspNetCore.Filters;

using BookwormsServer.Utils;

namespace BookwormsServer.Swagger;


// Swagger finds these automatically; there's technically no need to explicitly refer to them with
//   `SwaggerResponseExample` or `SwaggerRequestBodyExample`


// Content responses

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class SwaggerExamples
{
    // BookDetailsController
    
    public class BookDetailsExtendedResponseExample : IExamplesProvider<BookDetailsExtendedDTO>
    {
        public BookDetailsExtendedDTO GetExamples()
        {
            return new(
                "OL35966594M",
                "Giving Day",
                ["Cori Doerrfeld"],
                3.5,
                "<b>A sweet, fun-filled follow-up to <i>The Welcome Wagon</i> from acclaimed author Cori Doerrfeld!</b><br> <br> Every year, the town of Cubby Hill comes together for the Great Giving Festival, celebrating the spirit of giving and community that makes their town such a great place to live. And this year, Cooper Cub has a very special task: delivering his grandmother’s special honey to everyone in town! But with such a big job, can Cooper find a way to help his friends and sweeten up the Festival?<br> In this sweet follow-up to <i>The Welcome Wagon</i>, Cori Doerrfeld’s adorable animal citizens of Cubby Hill celebrate sharing with your community and offering a helping hand!<br>",
                [],
                "1683359046",
                "9781683359043",
                2020,
                40,
                [
                    new(
                        "Liam",
                        "Smith",
                        "Parent",
                        0,
                        new(2024, 01, 31),
                        0,
                        "We loved the colorful pictures and silly characters")
                ]
            );
        }
    }

    public class ImagesRequestBodyExample : IExamplesProvider<List<string>>
    {
        public List<string> GetExamples()
        {
            return ["OL3368286W", "OL3368273W", "OL48763W"];
        }
    }


    // ChildController
    
    public class ChildEditBodyExample : IExamplesProvider<ChildEditDTO>
    {
        public ChildEditDTO GetExamples()
        {
            return new("New_Child_Name", 0, "ReadA7", "WLF359", DateOnly.Parse("2017-3-4"));
        }
    }

    public class ChildResponseExample : IExamplesProvider<ChildResponseDTO>
    {
        public ChildResponseDTO GetExamples()
        {
            return new(Snowflake.Generate(), "Jackson", 1, "A5", "CLS098", DateOnly.Parse("2015-04-15"));
        }
    }
    
    public class ChildListResponseExample : IExamplesProvider<List<ChildResponseDTO>>
    {
        public List<ChildResponseDTO> GetExamples()
        {
            return
            [
                new(Snowflake.Generate(),"Ashley", 0,  "A7", "CLS098", DateOnly.Parse("2014-08-23")),
                new(Snowflake.Generate(),"Miles", 1, "B4", "CLS498", null),
                new(Snowflake.Generate(),"Joey", 2, null, null, DateOnly.Parse("2015-04-15"))
            ];
        }
    }

    
    // ReviewsController

    public class ReviewResponseExample : IExamplesProvider<ReviewDTO>
    {
        public ReviewDTO GetExamples()
        {
            return new(
                "Emma",
                "Johnson",
                "Teacher",
                0,
                new(2024, 01, 30),
                5.0,
                "Such a sweet and touching story.");
        }
    }

    public class ReviewsResponseExample : IExamplesProvider<List<ReviewDTO>>
    {
        public List<ReviewDTO> GetExamples()
        {
            return
            [
                new("Olivia", "Brown", "Teacher", 0, new(2024, 02, 15), 0.5, "Heartwarming and enjoyable"),
                new("Isabella", "Thomas", "Teacher", 0, new(2024, 02, 03), 2,
                    "A heartwarming book that we cherished"),
                new("Noah", "Davis", "Parent", 0, new(2024, 01, 11), 2.5,
                    "An exciting adventure we couldn't put down."),
            ];
        }
    }

    public class ReviewAddOrUpdateRequestBodyExample : IExamplesProvider<ReviewAddOrUpdateRequestDTO>
    {
        public ReviewAddOrUpdateRequestDTO GetExamples()
        {
            return new(4.5, "We loved the colorful pictures and silly characters");
        }
    }

    
    // SearchController

    public class BooksResponseExample : IExamplesProvider<List<BookDTO>>
    {
        public List<BookDTO> GetExamples()
        {
            return
            [
                new("OL3368288W", "The Giving Tree", ["Shel Silverstein"], 4.3, 10),
                new("OL48763W", "The three robbers", ["Tomi Ungerer"], 3.7, 15),
                new("OL47935W", "While the Clock Ticked", ["Franklin W. Dixon"], 5, 20),
                new("OL28633459W", "Silver Door", ["Holly Lisle"], 3.3, 15)
            ];
        }
    }

    
    // UserController
    
    public class UserLoginRequestBodyExample : IExamplesProvider<UserLoginDTO>
    {
        public UserLoginDTO GetExamples()
        {
            return new("_username_", "_password_");
        }
    }

    public class UserLoginSuccessResponseExample : IExamplesProvider<UserLoginSuccessDTO>
    {
        public UserLoginSuccessDTO GetExamples()
        {
            return new(
                "_jwt_token_string_");
        }
    }

    public class UserRegisterRequestBodyExample : IExamplesProvider<UserRegisterDTO>
    {
        public UserRegisterDTO GetExamples()
        {
            return new("_username_", "_password_", "Person Doe", "email@example.com", true);
        }
    }

    public class UserDetailsSuccessResponseExample : IExamplesProvider<UserDetailsDTO>
    {
        public UserDetailsDTO GetExamples()
        {
            return new("_username_", "_password_", "John Jackson", "Teacher", 0);
        }
    }

    public class UserDetailsListSuccessResponseExample : IExamplesProvider<List<UserDetailsDTO>>
    {
        public List<UserDetailsDTO> GetExamples()
        {
            return
            [
                new("_username_", "_password_", "John Jackson", "Teacher", 0),
                new("_username_", "_password_", "Ashley Reed", "Parent", 2),
                new("_username_", "_password_", "Wyatt Smith", "Admin", 1),
            ];
        }
    }

    public class BookshelfListResponseExample : IExamplesProvider<List<BookshelfPreviewResponseDTO>>
    {
        public List<BookshelfPreviewResponseDTO> GetExamples()
        {
            return
            [
                new("Some Books", [new("bookId1", "Title", "Author"), new("bookId2", "Title", "Author"), new("bookId3", "Title", "Author")]),
                new("Not many Books", []),
                new("Some More Books", [new("bookId1", "Title", "Author")])
            ];
        }
    }

    public class BookshelfResponseExample : IExamplesProvider<BookshelfPreviewResponseDTO>
    {
        public BookshelfPreviewResponseDTO GetExamples()
        {
            return new("Bookshelf Name",
            [
                new("bookId1", "Title", "Author"), 
                new("bookId2", "Title", "Author"), 
                new("bookId3", "Title", "Author"),
                new("bookId4", "Title", "Author")
            ]);
        }
    }
}