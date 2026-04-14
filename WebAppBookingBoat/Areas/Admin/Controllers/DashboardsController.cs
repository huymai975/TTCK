using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WebAppBookingBoat.Areas.Admin.ViewModels;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string filter = "today")
        {
            try
            {
                var model = await GetDashboardsData(filter);
                return View(model);
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi Dashboard", ex.Message, "Error");
                return View(new DashboardViewModel());
            }
        }

        public async Task<IActionResult> ExportPdf(string filter = "today")
        {
            var model = await GetDashboardsData(filter);
            return new ViewAsPdf("ExportPdf", model)
            {
                FileName = $"BaoCao_Dashboards_{DateTime.Now:ddMMyyyy}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10),
                CustomSwitches = "--encoding utf-8"
            };
        }

        #region PRIVATE LOGIC

        private async Task<DashboardViewModel> GetDashboardsData(string filter = "today")
        {
            var today = DateTime.Today;
            DateTime startDate;

            switch (filter.ToLower())
            {
                case "week": startDate = today.AddDays(-7); break;
                case "month": startDate = new DateTime(today.Year, today.Month, 1); break;
                default: startDate = today; break;
            }

            var hoaDonsThanhToan = _context.HoaDons.Where(h => h.TrangThai == "Đã thanh toán");

            // 1. Dữ liệu biểu đồ 7 ngày
            var labels = new List<string>();
            var values = new List<decimal>();
            var dateRangeStart = today.AddDays(-6);

            var hoaDonsInRange = await hoaDonsThanhToan
                .Where(h => h.NgayLap >= dateRangeStart)
                .Select(h => new { h.NgayLap, h.TongTien })
                .ToListAsync();

            for (int i = 6; i >= 0; i--)
            {
                var targetDate = today.AddDays(-i);
                labels.Add(targetDate.ToString("dd/MM"));
                values.Add(hoaDonsInRange.Where(x => x.NgayLap.Date == targetDate).Sum(x => x.TongTien));
            }

            // 2. Tính toán tỷ lệ lấp đầy trong ngày hôm nay
            var startOfToday = DateTime.Today;
            var endOfToday = startOfToday.AddDays(1).AddTicks(-1);

            var lichTrinhHnay = await _context.LichTrinhs
                .Where(lt => lt.NgayGioKhoiHanh >= startOfToday && lt.NgayGioKhoiHanh <= endOfToday && lt.TrangThai != "Đã hủy")
                .ToListAsync();

            double tongGheHnay = 0;
            double gheTrongHnay = 0;

            if (lichTrinhHnay.Any())
            {
                foreach (var lt in lichTrinhHnay)
                {
                    // Đếm số ghế thực tế từ thiết lập của tàu
                    int soGheTauNay = await _context.Ghes.CountAsync(g => g.MaTau == lt.MaTau);
                    tongGheHnay += soGheTauNay;
                    gheTrongHnay += lt.SoGheTrong;
                }
            }

            double tyLeFinal = 0;
            if (tongGheHnay > 0)
            {
                double gheDaDat = tongGheHnay - gheTrongHnay;
                tyLeFinal = Math.Round((gheDaDat / tongGheHnay) * 100, 1);
            }

            // 3. Khởi tạo và trả về ViewModel duy nhất
            return new DashboardViewModel
            {
                CurrentFilter = filter,
                TongDoanhThu = await hoaDonsThanhToan.SumAsync(h => (decimal?)h.TongTien) ?? 0,
                DoanhThuThangNay = await hoaDonsThanhToan
                    .Where(h => h.NgayLap.Month == today.Month && h.NgayLap.Year == today.Year)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0,

                SoKhachHang = await _context.KhachHangs.CountAsync(),
                SoTauDangChay = await _context.Taus.CountAsync(t => t.TrangThai == true),
                HoaDonChoXuLy = await _context.HoaDons.CountAsync(h => h.TrangThai == "Chưa thanh toán"),
                HoaDonMoiTrongNgay = await _context.HoaDons.CountAsync(h => h.NgayLap >= startDate),

                // Gán tỷ lệ đã tính toán
                TyLeLapDay = tyLeFinal,

                Labels7Ngay = labels,
                DoanhThu7Ngay = values,

                HoaDonMoiNhat = await _context.HoaDons
                    .Include(h => h.KhachHang)
                    .OrderByDescending(h => h.NgayLap)
                    .Take(5).ToListAsync(),

                LogGanDay = await _context.Logs
                    .OrderByDescending(l => l.ThoiGian)
                    .Take(50).ToListAsync()
            };
        }

        private async Task GhiLogHeThong(string hanhDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = "Dashboard",
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}