using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;

namespace WebAppBookingBoat.Repository
{
    public static class DbInitializer
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // --- 1. SEED KHUYẾN MÃI ---
            modelBuilder.Entity<KhuyenMai>().HasData(
                new KhuyenMai
                {
                    MaKM = "KM10",
                    TenChuongTrinh = "Giảm giá khai trương",
                    PhanTramGiam = 10,
                    SoTienToiDaGiam = 50000,
                    NgayBatDau = new DateTime(2026, 1, 1),
                    NgayKetThuc = new DateTime(2026, 12, 31),
                    TrangThai = true
                },
                new KhuyenMai
                {
                    MaKM = "SUMMER26",
                    TenChuongTrinh = "Ưu đãi mùa hè",
                    PhanTramGiam = 15,
                    SoTienToiDaGiam = 100000,
                    NgayBatDau = new DateTime(2026, 6, 1),
                    NgayKetThuc = new DateTime(2026, 8, 31),
                    TrangThai = true
                }
            );

            // --- 2. SEED TÀI KHOẢN ---
            //modelBuilder.Entity<TaiKhoan>().HasData(
            //    new TaiKhoan { MaTK = 1, TenDangNhap = "admin", MatKhau = "admin123", VaiTro = "Admin", NgayTao = DateTime.Now },
            //    new TaiKhoan { MaTK = 2, TenDangNhap = "nhanvien01", MatKhau = "123456", VaiTro = "Nhân viên", NgayTao = DateTime.Now },
            //    new TaiKhoan { MaTK = 3, TenDangNhap = "khachhang01", MatKhau = "123456", VaiTro = "Khách hàng", NgayTao = DateTime.Now }
            //);

            // --- 3. SEED NHÂN VIÊN ---
            modelBuilder.Entity<NhanVien>().HasData(
                new NhanVien { MaNV = 1, MaTK = 2, HoTen = "Nguyễn Văn Chạy", Email = "chay.nv@boat.com", Sdt = "0987654321", ChucVu = "Bán vé" }
            );

            // --- 4. SEED KHÁCH HÀNG ---
            modelBuilder.Entity<KhachHang>().HasData(
                new KhachHang { MaKH = 1, MaTK = 3, HoTen = "Trần Thị Khách", Email = "khach.tran@gmail.com", Sdt = "0912345678", NgaySinh = new DateTime(1995, 5, 20) }
            );

            // --- 5. SEED TUYẾN ĐƯỜNG ---
            modelBuilder.Entity<TuyenDuong>().HasData(
                new TuyenDuong { MaTuyen = 1, TenTuyen = "Sài Gòn - Vũng Tàu", DiemDi = "Sài Gòn", DiemDen = "Vũng Tàu", KhoangCach = 100, ThoiGianDuKien = new TimeSpan(2, 30, 0), HinhAnh = "vungtau.jpg" },
                new TuyenDuong { MaTuyen = 2, TenTuyen = "Rạch Giá - Phú Quốc", DiemDi = "Rạch Giá", DiemDen = "Phú Quốc", KhoangCach = 120, ThoiGianDuKien = new TimeSpan(2, 45, 0), HinhAnh = "phuquoc.jpg" }
            );

            // --- 6. SEED TÀU ---
            modelBuilder.Entity<Tau>().HasData(
                new Tau { MaTau = 1, TenTau = "Tàu Cao Tốc 01", TongSoGhe = 20, SoGheThuong = 15, SoGheVIP = 5, TrangThai = true, HinhAnh = "tau01.jpg" },
                new Tau { MaTau = 2, TenTau = "Tàu Express 02", TongSoGhe = 20, SoGheThuong = 15, SoGheVIP = 5, TrangThai = true, HinhAnh = "tau02.jpg" }
            );

            // --- 7. TỰ ĐỘNG SEED GHẾ (40 ghế cho 2 tàu) ---
            var ghes = new List<Ghe>();
            for (int t = 1; t <= 2; t++)
            {
                for (int i = 1; i <= 20; i++)
                {
                    int maGheGlobal = (t - 1) * 20 + i;
                    ghes.Add(new Ghe
                    {
                        MaGhe = maGheGlobal,
                        MaTau = t,
                        TenGhe = i <= 15 ? $"T-{i:D2}" : $"V-{i:D2}",
                        LoaiGhe = i <= 15 ? "Thường" : "VIP"
                    });
                }
            }
            modelBuilder.Entity<Ghe>().HasData(ghes);

            // --- 8. SEED LỊCH TRÌNH (Chuyến đi trong tương lai) ---
            modelBuilder.Entity<LichTrinh>().HasData(
                new LichTrinh
                {
                    MaLichTrinh = 1,
                    MaTuyen = 1,
                    MaTau = 1,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(1).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(1).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }
            );

            // --- 9. SEED HÓA ĐƠN MẪU ---
            modelBuilder.Entity<HoaDon>().HasData(
                new HoaDon
                {
                    MaHoaDon = 1,
                    MaKH = 1,
                    MaNV = 1,
                    MaKM = "KM10",
                    NgayLap = DateTime.Now,
                    SoLuongVe = 1,
                    TamTinh = 200000,
                    SoTienGiam = 20000,
                    TongTien = 180000,
                    PhuongThucTT = "Tiền mặt",
                    TrangThai = "Đã thanh toán"
                }
            );

            // --- 10. SEED VÉ MẪU ---
            modelBuilder.Entity<Ve>().HasData(
                new Ve
                {
                    MaVe = 1,
                    MaHoaDon = 1,
                    MaLichTrinh = 1,
                    GiaVe = 180000,
                    TrangThai = "Hợp lệ"
                }
            );

            // --- 11. SEED LOG HỆ THỐNG ---
            modelBuilder.Entity<Log>().HasData(
                new Log
                {
                    MaLog = 1,
                    MaTK = 1,
                    HanhDong = "Khởi tạo hệ thống",
                    BangTacDong = "Hệ thống",
                    NoiDungChiTiet = "Seed dữ liệu mẫu thành công",
                    ThoiGian = DateTime.Now
                }
            );
        }
    }
}