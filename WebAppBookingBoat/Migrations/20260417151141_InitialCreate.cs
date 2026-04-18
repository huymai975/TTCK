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
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
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
                    HinhAnh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PhanTramGiam = table.Column<double>(type: "float", nullable: false),
                    SoTienToiDaGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMai", x => x.MaKM);
                    table.CheckConstraint("CK_KM_PhanTram", "[PhanTramGiam] >= 0 AND [PhanTramGiam] <= 100");
                    table.CheckConstraint("CK_KM_SoTienToiDa", "[SoTienToiDaGiam] >= 0");
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
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tau", x => x.MaTau);
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
                    table.CheckConstraint("CK_TD_DiemKhacNhau", "[DiemDi] <> [DiemDen]");
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
                name: "Logs",
                columns: table => new
                {
                    MaLog = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LoaiLog = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BangTacDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoiDungChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.MaLog);
                    table.CheckConstraint("CK_Log_Loai", "[LoaiLog] IN ('Info', 'Warning', 'Error', 'Critical')");
                    table.ForeignKey(
                        name: "FK_Logs_AspNetUsers_MaTK",
                        column: x => x.MaTK,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.CheckConstraint("CK_LT_TrangThai", "[TrangThai] IN (N'Sắp khởi hành', N'Đang vận hành', N'Hoàn thành', N'Đã hủy')");
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
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoLuongVe = table.Column<int>(type: "int", nullable: false),
                    TamTinh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoTienGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThucTT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                name: "DanhGia",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhanHoiAdmin = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NgayPhanHoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoaDonMaHoaDon = table.Column<int>(type: "int", nullable: true),
                    LichTrinhMaLichTrinh = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.CheckConstraint("CK_DG_NgayPhanHoi", "[NgayPhanHoi] IS NULL OR [NgayPhanHoi] >= [NgayDanhGia]");
                    table.CheckConstraint("CK_DG_SoSao", "[SoSao] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_DG_TrangThai", "[TrangThai] IN (N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn')");
                    table.ForeignKey(
                        name: "FK_DanhGia_HoaDon_HoaDonMaHoaDon",
                        column: x => x.HoaDonMaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "MaHoaDon");
                    table.ForeignKey(
                        name: "FK_DanhGia_HoaDon_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "MaHoaDon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGia_LichTrinh_LichTrinhMaLichTrinh",
                        column: x => x.LichTrinhMaLichTrinh,
                        principalTable: "LichTrinh",
                        principalColumn: "MaLichTrinh",
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
                    MaGhe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ve", x => x.MaVe);
                    table.CheckConstraint("CK_Ve_GiaVe", "[GiaVe] >= 0");
                    table.CheckConstraint("CK_Ve_TrangThai", "[TrangThai] IN (N'Đang chờ', N'Hợp lệ', N'Đã hủy')");
                    table.ForeignKey(
                        name: "FK_Ve_Ghe_MaGhe",
                        column: x => x.MaGhe,
                        principalTable: "Ghe",
                        principalColumn: "MaGhe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ve_HoaDon_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "MaHoaDon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ve_LichTrinh_MaLichTrinh",
                        column: x => x.MaLichTrinh,
                        principalTable: "LichTrinh",
                        principalColumn: "MaLichTrinh",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, "Admin", "ADMIN" },
                    { "2", null, "Staff", "STAFF" },
                    { "3", null, "Customer", "CUSTOMER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TrangThai", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "711c81c5-997b-4ad1-824d-00a0e4237029", 0, "fb1a1ad1-1e26-4922-bb49-a681fc01ed30", "khachhang1@gmail.com", true, false, null, "KHACHHANG1@GMAIL.COM", "KHACHHANG1", "AQAAAAIAAYagAAAAEM9+bYZRzcU+dnCRpUQHItrpsS1gk9yELh79T+ZFRcVCnVMr8MTCBKU+ocW6YW8yWw==", null, false, "bcf47bd2-8b4b-448b-b00c-600b43bbfc35", true, false, "khachhang1" },
                    { "7667fed0-c6f2-4dff-a12b-f4c5b064f9f2", 0, "ce5f96d5-d52f-4039-823c-03cc9bdf85e1", "nhanvien1@booking.com", true, false, null, "NHANVIEN1@BOOKING.COM", "NHANVIEN1", "AQAAAAIAAYagAAAAECayK2xNLqsc1zsvb73kbhS/nDkAY1I3PdRbh3pzXXBAD/mxGz0KgYsBd6AkyhQA3Q==", null, false, "a652bf4d-9407-4c99-8465-b148e2e74f8d", true, false, "nhanvien1" },
                    { "7d328812-88db-422a-b83e-14d56dd91dde", 0, "a3a1c257-08d9-4465-ab34-9f8d35edcf54", "khachhang2@gmail.com", true, false, null, "KHACHHANG2@GMAIL.COM", "KHACHHANG2", "AQAAAAIAAYagAAAAEHYWSeke9O9rJZSsYcx6+eOZ4RVuYO+Un1UODBRIE57gLfTk79Iiuu6yP7zbeY2CNA==", null, false, "4d08d3d4-341d-40d2-8a7c-ecf6e93a2430", true, false, "khachhang2" },
                    { "8e3ade7b-c691-4276-abd5-3e1fb0f02a05", 0, "b4835b6a-3669-48ce-8496-69ede6e1aa1e", "testuser@gmail.com", true, false, null, "TESTUSER@GMAIL.COM", "TESTUSER", "AQAAAAIAAYagAAAAEDsoNw73Xg+hXDD1Fm8EMpLANvMCkU6oXJj+37wryxCNKpl6mpzJZC9oRUnizM2qgQ==", null, false, "10249c4c-755a-48e9-92b9-46b4cf9fcd3a", true, false, "testuser" },
                    { "c4b3adc5-a6a7-4f7e-bd1c-2806c328191b", 0, "d384f5bf-81ff-4ec0-a2aa-4377f3d94e92", "admin@booking.com", true, false, null, "ADMIN@BOOKING.COM", "ADMIN", "AQAAAAIAAYagAAAAEHJXYAQhZDUNBWTNcgkCaOtc0r/eWeEjYGwJuD+g6pA9VwSNbM2RdpuZGFyJEIKi4g==", null, false, "dbabf772-6b59-46e1-b9b9-fa8cdb51540c", true, false, "admin" }
                });

            migrationBuilder.InsertData(
                table: "KhuyenMai",
                columns: new[] { "MaKM", "HinhAnh", "MoTa", "NgayBatDau", "NgayKetThuc", "PhanTramGiam", "SoTienToiDaGiam", "TenChuongTrinh", "TrangThai" },
                values: new object[,]
                {
                    { "DONGAM2026", "215b54dc-a2eb-4d82-8091-e4e885928754.jpg", "Ưu đãi sưởi ấm những chuyến đi cuối năm. Giảm giá sâu cho các tuyến tàu ra đảo nghỉ dưỡng.", new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 25.0, 250000m, "Mùa Đông Ấm Áp", "Chưa diễn ra" },
                    { "GIADO304", "a64e7e7d-4495-4950-b2ec-ec62db4cfdbb.jpg", "Ưu đãi cực lớn dành cho các tuyến tàu cao tốc du lịch trong kỳ nghỉ lễ 30/4 và 1/5.", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 30.0, 400000m, "Mừng Đại Lễ - Giảm Giá Mê", "Sắp diễn ra" },
                    { "KM10", "322b0521-7bf6-40e5-aeea-b28fcfb0c5fd.jpg", "Chào mừng hệ thống WebAppBookingBoat đi vào hoạt động. Giảm ngay 50% cho tất cả các tuyến tàu cao tốc.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 50.0, 200000m, "Giảm giá khai trương", "Chưa diễn ra" },
                    { "SUMMER26", "9c2fb5b6-9d1e-4a2c-91be-51c61d3d10d9.jpg", "Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 10% khi đặt vé.", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, 100000m, "Ưu đãi mùa hè rực rỡ", "Chưa diễn ra" },
                    { "SUMMER27", "8b779b59-820c-48bd-bc11-b85e27021424.jpg", "Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 30% khi đặt vé.", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 30.0, 100000m, "Ưu đãi mùa hè hết cỡ", "Chưa diễn ra" },
                    { "TET2026", "0b00c585-fb35-4462-9b5d-8e1aee28b429.jpg", "Chương trình khuyến mãi đặc biệt dành cho khách hàng về quê ăn Tết hoặc du xuân cùng gia đình.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 30.0, 300000m, "Vui Tết sum vầy", "Chưa diễn ra" }
                });

            migrationBuilder.InsertData(
                table: "Tau",
                columns: new[] { "MaTau", "HinhAnh", "TenTau", "TongSoGhe", "TrangThai" },
                values: new object[,]
                {
                    { 1, "0885ba23-96cd-46c0-9cce-5b739d92c445_Tau-cao-toc-Phu-Quoc-Express-Vung-Tau-Con-Dao.jpg", "Phú Quốc Express 1", 20, true },
                    { 2, "4f471677-b064-46f6-ba00-1840314b7226_tau-trung-trac-tren-tuyen-cao-toc-phan-thiet-phu-quy-1024x768-1.jpg", "Phú Quốc Express 2", 20, true },
                    { 3, "ca4c374d-3d0c-48be-8b36-e79219a62796_Tau-cao-toc-Con-Dao-Express-36-1536x863.jpg", "Phú Quốc Express 3", 20, true },
                    { 4, "dfa33889-692e-4f3a-84c2-491e2a402329_Tau-cao-toc-Trung-Nhi.jpg", "Phú Quốc Express 4", 20, true },
                    { 5, "ca4c374d-3d0c-48be-8b36-e79219a62796_Tau-cao-toc-Con-Dao-Express-36-1536x863.jpg", "Phú Quốc Express 5", 20, true },
                    { 6, "0885ba23-96cd-46c0-9cce-5b739d92c445_Tau-cao-toc-Phu-Quoc-Express-Vung-Tau-Con-Dao.jpg", "Phú Quốc Express 6", 20, true },
                    { 7, "0885ba23-96cd-46c0-9cce-5b739d92c445_Tau-cao-toc-Phu-Quoc-Express-Vung-Tau-Con-Dao.jpg", "Phú Quốc Express 7", 20, true },
                    { 8, "dfa33889-692e-4f3a-84c2-491e2a402329_Tau-cao-toc-Trung-Nhi.jpg", "Phú Quốc Express 8", 20, true },
                    { 9, "4f471677-b064-46f6-ba00-1840314b7226_tau-trung-trac-tren-tuyen-cao-toc-phan-thiet-phu-quy-1024x768-1.jpg", "Phú Quốc Express 9", 20, true },
                    { 10, "4f471677-b064-46f6-ba00-1840314b7226_tau-trung-trac-tren-tuyen-cao-toc-phan-thiet-phu-quy-1024x768-1.jpg", "Phú Quốc Express 10", 20, true }
                });

            migrationBuilder.InsertData(
                table: "TuyenDuong",
                columns: new[] { "MaTuyen", "DiemDen", "DiemDi", "HinhAnh", "KhoangCach", "TenTuyen", "ThoiGianDuKien" },
                values: new object[,]
                {
                    { 1, "Phú Quý", "Phan Thiết", "04e1440f-0c96-46c6-82b1-c10f63b4db23_phu-quy.jpg", 105.0, "Phan Thiết - Phú Quý", new TimeSpan(0, 2, 30, 0, 0) },
                    { 2, "Cát Bà", "Hải Phòng", "c31132fa-0e14-4787-8d05-fd22a23a3411_cat-ba.jpg", 30.0, "Hải Phòng - Cát Bà", new TimeSpan(0, 0, 45, 0, 0) },
                    { 3, "Côn Đảo", "Vũng Tàu", "7abf98e9-a440-4779-8a3a-529b3a400bbd_con-dao.jpg", 180.0, "Vũng Tàu - Côn Đảo", new TimeSpan(0, 3, 45, 0, 0) },
                    { 4, "Hòn Sơn", "Rạch Giá", "c051024a-ca91-4835-8751-6a6b40f5c124_hon-son.jpg", 65.0, "Rạch Giá - Hòn Sơn", new TimeSpan(0, 1, 30, 0, 0) },
                    { 5, "Lý Sơn", "Sa Kỳ", "a3c8c7b7-69df-4c73-9acd-11db5b8433e8_ly-son.jpg", 30.0, "Sa Kỳ - Lý Sơn", new TimeSpan(0, 0, 45, 0, 0) },
                    { 6, "Nam Du", "Rạch Giá", "6f1e4c0e-2db1-4540-b7a0-bdafd5a151cd_nam-du.jpg", 80.0, "Rạch Giá - Nam Du", new TimeSpan(0, 2, 15, 0, 0) },
                    { 7, "Phú Quốc", "Hà Tiên", "bcd3de60-2b13-4d14-95c5-37ca27af887d_phu-quoc.jpg", 45.0, "Hà Tiên - Phú Quốc", new TimeSpan(0, 1, 15, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "2", "7667fed0-c6f2-4dff-a12b-f4c5b064f9f2" },
                    { "1", "c4b3adc5-a6a7-4f7e-bd1c-2806c328191b" }
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
                    { 40, "VIP", 2, "V-20" },
                    { 41, "Thường", 3, "T-01" },
                    { 42, "Thường", 3, "T-02" },
                    { 43, "Thường", 3, "T-03" },
                    { 44, "Thường", 3, "T-04" },
                    { 45, "Thường", 3, "T-05" },
                    { 46, "Thường", 3, "T-06" },
                    { 47, "Thường", 3, "T-07" },
                    { 48, "Thường", 3, "T-08" },
                    { 49, "Thường", 3, "T-09" },
                    { 50, "Thường", 3, "T-10" },
                    { 51, "Thường", 3, "T-11" },
                    { 52, "Thường", 3, "T-12" },
                    { 53, "Thường", 3, "T-13" },
                    { 54, "Thường", 3, "T-14" },
                    { 55, "Thường", 3, "T-15" },
                    { 56, "VIP", 3, "V-16" },
                    { 57, "VIP", 3, "V-17" },
                    { 58, "VIP", 3, "V-18" },
                    { 59, "VIP", 3, "V-19" },
                    { 60, "VIP", 3, "V-20" },
                    { 61, "Thường", 4, "T-01" },
                    { 62, "Thường", 4, "T-02" },
                    { 63, "Thường", 4, "T-03" },
                    { 64, "Thường", 4, "T-04" },
                    { 65, "Thường", 4, "T-05" },
                    { 66, "Thường", 4, "T-06" },
                    { 67, "Thường", 4, "T-07" },
                    { 68, "Thường", 4, "T-08" },
                    { 69, "Thường", 4, "T-09" },
                    { 70, "Thường", 4, "T-10" },
                    { 71, "Thường", 4, "T-11" },
                    { 72, "Thường", 4, "T-12" },
                    { 73, "Thường", 4, "T-13" },
                    { 74, "Thường", 4, "T-14" },
                    { 75, "Thường", 4, "T-15" },
                    { 76, "VIP", 4, "V-16" },
                    { 77, "VIP", 4, "V-17" },
                    { 78, "VIP", 4, "V-18" },
                    { 79, "VIP", 4, "V-19" },
                    { 80, "VIP", 4, "V-20" }
                });

            migrationBuilder.InsertData(
                table: "KhachHang",
                columns: new[] { "MaKH", "DiaChi", "Email", "HoTen", "MaTK", "NgaySinh", "Sdt" },
                values: new object[] { 1, null, "khach.tran@gmail.com", "Trần Thị Khách", "8e3ade7b-c691-4276-abd5-3e1fb0f02a05", new DateTime(1995, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0912345678" });

            migrationBuilder.InsertData(
                table: "LichTrinh",
                columns: new[] { "MaLichTrinh", "GiaVeCoBan", "MaTau", "MaTuyen", "NgayGioCapBenDuKien", "NgayGioKhoiHanh", "SoGheTrong", "TrangThai" },
                values: new object[,]
                {
                    { 1, 200000m, 1, 1, new DateTime(2026, 4, 18, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 18, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 2, 200000m, 2, 2, new DateTime(2026, 4, 18, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 18, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 3, 200000m, 3, 3, new DateTime(2026, 4, 19, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 19, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 4, 200000m, 4, 4, new DateTime(2026, 4, 21, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 21, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 5, 200000m, 5, 5, new DateTime(2026, 4, 22, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 22, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 6, 200000m, 6, 6, new DateTime(2026, 4, 24, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 24, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 7, 200000m, 7, 7, new DateTime(2026, 4, 24, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 24, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 8, 200000m, 1, 1, new DateTime(2026, 4, 25, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 25, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 9, 200000m, 2, 2, new DateTime(2026, 4, 26, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 26, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 10, 200000m, 3, 3, new DateTime(2026, 4, 26, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 26, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" }
                });

            migrationBuilder.InsertData(
                table: "Logs",
                columns: new[] { "MaLog", "BangTacDong", "HanhDong", "IpAddress", "LoaiLog", "MaTK", "NoiDungChiTiet", "ThoiGian" },
                values: new object[,]
                {
                    { 1, "System", "Khởi tạo hệ thống", "127.0.0.1", "Info", "c4b3adc5-a6a7-4f7e-bd1c-2806c328191b", "Hệ thống đã khởi tạo dữ liệu mẫu (Seed Data) thành công.", new DateTime(2026, 4, 13, 9, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "AspNetUsers", "Cấu hình bảo mật", "127.0.0.1", "Info", "c4b3adc5-a6a7-4f7e-bd1c-2806c328191b", "Thiết lập quyền Quản trị viên (Admin) cho hệ thống.", new DateTime(2026, 4, 13, 9, 0, 5, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "NhanVien",
                columns: new[] { "MaNV", "ChucVu", "Email", "HoTen", "Luong", "MaTK", "Sdt", "TrangThai" },
                values: new object[] { 1, "Admin", "maihuy@booking.com", "Mai Nhứt Huy", 0m, "c4b3adc5-a6a7-4f7e-bd1c-2806c328191b", "0386747090", true });

            migrationBuilder.InsertData(
                table: "HoaDon",
                columns: new[] { "MaHoaDon", "GhiChu", "MaKH", "MaKM", "MaNV", "NgayLap", "NgayThanhToan", "PhuongThucTT", "SoLuongVe", "SoTienGiam", "TamTinh", "TongTien", "TrangThai" },
                values: new object[,]
                {
                    { 1, "", 1, "KM10", 1, new DateTime(2026, 4, 17, 22, 11, 39, 554, DateTimeKind.Local).AddTicks(5477), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" },
                    { 2, "", 1, "KM10", 1, new DateTime(2026, 4, 17, 22, 11, 39, 554, DateTimeKind.Local).AddTicks(5614), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" },
                    { 3, "", 1, "KM10", 1, new DateTime(2026, 4, 17, 22, 11, 39, 554, DateTimeKind.Local).AddTicks(5667), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" },
                    { 4, "", 1, "KM10", 1, new DateTime(2026, 4, 17, 22, 11, 39, 554, DateTimeKind.Local).AddTicks(5619), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" }
                });

            migrationBuilder.InsertData(
                table: "DanhGia",
                columns: new[] { "MaDanhGia", "HinhAnh", "HoaDonMaHoaDon", "LichTrinhMaLichTrinh", "MaHoaDon", "NgayDanhGia", "NgayPhanHoi", "NoiDung", "PhanHoiAdmin", "SoSao", "TrangThai" },
                values: new object[,]
                {
                    { 1, "6a4b2c8d-1e5f-4a3b-9c2d-8e7f6a5b4c3d_review-phu-quoc.jpg", null, null, 1, new DateTime(2026, 4, 10, 8, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 14, 0, 0, 0, DateTimeKind.Unspecified), "Chuyến đi tuyệt vời, tàu chạy rất êm và đúng giờ. Nhân viên hỗ trợ nhiệt tình!", "Cảm ơn bạn đã ủng hộ WebAppBookingBoat! Rất mong được phục vụ bạn trong những chuyến đi tới.", 5, "Đã hiển thị" },
                    { 2, "8e7d6c5b-4a3f-4e2d-9c1b-0a9b8c7d6e5f_review-thang-long.jpg", null, null, 2, new DateTime(2026, 4, 11, 15, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 9, 15, 0, 0, DateTimeKind.Unspecified), "Chất lượng ghế VIP rất tốt, tuy nhiên đồ ăn nhẹ trên tàu hơi ít lựa chọn.", "Chào bạn, Admin ghi nhận góp ý và sẽ làm việc với bếp tàu để cải thiện thực đơn ạ!", 4, "Đã hiển thị" },
                    { 3, "2c3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5e6f_view-bien.jpg", null, null, 3, new DateTime(2026, 4, 17, 20, 11, 39, 554, DateTimeKind.Local).AddTicks(6008), null, "Đặt vé cực nhanh, thanh toán tiện lợi. Sẽ quay lại!", null, 5, "Chờ duyệt" },
                    { 4, "5f4e3d2c-1b0a-4c9d-8e7f-6a5b4c3d2e1f_tau-phu-quy.jpg", null, null, 4, new DateTime(2026, 4, 17, 22, 11, 39, 554, DateTimeKind.Local).AddTicks(6014), null, "Gia đình mình đi tuyến Hà Tiên - Phú Quốc rất hài lòng...", null, 5, "Chờ duyệt" }
                });

            migrationBuilder.InsertData(
                table: "Ve",
                columns: new[] { "MaVe", "GiaVe", "MaGhe", "MaHoaDon", "MaLichTrinh", "TrangThai" },
                values: new object[,]
                {
                    { 1, 180000m, 2, 1, 2, "Hợp lệ" },
                    { 2, 180000m, 3, 2, 1, "Hợp lệ" },
                    { 3, 180000m, 4, 3, 1, "Hợp lệ" },
                    { 4, 180000m, 5, 4, 2, "Hợp lệ" }
                });

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
                name: "IX_DanhGia_HoaDonMaHoaDon",
                table: "DanhGia",
                column: "HoaDonMaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_LichTrinhMaLichTrinh",
                table: "DanhGia",
                column: "LichTrinhMaLichTrinh");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaHoaDon",
                table: "DanhGia",
                column: "MaHoaDon",
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
                name: "IX_Logs_MaTK",
                table: "Logs",
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
                unique: true,
                filter: "[MaTK] IS NOT NULL");

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
                name: "IX_Ve_MaGhe",
                table: "Ve",
                column: "MaGhe");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaHoaDon",
                table: "Ve",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaLichTrinh_MaGhe",
                table: "Ve",
                columns: new[] { "MaLichTrinh", "MaGhe" },
                unique: true);
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
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Ve");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Ghe");

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
