using System.ComponentModel.DataAnnotations;

namespace AquaCMS.Models.ViewModels;

/// <summary>
/// ViewModel cho form đăng nhập.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>URL redirect sau khi đăng nhập thành công</summary>
    public string? ReturnUrl { get; set; }

    /// <summary>Thông báo lỗi hiển thị trên form</summary>
    public string? ErrorMessage { get; set; }
}
