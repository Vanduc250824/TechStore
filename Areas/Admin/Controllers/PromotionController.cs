using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models;

namespace TechStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PromotionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var promotions = await _context.Promotions
                .Include(p => p.ProductPromotions)
                .ToListAsync();
            return View(promotions);
        }

        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = _context.Products.ToList();
            return View(promotion);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .Include(p => p.ProductPromotions)
                .FirstOrDefaultAsync(p => p.PromotionID == id);

            if (promotion == null)
            {
                return NotFound();
            }

            ViewBag.Products = _context.Products.ToList();
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Promotion promotion)
        {
            if (id != promotion.PromotionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promotion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(promotion.PromotionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = _context.Products.ToList();
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.PromotionID == id);
        }
    }
} 