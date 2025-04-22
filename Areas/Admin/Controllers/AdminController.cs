using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Data;
using TechStore.Models;
using Microsoft.EntityFrameworkCore;


namespace TechStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Hiển thị thông báo chào mừng khi đăng nhập thành công
            if (TempData["LoginSuccess"] != null)
            {
                ViewBag.WelcomeMessage = $"Welcome back, {User.Identity?.Name}!";
            }

            // Thống kê cho dashboard
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.ActivePromotions = await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                .CountAsync();
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.LowStockProducts = await _context.Products
                .Where(p => p.StockQuantity < 10)
                .CountAsync();

            // Lấy danh sách đơn hàng gần đây
            ViewBag.RecentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    Id = o.OrderID,
                    CustomerName = o.User.FullName,
                    Total = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            // Lấy top sản phẩm bán chạy
            ViewBag.TopProducts = await _context.OrderDetails
                .GroupBy(od => od.Product)
                .Select(g => new
                {
                    Name = g.Key.Name,
                    SalesCount = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.SalesCount)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
} 