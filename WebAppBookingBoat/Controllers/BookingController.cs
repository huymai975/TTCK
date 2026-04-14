using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Models.ViewModels;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.Services;

namespace WebAppBookingBoat.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public BookingController(ApplicationDbContext context, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // --- HÀM GHI LOG HỆ THỐNG (Đã sửa linh hoạt) ---
        private async Task GhiLogHeThong(string hanhDong, string bangTacDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = bangTacDong,
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Details(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(l => l!.TuyenDuong)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(l => l!.Tau)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null) return NotFound();
            return View(hoaDon);
        }

        [HttpGet]
        public async Task<IActionResult> Evaluate(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(l => l!.TuyenDuong)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null || hoaDon.TrangThai != "Đã thanh toán") return RedirectToAction("MyOrders");

            var daDanhGia = await _context.DanhGias.AnyAsync(d => d.MaHoaDon == id);
            if (daDanhGia)
            {
                TempData["Message"] = "Đơn hàng này đã được đánh giá rồi.";
                return RedirectToAction("MyOrders");
            }

            ViewBag.MaHoaDon = id;
            ViewBag.HoaDon = hoaDon;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate(DanhGia model, IFormFile? fHinhAnh)
        {
            if (ModelState.IsValid)
            {
                if (fHinhAnh != null && fHinhAnh.Length > 0)
                {
                    string fileName = "review-" + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(fHinhAnh.FileName);
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/danh-gia");

                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fHinhAnh.CopyToAsync(stream);
                    }
                    model.HinhAnh = fileName;
                }

                model.NgayDanhGia = DateTime.Now;
                model.TrangThai = "Chờ duyệt";

                _context.DanhGias.Add(model);
                await _context.SaveChangesAsync();

                // Ghi log vào bảng DanhGias
                await GhiLogHeThong("Đánh giá chuyến đi", "DanhGias", $"Khách hàng đánh giá hóa đơn #{model.MaHoaDon} - {model.SoSao} sao");

                return RedirectToAction("MyOrders", new { success = true });
            }
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Search(int maTuyen, DateTime? ngayGioKhoiHanh)
        {
            if (maTuyen == 0) return RedirectToAction("Index", "Home");

            var query = _context.LichTrinhs
                .Include(l => l.TuyenDuong)
                .Include(l => l.Tau)
                .Where(l => l.MaTuyen == maTuyen && l.TrangThai == "Sắp khởi hành");

            if (ngayGioKhoiHanh.HasValue)
            {
                var searchDate = ngayGioKhoiHanh.Value.Date;
                query = query.Where(l => l.NgayGioKhoiHanh.Date == searchDate);
            }

            var model = new SearchViewModel
            {
                TuyenDuong = await _context.TuyenDuongs.FirstOrDefaultAsync(m => m.MaTuyen == maTuyen),
                NgayGioKhoiHanh = ngayGioKhoiHanh,
                KetQuaLichTrinh = await query.OrderBy(l => l.NgayGioKhoiHanh).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> BookTicket(int id)
        {
            var lichTrinh = await _context.LichTrinhs
                .Include(lt => lt.TuyenDuong)
                .Include(lt => lt.Tau).ThenInclude(t => t.Ghes)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == id);

            if (lichTrinh == null || lichTrinh.TrangThai != "Sắp khởi hành")
            {
                TempData["ErrorMessage"] = "Lịch trình này không còn nhận đặt chỗ.";
                return RedirectToAction("Index", "Home");
            }

            var gheDaBan = await _context.Ves
                .Where(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy")
                .Select(v => v.MaGhe).ToListAsync();

            var viewModel = new BookingStepViewModel
            {
                MaLichTrinh = id,
                LichTrinh = lichTrinh,
                GheDaBanIds = gheDaBan,
                GiaVeCoBan = lichTrinh.GiaVeCoBan
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(BookingStepViewModel vm)
        {
            if (vm.SelectedGheIds == null || !vm.SelectedGheIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một chỗ ngồi.");
                return await RebuildBookingView(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = _userManager.GetUserId(User);
                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
                if (khachHang == null) return RedirectToAction("Index", "Home");

                var lichTrinh = await _context.LichTrinhs.FindAsync(vm.MaLichTrinh);

                var gheVuaBiDat = await _context.Ves
                    .AnyAsync(v => v.MaLichTrinh == vm.MaLichTrinh && vm.SelectedGheIds.Contains(v.MaGhe) && v.TrangThai != "Đã hủy");

                if (gheVuaBiDat)
                {
                    ModelState.AddModelError("", "Một trong số các ghế bạn chọn vừa có người khác đặt.");
                    await transaction.RollbackAsync();
                    return await RebuildBookingView(vm);
                }

                var hoaDon = new HoaDon
                {
                    MaKH = khachHang.MaKH,
                    NgayLap = DateTime.Now,
                    TrangThai = "Chờ thanh toán",
                    PhuongThucTT = "VNPay",
                    SoLuongVe = vm.SelectedGheIds.Count,
                    MaKM = vm.MaKM
                };

                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync();

                decimal tongTienGoc = 0;
                foreach (var maGhe in vm.SelectedGheIds)
                {
                    var ghe = await _context.Ghes.FindAsync(maGhe);
                    decimal giaThucTe = (ghe?.LoaiGhe == "VIP") ? (lichTrinh!.GiaVeCoBan * 1.2m) : lichTrinh!.GiaVeCoBan;
                    tongTienGoc += giaThucTe;

                    _context.Ves.Add(new Ve
                    {
                        MaHoaDon = hoaDon.MaHoaDon,
                        MaLichTrinh = vm.MaLichTrinh,
                        MaGhe = maGhe,
                        GiaVe = giaThucTe,
                        TrangThai = "Đang chờ"
                    });
                }

                decimal soTienGiam = 0;
                if (!string.IsNullOrEmpty(vm.MaKM))
                {
                    var khuyenMai = await _context.KhuyenMais
                        .FirstOrDefaultAsync(km => km.MaKM == vm.MaKM && km.NgayKetThuc >= DateTime.Now && km.TrangThai == "Đang diễn ra");
                    if (khuyenMai != null) soTienGiam = tongTienGoc * ((decimal)khuyenMai.PhanTramGiam / 100m);
                }

                hoaDon.TamTinh = tongTienGoc;
                hoaDon.TongTien = tongTienGoc - soTienGiam;
                lichTrinh!.SoGheTrong = Math.Max(0, lichTrinh.SoGheTrong - vm.SelectedGheIds.Count);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Ghi log vào bảng HoaDons
                await GhiLogHeThong("Đặt vé tạm", "HoaDons", $"Tạo hóa đơn #{hoaDon.MaHoaDon} cho {hoaDon.SoLuongVe} ghế");

                return RedirectToAction("Payment", new { id = hoaDon.MaHoaDon });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return View("Error");
            }
        }

        public async Task<IActionResult> Payment(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.Tau)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null || hoaDon.KhachHang == null) return NotFound();
            if (hoaDon.TrangThai == "Đã thanh toán") return RedirectToAction("BookingSuccess", new { id = hoaDon.MaHoaDon });
            if (hoaDon.TrangThai == "Đã hủy" || hoaDon.TrangThai == "Thanh toán thất bại")
            {
                TempData["ErrorMessage"] = "Đơn hàng này đã bị hủy hoặc hết hạn.";
                return RedirectToAction("MyOrders");
            }

            return View(hoaDon);
        }

        [HttpPost]
        public IActionResult CreateVnPayPayment(int maHoaDon)
        {
            var hoaDon = _context.HoaDons.Find(maHoaDon);
            if (hoaDon == null) return NotFound();

            var vnp_Params = _configuration.GetSection("Vnpay");
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", vnp_Params["Version"] ?? "2.1.0");
            vnpay.AddRequestData("vnp_Command", vnp_Params["Command"] ?? "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_Params["TmnCode"]!);
            vnpay.AddRequestData("vnp_Amount", ((long)(hoaDon.TongTien * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", vnp_Params["CurrCode"] ?? "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", vnp_Params["Locale"] ?? "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan hoa don " + maHoaDon);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Params["ReturnUrl"]!);
            vnpay.AddRequestData("vnp_TxnRef", maHoaDon.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Params["BaseUrl"]!, vnp_Params["HashSecret"]!);
            return Redirect(paymentUrl);
        }

        public async Task<IActionResult> PaymentCallback()
        {
            var queryData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();
            var vnp_Params = _configuration.GetSection("Vnpay");

            foreach (var key in queryData.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, queryData[key].ToString());
            }

            string txnRef = vnpay.GetResponseData("vnp_TxnRef");
            if (!int.TryParse(txnRef, out int maHoaDon)) return RedirectToAction("Index", "Home");

            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_SecureHash = vnpay.GetResponseData("vnp_SecureHash");

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_Params["HashSecret"]!);

            if (checkSignature && vnp_ResponseCode == "00")
            {
                var hoaDon = await _context.HoaDons.Include(h => h.Ves).FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon);
                if (hoaDon != null && hoaDon.TrangThai != "Đã thanh toán")
                {
                    hoaDon.TrangThai = "Đã thanh toán";
                    hoaDon.NgayThanhToan = DateTime.Now;
                    if (hoaDon.Ves != null)
                    {
                        foreach (var ve in hoaDon.Ves) ve.TrangThai = "Đã thanh toán";
                    }
                    await _context.SaveChangesAsync();

                    // Ghi log thanh toán thành công
                    await GhiLogHeThong("Thanh toán VNPay", "HoaDons", $"Thanh toán thành công hóa đơn #{maHoaDon}", "Success");
                }
                return RedirectToAction("BookingSuccess", new { id = maHoaDon });
            }

            // Ghi log thanh toán thất bại
            await GhiLogHeThong("Thanh toán thất bại", "HoaDons", $"Lỗi VNPay hoặc khách hủy giao dịch cho hóa đơn #{maHoaDon}", "Warning");

            TempData["ErrorMessage"] = "Thanh toán không thành công.";
            return RedirectToAction("Payment", new { id = maHoaDon });
        }

        public IActionResult BookingSuccess(int id) => View(id);

        public async Task<IActionResult> ExportPdf(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null || hoaDon.TrangThai != "Đã thanh toán") return NotFound();

            // Ghi log xuất PDF
            await GhiLogHeThong("Xuất PDF", "HoaDons", $"Khách hàng tải vé PDF của hóa đơn #{id}");

            return new ViewAsPdf("ExportPdf", hoaDon)
            {
                FileName = $"Ve_Tau_{id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        private async Task<IActionResult> RebuildBookingView(BookingStepViewModel vm)
        {
            vm.LichTrinh = await _context.LichTrinhs.Include(lt => lt.TuyenDuong).Include(lt => lt.Tau).ThenInclude(t => t.Ghes).FirstOrDefaultAsync(lt => lt.MaLichTrinh == vm.MaLichTrinh);
            vm.GheDaBanIds = await _context.Ves.Where(v => v.MaLichTrinh == vm.MaLichTrinh && v.TrangThai != "Đã hủy").Select(v => v.MaGhe).ToListAsync();
            return View("BookTicket", vm);
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
            var orders = await _context.HoaDons.Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Where(h => h.MaKH == khachHang!.MaKH).OrderByDescending(h => h.NgayLap).ToListAsync();
            return View(orders);
        }
    }
}