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
            // Loại bỏ validation cho các trường không nhập từ form
            ModelState.Remove("HoaDon");
            ModelState.Remove("TrangThai");
            ModelState.Remove("NgayDanhGia");

            if (ModelState.IsValid)
            {
                try
                {
                    if (fHinhAnh != null && fHinhAnh.Length > 0)
                    {
                        string fileName = "review-" + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(fHinhAnh.FileName);
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/danh-gia");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                        string filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) { await fHinhAnh.CopyToAsync(stream); }
                        model.HinhAnh = fileName;
                    }

                    model.NgayDanhGia = DateTime.Now;
                    model.TrangThai = "Chờ duyệt";
                    _context.DanhGias.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cảm ơn bạn! Đánh giá của bạn đã được gửi thành công.";
                    return RedirectToAction("MyOrders");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi đánh giá. Vui lòng thử lại.";
                }
            }

            // Nếu lỗi quay lại trang đánh giá
            return RedirectToAction("Evaluate", new { id = model.MaHoaDon });
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
            // 1. Kiểm tra đầu vào cơ bản
            if (vm.SelectedGheIds == null || !vm.SelectedGheIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một chỗ ngồi.";
                return await RebuildBookingView(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = _userManager.GetUserId(User);
                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
                if (khachHang == null) return RedirectToAction("Index", "Home");

                // 2. Kiểm tra lịch trình còn tồn tại và còn đủ ghế không
                var lichTrinh = await _context.LichTrinhs.FindAsync(vm.MaLichTrinh);
                if (lichTrinh == null)
                {
                    TempData["ErrorMessage"] = "Lịch trình không còn tồn tại.";
                    return RedirectToAction("Index", "Home");
                }

                if (lichTrinh.SoGheTrong < vm.SelectedGheIds.Count)
                {
                    TempData["ErrorMessage"] = $"Xin lỗi, chuyến đi này chỉ còn {lichTrinh.SoGheTrong} ghế trống.";
                    return await RebuildBookingView(vm);
                }

                // 3. Kiểm tra ghế đã có người đặt chưa (Concurrency check)
                var gheVuaBiDat = await _context.Ves
                    .AnyAsync(v => v.MaLichTrinh == vm.MaLichTrinh &&
                                   vm.SelectedGheIds.Contains(v.MaGhe) &&
                                   v.TrangThai != "Đã hủy");

                if (gheVuaBiDat)
                {
                    TempData["ErrorMessage"] = "Một trong số các ghế bạn chọn vừa có người khác đặt. Vui lòng chọn ghế khác.";
                    await transaction.RollbackAsync();
                    return await RebuildBookingView(vm);
                }

                // 4. Khởi tạo hóa đơn
                var hoaDon = new HoaDon
                {
                    MaKH = khachHang.MaKH,
                    NgayLap = DateTime.Now,
                    TrangThai = "Chờ thanh toán",
                    PhuongThucTT = "VNPay", // Mặc định hoặc theo vm
                    SoLuongVe = vm.SelectedGheIds.Count,
                    MaKM = vm.MaKM
                };

                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync(); // Lưu để lấy MaHoaDon cho bảng Ve

                // 5. Tính tiền và tạo vé chi tiết
                decimal tongTienGoc = 0;
                foreach (var maGhe in vm.SelectedGheIds)
                {
                    var ghe = await _context.Ghes.FindAsync(maGhe);
                    // Tính giá dựa trên loại ghế (VIP +20% giá gốc)
                    decimal giaThucTe = (ghe?.LoaiGhe == "VIP") ? (lichTrinh.GiaVeCoBan * 1.2m) : lichTrinh.GiaVeCoBan;
                    tongTienGoc += giaThucTe;

                    _context.Ves.Add(new Ve
                    {
                        MaHoaDon = hoaDon.MaHoaDon,
                        MaLichTrinh = vm.MaLichTrinh,
                        MaGhe = maGhe,
                        GiaVe = giaThucTe,
                        TrangThai = "Chờ thanh toán"
                    });
                }

                // 6. Xử lý khuyến mãi (Kiểm tra điều kiện chặt chẽ)
                decimal soTienGiam = 0;
                if (!string.IsNullOrEmpty(vm.MaKM))
                {
                    var khuyenMai = await _context.KhuyenMais
                        .FirstOrDefaultAsync(km => km.MaKM == vm.MaKM &&
                                                   km.NgayKetThuc >= DateTime.Now &&
                                                   km.TrangThai == "Đang diễn ra");

                    if (khuyenMai != null)
                    {
                        soTienGiam = tongTienGoc * ((decimal)khuyenMai.PhanTramGiam / 100m);
                    }
                    else
                    {
                        // Nếu mã không hợp lệ thì xóa mã khỏi hóa đơn
                        hoaDon.MaKM = null;
                    }
                }

                // 7. Cập nhật tổng tiền hóa đơn và trừ số ghế trống
                hoaDon.TamTinh = tongTienGoc;
                hoaDon.TongTien = tongTienGoc - soTienGiam;
                lichTrinh.SoGheTrong -= vm.SelectedGheIds.Count;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 8. Ghi log hệ thống
                await GhiLogHeThong("Đặt vé", "HoaDons", $"Khách hàng {khachHang.HoTen} tạo hóa đơn #{hoaDon.MaHoaDon} ({hoaDon.SoLuongVe} ghế). Tổng tiền: {hoaDon.TongTien:N0} VNĐ", "Info");

                TempData["SuccessMessage"] = "Đã giữ chỗ thành công! Vui lòng thanh toán để hoàn tất đơn hàng.";
                return RedirectToAction("Payment", new { id = hoaDon.MaHoaDon });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Ghi log lỗi để debug
                await GhiLogHeThong("Lỗi đặt vé", "System", $"Lỗi khi xác nhận đặt vé: {ex.Message}", "Error");
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình đặt vé. Vui lòng thử lại.";
                return RedirectToAction("Index", "Home");
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
                    await GhiLogHeThong("Thanh toán VNPay", "HoaDons", $"Thanh toán thành công hóa đơn #{maHoaDon}", "Info");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = _userManager.GetUserId(User);
            var hoaDon = await _context.HoaDons
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null) return NotFound();

            // Kiểm tra điều kiện hủy (Ví dụ: trước 24h chuyến đi đầu tiên trong đơn khởi hành)
            var firstTicket = hoaDon.Ves.FirstOrDefault();
            if (firstTicket?.LichTrinh != null)
            {
                var thoiGianKhoiHanh = firstTicket.LichTrinh.NgayGioKhoiHanh;
                if (DateTime.Now.AddHours(24) >= thoiGianKhoiHanh)
                {
                    TempData["ErrorMessage"] = "Không thể hủy vé vì đã quá thời hạn cho phép (trước 24h khởi hành).";
                    return RedirectToAction("MyOrders");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật trạng thái hóa đơn
                hoaDon.TrangThai = "Đã hủy";

                // 2. Cập nhật trạng thái từng vé và hoàn lại số lượng ghế trống cho lịch trình
                foreach (var ve in hoaDon.Ves)
                {
                    ve.TrangThai = "Đã hủy";
                    var lichTrinh = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);
                    if (lichTrinh != null)
                    {
                        lichTrinh.SoGheTrong += 1; // Hoàn lại 1 ghế
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await GhiLogHeThong("Hủy đơn hàng", "HoaDons", $"Khách hàng đã tự hủy hóa đơn #{id}", "Warning");
                TempData["SuccessMessage"] = "Đơn hàng của bạn đã được hủy thành công.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await GhiLogHeThong("Lỗi hủy đơn", "System", ex.Message, "Error");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đơn hàng.";
            }

            return RedirectToAction("MyOrders");
        }

        private async Task<IActionResult> RebuildBookingView(BookingStepViewModel vm)
        {
            vm.LichTrinh = await _context.LichTrinhs.Include(lt => lt.TuyenDuong).Include(lt => lt.Tau).ThenInclude(t => t.Ghes).FirstOrDefaultAsync(lt => lt.MaLichTrinh == vm.MaLichTrinh);
            vm.GheDaBanIds = await _context.Ves.Where(v => v.MaLichTrinh == vm.MaLichTrinh && v.TrangThai != "Đã hủy").Select(v => v.MaGhe).ToListAsync();
            return View("BookTicket", vm);
        }

        public async Task<IActionResult> MyOrders(string searchTerm, string status, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);

            if (khachHang == null) return NotFound();

            // 1. Khởi tạo Query ban đầu
            var query = _context.HoaDons
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(l => l!.TuyenDuong)
                .Include(h => h.DanhGias)
                .Where(h => h.MaKH == khachHang.MaKH)
                .AsQueryable();

            // 2. Logic Tìm kiếm (Mã hóa đơn hoặc Tên tuyến)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(h => h.MaHoaDon.ToString().Contains(searchTerm) ||
                                         h.Ves.Any(v => v.LichTrinh!.TuyenDuong.TenTuyen.Contains(searchTerm)));
            }

            // 3. Lọc theo Trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(h => h.TrangThai == status);
            }

            // 4. Lọc theo Khoảng ngày
            if (fromDate.HasValue)
            {
                query = query.Where(h => h.NgayLap >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                // Cộng thêm 1 ngày để lấy hết dữ liệu của ngày kết thúc (tránh sót giờ)
                var endOfDay = toDate.Value.AddDays(1);
                query = query.Where(h => h.NgayLap < endOfDay);
            }

            // 5. Xử lý Phân trang
            int pageSize = 5; // Số lượng đơn trên 1 trang
            int totalItems = await query.CountAsync();
            var lichSu = await query
                .OrderByDescending(h => h.NgayLap)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Gửi dữ liệu filter ngược lại View để giữ trạng thái trên các ô nhập liệu
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(lichSu);
        }
    }
}