using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookwormsServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BookId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Authors = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subjects = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Isbn10 = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Isbn13 = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverId = table.Column<int>(type: "int", nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    PublishYear = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: true),
                    LevelIsLocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StarRating = table.Column<double>(type: "double", nullable: true),
                    TimeAdded = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    SimilarBooks = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BookId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserIcon = table.Column<int>(type: "int", nullable: false),
                    Hash = table.Column<byte[]>(type: "longblob", nullable: false),
                    Salt = table.Column<byte[]>(type: "longblob", nullable: false),
                    Discriminator = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Children",
                columns: table => new
                {
                    ChildId = table.Column<string>(type: "char(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChildIcon = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    ReadingLevel = table.Column<int>(type: "int", nullable: true),
                    ParentUsername = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Children", x => x.ChildId);
                    table.ForeignKey(
                        name: "FK_Children_Users_ParentUsername",
                        column: x => x.ParentUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    ClassroomCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TeacherUsername = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassroomName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassIcon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.ClassroomCode);
                    table.ForeignKey(
                        name: "FK_Classrooms_Users_TeacherUsername",
                        column: x => x.TeacherUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Username = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StarRating = table.Column<double>(type: "double", nullable: false),
                    ReviewText = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_Reviews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_Username",
                        column: x => x.Username,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChildBookshelves",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChildId = table.Column<string>(type: "char(22)", maxLength: 22, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildBookshelves", x => x.BookshelfId);
                    table.ForeignKey(
                        name: "FK_ChildBookshelves_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChildGoals",
                columns: table => new
                {
                    ChildGoalId = table.Column<string>(type: "char(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChildId = table.Column<string>(type: "char(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Discriminator = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Progress = table.Column<float>(type: "float", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    TargetNumBooks = table.Column<int>(type: "int", nullable: true),
                    NumBooks = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildGoals", x => x.ChildGoalId);
                    table.ForeignKey(
                        name: "FK_ChildGoals_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompletedBookshelves",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChildId = table.Column<string>(type: "char(22)", maxLength: 22, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedBookshelves", x => x.BookshelfId);
                    table.ForeignKey(
                        name: "FK_CompletedBookshelves_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DifficultyRatings",
                columns: table => new
                {
                    BookId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChildId = table.Column<string>(type: "char(22)", maxLength: 22, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReadingLevelAtRatingTime = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DifficultyRatings", x => new { x.BookId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_DifficultyRatings_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DifficultyRatings_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InProgressBookshelves",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChildId = table.Column<string>(type: "char(22)", maxLength: 22, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProgressBookshelves", x => x.BookshelfId);
                    table.ForeignKey(
                        name: "FK_InProgressBookshelves_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassGoals",
                columns: table => new
                {
                    ClassGoalId = table.Column<string>(type: "char(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Discriminator = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetNumBooks = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGoals", x => x.ClassGoalId);
                    table.ForeignKey(
                        name: "FK_ClassGoals_Classrooms_ClassCode",
                        column: x => x.ClassCode,
                        principalTable: "Classrooms",
                        principalColumn: "ClassroomCode",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassroomAnnouncements",
                columns: table => new
                {
                    AnnouncementId = table.Column<string>(type: "char(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Time = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomAnnouncements", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_ClassroomAnnouncements_Classrooms_ClassCode",
                        column: x => x.ClassCode,
                        principalTable: "Classrooms",
                        principalColumn: "ClassroomCode",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassroomBookshelves",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassroomCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomBookshelves", x => x.BookshelfId);
                    table.ForeignKey(
                        name: "FK_ClassroomBookshelves_Classrooms_ClassroomCode",
                        column: x => x.ClassroomCode,
                        principalTable: "Classrooms",
                        principalColumn: "ClassroomCode",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassroomChildren",
                columns: table => new
                {
                    ClassroomCode = table.Column<string>(type: "varchar(6)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChildId = table.Column<string>(type: "char(14)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomChildren", x => new { x.ClassroomCode, x.ChildId });
                    table.ForeignKey(
                        name: "FK_ClassroomChildren_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomChildren_Classrooms_ClassroomCode",
                        column: x => x.ClassroomCode,
                        principalTable: "Classrooms",
                        principalColumn: "ClassroomCode",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChildBookshelfBooks",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildBookshelfBooks", x => new { x.BookshelfId, x.BookId });
                    table.ForeignKey(
                        name: "FK_ChildBookshelfBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildBookshelfBooks_ChildBookshelves_BookshelfId",
                        column: x => x.BookshelfId,
                        principalTable: "ChildBookshelves",
                        principalColumn: "BookshelfId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompletedBookshelfBooks",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StarRating = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedBookshelfBooks", x => new { x.BookshelfId, x.BookId });
                    table.ForeignKey(
                        name: "FK_CompletedBookshelfBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedBookshelfBooks_CompletedBookshelves_BookshelfId",
                        column: x => x.BookshelfId,
                        principalTable: "CompletedBookshelves",
                        principalColumn: "BookshelfId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InProgressBookshelfBooks",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProgressBookshelfBooks", x => new { x.BookshelfId, x.BookId });
                    table.ForeignKey(
                        name: "FK_InProgressBookshelfBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InProgressBookshelfBooks_InProgressBookshelves_BookshelfId",
                        column: x => x.BookshelfId,
                        principalTable: "InProgressBookshelves",
                        principalColumn: "BookshelfId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassroomBookshelfBooks",
                columns: table => new
                {
                    BookshelfId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomBookshelfBooks", x => new { x.BookshelfId, x.BookId });
                    table.ForeignKey(
                        name: "FK_ClassroomBookshelfBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomBookshelfBooks_ClassroomBookshelves_BookshelfId",
                        column: x => x.BookshelfId,
                        principalTable: "ClassroomBookshelves",
                        principalColumn: "BookshelfId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassGoalLogs",
                columns: table => new
                {
                    ClassGoalId = table.Column<string>(type: "char(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChildId = table.Column<string>(type: "char(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Discriminator = table.Column<string>(type: "varchar(34)", maxLength: 34, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Progress = table.Column<float>(type: "float", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    NumBooks = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGoalLogs", x => new { x.ClassGoalId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_ClassGoalLogs_ClassGoals_ClassGoalId",
                        column: x => x.ClassGoalId,
                        principalTable: "ClassGoals",
                        principalColumn: "ClassGoalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassGoalLogs_ClassroomChildren_ClassCode_ChildId",
                        columns: x => new { x.ClassCode, x.ChildId },
                        principalTable: "ClassroomChildren",
                        principalColumns: new[] { "ClassroomCode", "ChildId" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChildBookshelfBooks_BookId",
                table: "ChildBookshelfBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildBookshelves_ChildId",
                table: "ChildBookshelves",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildGoals_ChildId",
                table: "ChildGoals",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Children_ParentUsername",
                table: "Children",
                column: "ParentUsername");

            migrationBuilder.CreateIndex(
                name: "IX_ClassGoalLogs_ClassCode_ChildId",
                table: "ClassGoalLogs",
                columns: new[] { "ClassCode", "ChildId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassGoals_ClassCode",
                table: "ClassGoals",
                column: "ClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomAnnouncements_ClassCode",
                table: "ClassroomAnnouncements",
                column: "ClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomBookshelfBooks_BookId",
                table: "ClassroomBookshelfBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomBookshelves_ClassroomCode",
                table: "ClassroomBookshelves",
                column: "ClassroomCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomChildren_ChildId",
                table: "ClassroomChildren",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_TeacherUsername",
                table: "Classrooms",
                column: "TeacherUsername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompletedBookshelfBooks_BookId",
                table: "CompletedBookshelfBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedBookshelves_ChildId",
                table: "CompletedBookshelves",
                column: "ChildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DifficultyRatings_ChildId",
                table: "DifficultyRatings",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_InProgressBookshelfBooks_BookId",
                table: "InProgressBookshelfBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_InProgressBookshelves_ChildId",
                table: "InProgressBookshelves",
                column: "ChildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookId_Username",
                table: "Reviews",
                columns: new[] { "BookId", "Username" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Username",
                table: "Reviews",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildBookshelfBooks");

            migrationBuilder.DropTable(
                name: "ChildGoals");

            migrationBuilder.DropTable(
                name: "ClassGoalLogs");

            migrationBuilder.DropTable(
                name: "ClassroomAnnouncements");

            migrationBuilder.DropTable(
                name: "ClassroomBookshelfBooks");

            migrationBuilder.DropTable(
                name: "CompletedBookshelfBooks");

            migrationBuilder.DropTable(
                name: "DifficultyRatings");

            migrationBuilder.DropTable(
                name: "InProgressBookshelfBooks");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "ChildBookshelves");

            migrationBuilder.DropTable(
                name: "ClassGoals");

            migrationBuilder.DropTable(
                name: "ClassroomChildren");

            migrationBuilder.DropTable(
                name: "ClassroomBookshelves");

            migrationBuilder.DropTable(
                name: "CompletedBookshelves");

            migrationBuilder.DropTable(
                name: "InProgressBookshelves");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Children");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
