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
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.CheckConstraint("CK_DG_NgayPhanHoi", "[NgayPhanHoi] IS NULL OR [NgayPhanHoi] >= [NgayDanhGia]");
                    table.CheckConstraint("CK_DG_SoSao", "[SoSao] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_DG_TrangThai", "[TrangThai] IN (N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn')");
                    table.ForeignKey(
                        name: "FK_DanhGia_HoaDon_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "MaHoaDon",
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
                    { "72de7e88-f2c5-4fd9-bbb0-b6f5b7b6976e", 0, "7634ed00-5c49-4826-9e3d-ade04b4f48ee", "khachhang4@gmail.com", true, false, null, "KHACHHANG4@GMAIL.COM", "KHACHHANG4", "AQAAAAIAAYagAAAAECQcUJuJ4h0LExeBNhzUR1oeZnCFs2G4pL0PQ/2HaN3hECVy4COccoK8lG/HegdeSQ==", null, false, "5f405454-ba94-4346-bb16-0cdc2cbe476c", true, false, "khachhang4" },
                    { "7687c3e8-e4be-47a2-b76a-ba15931db05c", 0, "3d63fbe3-c183-4e53-92b0-d4c26c757c5a", "khachhang1@gmail.com", true, false, null, "KHACHHANG1@GMAIL.COM", "KHACHHANG1", "AQAAAAIAAYagAAAAEKvHbRcFWhQVTzpZIniIvxCsi/VJD7bNF4LBhhqmwnd+O2e3JSSAlUgubL8NST5zBw==", null, false, "3b07a29c-ffe0-4a6d-b0da-e36e70edd2e4", true, false, "khachhang1" },
                    { "7d68d303-94f0-4952-abda-7ac3ee9ef6f1", 0, "598de154-28f2-41ce-9dba-92799d10b00a", "nhanvien1@booking.com", true, false, null, "NHANVIEN1@BOOKING.COM", "NHANVIEN1", "AQAAAAIAAYagAAAAEA6uP4K6tO88PR6KhlaaEY9bkjTOpFgJPOkvO6c+OfTPjHlh/MRoDlksTnbjjZE8WA==", null, false, "5b073df4-78ce-448d-8c2d-48568072ded4", true, false, "nhanvien1" },
                    { "8ae021f8-cde5-41b2-9fba-4f0ee7c2a941", 0, "0f932f69-2d36-405f-afd1-eed88c0547ac", "khachhang3@gmail.com", true, false, null, "KHACHHANG3@GMAIL.COM", "KHACHHANG3", "AQAAAAIAAYagAAAAEG4nxLn49VM9TIuSKA3TKr9SQ9UVRsrAcbo0JW5ruWds6WhEp73UauCRciqObw0Q9A==", null, false, "20c75741-c234-4cf1-b14c-48b114cd6da2", true, false, "khachhang3" },
                    { "8e082baf-3266-4245-b6c9-f66d0f71b9bd", 0, "12103f63-4952-43b9-bc3f-7e121bce86cb", "khachhang2@gmail.com", true, false, null, "KHACHHANG2@GMAIL.COM", "KHACHHANG2", "AQAAAAIAAYagAAAAEHNBuJnEmX7Ndnf+sAOFhKqWEenHVMqzDpugVVSHZSBKhC8QgYvD4CT67HaPYScYlQ==", null, false, "48b140e8-1e76-479d-ba8e-2e294344c720", true, false, "khachhang2" },
                    { "d35095a3-62ed-474f-8ecf-5397ed08d10e", 0, "b168aa75-2a22-4be1-96ad-1630c9c6878c", "admin@booking.com", true, false, null, "ADMIN@BOOKING.COM", "ADMIN", "AQAAAAIAAYagAAAAEHenvPOXwXta3Iz2uOPbMQD0qU0jSjjy7tN72lpHWvEEe9CjFlFc2xBD5e1Gbgw3ig==", null, false, "2818f961-f720-4e02-bee0-88483339426a", true, false, "admin" }
                });

            migrationBuilder.InsertData(
                table: "KhuyenMai",
                columns: new[] { "MaKM", "HinhAnh", "MoTa", "NgayBatDau", "NgayKetThuc", "PhanTramGiam", "SoTienToiDaGiam", "TenChuongTrinh", "TrangThai" },
                values: new object[,]
                {
                    { "DONGAM2026", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423145/WebAppBookingBoat/KhuyenMai/hejcsoy1vxd8ooftqlqf.jpg", "Ưu đãi sưởi ấm những chuyến đi cuối năm. Giảm giá sâu cho các tuyến tàu ra đảo nghỉ dưỡng.", new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 25.0, 250000m, "Mùa Đông Ấm Áp", "Chưa diễn ra" },
                    { "GIADO304", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423157/WebAppBookingBoat/KhuyenMai/va9cgbewyqdxcfn3ggna.jpg", "Ưu đãi cực lớn dành cho các tuyến tàu cao tốc du lịch trong kỳ nghỉ lễ 30/4 và 1/5.", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 30.0, 400000m, "Mừng Đại Lễ - Giảm Giá Mê", "Sắp diễn ra" },
                    { "KM10", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423169/WebAppBookingBoat/KhuyenMai/mqzylxgtpe7afhspqnr4.jpg", "Chào mừng hệ thống WebAppBookingBoat đi vào hoạt động. Giảm ngay 50% cho tất cả các tuyến tàu cao tốc.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 50.0, 200000m, "Giảm giá khai trương", "Chưa diễn ra" },
                    { "SUMMER26", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423185/WebAppBookingBoat/KhuyenMai/ezhwyslezlz3rfcfanzj.jpg", "Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 10% khi đặt vé.", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, 100000m, "Ưu đãi mùa hè rực rỡ", "Chưa diễn ra" },
                    { "SUMMER27", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423198/WebAppBookingBoat/KhuyenMai/b6zrdnmmywdeqr09k7y4.jpg", "Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 30% khi đặt vé.", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 30.0, 100000m, "Ưu đãi mùa hè hết cỡ", "Chưa diễn ra" },
                    { "TET2026", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777423230/WebAppBookingBoat/KhuyenMai/o1sh0nzmy24brnrriluo.jpg", "Chương trình khuyến mãi đặc biệt dành cho khách hàng về quê ăn Tết hoặc du xuân cùng gia đình.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 30.0, 300000m, "Vui Tết sum vầy", "Chưa diễn ra" }
                });

            migrationBuilder.InsertData(
                table: "Tau",
                columns: new[] { "MaTau", "HinhAnh", "TenTau", "TongSoGhe", "TrangThai" },
                values: new object[,]
                {
                    { 1, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395965/WebAppBookingBoat/Taus/lavhtb0wfmiqxaest57x.jpg", "Phú Quốc Express 1", 20, true },
                    { 2, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg", "Phú Quốc Express 2", 20, true },
                    { 3, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395947/WebAppBookingBoat/Taus/kc65kynwwjci9tg2tbf0.jpg", "Phú Quốc Express 3", 20, true },
                    { 4, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777419297/WebAppBookingBoat/Taus/qnizabxmp2xhxhm5mqki.jpg", "Phú Quốc Express 4", 20, true },
                    { 5, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395965/WebAppBookingBoat/Taus/lavhtb0wfmiqxaest57x.jpg", "Phú Quốc Express 5", 20, true },
                    { 6, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg", "Phú Quốc Express 6", 20, true },
                    { 7, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg", "Phú Quốc Express 7", 20, true },
                    { 8, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg", "Phú Quốc Express 8", 20, true },
                    { 9, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg", "Phú Quốc Express 9", 20, true },
                    { 10, "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777395957/WebAppBookingBoat/Taus/gaa2knlpnuvrelshtbhi.jpg", "Phú Quốc Express 10", 20, true }
                });

            migrationBuilder.InsertData(
                table: "TuyenDuong",
                columns: new[] { "MaTuyen", "DiemDen", "DiemDi", "HinhAnh", "KhoangCach", "TenTuyen", "ThoiGianDuKien" },
                values: new object[,]
                {
                    { 1, "Phú Quý", "Phan Thiết", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420178/WebAppBookingBoat/TuyenDuongs/q14qzet524aqqms6kcda.jpg", 105.0, "Phan Thiết - Phú Quý", new TimeSpan(0, 2, 30, 0, 0) },
                    { 2, "Cát Bà", "Hải Phòng", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420166/WebAppBookingBoat/TuyenDuongs/vzapbdbl0im8kpkm5f2m.jpg", 30.0, "Hải Phòng - Cát Bà", new TimeSpan(0, 0, 45, 0, 0) },
                    { 3, "Côn Đảo", "Vũng Tàu", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420156/WebAppBookingBoat/TuyenDuongs/vnfyfqqtnqvqg7hrxbha.jpg", 180.0, "Vũng Tàu - Côn Đảo", new TimeSpan(0, 3, 45, 0, 0) },
                    { 4, "Hòn Sơn", "Rạch Giá", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420134/WebAppBookingBoat/TuyenDuongs/syeveinoy2fsukilmuqr.jpg", 65.0, "Rạch Giá - Hòn Sơn", new TimeSpan(0, 1, 30, 0, 0) },
                    { 5, "Lý Sơn", "Sa Kỳ", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420123/WebAppBookingBoat/TuyenDuongs/ghxji0xxpn91dggqgypn.jpg", 30.0, "Sa Kỳ - Lý Sơn", new TimeSpan(0, 0, 45, 0, 0) },
                    { 6, "Nam Du", "Rạch Giá", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420114/WebAppBookingBoat/TuyenDuongs/ijtbq0xjjranxskeawrg.jpg", 80.0, "Rạch Giá - Nam Du", new TimeSpan(0, 2, 15, 0, 0) },
                    { 7, "Phú Quốc", "Hà Tiên", "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1777420103/WebAppBookingBoat/TuyenDuongs/bfocircuvbvg36neglex.jpg", 45.0, "Hà Tiên - Phú Quốc", new TimeSpan(0, 1, 15, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "3", "72de7e88-f2c5-4fd9-bbb0-b6f5b7b6976e" },
                    { "3", "7687c3e8-e4be-47a2-b76a-ba15931db05c" },
                    { "2", "7d68d303-94f0-4952-abda-7ac3ee9ef6f1" },
                    { "3", "8ae021f8-cde5-41b2-9fba-4f0ee7c2a941" },
                    { "3", "8e082baf-3266-4245-b6c9-f66d0f71b9bd" },
                    { "1", "d35095a3-62ed-474f-8ecf-5397ed08d10e" }
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
                values: new object[,]
                {
                    { 1, null, "khach.tran@gmail.com", "Trần Thị Khách", "7687c3e8-e4be-47a2-b76a-ba15931db05c", new DateTime(1995, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0912345678" },
                    { 2, null, "khach.nguyen@gmail.com", "Nguyễn Thị Khách", "8e082baf-3266-4245-b6c9-f66d0f71b9bd", new DateTime(1995, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0912345679" },
                    { 3, null, "khach.le@gmail.com", "Lê Thị Khách", "8ae021f8-cde5-41b2-9fba-4f0ee7c2a941", new DateTime(1995, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0912345676" },
                    { 4, null, "khach.do@gmail.com", "Đỗ Thị Khách", "72de7e88-f2c5-4fd9-bbb0-b6f5b7b6976e", new DateTime(1995, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0912345675" }
                });

            migrationBuilder.InsertData(
                table: "LichTrinh",
                columns: new[] { "MaLichTrinh", "GiaVeCoBan", "MaTau", "MaTuyen", "NgayGioCapBenDuKien", "NgayGioKhoiHanh", "SoGheTrong", "TrangThai" },
                values: new object[,]
                {
                    { 1, 200000m, 1, 1, new DateTime(2026, 4, 30, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 30, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 2, 200000m, 2, 2, new DateTime(2026, 4, 30, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 4, 30, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 3, 200000m, 3, 3, new DateTime(2026, 5, 1, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 4, 200000m, 4, 4, new DateTime(2026, 5, 3, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 3, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 5, 200000m, 5, 5, new DateTime(2026, 5, 7, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 7, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 6, 200000m, 6, 6, new DateTime(2026, 5, 9, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 9, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 7, 200000m, 1, 7, new DateTime(2026, 5, 12, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 8, 200000m, 2, 1, new DateTime(2026, 5, 12, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 12, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 9, 200000m, 3, 2, new DateTime(2026, 5, 13, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 13, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" },
                    { 10, 200000m, 4, 3, new DateTime(2026, 5, 15, 10, 30, 0, 0, DateTimeKind.Local), new DateTime(2026, 5, 15, 8, 0, 0, 0, DateTimeKind.Local), 20, "Sắp khởi hành" }
                });

            migrationBuilder.InsertData(
                table: "Logs",
                columns: new[] { "MaLog", "BangTacDong", "HanhDong", "IpAddress", "LoaiLog", "MaTK", "NoiDungChiTiet", "ThoiGian" },
                values: new object[,]
                {
                    { 1, "System", "Khởi tạo hệ thống", "127.0.0.1", "Info", "d35095a3-62ed-474f-8ecf-5397ed08d10e", "Hệ thống đã khởi tạo dữ liệu mẫu (Seed Data) thành công.", new DateTime(2026, 4, 13, 9, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "AspNetUsers", "Cấu hình bảo mật", "127.0.0.1", "Info", "d35095a3-62ed-474f-8ecf-5397ed08d10e", "Thiết lập quyền Quản trị viên (Admin) cho hệ thống.", new DateTime(2026, 4, 13, 9, 0, 5, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "NhanVien",
                columns: new[] { "MaNV", "ChucVu", "Email", "HoTen", "Luong", "MaTK", "Sdt", "TrangThai" },
                values: new object[,]
                {
                    { 1, "Admin", "maihuy@booking.com", "Mai Nhứt Huy", 0m, "d35095a3-62ed-474f-8ecf-5397ed08d10e", "0386747090", true },
                    { 2, "Nhân viên", "jerry@booking.com", "Jerry", 0m, "7d68d303-94f0-4952-abda-7ac3ee9ef6f1", "0386747091", true }
                });

            migrationBuilder.InsertData(
                table: "HoaDon",
                columns: new[] { "MaHoaDon", "GhiChu", "MaKH", "MaKM", "MaNV", "NgayLap", "NgayThanhToan", "PhuongThucTT", "SoLuongVe", "SoTienGiam", "TamTinh", "TongTien", "TrangThai" },
                values: new object[,]
                {
                    { 1, "", 4, "KM10", 1, new DateTime(2026, 4, 29, 8, 3, 43, 330, DateTimeKind.Local).AddTicks(215), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" },
                    { 2, "", 2, "KM10", 1, new DateTime(2026, 4, 29, 8, 3, 43, 330, DateTimeKind.Local).AddTicks(223), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" },
                    { 3, "", 3, "KM10", 1, new DateTime(2026, 4, 29, 8, 3, 43, 330, DateTimeKind.Local).AddTicks(228), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" },
                    { 4, "", 1, "KM10", 1, new DateTime(2026, 4, 29, 8, 3, 43, 330, DateTimeKind.Local).AddTicks(233), null, "Tiền mặt", 1, 20000m, 200000m, 180000m, "Đã thanh toán" }
                });

            migrationBuilder.InsertData(
                table: "DanhGia",
                columns: new[] { "MaDanhGia", "HinhAnh", "MaHoaDon", "NgayDanhGia", "NgayPhanHoi", "NoiDung", "PhanHoiAdmin", "SoSao", "TrangThai" },
                values: new object[,]
                {
                    { 1, "6a4b2c8d-1e5f-4a3b-9c2d-8e7f6a5b4c3d_review-phu-quoc.jpg", 1, new DateTime(2026, 4, 10, 8, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 14, 0, 0, 0, DateTimeKind.Unspecified), "Chuyến đi tuyệt vời, tàu chạy rất êm và đúng giờ. Nhân viên hỗ trợ nhiệt tình!", "Cảm ơn bạn đã ủng hộ WebAppBookingBoat! Rất mong được phục vụ bạn trong những chuyến đi tới.", 5, "Đã hiển thị" },
                    { 2, "8e7d6c5b-4a3f-4e2d-9c1b-0a9b8c7d6e5f_review-thang-long.jpg", 2, new DateTime(2026, 4, 11, 15, 20, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 12, 9, 15, 0, 0, DateTimeKind.Unspecified), "Chất lượng ghế VIP rất tốt, tuy nhiên đồ ăn nhẹ trên tàu hơi ít lựa chọn.", "Chào bạn, Admin ghi nhận góp ý và sẽ làm việc với bếp tàu để cải thiện thực đơn ạ!", 4, "Đã hiển thị" },
                    { 3, "2c3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5e6f_view-bien.jpg", 3, new DateTime(2026, 4, 29, 6, 3, 43, 330, DateTimeKind.Local).AddTicks(703), null, "Đặt vé cực nhanh, thanh toán tiện lợi. Sẽ quay lại!", null, 5, "Chờ duyệt" },
                    { 4, "5f4e3d2c-1b0a-4c9d-8e7f-6a5b4c3d2e1f_tau-phu-quy.jpg", 4, new DateTime(2026, 4, 29, 8, 3, 43, 330, DateTimeKind.Local).AddTicks(721), null, "Gia đình mình đi tuyến Hà Tiên - Phú Quốc rất hài lòng...", null, 5, "Chờ duyệt" }
                });

            migrationBuilder.InsertData(
                table: "Ve",
                columns: new[] { "MaVe", "GiaVe", "MaGhe", "MaHoaDon", "MaLichTrinh", "TrangThai" },
                values: new object[,]
                {
                    { 1, 180000m, 21, 1, 2, "Hợp lệ" },
                    { 2, 180000m, 3, 2, 1, "Hợp lệ" },
                    { 3, 180000m, 4, 3, 1, "Hợp lệ" },
                    { 4, 180000m, 26, 4, 2, "Hợp lệ" }
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
                unique: true,
                filter: "[TrangThai] <> N'Đã hủy'");
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
