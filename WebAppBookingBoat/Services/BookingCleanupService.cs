using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Repository;


public class BookingCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public BookingCleanupService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var expirationTime = DateTime.Now.AddMinutes(-15);

                var expiredOrders = await context.HoaDons
                    .Include(h => h.Ves) // Load danh sách vé để xử lý
                    .Where(h => h.TrangThai == "Chưa thanh toán" && h.NgayLap <= expirationTime)
                    .ToListAsync();

                foreach (var hoaDon in expiredOrders)
                {
                    // 1. Cập nhật trạng thái Hóa đơn
                    hoaDon.TrangThai = "Đã hủy";

                    // 2. Cập nhật trạng thái tất cả Vé thuộc hóa đơn này
                    foreach (var ve in hoaDon.Ves)
                    {
                        ve.TrangThai = "Đã hủy"; // Giả định thuộc tính này là TrangThai hoặc tương đương trong Model Ve

                        // 3. Hoàn trả số lượng ghế cho Lịch trình
                        var lichTrinh = await context.LichTrinhs.FindAsync(ve.MaLichTrinh);
                        if (lichTrinh != null)
                        {
                            lichTrinh.SoGheTrong += 1;
                        }
                    }
                }

                if (expiredOrders.Any())
                {
                    await context.SaveChangesAsync();
                }
            }
            // Quét mỗi 5 phút
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}