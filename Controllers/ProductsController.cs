using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStore.Models;
using X.PagedList;
using Ganss;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;


namespace TechStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        //Product
        public IActionResult Laptops()
        {
            return View();
        }
        public IActionResult DesktopPCs()
        {
            return View();
        }
        public IActionResult NetworkingDevices()
        {
            return View();
        }
        public IActionResult PrinterScanner()
        {
            return View();
        }
        public IActionResult PCParts()
        {
            return View();
        }
        public IActionResult AllOtherProducts()
        {
            return View();
        }

        //Index
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1; // Nếu null thì mặc định trang 1

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductID);

            var pagedProducts = await productsQuery.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedProducts); // Đảm bảo trả về IPagedList<Product>
        }

        //Create
        [HttpGet]
        public IActionResult Create()
        {
            var categories = new SelectList(_context.Categories, "CategoryID", "Name");
            Console.WriteLine(categories);
            ViewData["CategoryID"] = categories;
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
            existingProduct.OriginalPrice = model.DiscountedPrice;
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
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                // Nếu không có từ khóa tìm kiếm, trả về tất cả sản phẩm
                var allProducts = await _context.Products.ToListAsync();
                return View(allProducts);
            }

            // Tìm kiếm sản phẩm theo tên (case insensitive)
            var searchResults = await _context.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToListAsync();

            return View(searchResults);
        }
    }

}