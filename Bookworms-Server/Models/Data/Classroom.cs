namespace BookwormsServer.Models.Data;

public record ClassroomTeacherResponse(
    string ClassCode,
    string ClassroomName,
    List<ChildResponse> Children,
    List<BookshelfResponse> Bookshelves);
    

public record ClassroomChildResponse(
    string ClassCode,
    string ClassroomName,
    string TeacherName,
    List<BookshelfResponse> Bookshelves);