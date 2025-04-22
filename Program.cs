using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using TechStore.Data;
using TechStore.Models;


var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ cần thiết
builder.Services.AddAuthorization(options =>
{
    // Định nghĩa một chính sách phân quyền "Admin"
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
});
// 🔥 Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// 🔥 Đăng ký ApplicationDbContext
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Kiểm tra và tạo role nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAsync(services);
}

// Cấu hình pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Khu vực Admin
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Admin" },
    constraints: new { area = "Admin" }
).RequireAuthorization("Admin");  // Áp dụng phân quyền Admin

// Khu vực User
app.MapControllerRoute(
    name: "user",
    pattern: "User/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "User" },
    constraints: new { area = "User" }
);

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=ThumbNail}/{id?}"
);

app.Run();

async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "User", "Admin" };  // Vai trò mặc định

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            try
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating role {role}: {ex.Message}");
            }
        }
    }
}


