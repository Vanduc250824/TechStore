using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;
using TechStore.Data;
using System.Threading.Tasks;
using System.Linq;
using X.PagedList;
using X.PagedList.Mvc.Core;

namespace TechStore.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PromotionController> _logger;

        public PromotionController(ApplicationDbContext context, ILogger<PromotionController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: User/Promotion
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var promotions = await _context.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.Now)
                .ToPagedListAsync(pageNumber, pageSize);
            return View(promotions);
        }

        // GET: User/Promotion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(m => m.PromotionID == id);
            if (promotion == null)
            {
                return NotFound();
            }

            return View(promotion);
        }

        // Hiển thị form tạo mới
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý tạo khuyến mãi mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,DiscountPercent,StartDate,EndDate,Description")] Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(promotion);
        }

        // Hiển thị form chỉnh sửa
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();
            return View(promotion);
        }

        // Xử lý cập nhật khuyến mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PromotionID,Name,DiscountPercent,StartDate,EndDate,Description")] Promotion promotion)
        {
            if (id != promotion.PromotionID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(promotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(promotion);
        }

        // Xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();
            return View(promotion);
        }

        // Xử lý xóa khuyến mãi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
