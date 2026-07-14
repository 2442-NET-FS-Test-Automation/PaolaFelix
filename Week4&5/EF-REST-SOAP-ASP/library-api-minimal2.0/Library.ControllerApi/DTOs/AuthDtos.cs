using System.ComponentModel.DataAnnotations;
namespace Library.ControllerApi.DTOs;

public record RegisterDto(
    [Required, MaxLength(64)] string UserName,
    [Required, MinLength(8)] string Password
    // could ask for phone number, email, etc as well
);

public record LoginDto(
    [Required] string UserName,
    [Required] string Password
);