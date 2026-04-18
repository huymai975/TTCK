using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "Staff,Admin, Nhân viên")] // Cho phép cả NV và Admin truy cập
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

            // Lấy danh sách hành khách
            var passengers = await _context.Ves
                .Where(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy")
                .Include(v => v.Ghe)
                .Include(v => v.HoaDon).ThenInclude(h => h.KhachHang)
                .Select(v => new
                {
                    MaVe = v.MaVe,
                    TenHanhKhach = v.HoaDon.KhachHang.HoTen,
                    SoDienThoai = v.HoaDon.KhachHang.Sdt,
                    Email = v.HoaDon.KhachHang.Email,
                    TenGhe = v.Ghe.TenGhe,
                    LoaiGhe = v.Ghe.LoaiGhe,
                    TrangThaiVe = v.TrangThai
                })
                .OrderBy(v => v.TenGhe)
                .ToListAsync();

            ViewBag.DanhSachHanhKhach = passengers;

            // Giả sử bạn đã tính toán các ViewBag này trước đó
            ViewBag.TongSoGheThucTe = lichTrinh.Tau.Ghes?.Count ?? 0;
            ViewBag.SoGheTrongThucTe = lichTrinh.SoGheTrong;

            return View(lichTrinh);
        }


        public async Task<IActionResult> Passengers(int id)
        {
            var lichTrinh = await _context.LichTrinhs
                .Include(lt => lt.Tau)
                .Include(lt => lt.TuyenDuong)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == id);

            if (lichTrinh == null) return NotFound();

            var model = new PassengerListViewModel
            {
                MaLichTrinh = lichTrinh.MaLichTrinh,
                TenTau = lichTrinh.Tau.TenTau,
                TuyenDuong = $"{lichTrinh.TuyenDuong.DiemDi} - {lichTrinh.TuyenDuong.DiemDen}",
                NgayKhoiHanh = lichTrinh.NgayGioKhoiHanh,
                Passengers = await _context.Ves
                    .Where(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy")
                    .Include(v => v.HoaDon).ThenInclude(h => h.KhachHang)
                    .Include(v => v.Ghe)
                    .Select(v => new PassengerItem
                    {
                        MaVe = v.MaVe,
                        TenHanhKhach = v.HoaDon.KhachHang.HoTen, // Dữ liệu từ bảng Khách Hàng bạn vừa đưa
                        SoDienThoai = v.HoaDon.KhachHang.Sdt,
                        Email = v.HoaDon.KhachHang.Email,
                        TenGhe = v.Ghe.TenGhe,
                        LoaiGhe = v.Ghe.LoaiGhe,
                        TrangThaiVe = v.TrangThai,
                        GiaVe = v.GiaVe
                    })
                    .OrderBy(v => v.TenGhe)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}