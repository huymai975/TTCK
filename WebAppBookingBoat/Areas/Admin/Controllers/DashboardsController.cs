using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
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
            DateTime startDate = filter.ToLower() switch
            {
                "week" => today.AddDays(-7),
                "month" => new DateTime(today.Year, today.Month, 1),
                _ => today
            };

            // Sử dụng AsNoTracking để tăng tốc độ truy vấn chỉ đọc
            var hoaDonsThanhToanQuery = _context.HoaDons.AsNoTracking().Where(h => h.TrangThai == "Đã thanh toán");

            // 1. Dữ liệu biểu đồ 7 ngày (Tối ưu bằng cách chỉ chọn các cột cần thiết)
            var dateRangeStart = today.AddDays(-6);
            var hoaDonsInRange = await hoaDonsThanhToanQuery
                .Where(h => h.NgayLap >= dateRangeStart)
                .Select(h => new { h.NgayLap.Date, h.TongTien })
                .ToListAsync();

            var labels = new List<string>();
            var values = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var targetDate = today.AddDays(-i);
                labels.Add(targetDate.ToString("dd/MM"));
                values.Add(hoaDonsInRange.Where(x => x.Date == targetDate).Sum(x => x.TongTien));
            }

            // 2. Tính tỷ lệ lấp đầy (Tối ưu: Không dùng CountAsync trong vòng lặp)
            var startOfToday = DateTime.Today;
            var endOfToday = startOfToday.AddDays(1).AddTicks(-1);

            var lichTrinhHnay = await _context.LichTrinhs.AsNoTracking()
                .Where(lt => lt.NgayGioKhoiHanh >= startOfToday && lt.NgayGioKhoiHanh <= endOfToday && lt.TrangThai != "Đã hủy")
                .Select(lt => new { lt.MaTau, lt.SoGheTrong })
                .ToListAsync();

            double tyLeFinal = 0;
            if (lichTrinhHnay.Any())
            {
                // Lấy danh sách ID tàu hôm nay có chạy
                var maTaus = lichTrinhHnay.Select(lt => lt.MaTau).Distinct().ToList();

                // Truy vấn 1 lần duy nhất để lấy tổng số ghế của các tàu này
                var soGheCacTau = await _context.Taus.AsNoTracking()
                    .Where(t => maTaus.Contains(t.MaTau))
                    .Select(t => new { t.MaTau, SoGhe = t.Ghes.Count }) // Giả sử bạn có Navigation Property t.Ghes
                    .ToDictionaryAsync(x => x.MaTau, x => x.SoGhe);

                double tongGheHnay = 0;
                double gheTrongHnay = 0;

                foreach (var lt in lichTrinhHnay)
                {
                    if (soGheCacTau.TryGetValue(lt.MaTau, out int soGhe))
                    {
                        tongGheHnay += soGhe;
                        gheTrongHnay += lt.SoGheTrong;
                    }
                }

                if (tongGheHnay > 0)
                    tyLeFinal = Math.Round(((tongGheHnay - gheTrongHnay) / tongGheHnay) * 100, 1);
            }

            // 3. Các con số thống kê (Tối ưu: Count trực tiếp trên DB)
            return new DashboardViewModel
            {
                CurrentFilter = filter,
                TongDoanhThu = await hoaDonsThanhToanQuery.SumAsync(h => (decimal?)h.TongTien) ?? 0,
                DoanhThuThangNay = await hoaDonsThanhToanQuery
                    .Where(h => h.NgayLap.Month == today.Month && h.NgayLap.Year == today.Year)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0,

                SoKhachHang = await _context.KhachHangs.CountAsync(),
                SoTauDangChay = await _context.Taus.CountAsync(t => t.TrangThai == true),
                HoaDonChoXuLy = await _context.HoaDons.CountAsync(h => h.TrangThai == "Chưa thanh toán"),
                HoaDonMoiTrongNgay = await _context.HoaDons.CountAsync(h => h.NgayLap >= startDate),

                TyLeLapDay = tyLeFinal,
                Labels7Ngay = labels,
                DoanhThu7Ngay = values,

                HoaDonMoiNhat = await _context.HoaDons.AsNoTracking()
                    .Include(h => h.KhachHang)
                    .OrderByDescending(h => h.NgayLap)
                    .Take(5).ToListAsync(),

                // Chỉ lấy 10 Log gần nhất (Dashboard không nên lấy quá nhiều)
                LogGanDay = await _context.Logs.AsNoTracking()
                    .OrderByDescending(l => l.ThoiGian)
                    .Take(10).ToListAsync()
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