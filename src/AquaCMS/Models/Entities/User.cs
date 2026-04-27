using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaCMS.Models.Entities;

/// <summary>
/// Entity người dùng quản trị.
/// Chỉ admin/manager/editor/sale — không có user khách hàng.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Tên hiển thị</summary>
    [Required(ErrorMessage = "Tên không được để trống")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Email đăng nhập — unique</summary>
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Mật khẩu đã hash bằng Argon2id</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Vai trò: SUPER_ADMIN, MANAGER, EDITOR, SALE</summary>
    public UserRole Role { get; set; } = UserRole.EDITOR;

    /// <summary>URL ảnh đại diện</summary>
    [MaxLength(500)]
    public string? Avatar { get; set; }

    /// <summary>Tài khoản có đang hoạt động không</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Thời điểm đăng nhập cuối cùng</summary>
    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enum vai trò quản trị viên.
/// Lưu ý: tên phải khớp với PostgreSQL ENUM 'user_role'.
/// </summary>
public enum UserRole
{
    SUPER_ADMIN,
    MANAGER,
    EDITOR,
    SALE
}
