using BookwormsServer.Models.Data;
using Swashbuckle.AspNetCore.Filters;

namespace BookwormsServer.Controllers;


// Swagger finds these automatically; there's technically no need to explicitly refer to them with
//   `SwaggerResponseExample` or `SwaggerRequestBodyExample`


// Content responses

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


public class BookDetailsResponseExample : IExamplesProvider<BookDetailsDTO>
{
    public BookDetailsDTO GetExamples()
    {
        return new(
            "<b>A sweet, fun-filled follow-up to <i>The Welcome Wagon</i> from acclaimed author Cori Doerrfeld!</b><br> <br> Every year, the town of Cubby Hill comes together for the Great Giving Festival, celebrating the spirit of giving and community that makes their town such a great place to live. And this year, Cooper Cub has a very special task: delivering his grandmother’s special honey to everyone in town! But with such a big job, can Cooper find a way to help his friends and sweeten up the Festival?<br> In this sweet follow-up to <i>The Welcome Wagon</i>, Cori Doerrfeld’s adorable animal citizens of Cubby Hill celebrate sharing with your community and offering a helping hand!<br>",
            [],
            "1683359046",
            "9781683359043",
            "Abrams",
            "2020-10-27",
            40,
            [
                new ReviewDTO(
                    "Liam",
                    "Smith",
                    "Parent",
                    UserIcon.Icon1,
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
        return ["XlrMDwAAQBAJ", "sQSDzQEACAAJ", "_BxZ71b-u5QC"];
    }
}


public class ReviewResponseExample : IExamplesProvider<ReviewDTO>
{
    public ReviewDTO GetExamples()
    {
        return new(
            "Emma", 
            "Johnson",
            "Teacher",
            UserIcon.Icon1,
            new(2024, 01, 30),
            5.0,
            "Such a sweet and touching story.");
    }
}

public class ReviewsResponseExample : IExamplesProvider<List<ReviewDTO>>
{
    public List<ReviewDTO> GetExamples()
    {
        return [
            new("Olivia", "Brown", "Teacher", UserIcon.Icon1, new(2024, 02, 15), 0.5, "Heartwarming and enjoyable"),
            new("Isabella", "Thomas", "Teacher", UserIcon.Icon1, new(2024, 02, 03), 2, "A heartwarming book that we cherished"),
            new("Noah", "Davis", "Parent", UserIcon.Icon1, new(2024, 01, 11), 2.5, "An exciting adventure we couldn't put down."),
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


public class BooksResponseExample : IExamplesProvider<List<BookDto>>
{
    public List<BookDto> GetExamples()
    {
        return [
            new("1IleAgAAQBAJ", "The Giving Tree", ["Shel Silverstein"], 4.3, "A1"),
            new("bSSW15rcQbsC", "The Giving Book", ["Ellen Sabin"], 3.7, "C2"),
            new("IUnGtgEACAAJ", "The Giving Sack", ["Leanne Hill"], 5, "F9"),
            new("jXvSDwAAQBAJ", "The Giving Day", ["Cori Doerrfeld"], 3.3, "Z2")
        ];
    }
}