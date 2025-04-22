using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TechStore.Models;
using TechStore.Models.ViewModels;
using TechStore.Data;
using X.PagedList;
using X.PagedList.Mvc.Core;

namespace TechStore.Areas.User.Controllers
{
    [Area("User")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: User/Account/Login
        public IActionResult Login()
        {
            ViewBag.Breadcrumb = "Account > Login";
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email does not exist");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been locked");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Invalid password");
                    return View(model);
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Account", new { area = "User" });
                }

                TempData["Message"] = "Đăng nhập thành công!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                TempData["Message"] = "Email hoặc mật khẩu không đúng!";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Login");
            }
        }

        // GET: User/Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Breadcrumb = "Account > Register";
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var emailExists = await _userManager.FindByEmailAsync(model.Email);
                if (emailExists != null)
                {
                    ModelState.AddModelError("", "Email is already in use");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    FullName = model.FullName,
                    Address = model.Address,
                    ProfilePicture = "~/Images/Avatars/Image_Default.png",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                TempData["Message"] = "Đăng ký tài khoản thành công!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "An error occurred during registration");
                TempData["Message"] = "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin!";
                TempData["MessageType"] = "danger";
            }

            return View(model);
        }

        // POST: User/Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        private bool UserExists(string id)
        {
            return _userManager.Users.Any(e => e.Id == id);
        }
        // Trang hiển thị thông tin người dùng
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            Console.WriteLine($"Đang chỉnh sửa người dùng với ID: {id}");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                Console.WriteLine("Không tìm thấy người dùng.");
                return NotFound();
            }


            var editUserView = new EditUserView
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Address = user.Address,
                ProfilePicture = user.ProfilePicture
            };

            Console.WriteLine($"Đã tìm thấy người dùng: {user.UserName}");
            return View(editUserView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserView userView, IFormFile image)
        {
            Console.WriteLine("Yêu cầu POST để chỉnh sửa hồ sơ người dùng.");

            if (!ModelState.IsValid)
            {
                // Nếu ModelState không hợp lệ, log tất cả các lỗi
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Lỗi trong ModelState: {error.ErrorMessage}");
                }
                return View(userView);  // Trả về view nếu có lỗi
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                Console.WriteLine("Không tìm thấy người dùng trong phiên làm việc hiện tại.");
                return NotFound();
            }

            // Cập nhật các thông tin từ EditUserView vào ApplicationUser
            currentUser.UserName = userView.UserName;
            currentUser.Email = userView.Email;
            currentUser.FullName = userView.FullName;
            currentUser.Address = userView.Address;

            // Kiểm tra nếu có ảnh được tải lên
            if (image != null && image.Length > 0)
            {
                Console.WriteLine($"Ảnh được tải lên: {image.FileName}, kích thước: {image.Length} bytes");
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Avatars", fileName);

                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine("Không tìm thấy thư mục, đang tạo thư mục.");
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                currentUser.ProfilePicture = "/Images/Avatars/" + fileName;
                Console.WriteLine($"Đã cập nhật ảnh đại diện: {currentUser.ProfilePicture}");
            }
            else
            {
                // Nếu không có ảnh tải lên, sử dụng ảnh mặc định
                currentUser.ProfilePicture = "/Images/Avatars/Image_Default.png";
                Console.WriteLine("Không có ảnh tải lên, sử dụng ảnh mặc định.");
            }

            // Cập nhật thông tin người dùng trong cơ sở dữ liệu
            var result = await _userManager.UpdateAsync(currentUser);
            if (result.Succeeded)
            {
                Console.WriteLine("Cập nhật hồ sơ người dùng thành công.");
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Lỗi khi cập nhật: {error.Description}");
            }

            ModelState.AddModelError(string.Empty, "Cập nhật hồ sơ người dùng thất bại.");
            return View(userView);
        }
    }
}