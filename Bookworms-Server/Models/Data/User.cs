using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Data;

/// <summary>
/// 
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
/// <param name="IsParent"></param>
public record UserRegisterRequest(
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Username,

    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Password,

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    string FirstName,

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    string LastName,

    bool IsParent);

/// <summary>
/// 
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
public record UserLoginRequest(
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Username,
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Password
    );

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
public record UserLoginSuccessResponse(string Token);

/// <summary>
/// 
/// </summary>
/// <param name="Username"></param>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
/// <param name="Role"></param>
/// <param name="Icon"></param>
public record UserDetailsResponse(string Username, string FirstName, string LastName, string Role, int Icon);

/// <summary>
/// 
/// </summary>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
/// <param name="Icon"></param>
/// <param name="Password"></param>
public record UserDetailsEditRequest(
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)] 
    string? FirstName = null, 
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)] 
    string? LastName = null, 
    
    [Range(0, int.MaxValue, ErrorMessage = "{0} must be a positive integer.")]
    int? Icon = null, 
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string? Password = null);