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

        // Action chính xử lý hiển thị Dashboard và Lọc
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

        // Action xuất PDF
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

            // Xác định mốc thời gian dựa trên filter
            switch (filter.ToLower())
            {
                case "week":
                    startDate = today.AddDays(-7);
                    break;
                case "month":
                    startDate = new DateTime(today.Year, today.Month, 1);
                    break;
                default: // today
                    startDate = today;
                    break;
            }

            // 1. Thống kê cơ bản
            var hoaDonsThanhToan = _context.HoaDons.Where(h => h.TrangThai == "Đã thanh toán");

            var dtThangNay = await hoaDonsThanhToan
                .Where(h => h.NgayLap.Month == today.Month && h.NgayLap.Year == today.Year)
                .SumAsync(h => (decimal?)h.TongTien) ?? 0;

            // 2. Xử lý dữ liệu biểu đồ (7 ngày gần nhất)
            var labels = new List<string>();
            var values = new List<decimal>();
            var dateRangeStart = today.AddDays(-6);

            var hoaDonsInRange = await hoaDonsThanhToan
                .Where(h => h.NgayLap.Date >= dateRangeStart)
                .Select(h => new { h.NgayLap.Date, h.TongTien })
                .ToListAsync();

            for (int i = 6; i >= 0; i--)
            {
                var targetDate = today.AddDays(-i);
                labels.Add(targetDate.ToString("dd/MM"));
                values.Add(hoaDonsInRange.Where(x => x.Date == targetDate).Sum(x => x.TongTien));
            }

            // 3. Tính Tỷ lệ lấp đầy (Dựa trên lịch trình hôm nay)
            var lichTrinhHnay = await _context.LichTrinhs
                .Include(lt => lt.Tau)
                .Where(lt => lt.NgayGioKhoiHanh.Date == today)
                .ToListAsync();

            int tongGhe = lichTrinhHnay.Sum(lt => lt.Tau?.TongSoGhe ?? 0);
            int gheTrong = lichTrinhHnay.Sum(lt => lt.SoGheTrong);
            double tyLe = tongGhe > 0 ? (double)(tongGhe - gheTrong) / tongGhe * 100 : 0;

            // 4. Khởi tạo và trả về ViewModel
            return new DashboardViewModel
            {
                CurrentFilter = filter,
                TongDoanhThu = await hoaDonsThanhToan.SumAsync(h => (decimal?)h.TongTien) ?? 0,
                DoanhThuThangNay = dtThangNay,
                SoKhachHang = await _context.KhachHangs.CountAsync(),
                SoTauDangChay = await _context.Taus.CountAsync(t => t.TrangThai == true),

                HoaDonChoXuLy = await _context.HoaDons.CountAsync(h => h.TrangThai == "Chưa thanh toán"),
                HoaDonMoiTrongNgay = await _context.HoaDons.CountAsync(h => h.NgayLap >= startDate),

                TyLeLapDay = Math.Round(tyLe, 1),
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