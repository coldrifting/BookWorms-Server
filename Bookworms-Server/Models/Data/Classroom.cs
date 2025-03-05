namespace BookwormsServer.Models.Data;

public record ClassroomTeacherResponse(
    string ClassCode,
    string ClassroomName,
    int ClassIcon,
    List<ChildResponse> Children,
    List<BookshelfResponse> Bookshelves);
    

public record ClassroomChildResponse(
    string ClassCode,
    string ClassroomName,
    string TeacherName,
    int ClassIcon,
    List<BookshelfResponse> Bookshelves);