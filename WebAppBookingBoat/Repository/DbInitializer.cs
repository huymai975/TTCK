using Microsoft.AspNetCore.Identity;
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



            // ---  SEED TÀI KHOẢN ---
            // Khởi tạo Password Hasher
            var hasher = new PasswordHasher<AppUser>();

            // Tạo một vài tài khoản mẫu
            var adminUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(), // Tạo một ID ngẫu nhiên chuẩn GUID, 
                UserName = "admin",
                Email = "admin@booking.com",

                NormalizedUserName = "ADMIN",
                NormalizedEmail = "ADMIN@BOOKING.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "1234");

            var user2 = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "nhanvien1",
                Email = "nhanvien1@booking.com",

                NormalizedUserName = "NHANVIEN1",
                NormalizedEmail = "NHANVIEN1@BOOKING.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user2.PasswordHash = hasher.HashPassword(user2, "1234");

            var user3 = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "khachhang1",
                Email = "khachhang1@gmail.com",

                NormalizedUserName = "KHACHHANG1",
                NormalizedEmail = "KHACHHANG1@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user3.PasswordHash = hasher.HashPassword(user3, "1234");

            var user4 = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "khachhang2",
                Email = "khachhang2@gmail.com",

                NormalizedUserName = "KHACHHANG2",
                NormalizedEmail = "KHACHHANG2@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user4.PasswordHash = hasher.HashPassword(user4, "1234");

            var testuId = Guid.NewGuid().ToString();
            var user5 = new AppUser
            {
                Id = testuId,
                UserName = "testuser",
                Email = "testuser@gmail.com",

                NormalizedUserName = "TESTUSER",
                NormalizedEmail = "TESTUSER@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user5.PasswordHash = hasher.HashPassword(user5, "1234");

            // Đưa vào Database
            modelBuilder.Entity<AppUser>().HasData(
                adminUser, user2, user3, user4, user5
            );


            // ---  SEED ROLE ---


            // THAY ĐỔI: Cấu hình ID theo thứ tự mới
            string adminRoleId = "1";
            string staffRoleId = "2";
            string customerRoleId = "3";

            // Seed Roles vào bảng AspNetRoles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = staffRoleId, Name = "Nhân viên", NormalizedName = "NHÂN VIÊN" },
                new IdentityRole { Id = customerRoleId, Name = "Khách hàng", NormalizedName = "KHÁCH HÀNG" }
            );

            // Gán quyền Admin (RoleId = 1) 
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = adminUser.Id
            });
            // Gán quyền Nhân viên (RoleId = 2) 
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = staffRoleId,
                UserId = user2.Id
            });



            //--- 3. SEED NHÂN VIÊN ---
            modelBuilder.Entity<NhanVien>().HasData(
                new NhanVien { MaNV = 1, MaTK = adminUser.Id, HoTen = "Nguyễn Văn Chạy", Email = "chay.nv@boat.com", Sdt = "0987654321", ChucVu = "Bán vé" }
            );

            // --- 4. SEED KHÁCH HÀNG ---
            modelBuilder.Entity<KhachHang>().HasData(
                new KhachHang { MaKH = 1, MaTK = testuId, HoTen = "Trần Thị Khách", Email = "khach.tran@gmail.com", Sdt = "0912345678", NgaySinh = new DateTime(1995, 5, 20) }
            );

            // --- 5. SEED TUYẾN ĐƯỜNG ---
            modelBuilder.Entity<TuyenDuong>().HasData(
                new TuyenDuong { MaTuyen = 1, TenTuyen = "Sài Gòn - Vũng Tàu", DiemDi = "Sài Gòn", DiemDen = "Vũng Tàu", KhoangCach = 100, ThoiGianDuKien = new TimeSpan(2, 30, 0), HinhAnh = "80eec6b7-1650-400a-afda-eec7573a7f48.jfif" },
                new TuyenDuong { MaTuyen = 2, TenTuyen = "Rạch Giá - Phú Quốc", DiemDi = "Rạch Giá", DiemDen = "Phú Quốc", KhoangCach = 120, ThoiGianDuKien = new TimeSpan(2, 45, 0), HinhAnh = "dcc6e003-7560-4d50-8933-98682d3da2ef.jfif" }
            );

            // --- 6. SEED TÀU ---
            modelBuilder.Entity<Tau>().HasData(
                new Tau { MaTau = 1, TenTau = "Tàu Cao Tốc 01", TongSoGhe = 20, TrangThai = true, HinhAnh = "0ed53f9a-2e39-46ab-897c-856e7cde576d.jpg" },
                new Tau { MaTau = 2, TenTau = "Tàu Express 01", TongSoGhe = 20, TrangThai = true, HinhAnh = "2073e0cb-cf50-45b1-aa5e-9d40af4b7477.jpg" },
                new Tau { MaTau = 3, TenTau = "Tàu Cao Tốc 02", TongSoGhe = 20, TrangThai = true, HinhAnh = "872b8a7a-79ae-4f8b-96da-53c7e3caa5e3.jpg" },
                new Tau { MaTau = 4, TenTau = "Tàu Express 02", TongSoGhe = 20, TrangThai = true, HinhAnh = "d0c7cc56-fd6c-4750-8095-2c250c2c3eed.jpg" }
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

            modelBuilder.Entity<HoaDon>().HasData(
                new HoaDon
                {
                    MaHoaDon = 2,
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

            modelBuilder.Entity<HoaDon>().HasData(
                new HoaDon
                {
                    MaHoaDon = 3,
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
                    MaGhe = 2,
                    MaHoaDon = 1,
                    MaLichTrinh = 1,
                    GiaVe = 180000,
                    TrangThai = "Hợp lệ"
                }
            );

            modelBuilder.Entity<Ve>().HasData(
                new Ve
                {
                    MaVe = 2,
                    MaGhe = 3,
                    MaHoaDon = 2,
                    MaLichTrinh = 1,
                    GiaVe = 180000,
                    TrangThai = "Hợp lệ"
                }
            );

            modelBuilder.Entity<Ve>().HasData(
               new Ve
               {
                   MaVe = 3,
                   MaGhe = 4,
                   MaHoaDon = 3,
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
                    MaTK = adminUser.Id,
                    HanhDong = "Khởi tạo hệ thống",
                    BangTacDong = "Hệ thống",
                    NoiDungChiTiet = "Seed dữ liệu mẫu thành công",
                    ThoiGian = DateTime.Now
                }
            );
        }
    }
}