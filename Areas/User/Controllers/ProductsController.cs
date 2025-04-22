using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStore.Models;
using TechStore.Data;
using X.PagedList;
using X.PagedList.Mvc.Core;
using Ganss;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace TechStore.Areas.User.Controllers
{
    [Area("User")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<ProductsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        //Product


        public IActionResult Category(int id)
        {
           
            var products = _context.Products.Where(p => p.CategoryID == id).ToList();

            ViewBag.CategoryId = id;

            return View(products);
        }



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var products = await _context.Products
                .Include(p => p.Category)
                .ToPagedListAsync(pageNumber, pageSize);
            return View(products);
        }



        //Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,CategoryID, ProductType,OriginalPrice, DiscountedPrice,StockQuantity,ImageFile,Description")] Product model)
        {
            Console.WriteLine($"ProductID: {model.ProductID}");
            Console.WriteLine($"Name: {model.Name}");
            Console.WriteLine($"CategoryID: {model.CategoryID}");
            Console.WriteLine($"Price: {model.OriginalPrice}");
            Console.WriteLine($"Price: {model.DiscountedPrice}");
            Console.WriteLine($"StockQuantity: {model.StockQuantity}");
            Console.WriteLine($"ImageUrl: {model.ImageUrl}");
            Console.WriteLine($"Description: {model.Description}");
            ModelState.Clear();
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Lỗi ModelState: " + error.ErrorMessage);
                }
                return View(model);
            }
            if (ModelState.IsValid)
            {
                // In ra console để kiểm tra dữ liệu nhập vào

                model.DiscountedPrice = model.OriginalPrice;
                if (model.ImageFile != null)
                {
                    //Đặt tên
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string uploadFile = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Products");

                    // Save
                    string filePath = Path.Combine(uploadFile, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }


                    model.ImageUrl = "/Images/Products/" + fileName;
                }
                _context.Add(model);
                await _context.SaveChangesAsync();
                Console.WriteLine($"ProductID after save: {model.ProductID}");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] Product model)
        {
            if (id != model.ProductID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState không hợp lệ!");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"🔴 {key}: {error.ErrorMessage}");
                    }
                }
            }

            // Debug log
            Console.WriteLine("🔥 Description trước khi sanitize: " + model.Description);

            // Sử dụng HtmlSanitizer để ngăn XSS
            var sanitizer = new HtmlSanitizer();
            model.Description = sanitizer.Sanitize(model.Description);

            Console.WriteLine("🔥 Description sau khi sanitize: " + model.Description);

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                Console.WriteLine("❌ Không tìm thấy sản phẩm");
                return NotFound();
            }

            // Cập nhật thông tin sản phẩm
            existingProduct.Name = model.Name;
            existingProduct.CategoryID = model.CategoryID;
            existingProduct.OriginalPrice = model.DiscountedPrice ?? 0;
            existingProduct.DiscountedPrice = model.DiscountedPrice;
            existingProduct.StockQuantity = model.StockQuantity;
            existingProduct.Description = model.Description;

            // Nếu có ảnh mới, cập nhật ảnh
            if (model.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string uploadFile = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Products");
                string filePath = Path.Combine(uploadFile, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                existingProduct.ImageUrl = "/Images/Products/" + fileName;
            }
            try
            {
                _context.Update(existingProduct);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Lưu sản phẩm thành công!");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(model.ProductID))
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

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Xóa hình ảnh nếu có
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Details(int id)
        {
            var product = _context.Products
                                  .Include(p => p.Category)
                                  .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpGet]
        public IActionResult SearchProducts(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return Json(new List<object>());
            }

            try 
            {
                var searchTerm = q.ToLower();
                var products = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Name.ToLower().Contains(searchTerm) || 
                               (p.Description != null && p.Description.ToLower().Contains(searchTerm)) || 
                               p.Category.Name.ToLower().Contains(searchTerm))
                    .Select(p => new
                    {
                        productID = p.ProductID,
                        name = p.Name,
                        imageUrl = p.ImageUrl ?? "/Images/zip.png",
                        originalPrice = p.OriginalPrice,
                        discountedPrice = p.DiscountedPrice,
                        categoryName = p.Category.Name,
                        stockStatus = p.StockQuantity > 0 ? "In Stock" : "Out of Stock"
                    })
                    .Take(5)
                    .ToList();

                _logger.LogInformation($"Search term: {q}, Results found: {products.Count}");
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public IActionResult Search(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return RedirectToAction("Index", "Home");
            }

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(q) || 
                           p.Description.Contains(q) || 
                           p.Category.Name.Contains(q))
                .ToList();

            ViewBag.SearchTerm = q;
            return View(products);
        }
    }

}