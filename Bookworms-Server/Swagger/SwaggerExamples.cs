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
    
    public class BookResponseExample : IExamplesProvider<BookResponse>
    {
        public BookResponse GetExamples()
        {
            return new(
                "OL35966594M",
                "Giving Day",
                ["Cori Doerrfeld"],
                3.5,
                35
            );
        }
    }
    
    public class BookResponseExtendedExample : IExamplesProvider<BookResponseExtended>
    {
        public BookResponseExtended GetExamples()
        {
            return new(
                "OL35966594M",
                "Giving Day",
                ["Cori Doerrfeld"],
                3.5,
                25,
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
    
    public class ChildEditBodyExample : IExamplesProvider<ChildEditRequest>
    {
        public ChildEditRequest GetExamples()
        {
            return new("New_Child_Name", 0, 42, DateOnly.Parse("2017-3-4"));
        }
    }

    public class ChildResponseExample : IExamplesProvider<ChildResponse>
    {
        public ChildResponse GetExamples()
        {
            return new(Snowflake.Generate(), "Jackson", 1, 52, DateOnly.Parse("2015-04-15"));
        }
    }
    
    public class ChildListResponseExample : IExamplesProvider<List<ChildResponse>>
    {
        public List<ChildResponse> GetExamples()
        {
            return
            [
                new(Snowflake.Generate(),"Ashley", 0,  59, DateOnly.Parse("2014-08-23")),
                new(Snowflake.Generate(),"Miles", 1, null, null),
                new(Snowflake.Generate(),"Joey", 2, null, DateOnly.Parse("2015-04-15"))
            ];
        }
    }

    
    // ReviewsController

    public class ReviewResponseExample : IExamplesProvider<ReviewResponse>
    {
        public ReviewResponse GetExamples()
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

    public class ReviewListResponseExample : IExamplesProvider<List<ReviewResponse>>
    {
        public List<ReviewResponse> GetExamples()
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

    public class ReviewEditRequestExample : IExamplesProvider<ReviewEditRequest>
    {
        public ReviewEditRequest GetExamples()
        {
            return new(4.5, "We loved the colorful pictures and silly characters");
        }
    }

    
    // SearchController

    public class BooksResponseExample : IExamplesProvider<List<BookResponse>>
    {
        public List<BookResponse> GetExamples()
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
    
    public class UserLoginRequestExample : IExamplesProvider<UserLoginRequest>
    {
        public UserLoginRequest GetExamples()
        {
            return new("_username_", "_password_");
        }
    }

    public class UserLoginSuccessResponseExample : IExamplesProvider<UserLoginSuccessResponse>
    {
        public UserLoginSuccessResponse GetExamples()
        {
            return new(
                "_jwt_token_string_");
        }
    }

    public class UserRegisterRequestExample : IExamplesProvider<UserRegisterRequest>
    {
        public UserRegisterRequest GetExamples()
        {
            return new("_username_", "_password_", "Person Doe", "email@example.com", true);
        }
    }

    public class UserDetailsResponseResponseExample : IExamplesProvider<UserDetailsResponse>
    {
        public UserDetailsResponse GetExamples()
        {
            return new("_username_", "_password_", "John Jackson", "Teacher", 0);
        }
    }

    public class UserDetailsListResponseExample : IExamplesProvider<List<UserDetailsResponse>>
    {
        public List<UserDetailsResponse> GetExamples()
        {
            return
            [
                new("_username_", "_password_", "John Jackson", "Teacher", 0),
                new("_username_", "_password_", "Ashley Reed", "Parent", 2),
                new("_username_", "_password_", "Wyatt Smith", "Admin", 1),
            ];
        }
    }

    public class BookshelfListResponseExample : IExamplesProvider<List<BookshelfResponse>>
    {
        public List<BookshelfResponse> GetExamples()
        {
            return
            [
                new(BookshelfType.Completed, "Completed", [
                        new("bookId1", "Title1", ["Author1"], 3.5, 30),
                        new("bookId2", "Title2", ["Author2"], 4.0, 42),
                        new("bookId3", "Title3", ["Author3", "Author10"], 2.5, 55)
                    ]),
                new(BookshelfType.InProgress, "In Progress", []),
                new(BookshelfType.Custom, "Some other books", [
                    new("bookId1", "Title1", ["Author1"], 3.5, 30)
                ]),
                new(BookshelfType.Classroom, "Mrs. Smith's Class Reading List", [
                    new("bookId4", "Title4", ["Author4"], 5.0, 21)
                ])
            ];
        }
    }

    public class BookshelfResponseExample : IExamplesProvider<BookshelfResponse>
    {
        public BookshelfResponse GetExamples()
        {
            return new(BookshelfType.Custom, "Bookshelf Name",
            [
                new("bookId1", "Title", ["Author1"], 3.5, 30), 
                new("bookId2", "Title", ["Author2"], 4.0, 42), 
                new("bookId3", "Title", ["Author3", "Author10"], 2.5, 55),
                new("bookId4", "Title", ["Author4"], 5.0, 21)
            ]);
        }
    }
    
    // Classrooms
    
    public class ClassroomChildResponseExample : IExamplesProvider<ClassroomChildResponse>
    {
        public ClassroomChildResponse GetExamples()
        {
            return new("ABC123", "Ava's Class", "Mrs. Ava", 1, 
                [
                    new("AnnouncementId1", "New Announcement", "Announcement Body Text", DateTime.Today),
                    new("AnnouncementId2", "Old Announcement", "Old Announcement Body Text", DateTime.Today)
                ],
                [
                new(BookshelfType.Classroom, "Reading List", 
                [
                    new("bookId1", "Title", ["Author1"], 3.5, 30)
                ]),
                new(BookshelfType.Classroom, "Advanced Reading",
                [
                    new("bookId2", "Title", ["Author2"], 4.0, 42), 
                    new("bookId3", "Title", ["Author3", "Author10"], 2.5, 55)
                ])
            ]);
        }
    }
    
    public class ClassroomChildResponseListExample : IExamplesProvider<List<ClassroomChildResponse>>
    {
        public List<ClassroomChildResponse> GetExamples()
        {
            return
            [
                new("ABC123", "Ava's Class", "Mrs. Ava", 2, 
                [
                    new("AnnouncementId1", "New Announcement", "Announcement Body Text", DateTime.Today)
                ],
                [
                    new(BookshelfType.Classroom, "Reading List",
                    [
                        new("bookId1", "Title", ["Author1"], 3.5, 30)
                    ]),
                    new(BookshelfType.Classroom, "Advanced Reading",
                    [
                        new("bookId2", "Title", ["Author2"], 4.0, 42),
                        new("bookId3", "Title", ["Author3", "Author10"], 2.5, 55)
                    ])
                ]),
                new("ALP234", "Mustard's Class", "Mr. Mustard", 1, 
                    [
                        new("AnnouncementId1", "New Announcement", "Announcement Body Text", DateTime.Today)
                    ],
                    [
                    new(BookshelfType.Classroom, "Rad Reading List",
                    [
                        new("bookId1", "Title", ["Author1"], 3.5, 30)
                    ])
                ]),
            ];
        }
    }

    public class ClassroomResponseExample : IExamplesProvider<ClassroomTeacherResponse>
    {
        public ClassroomTeacherResponse GetExamples()
        {
            return new("ABC123", "Ava's Class", 3,
                [
                    new("childId1", "Jack", 0, 3, new(2017, 02, 15)),
                    new("childId2", "Wyatt", 1, 45, new(2019, 08, 17)),
                    new("childId3", "Sadie", 2, 72, new(2018, 05, 28))
                ], 
                [
                    new("AnnouncementId1", "New Announcement", "Announcement Body Text", DateTime.Today)
                ],
                [
                new(BookshelfType.Classroom, "Reading List", 
                [
                    new("bookId1", "Title", ["Author1"], 3.5, 30)
                ]),
                new(BookshelfType.Classroom, "Advanced Reading",
                [
                    new("bookId2", "Title", ["Author2"], 4.0, 42), 
                    new("bookId3", "Title", ["Author3", "Author10"], 2.5, 55)
                ])
            ]);
        }
    }

    public class ClassroomAnnouncementResponseExample : IExamplesProvider<ClassroomAnnouncementResponse>
    {
        public ClassroomAnnouncementResponse GetExamples()
        {
            return new(
                "AnnouncementId1", 
                "New Announcement", 
                "Announcement Body Text", 
                DateTime.Today);
        }
    }

    public class ClassroomAnnouncementResponseListExample : IExamplesProvider<List<ClassroomAnnouncementResponse>>
    {
        public List<ClassroomAnnouncementResponse> GetExamples()
        {
            return [
                new(
                    "AnnouncementId1", 
                    "New Announcement", 
                    "Announcement Body Text", 
                    DateTime.Today),
                new(
                    "AnnouncementId2", 
                    "Another Announcement", 
                    "Some more Body Text", 
                    DateTime.Today)
            ];
            
        }
    }

    public class ClassroomAnnouncementAddRequestExample : IExamplesProvider<ClassroomAnnouncementAddRequest>
    {
        public ClassroomAnnouncementAddRequest GetExamples()
        {
            return new(
                "__announcement_title__", 
                "__announcement_body__");
        }
    }

    public class ClassroomAnnouncementEditRequestExample : IExamplesProvider<ClassroomAnnouncementEditRequest>
    {
        public ClassroomAnnouncementEditRequest GetExamples()
        {
            return new(
                "__announcement_title_or_NULL_", 
                "__announcement_body_or_NULL_");
        }
    }

    public class ClassGoalAddRequestExample : IExamplesProvider<ClassGoalAddRequest>
    {
        public ClassGoalAddRequest GetExamples()
        {
            return new(
                "Goal_Title",
                DateOnly.Parse("2025-02-19"),
                null);
        }
    }

    public class ClassGoalEditRequestExample : IExamplesProvider<ClassGoalEditRequest>
    {
        public ClassGoalEditRequest GetExamples()
        {
            return new(
                "New_Goal_Title",
                DateOnly.Parse("2025-02-20"),
                5);
        }
    }

    public class ClassGoalLogEditRequestExample : IExamplesProvider<ClassGoalLogEditRequest>
    {
        public ClassGoalLogEditRequest GetExamples()
        {
            return new(
                0.5f,
                20,
                3);
        }
    }
}