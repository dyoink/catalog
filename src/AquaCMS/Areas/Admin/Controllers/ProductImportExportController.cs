using AquaCMS.Data;
using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>
/// Admin import/export sản phẩm dạng Excel (.xlsx).
/// </summary>
[Area("Admin")]
[Authorize(Policy = "ManagerUp")]
[Route("admin/products-io")]
public class ProductImportExportController : Controller
{
    private const string MimeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static readonly string[] Headers = new[]
    {
        "SKU", "Tên sản phẩm", "Slug", "Danh mục (slug)",
        "Giá (VNĐ)", "Mô tả ngắn", "Trạng thái", "Nổi bật",
        "Meta Title (SEO)", "Meta Description (SEO)", "Video URL"
    };

    private readonly AppDbContext _db;
    private readonly IActivityLogService _activity;
    private readonly ILogger<ProductImportExportController> _logger;

    public ProductImportExportController(
        AppDbContext db,
        IActivityLogService activity,
        ILogger<ProductImportExportController> logger)
    {
        _db = db;
        _activity = activity;
        _logger = logger;
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        try
        {
            var rows = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Metadata)
                .Include(p => p.Finance)
                .Include(p => p.Content)
                .OrderBy(p => p.Category != null ? p.Category.Name : "")
                .ThenBy(p => p.Name)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Sản phẩm");

            ws.Cell(1, 1).Value = "DANH SÁCH SẢN PHẨM — AquaCMS";
            ws.Range(1, 1, 1, Headers.Length).Merge();
            ws.Cell(1, 1).Style
                .Font.SetBold().Font.SetFontSize(14)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#55B3D9"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Row(1).Height = 28;

            ws.Cell(2, 1).Value = $"Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm} — Tổng: {rows.Count} sản phẩm";
            ws.Range(2, 1, 2, Headers.Length).Merge();
            ws.Cell(2, 1).Style.Font.SetItalic().Font.SetFontColor(XLColor.Gray);

            const int headerRow = 4;
            for (int i = 0; i < Headers.Length; i++)
            {
                var c = ws.Cell(headerRow, i + 1);
                c.Value = Headers[i];
                c.Style.Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#1F2937"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
            ws.Row(headerRow).Height = 24;

            int r = headerRow + 1;
            foreach (var p in rows)
            {
                ws.Cell(r, 1).Value = p.Sku ?? "";
                ws.Cell(r, 2).Value = p.Name;
                ws.Cell(r, 3).Value = p.Metadata?.Slug ?? "";
                ws.Cell(r, 4).Value = p.Category?.Slug ?? "";
                ws.Cell(r, 5).Value = p.Finance?.Price ?? 0;
                ws.Cell(r, 5).Style.NumberFormat.Format = "#,##0";
                ws.Cell(r, 6).Value = p.Content?.Description ?? "";
                ws.Cell(r, 7).Value = p.Status.ToString();
                ws.Cell(r, 8).Value = (p.Finance?.IsFeatured ?? false) ? "Có" : "Không";
                ws.Cell(r, 9).Value = p.Metadata?.MetaTitle ?? "";
                ws.Cell(r, 10).Value = p.Metadata?.MetaDesc ?? "";
                ws.Cell(r, 11).Value = p.Content?.VideoUrl ?? "";

                if ((r - headerRow) % 2 == 0)
                {
                    ws.Range(r, 1, r, Headers.Length).Style
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#F9FAFB"));
                }

                var statusCell = ws.Cell(r, 7);
                statusCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                statusCell.Style.Font.SetBold();
                statusCell.Style.Font.SetFontColor(p.Status switch
                {
                    ProductStatus.Available => XLColor.FromHtml("#059669"),
                    ProductStatus.Hidden => XLColor.FromHtml("#9CA3AF"),
                    _ => XLColor.FromHtml("#DC2626")
                });
                
                ws.Cell(r, 8).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                if (p.Finance?.IsFeatured == true)
                    ws.Cell(r, 8).Style.Font.SetFontColor(XLColor.FromHtml("#D97706"));

                r++;
            }

            if (rows.Count > 0)
            {
                var dataRange = ws.Range(headerRow, 1, r - 1, Headers.Length);
                dataRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                dataRange.Style.Alignment.SetWrapText(true);
                dataRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                ws.Range(headerRow, 1, r - 1, Headers.Length).SetAutoFilter();
            }

            ws.Column(1).Width = 12;
            ws.Column(2).Width = 35;
            ws.Column(3).Width = 28;
            ws.Column(4).Width = 18;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 45;
            ws.Column(7).Width = 14;
            ws.Column(8).Width = 10;
            ws.Column(9).Width = 28;
            ws.Column(10).Width = 40;
            ws.Column(11).Width = 30;

            ws.SheetView.FreezeRows(headerRow);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            await _activity.LogAsync("EXPORT", "Product", null,
                $"Xuất Excel {rows.Count} sản phẩm", "Success");

            return File(ms.ToArray(), MimeXlsx,
                $"san-pham-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export products to Excel failed");
            TempData["Error"] = "Lỗi xuất Excel: " + ex.Message;
            return RedirectToAction("Index", "Products");
        }
    }

