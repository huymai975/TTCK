using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebAppBookingBoat.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMai",
                columns: table => new
                {
                    MaKM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenChuongTrinh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhanTramGiam = table.Column<double>(type: "float", nullable: false),
                    SoTienToiDaGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMai", x => x.MaKM);
                    table.CheckConstraint("CK_KM_PhanTram", "[PhanTramGiam] >= 0 AND [PhanTramGiam] <= 100");
                    table.CheckConstraint("CK_KM_ThoiGian", "[NgayKetThuc] > [NgayBatDau]");
                });

            migrationBuilder.CreateTable(
                name: "Tau",
                columns: table => new
                {
                    MaTau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TongSoGhe = table.Column<int>(type: "int", nullable: false),
                    SoGheThuong = table.Column<int>(type: "int", nullable: false),
                    SoGheVIP = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tau", x => x.MaTau);
                    table.CheckConstraint("CK_Tau_TongGhe", "[TongSoGhe] = [SoGheThuong] + [SoGheVIP]");
                });

            migrationBuilder.CreateTable(
                name: "TuyenDuong",
                columns: table => new
                {
                    MaTuyen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTuyen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiemDi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiemDen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    KhoangCach = table.Column<double>(type: "float", nullable: false),
                    ThoiGianDuKien = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuyenDuong", x => x.MaTuyen);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKH = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.MaKH);
                    table.CheckConstraint("CK_KH_Email_Format", "[Email] LIKE '%_@_%._%'");
                    table.CheckConstraint("CK_KH_Sdt_Format", "LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'");
                    table.ForeignKey(
                        name: "FK_KhachHang_AspNetUsers_MaTK",
                        column: x => x.MaTK,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    MaLog = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BangTacDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoiDungChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.MaLog);
                    table.ForeignKey(
                        name: "FK_Log_AspNetUsers_MaTK",
                        column: x => x.MaTK,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChucVu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Luong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.MaNV);
                    table.CheckConstraint("CK_NV_Email_Format", "[Email] LIKE '%_@_%._%'");
                    table.CheckConstraint("CK_NV_Sdt_Format", "LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'");
                    table.ForeignKey(
                        name: "FK_NhanVien_AspNetUsers_MaTK",
                        column: x => x.MaTK,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ghe",
                columns: table => new
                {
                    MaGhe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTau = table.Column<int>(type: "int", nullable: false),
                    TenGhe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LoaiGhe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ghe", x => x.MaGhe);
                    table.CheckConstraint("CK_Ghe_LoaiGhe", "[LoaiGhe] IN (N'Thường', N'VIP')");
                    table.ForeignKey(
                        name: "FK_Ghe_Tau_MaTau",
                        column: x => x.MaTau,
                        principalTable: "Tau",
                        principalColumn: "MaTau",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichTrinh",
                columns: table => new
                {
                    MaLichTrinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTuyen = table.Column<int>(type: "int", nullable: false),
                    MaTau = table.Column<int>(type: "int", nullable: false),
                    NgayGioKhoiHanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayGioCapBenDuKien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GiaVeCoBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoGheTrong = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichTrinh", x => x.MaLichTrinh);
                    table.CheckConstraint("CK_LT_GheTrong", "[SoGheTrong] >= 0");
                    table.CheckConstraint("CK_LT_GiaVe", "[GiaVeCoBan] >= 0");
                    table.CheckConstraint("CK_LT_ThoiGian", "[NgayGioCapBenDuKien] > [NgayGioKhoiHanh]");
                    table.ForeignKey(
                        name: "FK_LichTrinh_Tau_MaTau",
                        column: x => x.MaTau,
                        principalTable: "Tau",
                        principalColumn: "MaTau",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichTrinh_TuyenDuong_MaTuyen",
                        column: x => x.MaTuyen,
                        principalTable: "TuyenDuong",
                        principalColumn: "MaTuyen",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKH = table.Column<int>(type: "int", nullable: false),
                    MaNV = table.Column<int>(type: "int", nullable: true),
                    MaKM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuongVe = table.Column<int>(type: "int", nullable: false),
                    TamTinh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoTienGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThucTT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.MaHoaDon);
                    table.CheckConstraint("CK_HD_SoLuong", "[SoLuongVe] > 0");
                    table.CheckConstraint("CK_HD_Tien", "[TamTinh] >= 0 AND [SoTienGiam] >= 0 AND [TongTien] >= 0");
                    table.ForeignKey(
                        name: "FK_HoaDon_KhachHang_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDon_KhuyenMai_MaKM",
                        column: x => x.MaKM,
                        principalTable: "KhuyenMai",
                        principalColumn: "MaKM",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDon_NhanVien_MaNV",
                        column: x => x.MaNV,
                        principalTable: "NhanVien",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ve",
                columns: table => new
                {
                    MaVe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<int>(type: "int", nullable: false),
                    MaLichTrinh = table.Column<int>(type: "int", nullable: false),
                    GiaVe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KhachHangMaKH = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ve", x => x.MaVe);
                    table.ForeignKey(
                        name: "FK_Ve_HoaDon_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "MaHoaDon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ve_KhachHang_KhachHangMaKH",
                        column: x => x.KhachHangMaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ve_LichTrinh_MaLichTrinh",
                        column: x => x.MaLichTrinh,
                        principalTable: "LichTrinh",
                        principalColumn: "MaLichTrinh",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaVe = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LichTrinhMaLichTrinh = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.CheckConstraint("CK_DG_SoSao", "[SoSao] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_DG_TrangThai", "[TrangThai] IN (N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn')");
                    table.ForeignKey(
                        name: "FK_DanhGia_LichTrinh_LichTrinhMaLichTrinh",
                        column: x => x.LichTrinhMaLichTrinh,
                        principalTable: "LichTrinh",
                        principalColumn: "MaLichTrinh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGia_Ve_MaVe",
                        column: x => x.MaVe,
                        principalTable: "Ve",
                        principalColumn: "MaVe",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "0d561bc1-98aa-4438-aadd-7e73fba2f7f9", 0, "3feed63a-9fdb-407d-acb6-316facbd57f2", "huymai@gmail.com", true, false, null, "HUYMAI@GMAIL.COM", "HUYMAI", "AQAAAAIAAYagAAAAEErDT+LWptpZyX7Uox4L1UOvVK0JmE500yI/Bfo50Dr+jqZNPsEZpATTWqu7bu36vw==", null, false, "365c5993-ab44-4eee-86d2-bc39cab4be28", false, "huymai" },
                    { "0e167202-57bc-4753-90e7-4d251a85c743", 0, "3a468631-2bec-4973-b20a-af377e000ac6", "admin@booking.com", true, false, null, "ADMIN@BOOKING.COM", "ADMIN", "AQAAAAIAAYagAAAAEDNsGrDXpmBh72FxKikC4lqXVK0NglivsTNJvhEl5KTyFosfZhbXzdgBI6ySFZCLuw==", null, false, "1641b3b8-6d80-4f46-a638-29a4aaa093a3", false, "admin" },
                    { "21a63afc-f2da-445c-b426-2c1fb9178af0", 0, "4e824a5a-544d-499d-a982-323653a7894b", "khachhang2@gmail.com", true, false, null, "KHACHHANG2@GMAIL.COM", "KHACHHANG2", "AQAAAAIAAYagAAAAEPs2Pi/Rf8LATsXQ6Kik2m6mLzbHWJsPOemXrn9FUjMgEErSQaOMkucI/tWtjYbD9A==", null, false, "0ecfda33-003e-407f-86b5-a791c061cf17", false, "khachhang2" },
                    { "2d73a84e-3ddf-4715-b93b-88cbfd434144", 0, "bd27d904-e618-457c-9726-9cd58a9982d4", "testuser@gmail.com", true, false, null, "TESTUSER@GMAIL.COM", "TESTUSER", "AQAAAAIAAYagAAAAEJb9DUhdu8ul0EK5M5U+o+AhV2ewG2Yn9RHXc4BwR6nfZCnqu2iaFK5UI6xRLq3YsQ==", null, false, "42d25828-ee47-4603-82a0-d5c32efe49ec", false, "testuser" },
                    { "3ba4a018-0d5e-41ea-8e9b-fd7119cd8138", 0, "69bd096b-839f-476d-b4ca-6b4f7c70b45f", "khachhang1@gmail.com", true, false, null, "KHACHHANG1@GMAIL.COM", "KHACHHANG1", "AQAAAAIAAYagAAAAEO+888ASBqI2m1rsgUqPmpF82R18pqJWN7/kEyopnnZkWmtJpfBl9HDBEHFr26QVDg==", null, false, "b707dd05-8ffe-4c38-aef1-2305d1b58058", false, "khachhang1" }
                });

            migrationBuilder.InsertData(
                table: "KhuyenMai",
                columns: new[] { "MaKM", "NgayBatDau", "NgayKetThuc", "PhanTramGiam", "SoTienToiDaGiam", "TenChuongTrinh", "TrangThai" },
                values: new object[,]
                {
                    { "KM10", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, 50000m, "Giảm giá khai trương", true },
                    { "SUMMER26", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, 100000m, "Ưu đãi mùa hè", true }
                });

            migrationBuilder.InsertData(
                table: "Tau",
                columns: new[] { "MaTau", "HinhAnh", "SoGheThuong", "SoGheVIP", "TenTau", "TongSoGhe", "TrangThai" },
                values: new object[,]
                {
                    { 1, "tau01.jpg", 15, 5, "Tàu Cao Tốc 01", 20, true },
                    { 2, "tau02.jpg", 15, 5, "Tàu Express 02", 20, true }
                });

            migrationBuilder.InsertData(
                table: "TuyenDuong",
                columns: new[] { "MaTuyen", "DiemDen", "DiemDi", "HinhAnh", "KhoangCach", "TenTuyen", "ThoiGianDuKien" },
                values: new object[,]
                {
                    { 1, "Vũng Tàu", "Sài Gòn", "vungtau.jpg", 100.0, "Sài Gòn - Vũng Tàu", new TimeSpan(0, 2, 30, 0, 0) },
                    { 2, "Phú Quốc", "Rạch Giá", "phuquoc.jpg", 120.0, "Rạch Giá - Phú Quốc", new TimeSpan(0, 2, 45, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Ghe",
                columns: new[] { "MaGhe", "LoaiGhe", "MaTau", "TenGhe" },
                values: new object[,]
                {
                    { 1, "Thường", 1, "T-01" },
                    { 2, "Thường", 1, "T-02" },
                    { 3, "Thường", 1, "T-03" },
                    { 4, "Thường", 1, "T-04" },
                    { 5, "Thường", 1, "T-05" },
                    { 6, "Thường", 1, "T-06" },
                    { 7, "Thường", 1, "T-07" },
                    { 8, "Thường", 1, "T-08" },
                    { 9, "Thường", 1, "T-09" },
                    { 10, "Thường", 1, "T-10" },
                    { 11, "Thường", 1, "T-11" },
                    { 12, "Thường", 1, "T-12" },
                    { 13, "Thường", 1, "T-13" },
                    { 14, "Thường", 1, "T-14" },
                    { 15, "Thường", 1, "T-15" },
                    { 16, "VIP", 1, "V-16" },
                    { 17, "VIP", 1, "V-17" },
                    { 18, "VIP", 1, "V-18" },
                    { 19, "VIP", 1, "V-19" },
                    { 20, "VIP", 1, "V-20" },
                    { 21, "Thường", 2, "T-01" },
                    { 22, "Thường", 2, "T-02" },
                    { 23, "Thường", 2, "T-03" },
                    { 24, "Thường", 2, "T-04" },
                    { 25, "Thường", 2, "T-05" },
                    { 26, "Thường", 2, "T-06" },
                    { 27, "Thường", 2, "T-07" },
                    { 28, "Thường", 2, "T-08" },
                    { 29, "Thường", 2, "T-09" },
                    { 30, "Thường", 2, "T-10" },
                    { 31, "Thường", 2, "T-11" },
                    { 32, "Thường", 2, "T-12" },
                    { 33, "Thường", 2, "T-13" },
                    { 34, "Thường", 2, "T-14" },
                    { 35, "Thường", 2, "T-15" },
                    { 36, "VIP", 2, "V-16" },
                    { 37, "VIP", 2, "V-17" },
                    { 38, "VIP", 2, "V-18" },
                    { 39, "VIP", 2, "V-19" },
                    { 40, "VIP", 2, "V-20" }
                });

            migrationBuilder.InsertData(
                table: "KhachHang",
                columns: new[] { "MaKH", "DiaChi", "Email", "HoTen", "MaTK", "NgaySinh", "Sdt" },
                values: new object[] { 1, null, "khach.tran@gmail.com", "Trần Thị Khách", "0e167202-57bc-4753-90e7-4d251a85c743", new DateTime(1995, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0912345678" });

            migrationBuilder.InsertData(
                table: "LichTrinh",
                columns: new[] { "MaLichTrinh", "GiaVeCoBan", "MaTau", "MaTuyen", "NgayGioCapBenDuKien", "NgayGioKhoiHanh", "SoGheTrong", "TrangThai" },
                values: new object[] { 1, 200000m, 1, 1, new DateTime(2026, 4, 10, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 10, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" });

            migrationBuilder.InsertData(
                table: "Log",
                columns: new[] { "MaLog", "BangTacDong", "HanhDong", "MaTK", "NoiDungChiTiet", "ThoiGian" },
                values: new object[] { 1, "Hệ thống", "Khởi tạo hệ thống", "0e167202-57bc-4753-90e7-4d251a85c743", "Seed dữ liệu mẫu thành công", new DateTime(2026, 4, 9, 22, 34, 27, 829, DateTimeKind.Local).AddTicks(6742) });

            migrationBuilder.InsertData(
                table: "NhanVien",
                columns: new[] { "MaNV", "ChucVu", "Email", "HoTen", "Luong", "MaTK", "Sdt", "TrangThai" },
                values: new object[] { 1, "Bán vé", "chay.nv@boat.com", "Nguyễn Văn Chạy", 0m, "0e167202-57bc-4753-90e7-4d251a85c743", "0987654321", true });

            migrationBuilder.InsertData(
                table: "HoaDon",
                columns: new[] { "MaHoaDon", "MaKH", "MaKM", "MaNV", "NgayLap", "PhuongThucTT", "SoLuongVe", "SoTienGiam", "TamTinh", "TongTien", "TrangThai" },
                values: new object[] { 1, 1, "KM10", 1, new DateTime(2026, 4, 9, 22, 34, 27, 829, DateTimeKind.Local).AddTicks(6607), "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" });

            migrationBuilder.InsertData(
                table: "Ve",
                columns: new[] { "MaVe", "GiaVe", "KhachHangMaKH", "MaHoaDon", "MaLichTrinh", "TrangThai" },
                values: new object[] { 1, 180000m, null, 1, 1, "Hợp lệ" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_LichTrinhMaLichTrinh",
                table: "DanhGia",
                column: "LichTrinhMaLichTrinh");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaVe",
                table: "DanhGia",
                column: "MaVe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ghe_MaTau_TenGhe",
                table: "Ghe",
                columns: new[] { "MaTau", "TenGhe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaKH",
                table: "HoaDon",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaKM",
                table: "HoaDon",
                column: "MaKM");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaNV",
                table: "HoaDon",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_Email",
                table: "KhachHang",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_MaTK",
                table: "KhachHang",
                column: "MaTK",
                unique: true,
                filter: "[MaTK] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_Sdt",
                table: "KhachHang",
                column: "Sdt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinh_MaTau_NgayGioKhoiHanh",
                table: "LichTrinh",
                columns: new[] { "MaTau", "NgayGioKhoiHanh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinh_MaTuyen",
                table: "LichTrinh",
                column: "MaTuyen");

            migrationBuilder.CreateIndex(
                name: "IX_Log_MaTK",
                table: "Log",
                column: "MaTK");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_Email",
                table: "NhanVien",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaTK",
                table: "NhanVien",
                column: "MaTK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_Sdt",
                table: "NhanVien",
                column: "Sdt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tau_TenTau",
                table: "Tau",
                column: "TenTau",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuyenDuong_DiemDi_DiemDen",
                table: "TuyenDuong",
                columns: new[] { "DiemDi", "DiemDen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ve_KhachHangMaKH",
                table: "Ve",
                column: "KhachHangMaKH");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaHoaDon",
                table: "Ve",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaLichTrinh",
                table: "Ve",
                column: "MaLichTrinh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "Ghe");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Ve");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "LichTrinh");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "KhuyenMai");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "Tau");

            migrationBuilder.DropTable(
                name: "TuyenDuong");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
