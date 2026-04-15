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
                    HinhAnh = "322b0521-7bf6-40e5-aeea-b28fcfb0c5fd.jpg", // Tên file ảnh mẫu trong wwwroot/images/khuyen-mai/
                    MoTa = "Chào mừng hệ thống WebAppBookingBoat đi vào hoạt động. Giảm ngay 50% cho tất cả các tuyến tàu cao tốc.",
                    PhanTramGiam = 50,
                    SoTienToiDaGiam = 200000,
                    NgayBatDau = new DateTime(2026, 1, 1),
                    NgayKetThuc = new DateTime(2026, 12, 31),
                    TrangThai = "Chưa diễn ra"
                },
                new KhuyenMai
                {
                    MaKM = "SUMMER26",
                    TenChuongTrinh = "Ưu đãi mùa hè rực rỡ",
                    HinhAnh = "summer-sale.jpg",
                    MoTa = "Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 30% khi đặt vé.",
                    PhanTramGiam = 15,
                    SoTienToiDaGiam = 100000,
                    NgayBatDau = new DateTime(2026, 6, 1),
                    NgayKetThuc = new DateTime(2026, 8, 31),
                    TrangThai = "Chưa diễn ra"
                },
                new KhuyenMai
                {
                    MaKM = "TET2026",
                    TenChuongTrinh = "Vui Tết sum vầy",
                    HinhAnh = "0b00c585-fb35-4462-9b5d-8e1aee28b429.jpg",
                    MoTa = "Chương trình khuyến mãi đặc biệt dành cho khách hàng về quê ăn Tết hoặc du xuân cùng gia đình.",
                    PhanTramGiam = 30,
                    SoTienToiDaGiam = 300000,
                    NgayBatDau = new DateTime(2026, 1, 15),
                    NgayKetThuc = new DateTime(2026, 2, 15),
                    TrangThai = "Chưa diễn ra"
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
                    MaTK = adminUser.Id, // Đã sửa: dùng biến adminUser đã khai báo ở trên
                    LoaiLog = "Info",
                    HanhDong = "Khởi tạo hệ thống", // Đã sửa: bỏ ký tự N
                    BangTacDong = "System",
                    NoiDungChiTiet = "Hệ thống đã khởi tạo dữ liệu mẫu (Seed Data) thành công.",
                    ThoiGian = new DateTime(2026, 4, 13, 9, 0, 0),
                    IpAddress = "127.0.0.1"
                },
                new Log
                {
                    MaLog = 2,
                    MaTK = adminUser.Id,
                    LoaiLog = "Info",
                    HanhDong = "Cấu hình bảo mật",
                    BangTacDong = "AspNetUsers",
                    NoiDungChiTiet = "Thiết lập quyền Quản trị viên (Admin) cho hệ thống.",
                    ThoiGian = new DateTime(2026, 4, 13, 9, 0, 5),
                    IpAddress = "127.0.0.1"
                }
            );

            // --- 12. SEED ĐÁNH GIÁ (Quan hệ 1-1 với Hóa đơn) ---
            modelBuilder.Entity<DanhGia>().HasData(
                new DanhGia
                {
                    MaDanhGia = 1,
                    MaHoaDon = 1, // Khớp với HoaDon 1 đã seed ở trên
                    SoSao = 5,
                    NoiDung = "Chuyến đi tuyệt vời, tàu chạy rất êm và đúng giờ. Nhân viên hỗ trợ nhiệt tình!",
                    HinhAnh = "review-tau-01.jpg", // Ảnh khách chụp
                    NgayDanhGia = new DateTime(2026, 4, 10, 8, 30, 0),
                    TrangThai = "Đã hiển thị",
                    // Phản hồi từ Admin
                    PhanHoiAdmin = "Cảm ơn bạn đã ủng hộ WebAppBookingBoat! Rất mong được phục vụ bạn trong những chuyến đi tới.",
                    NgayPhanHoi = new DateTime(2026, 4, 10, 14, 0, 0)
                },
                new DanhGia
                {
                    MaDanhGia = 2,
                    MaHoaDon = 2, // Khớp với HoaDon 2
                    SoSao = 4,
                    NoiDung = "Chất lượng ghế VIP rất tốt, tuy nhiên đồ ăn nhẹ trên tàu hơi ít lựa chọn.",
                    HinhAnh = "review-ghe-vip.jpg",
                    NgayDanhGia = new DateTime(2026, 4, 11, 15, 20, 0),
                    TrangThai = "Đã hiển thị",
                    PhanHoiAdmin = "Chào bạn, Admin ghi nhận góp ý và sẽ làm việc với bếp tàu để cải thiện thực đơn ạ!",
                    NgayPhanHoi = new DateTime(2026, 4, 12, 9, 15, 0)
                },
                new DanhGia
                {
                    MaDanhGia = 3,
                    MaHoaDon = 3, // Khớp với HoaDon 3
                    SoSao = 5,
                    NoiDung = "Đặt vé cực nhanh, thanh toán tiện lợi. Sẽ quay lại!",
                    HinhAnh = null, // Khách không gửi ảnh
                    NgayDanhGia = DateTime.Now.AddHours(-2),
                    TrangThai = "Chờ duyệt", // Đang đợi Admin kiểm duyệt
                    PhanHoiAdmin = null,
                    NgayPhanHoi = null
                }
            );
        }
    }
}