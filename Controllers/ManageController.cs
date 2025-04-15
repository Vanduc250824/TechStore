using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechStore.Models;

[Authorize]
public class ManageController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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
        return View(userView);  // Trả về view nếu cập nhật không thành công
    }
}
