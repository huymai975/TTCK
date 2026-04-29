using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

var builder = WebApplication.CreateBuilder(args);

// DB Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddControllersWithViews();

// --- PHẦN THÊM MỚI: CẤU HÌNH CLOUDINARY ---

// Đọc cấu hình từ appsettings.json
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoService (Service này bạn sẽ tạo ở bước sau)
builder.Services.AddScoped<PhotoService>();

// ------------------------------------------

// 1. Cấu hình Identity kết nối với ApplicationDbContext
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu (Tùy chỉnh theo ý bạn)
    options.Password.RequireDigit = false; // Không bắt buộc có số
    options.Password.RequiredLength = 4;    // Độ dài tối thiểu 6 ký tự
    options.Password.RequireNonAlphanumeric = false; // Không bắt buộc ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ hoa
    options.Password.RequireLowercase = false; // Không bắt buộc chữ thường

    // Cấu hình khóa tài khoản khi nhập sai
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //options.Lockout.MaxFailedAccessAttempts = 5;

    // Cấu hình người dùng
    options.User.RequireUniqueEmail = true; // Email không được trùng
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Lưu vào DB hiện tại
.AddDefaultTokenProviders();

// 2. Cấu hình Cookie (Đường dẫn trang Login, Logout, AccessDenied)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Error/403"; // Trang báo lỗi khi không có quyền
    options.Cookie.Name = "BoatBookingCookie";
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Ghi nhớ đăng nhập 30 ngày
    options.SlidingExpiration = true; // Tự động gia hạn khi người dùng truy cập lại
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Lưu cookie trong 30 ngày

});

builder.Services.AddHostedService<BookingCleanupService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Staff", "Customer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");


app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();
app.UseAuthentication(); // XÁC THỰC: Ai đang truy cập? 
app.UseAuthorization();  // PHÂN QUYỀN: Họ có quyền làm gì?


app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


IWebHostEnvironment env = app.Environment;
Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath, "Rotativa");

app.Run();