    [HttpGet("template")]
    public IActionResult Template()
    {
        try
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Sản phẩm");

            ws.Cell(1, 1).Value = "TEMPLATE NHẬP SẢN PHẨM — AquaCMS";
            ws.Range(1, 1, 1, Headers.Length).Merge();
            ws.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#55B3D9"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Row(1).Height = 28;

            ws.Cell(2, 1).Value = "Điền dữ liệu từ hàng 5 trở xuống. Đừng sửa hàng tiêu đề (4).";
            ws.Range(2, 1, 2, Headers.Length).Merge();
            ws.Cell(2, 1).Style.Font.SetItalic().Font.SetFontColor(XLColor.Gray);

            const int headerRow = 4;
            for (int i = 0; i < Headers.Length; i++)
            {
                var c = ws.Cell(headerRow, i + 1);
                c.Value = Headers[i];
                c.Style.Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#1F2937"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
            ws.Row(headerRow).Height = 24;

            // Demo row
            ws.Cell(5, 1).Value = "SP001";
            ws.Cell(5, 2).Value = "Sản phẩm mẫu";
            ws.Cell(5, 3).Value = "san-pham-mau";
            ws.Cell(5, 4).Value = "may-moc";
            ws.Cell(5, 5).Value = 1500000;
            ws.Cell(5, 5).Style.NumberFormat.Format = "#,##0";
            ws.Cell(5, 6).Value = "Mô tả ngắn của sản phẩm mẫu";
            ws.Cell(5, 7).Value = "Available";
            ws.Cell(5, 8).Value = "Có";
            ws.Cell(5, 9).Value = "Sản phẩm mẫu | AquaCMS";
            ws.Cell(5, 10).Value = "Mô tả SEO";
            ws.Cell(5, 11).Value = "";

            ws.Range(headerRow, 1, 5, Headers.Length).Style
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            ws.Column(1).Width = 12; ws.Column(2).Width = 35; ws.Column(3).Width = 28;
            ws.Column(4).Width = 18; ws.Column(5).Width = 14; ws.Column(6).Width = 45;
            ws.Column(7).Width = 14; ws.Column(8).Width = 10; ws.Column(9).Width = 28;
            ws.Column(10).Width = 40; ws.Column(11).Width = 30;
            ws.SheetView.FreezeRows(headerRow);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), MimeXlsx, "san-pham-template.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Template export failed");
            TempData["Error"] = "Lỗi tạo template: " + ex.Message;
            return RedirectToAction("Index", "Products");
        }
    }

    [HttpPost("import"), ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Chưa chọn file Excel (.xlsx).";
            return RedirectToAction("Index", "Products");
        }

        var created = 0; var updated = 0; var skipped = 0;
        var errors = new List<string>();

        try
        {
            var categories = await _db.Categories.ToDictionaryAsync(c => c.Slug, c => c.Id);

            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("File Excel không có sheet nào.");

            int headerRow = 4;
            for (int rr = 1; rr <= 10; rr++)
            {
                if (string.Equals(ws.Cell(rr, 1).GetString().Trim(), "SKU", StringComparison.OrdinalIgnoreCase))
                {
                    headerRow = rr;
                    break;
                }
            }

            int dataStart = headerRow + 1;
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? dataStart;

            for (int rr = dataStart; rr <= lastRow; rr++)
            {
                try
                {
                    var name = ws.Cell(rr, 2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var sku = NullIfEmpty(ws.Cell(rr, 1).GetString());
                    var rawSlug = ws.Cell(rr, 3).GetString().Trim();
                    var slug = !string.IsNullOrWhiteSpace(rawSlug)
                        ? SlugHelper.GenerateSlug(rawSlug)
                        : SlugHelper.GenerateSlug(name);

                    var catSlug = ws.Cell(rr, 4).GetString().Trim();
                    Guid? catId = null;
                    if (!string.IsNullOrWhiteSpace(catSlug) && categories.TryGetValue(catSlug, out var cid))
                        catId = cid;

                    decimal? price = null;
                    if (ws.Cell(rr, 5).TryGetValue<decimal>(out var pVal) && pVal > 0)
                        price = pVal;

                    var desc = NullIfEmpty(ws.Cell(rr, 6).GetString());
                    var statusStr = ws.Cell(rr, 7).GetString().Trim();
                    ProductStatus status = ProductStatus.Available;
                    if (!string.IsNullOrWhiteSpace(statusStr) && Enum.TryParse<ProductStatus>(statusStr, true, out var st))
                        status = st;

                    var featuredStr = ws.Cell(rr, 8).GetString().Trim().ToLowerInvariant();
                    bool isFeatured = featuredStr is "có" or "co" or "true" or "1" or "yes" or "x";

                    var metaTitle = NullIfEmpty(ws.Cell(rr, 9).GetString());
                    var metaDesc = NullIfEmpty(ws.Cell(rr, 10).GetString());
                    var videoUrl = NullIfEmpty(ws.Cell(rr, 11).GetString());

                    var existing = await _db.Products
                        .Include(p => p.Metadata)
                        .Include(p => p.Finance)
                        .Include(p => p.Content)
                        .FirstOrDefaultAsync(p => p.Metadata != null && p.Metadata.Slug == slug);

                    if (existing != null)
                    {
                        existing.Name = name;
                        existing.Sku = sku;
                        existing.CategoryId = catId;
                        existing.Status = status;
                        existing.UpdatedAt = DateTime.UtcNow;

                        existing.Metadata ??= new ProductMetadata { ProductId = existing.Id };
                        existing.Metadata.Slug = slug;
                        existing.Metadata.MetaTitle = metaTitle;
                        existing.Metadata.MetaDesc = metaDesc;

                        existing.Finance ??= new ProductFinance { ProductId = existing.Id };
                        existing.Finance.Price = price;
                        existing.Finance.IsFeatured = isFeatured;

                        existing.Content ??= new ProductContent { ProductId = existing.Id };
                        existing.Content.Description = desc;
                        existing.Content.VideoUrl = videoUrl;

                        updated++;
                    }
                    else
                    {
                        var productId = Guid.NewGuid();
                        var product = new Product
                        {
                            Id = productId,
                            Name = name,
                            Sku = sku,
                            CategoryId = catId,
                            Status = status,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Metadata = new ProductMetadata { ProductId = productId, Slug = slug, MetaTitle = metaTitle, MetaDesc = metaDesc },
                            Finance = new ProductFinance { ProductId = productId, Price = price, IsFeatured = isFeatured },
                            Content = new ProductContent { ProductId = productId, Description = desc, VideoUrl = videoUrl },
                            Statistic = new ProductStatistic { ProductId = productId, ViewCount = 0 }
                        };
                        _db.Products.Add(product);
                        created++;
                    }
                }
                catch (Exception ex)
                {
                    skipped++;
                    errors.Add($"Dòng {rr}: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Import xong: Tạo {created}, cập nhật {updated}, bỏ qua {skipped}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import products failed");
            TempData["Error"] = "Lỗi đọc Excel: " + ex.Message;
        }

        return RedirectToAction("Index", "Products");
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
