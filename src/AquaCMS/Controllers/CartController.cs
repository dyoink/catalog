using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Controllers;

/// <summary>
/// Giỏ hàng — server chỉ render trang, logic ở client (localStorage).
/// URL: /gio-hang
/// </summary>
public class CartController : Controller
{
    private readonly ISettingsService _settingsService;

    public CartController(ISettingsService settingsService)
        => _settingsService = settingsService;

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetSettingsAsync();
        ViewData["Title"] = $"Giỏ hàng | {settings.CompanyName}";
        ViewData["MetaDescription"] = "Giỏ hàng sản phẩm của bạn";
        return View();
    }
}
