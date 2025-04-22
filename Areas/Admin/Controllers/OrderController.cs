using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TechStore.Models;
using TechStore.Data;


namespace TechStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Orders Management
        public async Task<IActionResult> OrdersIndex()
        {
            var orders = await _context.Orders
                                 .Include(o => o.User)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                                .Include(o => o.User)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(od => od.Product)
                                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.OrderStatuses = new List<string> { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                TempData["ErrorMessage"] = "Please select a valid status.";
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = newStatus;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{orderId} status updated to {newStatus}.";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }
        #endregion

        #region Account Management
        public async Task<IActionResult> AccountsIndex()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> AccountDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserStatus(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = isActive;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"User status updated successfully.";
            return RedirectToAction(nameof(AccountDetails), new { id = userId });
        }
        #endregion

        #region Products Management
        public async Task<IActionResult> ProductsIndex()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Category != null ? p.Category.Name : string.Empty)
                .ThenBy(p => p.Name)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProductsIndex));
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ProductsIndex));
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View("ProductDetails", product);
        }
        #endregion

        #region Promotions Management
        public async Task<IActionResult> PromotionsIndex()
        {
            var promotions = await _context.Promotions
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
            return View(promotions);
        }

        public async Task<IActionResult> PromotionDetails(int? id)
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

        public IActionResult CreatePromotion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(PromotionsIndex));
            }
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promotion);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Promotion updated successfully.";
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
                return RedirectToAction(nameof(PromotionsIndex));
            }
            return View("PromotionDetails", promotion);
        }
        #endregion

        #region Helper Methods
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.PromotionID == id);
        }
        #endregion
    }
}