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
                //var expirationTime = DateTime.Now.AddMinutes(-3);
                var expirationTime = DateTime.Now.AddMinutes(-15);

                // 1. Chỉ lấy ID của các hóa đơn hết hạn để tránh giữ quá nhiều dữ liệu trong Tracking
                var expiredOrderIds = await context.HoaDons
                    .Where(h => h.TrangThai == "Chưa thanh toán" && h.NgayLap <= expirationTime)
                    .Select(h => h.MaHoaDon)
                    .ToListAsync();

                foreach (var orderId in expiredOrderIds)
                {
                    // 2. Nạp từng hóa đơn một để xử lý độc lập
                    var hoaDon = await context.HoaDons
                        .Include(h => h.Ves)
                        .ThenInclude(v => v.LichTrinh)
                        .FirstOrDefaultAsync(h => h.MaHoaDon == orderId);

                    if (hoaDon != null)
                    {
                        hoaDon.TrangThai = "Đã hủy";
                        foreach (var ve in hoaDon.Ves)
                        {
                            ve.TrangThai = "Đã hủy";
                            if (ve.LichTrinh != null)
                            {
                                ve.LichTrinh.SoGheTrong += 1;
                            }
                        }

                        try
                        {
                            // 3. Lưu ngay sau mỗi hóa đơn để giải phóng bộ nhớ
                            await context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // Nếu có ai đó vừa đặt vé hoặc tác động vào lịch trình này, bỏ qua để vòng quét sau xử lý tiếp
                            continue;
                        }
                    }
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}