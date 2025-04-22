using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TechStore.Models;
using System.Security.Claims;
using X.PagedList;
using TechStore.Data;
// using TechStore.Areas.User.Models;

namespace TechStore.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<CartController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<ShoppingCart> GetOrCreateCartAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Please login to use the cart");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserID == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserID = user.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<IActionResult> Index(int? page)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                var cartItems = cart.CartItems
                    .OrderByDescending(i => i.CartItemID)
                    .ToList();

                int pageSize = 10;
                int pageNumber = page ?? 1;
                return View(cartItems.ToPagedList(pageNumber, pageSize));
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                var existingItem = cart.CartItems
                    .FirstOrDefault(i => i.ProductID == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    var cartItem = new ShoppingCartItem
                    {
                        CartID = cart.CartID,
                        ProductID = productId,
                        Quantity = quantity
                    };
                    _context.ShoppingCartItems.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Product added to cart successfully!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                var cartItem = cart.CartItems
                    .FirstOrDefault(i => i.CartItemID == cartItemId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Item not found in cart" });
                }

                cartItem.Quantity = quantity;
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Quantity updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                var cartItem = cart.CartItems
                    .FirstOrDefault(i => i.CartItemID == cartItemId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Item not found in cart" });
                }

                _context.ShoppingCartItems.Remove(cartItem);
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Item removed from cart" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                if (!cart.CartItems.Any())
                {
                    return Json(new { success = false, message = "Cart is empty" });
                }

                // Check stock
                foreach (var item in cart.CartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductID);
                    if (product == null)
                    {
                        return Json(new { success = false, message = "Some products no longer exist" });
                    }
                    if (product.StockQuantity < item.Quantity)
                    {
                        return Json(new { success = false, message = $"Product {product.Name} only has {product.StockQuantity} items in stock" });
                    }
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Tạo đơn hàng
                    var order = new Order
                    {
                        UserID = cart.UserID,
                        TotalAmount = cart.CartItems.Sum(i => i.Quantity * i.Product.DiscountedPrice ?? 0),
                        OrderDate = DateTime.Now,
                        Status = "Chờ xác nhận"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Thêm chi tiết đơn hàng và cập nhật kho
                    foreach (var item in cart.CartItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductID);
                        
                        var orderDetail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            Price = item.Product.DiscountedPrice ?? 0
                        };
                        _context.OrderDetails.Add(orderDetail);

                        if (product != null)
                        {
                            product.StockQuantity -= item.Quantity;
                        }
                        if (product != null)
                        {
                            _context.Products.Update(product);
                        }
                    }

                    // Xóa giỏ hàng
                    _context.ShoppingCartItems.RemoveRange(cart.CartItems);
                    cart.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, orderId = order.OrderID });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return Json(new { success = false, message = "An error occurred during checkout" });
            }
        }

        // Lấy danh sách sản phẩm
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.ProductID,
                    p.Name,
                    p.Description,
                    p.ImageUrl,
                    p.OriginalPrice,
                    p.DiscountedPrice,
                    p.StockQuantity,
                    CategoryName = p.Category != null ? p.Category.Name : "Unknown"
                })
                .ToListAsync();

            return Json(new { success = true, products });
        }

        // Lấy chi tiết sản phẩm theo ID
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.ProductID,
                    p.Name,
                    p.Description,
                    p.ImageUrl,
                    p.OriginalPrice,
                    p.DiscountedPrice,
                    p.StockQuantity,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            return Json(new { success = true, product });
        }
    }

    public class AddToCartModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateQuantityModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemModel
    {
        public int ProductId { get; set; }
    }
} 