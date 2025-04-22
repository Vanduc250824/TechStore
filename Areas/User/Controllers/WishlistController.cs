using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;
using TechStore.Data;
using X.PagedList;
using X.PagedList.Mvc.Core;
// using TechStore.Areas.User;

namespace TechStore.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<WishlistController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<Wishlist> GetOrCreateWishlistAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Please login to use the wishlist");
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(w => w.UserID == user.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserID = user.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return wishlist;
        }

        public async Task<IActionResult> Index(int? page)
        {
            try
            {
                var wishlist = await GetOrCreateWishlistAsync();
                var wishlistItems = wishlist.WishlistItems
                    .OrderByDescending(w => w.AddedAt)
                    .ToList();

                int pageSize = 12;
                int pageNumber = page ?? 1;
                return View(wishlistItems.ToPagedList(pageNumber, pageSize));
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "", returnUrl = "/User/Wishlist" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            try
            {
                var wishlist = await GetOrCreateWishlistAsync();
                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                var existingItem = wishlist.WishlistItems
                    .FirstOrDefault(i => i.ProductID == productId);

                if (existingItem != null)
                {
                    return Json(new { success = false, message = "Product is already in your wishlist" });
                }

                var wishlistItem = new WishlistItem
                {
                    WishlistID = wishlist.WishlistID,
                    ProductID = productId,
                    AddedAt = DateTime.Now
                };

                _context.WishlistItems.Add(wishlistItem);
                wishlist.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Product added to wishlist successfully!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            try
            {
                var wishlist = await GetOrCreateWishlistAsync();
                var wishlistItem = wishlist.WishlistItems
                    .FirstOrDefault(i => i.ProductID == productId);

                if (wishlistItem == null)
                {
                    // Có thể hiển thị lỗi hoặc redirect kèm TempData nếu muốn
                    return RedirectToAction("Index", "Wishlist", new { area = "User" });
                }

                _context.WishlistItems.Remove(wishlistItem);
                wishlist.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Wishlist", new { area = "User" });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Có thể log lỗi hoặc chuyển hướng với TempData
                return RedirectToAction("Index", "Wishlist", new { area = "User" });
            }
        }

    }
}