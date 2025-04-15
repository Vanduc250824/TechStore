using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;
using TechStore.Models;
using X.PagedList;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
        ViewBag.Breadcrumb = "Account > Login";
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = "Vui lòng nhập thông tin hợp lệ!";
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ViewBag.ErrorMessage = "Email hoặc mật khẩu không chính xác!";
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
        if (!result.Succeeded)
        {
            ViewBag.ErrorMessage = "Đăng nhập thất bại!";
            return View(model);
        }

        // Lấy vai trò của user
        var roles = await _userManager.GetRolesAsync(user);

        // Nếu user có role "Admin" thì chuyển hướng đến ListAccounts
        if (roles.Contains("Admin"))
        {
            return RedirectToAction("Index", "Account");
        }

        // Nếu không phải admin, điều hướng về trang chủ hoặc trang khác
        return RedirectToAction("Index", "Home");
    }

    //Get:: Register
    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Breadcrumb = "Account > Register";
        return View();
    }
    //Post: Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.Phone,
                FullName = model.FullName,
                Address = model.Address,
                ProfilePicture = "~/Images/Avatars/Image_Default.png"            
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _userManager.AddToRoleAsync(user, "User");
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }


    // Đăng xuất
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(int? page)
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        Console.WriteLine("User IsAuthenticated: " + User.Identity.IsAuthenticated);
        Console.WriteLine("User Name: " + User.Identity.Name);

        int pageSize = 10;
        int pageNumber = page ?? 1;

        var users = _userManager.Users.OrderBy(u => u.UserName).ToPagedList(pageNumber, pageSize);
        var userRoles = new Dictionary<string, IList<string>>();

        foreach (var u in users)
        {
            var r = await _userManager.GetRolesAsync(u);
            userRoles[u.Id] = r;
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = _roleManager.Roles.Select(r => r.Name).ToList();
        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditAccountViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Address = user.Address,
            Phone = user.PhoneNumber,
            AvailableRoles = roles,
            SelectedRole = userRoles.FirstOrDefault() // Lấy role đầu tiên của user
        };

        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(EditAccountViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Address = model.Address;
        user.PhoneNumber = model.Phone;

        // Cập nhật thông tin người dùng
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Cập nhật thông tin thất bại!");
            return View(model);
        }

        // Xóa role cũ và gán role mới
        var oldRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, oldRoles);
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRole);
        }

        return RedirectToAction("Index", "Account");
    }

    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Account");
        }

        ModelState.AddModelError("", "Lỗi khi xóa tài khoản.");
        return View(user);
    }
}