using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Models.ViewModels;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.Services; // Thư mục chứa VnPayLibrary

namespace WebAppBookingBoat.Controllers
{
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


        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang) // QUAN TRỌNG: Load thông tin khách hàng
                .Include(h => h.Ves)
                    .ThenInclude(v => v.Ghe) // Load thông tin ghế
                .Include(h => h.Ves)
                    .ThenInclude(v => v.LichTrinh)
                        .ThenInclude(l => l!.TuyenDuong) // Load thông tin tuyến đường
                .Include(h => h.Ves)
                    .ThenInclude(v => v.LichTrinh)
                        .ThenInclude(l => l!.Tau) // QUAN TRỌNG: Load thông tin tàu
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }

        // --- 1. SEARCH LỊCH TRÌNH ---
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

        // --- 2. CHỌN GHẾ ---
        [Authorize]
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
                .Select(v => v.MaGhe)
                .ToListAsync();

            var viewModel = new BookingStepViewModel
            {
                MaLichTrinh = id,
                LichTrinh = lichTrinh,
                GheDaBanIds = gheDaBan,
                GiaVeCoBan = lichTrinh.GiaVeCoBan
            };

            return View(viewModel);
        }

        // --- 3. XÁC NHẬN CHỖ (TẠO HÓA ĐƠN TẠM) ---
        [Authorize]
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

                // 1. Kiểm tra trùng ghế
                var gheVuaBiDat = await _context.Ves
                    .AnyAsync(v => v.MaLichTrinh == vm.MaLichTrinh && vm.SelectedGheIds.Contains(v.MaGhe) && v.TrangThai != "Đã hủy");

                if (gheVuaBiDat)
                {
                    ModelState.AddModelError("", "Một trong số các ghế bạn chọn vừa có người khác đặt.");
                    await transaction.RollbackAsync();
                    return await RebuildBookingView(vm);
                }

                // 2. Tạo đối tượng hóa đơn cơ bản
                var hoaDon = new HoaDon
                {
                    MaKH = khachHang.MaKH,
                    NgayLap = DateTime.Now,
                    TrangThai = "Chờ thanh toán",
                    PhuongThucTT = "VNPay",
                    SoLuongVe = vm.SelectedGheIds.Count,
                    MaKM = vm.MaKM // Lưu mã KM người dùng nhập
                };

                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync(); // Lưu để lấy MaHoaDon

                // 3. Tính toán tiền ghế và tạo vé
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

                // 4. Logic Mã khuyến mãi (KM)
                decimal soTienGiam = 0;
                if (!string.IsNullOrEmpty(vm.MaKM))
                {
                    var khuyenMai = await _context.KhuyenMais
                        .FirstOrDefaultAsync(km => km.MaKM == vm.MaKM && km.NgayKetThuc >= DateTime.Now && km.TrangThai.Equals("Đang diễn ra"));

                    if (khuyenMai != null)
                    {
                        // Giả sử có cột PhanTramGiam (0-100)
                        soTienGiam = tongTienGoc * ((decimal)khuyenMai.PhanTramGiam / 100m);
                    }
                }

                // 5. Cập nhật số dư cuối cùng cho Hóa đơn
                hoaDon.TamTinh = tongTienGoc;
                hoaDon.TongTien = tongTienGoc - soTienGiam;

                // Trừ ghế trống
                lichTrinh!.SoGheTrong = Math.Max(0, lichTrinh.SoGheTrong - vm.SelectedGheIds.Count);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Payment", new { id = hoaDon.MaHoaDon });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return View("Error");
            }
        }

        // --- 4. TRANG CHỌN PHƯƠNG THỨC THANH TOÁN ---
        public async Task<IActionResult> Payment(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.Tau)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null || hoaDon.KhachHang == null) return NotFound();

            // 1. Nếu đã thanh toán rồi, chuyển hướng sang trang chi tiết hoặc trang thành công
            if (hoaDon.TrangThai == "Đã thanh toán")
            {
                // Bạn có thể tạo View Details hoặc dùng luôn View BookingSuccess
                return RedirectToAction("BookingSuccess", new { id = hoaDon.MaHoaDon });
            }

            // 2. Nếu hóa đơn đã bị hủy (do quá hạn 15p chẳng hạn)
            if (hoaDon.TrangThai == "Đã hủy" || hoaDon.TrangThai == "Thanh toán thất bại")
            {
                TempData["ErrorMessage"] = "Đơn hàng này đã bị hủy hoặc hết hạn thanh toán.";
                return RedirectToAction("MyOrders");
            }

            // 3. Nếu đang ở trạng thái "Chờ thanh toán", hiển thị View để khách chọn nút Thanh toán VNPay
            return View(hoaDon);
        }

        // --- 5. REDIRECT SANG VNPAY ---
        [Authorize]
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

            // LƯU Ý: OrderInfo KHÔNG để dấu tiếng Việt để tránh sai chữ ký
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan hoa don " + maHoaDon);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Params["ReturnUrl"]!);
            vnpay.AddRequestData("vnp_TxnRef", maHoaDon.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Params["BaseUrl"]!, vnp_Params["HashSecret"]!);
            return Redirect(paymentUrl);
        }

        // --- 6. XỬ LÝ KẾT QUẢ VNPAY TRẢ VỀ ---
        [Authorize]
        public async Task<IActionResult> PaymentCallback()
        {
            var queryData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();
            var vnp_Params = _configuration.GetSection("Vnpay");

            foreach (var key in queryData.Keys)
            {
                var value = queryData[key];
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            // Lấy ID hóa đơn an toàn
            string txnRef = vnpay.GetResponseData("vnp_TxnRef");
            if (string.IsNullOrEmpty(txnRef)) txnRef = queryData["vnp_TxnRef"].ToString();

            if (!int.TryParse(txnRef, out int maHoaDon))
            {
                return RedirectToAction("Index", "Home");
            }

            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_SecureHash = vnpay.GetResponseData("vnp_SecureHash");

            // Xác thực chữ ký (Hàm băm của bạn phải dùng .ToUpper())
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_Params["HashSecret"]!);

            if (checkSignature && vnp_ResponseCode == "00")
            {
                // KHÔNG dùng AsNoTracking vì cần Update dữ liệu
                var hoaDon = await _context.HoaDons
                    .Include(h => h.Ves)
                    .FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon);

                if (hoaDon != null)
                {
                    if (hoaDon.TrangThai != "Đã thanh toán")
                    {
                        // Cập nhật trạng thái và Ngày thanh toán
                        hoaDon.TrangThai = "Đã thanh toán";
                        hoaDon.NgayThanhToan = DateTime.Now; // Gán thời điểm hiện tại

                        if (hoaDon.Ves != null)
                        {
                            foreach (var ve in hoaDon.Ves)
                            {
                                ve.TrangThai = "Đã thanh toán";
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Đặt vé thành công!";
                    return RedirectToAction("BookingSuccess", new { id = maHoaDon });
                }
            }

            // Nếu lỗi hoặc hủy thanh toán
            TempData["ErrorMessage"] = "Thanh toán thất bại hoặc đã bị hủy.";
            return RedirectToAction("Payment", new { id = maHoaDon });
        }

        [Authorize]
        public IActionResult BookingSuccess(int id)
        {
            return View(id);
        }

        public async Task<IActionResult> ExportPdf(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null || hoaDon.TrangThai != "Đã thanh toán")
            {
                return NotFound();
            }

            return new ViewAsPdf("ExportPdf", hoaDon)
            {
                FileName = $"Ve_Tau_{id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking" // Giúp giữ đúng kích thước CSS
            };
        }
        private async Task<IActionResult> RebuildBookingView(BookingStepViewModel vm)
        {
            vm.LichTrinh = await _context.LichTrinhs
                .Include(lt => lt.TuyenDuong)
                .Include(lt => lt.Tau).ThenInclude(t => t.Ghes)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == vm.MaLichTrinh);
            vm.GheDaBanIds = await _context.Ves
                .Where(v => v.MaLichTrinh == vm.MaLichTrinh && v.TrangThai != "Đã hủy")
                .Select(v => v.MaGhe).ToListAsync();
            return View("BookTicket", vm);
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);

            // Lấy danh sách hóa đơn kèm thông tin lịch trình để hiển thị cho khách
            var orders = await _context.HoaDons
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Where(h => h.MaKH == khachHang!.MaKH)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return View(orders);
        }
    }
}