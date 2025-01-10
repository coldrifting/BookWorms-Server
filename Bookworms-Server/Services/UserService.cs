using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Services;

public static class UserService
{
    public static User CreateUser(string username, string password, string firstName, string lastName, UserIcon userIcon)
    {
        byte[] hash = AuthService.HashPassword(password, out byte[] salt);
        return new(username, hash, salt, firstName, lastName, userIcon);
    }
}