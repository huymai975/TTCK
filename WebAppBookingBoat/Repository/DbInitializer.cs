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
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423169/WebAppBookingBoat/KhuyenMai/mqzylxgtpe7afhspqnr4.jpg",
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
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423185/WebAppBookingBoat/KhuyenMai/ezhwyslezlz3rfcfanzj.jpg",
                    MoTa = "Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 10% khi đặt vé.",
                    PhanTramGiam = 10,
                    SoTienToiDaGiam = 100000,
                    NgayBatDau = new DateTime(2026, 6, 1),
                    NgayKetThuc = new DateTime(2026, 8, 31),
                    TrangThai = "Chưa diễn ra"
                },
                new KhuyenMai
                {
                    MaKM = "SUMMER27",
                    TenChuongTrinh = "Ưu đãi mùa hè hết cỡ",
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423198/WebAppBookingBoat/KhuyenMai/b6zrdnmmywdeqr09k7y4.jpg",
                    MoTa = "Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 30% khi đặt vé.",
                    PhanTramGiam = 30,
                    SoTienToiDaGiam = 100000,
                    NgayBatDau = new DateTime(2026, 6, 1),
                    NgayKetThuc = new DateTime(2026, 8, 31),
                    TrangThai = "Chưa diễn ra"
                },
                new KhuyenMai
                {
                    MaKM = "TET2026",
                    TenChuongTrinh = "Vui Tết sum vầy",
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423230/WebAppBookingBoat/KhuyenMai/o1sh0nzmy24brnrriluo.jpg",
                    MoTa = "Chương trình khuyến mãi đặc biệt dành cho khách hàng về quê ăn Tết hoặc du xuân cùng gia đình.",
                    PhanTramGiam = 30,
                    SoTienToiDaGiam = 300000,
                    NgayBatDau = new DateTime(2026, 1, 15),
                    NgayKetThuc = new DateTime(2026, 2, 15),
                    TrangThai = "Chưa diễn ra"
                },
                new KhuyenMai
                {
                    MaKM = "GIADO304",
                    TenChuongTrinh = "Mừng Đại Lễ - Giảm Giá Mê",
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423157/WebAppBookingBoat/KhuyenMai/va9cgbewyqdxcfn3ggna.jpg",
                    MoTa = "Ưu đãi cực lớn dành cho các tuyến tàu cao tốc du lịch trong kỳ nghỉ lễ 30/4 và 1/5.",
                    PhanTramGiam = 30,
                    SoTienToiDaGiam = 400000,
                    NgayBatDau = new DateTime(2026, 4, 25),
                    NgayKetThuc = new DateTime(2026, 5, 5),
                    TrangThai = "Sắp diễn ra"
                },
                new KhuyenMai
                {
                    MaKM = "DONGAM2026",
                    TenChuongTrinh = "Mùa Đông Ấm Áp",
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423145/WebAppBookingBoat/KhuyenMai/hejcsoy1vxd8ooftqlqf.jpg",
                    MoTa = "Ưu đãi sưởi ấm những chuyến đi cuối năm. Giảm giá sâu cho các tuyến tàu ra đảo nghỉ dưỡng.",
                    PhanTramGiam = 25,
                    SoTienToiDaGiam = 250000,
                    NgayBatDau = new DateTime(2026, 11, 1),
                    NgayKetThuc = new DateTime(2026, 12, 25),
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

            var user2Id = Guid.NewGuid().ToString();
            var user2 = new AppUser
            {
                Id = user2Id,
                UserName = "khachhang1",
                Email = "khachhang1@gmail.com",

                NormalizedUserName = "KHACHHANG1",
                NormalizedEmail = "KHACHHANG1@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user2.PasswordHash = hasher.HashPassword(user2, "1234");

            var user3Id = Guid.NewGuid().ToString();
            var user3 = new AppUser
            {
                Id = user3Id,
                UserName = "khachhang2",
                Email = "khachhang2@gmail.com",

                NormalizedUserName = "KHACHHANG2",
                NormalizedEmail = "KHACHHANG2@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user3.PasswordHash = hasher.HashPassword(user3, "1234");

            var user4Id = Guid.NewGuid().ToString();
            var user4 = new AppUser
            {
                Id = user4Id,
                UserName = "khachhang3",
                Email = "khachhang3@gmail.com",

                NormalizedUserName = "KHACHHANG3",
                NormalizedEmail = "KHACHHANG3@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user4.PasswordHash = hasher.HashPassword(user4, "1234");

            var user6Id = Guid.NewGuid().ToString();
            var user6 = new AppUser
            {
                Id = user6Id,
                UserName = "khachhang4",
                Email = "khachhang4@gmail.com",

                NormalizedUserName = "KHACHHANG4",
                NormalizedEmail = "KHACHHANG4@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user6.PasswordHash = hasher.HashPassword(user6, "1234");

            var user5Id = Guid.NewGuid().ToString();
            var user5 = new AppUser
            {
                Id = user5Id,
                UserName = "nhanvien1",
                Email = "nhanvien1@booking.com",

                NormalizedUserName = "NHANVIEN1",
                NormalizedEmail = "NHANVIEN1@BOOKING.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                TrangThai = true
            };
            user5.PasswordHash = hasher.HashPassword(user5, "1234");

            // Đưa vào Database
            modelBuilder.Entity<AppUser>().HasData(
                adminUser, user2, user3, user4, user5, user6
            );


            // ---  SEED ROLE ---


            // THAY ĐỔI: Cấu hình ID theo thứ tự mới
            string adminRoleId = "1";
            string staffRoleId = "2";
            string customerRoleId = "3";

            // Seed Roles vào bảng AspNetRoles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = staffRoleId, Name = "Staff", NormalizedName = "STAFF" },
                new IdentityRole { Id = customerRoleId, Name = "Customer", NormalizedName = "CUSTOMER" }
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
                UserId = user5.Id
            });

            // Gán quyền Khách hàng (RoleId = 3) 
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = customerRoleId,
                UserId = user2.Id
            },
            new IdentityUserRole<string>
            {
                RoleId = customerRoleId,
                UserId = user3.Id
            },
            new IdentityUserRole<string>
            {
                RoleId = customerRoleId,
                UserId = user4.Id
            },
            new IdentityUserRole<string>
            {
                RoleId = customerRoleId,
                UserId = user6.Id
            }

                );


            //--- 3. SEED NHÂN VIÊN ---
            modelBuilder.Entity<NhanVien>().HasData(
                new NhanVien { MaNV = 1, MaTK = adminUser.Id, HoTen = "Mai Nhứt Huy", Email = "maihuy@booking.com", Sdt = "0386747090", ChucVu = "Admin" },
                new NhanVien { MaNV = 2, MaTK = user5.Id, HoTen = "Jerry", Email = "jerry@booking.com", Sdt = "0386747091", ChucVu = "Nhân viên" }
            );

            // --- 4. SEED KHÁCH HÀNG ---
            modelBuilder.Entity<KhachHang>().HasData(
                new KhachHang { MaKH = 1, MaTK = user2Id, HoTen = "Trần Thị Khách", Email = "khach.tran@gmail.com", Sdt = "0912345678", NgaySinh = new DateTime(1995, 5, 20) },
                new KhachHang { MaKH = 2, MaTK = user3Id, HoTen = "Nguyễn Thị Khách", Email = "khach.nguyen@gmail.com", Sdt = "0912345679", NgaySinh = new DateTime(1995, 5, 20) },
                new KhachHang { MaKH = 3, MaTK = user4Id, HoTen = "Lê Thị Khách", Email = "khach.le@gmail.com", Sdt = "0912345676", NgaySinh = new DateTime(1995, 5, 20) },
                new KhachHang { MaKH = 4, MaTK = user6Id, HoTen = "Đỗ Thị Khách", Email = "khach.do@gmail.com", Sdt = "0912345675", NgaySinh = new DateTime(1995, 5, 20) }
            );

            // --- 5. SEED TUYẾN ĐƯỜNG ---
            modelBuilder.Entity<TuyenDuong>().HasData(
                new TuyenDuong
                {
                    MaTuyen = 1,
                    TenTuyen = "Phan Thiết - Phú Quý",
                    DiemDi = "Phan Thiết",
                    DiemDen = "Phú Quý",
                    KhoangCach = 105,
                    ThoiGianDuKien = new TimeSpan(2, 30, 0),
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420178/WebAppBookingBoat/TuyenDuongs/q14qzet524aqqms6kcda.jpg"
                },
                new TuyenDuong
                {
                    MaTuyen = 2,
                    TenTuyen = "Hải Phòng - Cát Bà",
                    DiemDi = "Hải Phòng",
                    DiemDen = "Cát Bà",
                    KhoangCach = 30,
                    ThoiGianDuKien = new TimeSpan(0, 45, 0),
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420166/WebAppBookingBoat/TuyenDuongs/vzapbdbl0im8kpkm5f2m.jpg"
                },
                new TuyenDuong
                {
                    MaTuyen = 3,
                    TenTuyen = "Vũng Tàu - Côn Đảo",
                    DiemDi = "Vũng Tàu",
                    DiemDen = "Côn Đảo",
                    KhoangCach = 180,
                    ThoiGianDuKien = new TimeSpan(3, 45, 0),
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420156/WebAppBookingBoat/TuyenDuongs/vnfyfqqtnqvqg7hrxbha.jpg"
                },
                new TuyenDuong
                {
                    MaTuyen = 4,
                    TenTuyen = "Rạch Giá - Hòn Sơn",
                    DiemDi = "Rạch Giá",
                    DiemDen = "Hòn Sơn",
                    KhoangCach = 65,
                    ThoiGianDuKien = new TimeSpan(1, 30, 0),
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420134/WebAppBookingBoat/TuyenDuongs/syeveinoy2fsukilmuqr.jpg"
                },
                new TuyenDuong
                {
                    MaTuyen = 5,
                    TenTuyen = "Sa Kỳ - Lý Sơn",
                    DiemDi = "Sa Kỳ",
                    DiemDen = "Lý Sơn",
                    KhoangCach = 30,
                    ThoiGianDuKien = new TimeSpan(0, 45, 0),
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420123/WebAppBookingBoat/TuyenDuongs/ghxji0xxpn91dggqgypn.jpg"
                },
                new TuyenDuong
                {
                    MaTuyen = 6,
                    TenTuyen = "Rạch Giá - Nam Du",
                    DiemDi = "Rạch Giá",
                    DiemDen = "Nam Du",
                    KhoangCach = 80,
                    ThoiGianDuKien = new TimeSpan(2, 15, 0),
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420114/WebAppBookingBoat/TuyenDuongs/ijtbq0xjjranxskeawrg.jpg"
                },
                new TuyenDuong
                {
                    MaTuyen = 7,
                    TenTuyen = "Hà Tiên - Phú Quốc",
                    DiemDi = "Hà Tiên",
                    DiemDen = "Phú Quốc",
                    KhoangCach = 45,
                    ThoiGianDuKien = new TimeSpan(1, 15, 0),
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420103/WebAppBookingBoat/TuyenDuongs/bfocircuvbvg36neglex.jpg"
                }
            );

            // --- 6. SEED TÀU ---
            modelBuilder.Entity<Tau>().HasData(
                new Tau
                {
                    MaTau = 1,
                    TenTau = "Phú Quốc Express 1",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395965/WebAppBookingBoat/Taus/lavhtb0wfmiqxaest57x.jpg"
                },
                new Tau
                {
                    MaTau = 2,
                    TenTau = "Phú Quốc Express 2",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg"
                },
                new Tau
                {
                    MaTau = 3,
                    TenTau = "Phú Quốc Express 3",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395947/WebAppBookingBoat/Taus/kc65kynwwjci9tg2tbf0.jpg"
                },
                new Tau
                {
                    MaTau = 4,
                    TenTau = "Phú Quốc Express 4",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777419297/WebAppBookingBoat/Taus/qnizabxmp2xhxhm5mqki.jpg"
                },
                new Tau
                {
                    MaTau = 5,
                    TenTau = "Phú Quốc Express 5",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395965/WebAppBookingBoat/Taus/lavhtb0wfmiqxaest57x.jpg"
                },
                new Tau
                {
                    MaTau = 6,
                    TenTau = "Phú Quốc Express 6",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg"
                },
                new Tau
                {
                    MaTau = 7,
                    TenTau = "Phú Quốc Express 7",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg"
                },
                new Tau
                {
                    MaTau = 8,
                    TenTau = "Phú Quốc Express 8",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg"
                },
                new Tau
                {
                    MaTau = 9,
                    TenTau = "Phú Quốc Express 9",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg"
                },
                new Tau
                {
                    MaTau = 10,
                    TenTau = "Phú Quốc Express 10",
                    TongSoGhe = 20,
                    TrangThai = true,
                    HinhAnh = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg"
                }
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
                },
                new LichTrinh
                {
                    MaLichTrinh = 2,
                    MaTuyen = 2,
                    MaTau = 2,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(1).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(1).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 3,
                    MaTuyen = 3,
                    MaTau = 3,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(2).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(2).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 4,
                    MaTuyen = 4,
                    MaTau = 4,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(4).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(4).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 5,
                    MaTuyen = 5,
                    MaTau = 5,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(8).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(8).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 6,
                    MaTuyen = 6,
                    MaTau = 6,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(10).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(10).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 7,
                    MaTuyen = 7,
                    MaTau = 1,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(13).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(13).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 8,
                    MaTuyen = 1,
                    MaTau = 2,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(13).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(13).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 9,
                    MaTuyen = 2,
                    MaTau = 3,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(14).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(14).Date.AddHours(10).AddMinutes(30),
                    GiaVeCoBan = 200000,
                    SoGheTrong = 20,
                    TrangThai = "Sắp khởi hành"
                }, new LichTrinh
                {
                    MaLichTrinh = 10,
                    MaTuyen = 3,
                    MaTau = 4,
                    NgayGioKhoiHanh = DateTime.Now.AddDays(16).Date.AddHours(8), // 8h sáng mai
                    NgayGioCapBenDuKien = DateTime.Now.AddDays(16).Date.AddHours(10).AddMinutes(30),
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
        MaKH = 4,
        MaNV = 1,
        MaKM = "KM10",
        NgayLap = DateTime.Now,
        SoLuongVe = 1,
        TamTinh = 200000,
        SoTienGiam = 20000,
        TongTien = 180000,
        PhuongThucTT = "Tiền mặt",
        TrangThai = "Đã thanh toán"
    },
    new HoaDon
    {
        MaHoaDon = 2,
        MaKH = 2,
        MaNV = 1,
        MaKM = "KM10",
        NgayLap = DateTime.Now,
        SoLuongVe = 1,
        TamTinh = 200000,
        SoTienGiam = 20000,
        TongTien = 180000,
        PhuongThucTT = "Tiền mặt",
        TrangThai = "Đã thanh toán"
    },
    new HoaDon
    {
        MaHoaDon = 3,
        MaKH = 3,
        MaNV = 1,
        MaKM = "KM10",
        NgayLap = DateTime.Now,
        SoLuongVe = 1,
        TamTinh = 200000,
        SoTienGiam = 20000,
        TongTien = 180000,
        PhuongThucTT = "Tiền mặt",
        TrangThai = "Đã thanh toán"
    },
    new HoaDon
    {
        MaHoaDon = 4,
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
    new Ve { MaVe = 1, MaGhe = 21, MaHoaDon = 1, MaLichTrinh = 2, GiaVe = 180000, TrangThai = "Hợp lệ" },
    new Ve { MaVe = 2, MaGhe = 3, MaHoaDon = 2, MaLichTrinh = 1, GiaVe = 180000, TrangThai = "Hợp lệ" },
    new Ve { MaVe = 3, MaGhe = 4, MaHoaDon = 3, MaLichTrinh = 1, GiaVe = 180000, TrangThai = "Hợp lệ" },
    new Ve { MaVe = 4, MaGhe = 26, MaHoaDon = 4, MaLichTrinh = 2, GiaVe = 180000, TrangThai = "Hợp lệ" }
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
        MaHoaDon = 1,
        SoSao = 5,
        NoiDung = "Chuyến đi tuyệt vời, tàu chạy rất êm và đúng giờ. Nhân viên hỗ trợ nhiệt tình!",
        HinhAnh = "6a4b2c8d-1e5f-4a3b-9c2d-8e7f6a5b4c3d_review-phu-quoc.jpg",
        NgayDanhGia = new DateTime(2026, 4, 10, 8, 30, 0),
        TrangThai = "Đã hiển thị",
        PhanHoiAdmin = "Cảm ơn bạn đã ủng hộ WebAppBookingBoat! Rất mong được phục vụ bạn trong những chuyến đi tới.",
        NgayPhanHoi = new DateTime(2026, 4, 10, 14, 0, 0)
    },
    new DanhGia
    {
        MaDanhGia = 2,
        MaHoaDon = 2,
        SoSao = 4,
        NoiDung = "Chất lượng ghế VIP rất tốt, tuy nhiên đồ ăn nhẹ trên tàu hơi ít lựa chọn.",
        HinhAnh = "8e7d6c5b-4a3f-4e2d-9c1b-0a9b8c7d6e5f_review-thang-long.jpg",
        NgayDanhGia = new DateTime(2026, 4, 11, 15, 20, 0),
        TrangThai = "Đã hiển thị",
        PhanHoiAdmin = "Chào bạn, Admin ghi nhận góp ý và sẽ làm việc với bếp tàu để cải thiện thực đơn ạ!",
        NgayPhanHoi = new DateTime(2026, 4, 12, 9, 15, 0)
    },
    new DanhGia
    {
        MaDanhGia = 3,
        MaHoaDon = 3,
        SoSao = 5,
        NoiDung = "Đặt vé cực nhanh, thanh toán tiện lợi. Sẽ quay lại!",
        HinhAnh = "2c3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5e6f_view-bien.jpg",
        NgayDanhGia = DateTime.Now.AddHours(-2),
        TrangThai = "Chờ duyệt",
        PhanHoiAdmin = null,
        NgayPhanHoi = null
    },
    new DanhGia
    {
        MaDanhGia = 4,
        MaHoaDon = 4,
        SoSao = 5,
        NoiDung = "Gia đình mình đi tuyến Hà Tiên - Phú Quốc rất hài lòng...",
        HinhAnh = "5f4e3d2c-1b0a-4c9d-8e7f-6a5b4c3d2e1f_tau-phu-quy.jpg",
        NgayDanhGia = DateTime.Now,
        TrangThai = "Chờ duyệt",
        PhanHoiAdmin = null,
        NgayPhanHoi = null
    }
    );
        }
    }
}