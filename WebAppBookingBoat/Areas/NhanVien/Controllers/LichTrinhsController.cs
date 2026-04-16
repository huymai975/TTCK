using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVien,Admin")] // Cho phép cả NV và Admin truy cập
    public class LichTrinhsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichTrinhsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NhanVien/LichTrinhs
        public async Task<IActionResult> Index()
        {
            var bayGio = DateTime.Now;

            // 1. Tự động cập nhật trạng thái (Chỉ đọc và cập nhật nếu cần)
            var lichTrinhsUpdate = await _context.LichTrinhs
                .Where(l => l.TrangThai == "Sắp khởi hành" || l.TrangThai == "Đang vận hành")
                .ToListAsync();

            bool coThayDoi = false;
            foreach (var item in lichTrinhsUpdate)
            {
                var trangThaiGoc = item.TrangThai;
                if (bayGio >= item.NgayGioCapBenDuKien) item.TrangThai = "Hoàn thành";
                else if (bayGio >= item.NgayGioKhoiHanh) item.TrangThai = "Đang vận hành";

                if (trangThaiGoc != item.TrangThai) coThayDoi = true;
            }
            if (coThayDoi) await _context.SaveChangesAsync();

            // 2. Lấy dữ liệu hiển thị cho nhân viên (Sử dụng nvLichTrinhViewModel)
            var list = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .OrderByDescending(l => l.NgayGioKhoiHanh)
                .Select(l => new nvLichTrinhViewModel
                {
                    MaLichTrinh = l.MaLichTrinh,
                    NgayGioKhoiHanh = l.NgayGioKhoiHanh,
                    NgayGioCapBenDuKien = l.NgayGioCapBenDuKien,
                    GiaVeCoBan = l.GiaVeCoBan,
                    TrangThai = l.TrangThai,
                    TenTuyen = l.TuyenDuong!.TenTuyen,
                    DiemDi = l.TuyenDuong!.DiemDi,
                    DiemDen = l.TuyenDuong!.DiemDen,
                    TenTau = l.Tau!.TenTau,
                    TongSoGhe = _context.Ghes.Count(g => g.MaTau == l.MaTau),
                    SoGheTrong = _context.Ghes.Count(g => g.MaTau == l.MaTau)
                                 - _context.Ves.Count(v => v.MaLichTrinh == l.MaLichTrinh && v.TrangThai != "Đã hủy")
                })
                .ToListAsync();

            return View(list);
        }

        // GET: NhanVien/LichTrinhs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null) return NotFound();

            // Tính toán số liệu thực tế để NV tư vấn khách
            ViewBag.TongSoGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == lichTrinh.MaTau);
            var soVeDaDat = await _context.Ves.CountAsync(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy");
            ViewBag.SoGheTrongThucTe = (int)ViewBag.TongSoGheThucTe - soVeDaDat;

            return View(lichTrinh);
        }

        /* Lưu ý: Các hàm Create, Edit, Delete đã được loại bỏ 
           vì Nhân viên thường không có quyền thay đổi cấu trúc lịch trình của công ty.
           Nếu bạn muốn nhân viên vẫn được sửa, hãy copy lại các hàm đó và đổi ViewModel tương ứng.
        */
    }
}