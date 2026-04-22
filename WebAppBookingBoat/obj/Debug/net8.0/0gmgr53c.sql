IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [TrangThai] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [KhuyenMai] (
    [MaKM] nvarchar(50) NOT NULL,
    [TenChuongTrinh] nvarchar(255) NOT NULL,
    [HinhAnh] nvarchar(500) NULL,
    [MoTa] nvarchar(1000) NULL,
    [PhanTramGiam] float NOT NULL,
    [SoTienToiDaGiam] decimal(18,2) NOT NULL,
    [NgayBatDau] datetime2 NOT NULL,
    [NgayKetThuc] datetime2 NOT NULL,
    [TrangThai] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_KhuyenMai] PRIMARY KEY ([MaKM]),
    CONSTRAINT [CK_KM_PhanTram] CHECK ([PhanTramGiam] >= 0 AND [PhanTramGiam] <= 100),
    CONSTRAINT [CK_KM_SoTienToiDa] CHECK ([SoTienToiDaGiam] >= 0),
    CONSTRAINT [CK_KM_ThoiGian] CHECK ([NgayKetThuc] > [NgayBatDau])
);
GO

CREATE TABLE [Tau] (
    [MaTau] int NOT NULL IDENTITY,
    [TenTau] nvarchar(100) NOT NULL,
    [HinhAnh] nvarchar(255) NULL,
    [TongSoGhe] int NOT NULL,
    [TrangThai] bit NOT NULL,
    CONSTRAINT [PK_Tau] PRIMARY KEY ([MaTau])
);
GO

CREATE TABLE [TuyenDuong] (
    [MaTuyen] int NOT NULL IDENTITY,
    [TenTuyen] nvarchar(200) NOT NULL,
    [DiemDi] nvarchar(100) NOT NULL,
    [DiemDen] nvarchar(100) NOT NULL,
    [HinhAnh] nvarchar(255) NULL,
    [KhoangCach] float NOT NULL,
    [ThoiGianDuKien] time NOT NULL,
    CONSTRAINT [PK_TuyenDuong] PRIMARY KEY ([MaTuyen]),
    CONSTRAINT [CK_TD_DiemKhacNhau] CHECK ([DiemDi] <> [DiemDen])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [KhachHang] (
    [MaKH] int NOT NULL IDENTITY,
    [MaTK] nvarchar(450) NULL,
    [HoTen] nvarchar(100) NOT NULL,
    [NgaySinh] datetime2 NULL,
    [Sdt] nvarchar(15) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [DiaChi] nvarchar(255) NULL,
    CONSTRAINT [PK_KhachHang] PRIMARY KEY ([MaKH]),
    CONSTRAINT [CK_KH_Email_Format] CHECK ([Email] LIKE '%_@_%._%'),
    CONSTRAINT [CK_KH_Sdt_Format] CHECK (LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'),
    CONSTRAINT [FK_KhachHang_AspNetUsers_MaTK] FOREIGN KEY ([MaTK]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Logs] (
    [MaLog] int NOT NULL IDENTITY,
    [MaTK] nvarchar(450) NULL,
    [LoaiLog] nvarchar(50) NOT NULL,
    [HanhDong] nvarchar(100) NOT NULL,
    [BangTacDong] nvarchar(100) NOT NULL,
    [NoiDungChiTiet] nvarchar(max) NULL,
    [IpAddress] nvarchar(50) NULL,
    [ThoiGian] datetime2 NOT NULL,
    CONSTRAINT [PK_Logs] PRIMARY KEY ([MaLog]),
    CONSTRAINT [CK_Log_Loai] CHECK ([LoaiLog] IN ('Info', 'Warning', 'Error', 'Critical')),
    CONSTRAINT [FK_Logs_AspNetUsers_MaTK] FOREIGN KEY ([MaTK]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [NhanVien] (
    [MaNV] int NOT NULL IDENTITY,
    [MaTK] nvarchar(450) NULL,
    [HoTen] nvarchar(100) NOT NULL,
    [Sdt] nvarchar(15) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [ChucVu] nvarchar(20) NULL,
    [Luong] decimal(18,2) NOT NULL,
    [TrangThai] bit NOT NULL,
    CONSTRAINT [PK_NhanVien] PRIMARY KEY ([MaNV]),
    CONSTRAINT [CK_NV_Email_Format] CHECK ([Email] LIKE '%_@_%._%'),
    CONSTRAINT [CK_NV_Sdt_Format] CHECK (LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'),
    CONSTRAINT [FK_NhanVien_AspNetUsers_MaTK] FOREIGN KEY ([MaTK]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Ghe] (
    [MaGhe] int NOT NULL IDENTITY,
    [MaTau] int NOT NULL,
    [TenGhe] nvarchar(10) NOT NULL,
    [LoaiGhe] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Ghe] PRIMARY KEY ([MaGhe]),
    CONSTRAINT [CK_Ghe_LoaiGhe] CHECK ([LoaiGhe] IN (N'Thường', N'VIP')),
    CONSTRAINT [FK_Ghe_Tau_MaTau] FOREIGN KEY ([MaTau]) REFERENCES [Tau] ([MaTau]) ON DELETE NO ACTION
);
GO

CREATE TABLE [LichTrinh] (
    [MaLichTrinh] int NOT NULL IDENTITY,
    [MaTuyen] int NOT NULL,
    [MaTau] int NOT NULL,
    [NgayGioKhoiHanh] datetime2 NOT NULL,
    [NgayGioCapBenDuKien] datetime2 NOT NULL,
    [GiaVeCoBan] decimal(18,2) NOT NULL,
    [SoGheTrong] int NOT NULL,
    [TrangThai] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_LichTrinh] PRIMARY KEY ([MaLichTrinh]),
    CONSTRAINT [CK_LT_GheTrong] CHECK ([SoGheTrong] >= 0),
    CONSTRAINT [CK_LT_GiaVe] CHECK ([GiaVeCoBan] >= 0),
    CONSTRAINT [CK_LT_ThoiGian] CHECK ([NgayGioCapBenDuKien] > [NgayGioKhoiHanh]),
    CONSTRAINT [CK_LT_TrangThai] CHECK ([TrangThai] IN (N'Sắp khởi hành', N'Đang vận hành', N'Hoàn thành', N'Đã hủy')),
    CONSTRAINT [FK_LichTrinh_Tau_MaTau] FOREIGN KEY ([MaTau]) REFERENCES [Tau] ([MaTau]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LichTrinh_TuyenDuong_MaTuyen] FOREIGN KEY ([MaTuyen]) REFERENCES [TuyenDuong] ([MaTuyen]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HoaDon] (
    [MaHoaDon] int NOT NULL IDENTITY,
    [MaKH] int NOT NULL,
    [MaNV] int NULL,
    [MaKM] nvarchar(50) NULL,
    [NgayLap] datetime2 NOT NULL,
    [NgayThanhToan] datetime2 NULL,
    [SoLuongVe] int NOT NULL,
    [TamTinh] decimal(18,2) NOT NULL,
    [SoTienGiam] decimal(18,2) NOT NULL,
    [TongTien] decimal(18,2) NOT NULL,
    [PhuongThucTT] nvarchar(50) NOT NULL,
    [TrangThai] nvarchar(50) NOT NULL,
    [GhiChu] nvarchar(500) NULL,
    CONSTRAINT [PK_HoaDon] PRIMARY KEY ([MaHoaDon]),
    CONSTRAINT [CK_HD_SoLuong] CHECK ([SoLuongVe] > 0),
    CONSTRAINT [CK_HD_Tien] CHECK ([TamTinh] >= 0 AND [SoTienGiam] >= 0 AND [TongTien] >= 0),
    CONSTRAINT [FK_HoaDon_KhachHang_MaKH] FOREIGN KEY ([MaKH]) REFERENCES [KhachHang] ([MaKH]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HoaDon_KhuyenMai_MaKM] FOREIGN KEY ([MaKM]) REFERENCES [KhuyenMai] ([MaKM]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HoaDon_NhanVien_MaNV] FOREIGN KEY ([MaNV]) REFERENCES [NhanVien] ([MaNV]) ON DELETE NO ACTION
);
GO

CREATE TABLE [DanhGia] (
    [MaDanhGia] int NOT NULL IDENTITY,
    [MaHoaDon] int NOT NULL,
    [SoSao] int NOT NULL,
    [NoiDung] nvarchar(1000) NULL,
    [HinhAnh] nvarchar(255) NULL,
    [NgayDanhGia] datetime2 NOT NULL,
    [PhanHoiAdmin] nvarchar(1000) NULL,
    [NgayPhanHoi] datetime2 NULL,
    [TrangThai] nvarchar(50) NOT NULL,
    [HoaDonMaHoaDon] int NULL,
    [LichTrinhMaLichTrinh] int NULL,
    CONSTRAINT [PK_DanhGia] PRIMARY KEY ([MaDanhGia]),
    CONSTRAINT [CK_DG_NgayPhanHoi] CHECK ([NgayPhanHoi] IS NULL OR [NgayPhanHoi] >= [NgayDanhGia]),
    CONSTRAINT [CK_DG_SoSao] CHECK ([SoSao] BETWEEN 1 AND 5),
    CONSTRAINT [CK_DG_TrangThai] CHECK ([TrangThai] IN (N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn')),
    CONSTRAINT [FK_DanhGia_HoaDon_HoaDonMaHoaDon] FOREIGN KEY ([HoaDonMaHoaDon]) REFERENCES [HoaDon] ([MaHoaDon]),
    CONSTRAINT [FK_DanhGia_HoaDon_MaHoaDon] FOREIGN KEY ([MaHoaDon]) REFERENCES [HoaDon] ([MaHoaDon]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DanhGia_LichTrinh_LichTrinhMaLichTrinh] FOREIGN KEY ([LichTrinhMaLichTrinh]) REFERENCES [LichTrinh] ([MaLichTrinh]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Ve] (
    [MaVe] int NOT NULL IDENTITY,
    [MaHoaDon] int NOT NULL,
    [MaLichTrinh] int NOT NULL,
    [GiaVe] decimal(18,2) NOT NULL,
    [TrangThai] nvarchar(50) NOT NULL,
    [MaGhe] int NOT NULL,
    CONSTRAINT [PK_Ve] PRIMARY KEY ([MaVe]),
    CONSTRAINT [CK_Ve_GiaVe] CHECK ([GiaVe] >= 0),
    CONSTRAINT [CK_Ve_TrangThai] CHECK ([TrangThai] IN (N'Đang chờ', N'Hợp lệ', N'Đã hủy')),
    CONSTRAINT [FK_Ve_Ghe_MaGhe] FOREIGN KEY ([MaGhe]) REFERENCES [Ghe] ([MaGhe]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Ve_HoaDon_MaHoaDon] FOREIGN KEY ([MaHoaDon]) REFERENCES [HoaDon] ([MaHoaDon]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Ve_LichTrinh_MaLichTrinh] FOREIGN KEY ([MaLichTrinh]) REFERENCES [LichTrinh] ([MaLichTrinh]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
    SET IDENTITY_INSERT [AspNetRoles] ON;
INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
VALUES (N'1', NULL, N'Admin', N'ADMIN'),
(N'2', NULL, N'Staff', N'STAFF'),
(N'3', NULL, N'Customer', N'CUSTOMER');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
    SET IDENTITY_INSERT [AspNetRoles] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'Email', N'EmailConfirmed', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'SecurityStamp', N'TrangThai', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[AspNetUsers]'))
    SET IDENTITY_INSERT [AspNetUsers] ON;
INSERT INTO [AspNetUsers] ([Id], [AccessFailedCount], [ConcurrencyStamp], [Email], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TrangThai], [TwoFactorEnabled], [UserName])
VALUES (N'711c81c5-997b-4ad1-824d-00a0e4237029', 0, N'fb1a1ad1-1e26-4922-bb49-a681fc01ed30', N'khachhang1@gmail.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'KHACHHANG1@GMAIL.COM', N'KHACHHANG1', N'AQAAAAIAAYagAAAAEM9+bYZRzcU+dnCRpUQHItrpsS1gk9yELh79T+ZFRcVCnVMr8MTCBKU+ocW6YW8yWw==', NULL, CAST(0 AS bit), N'bcf47bd2-8b4b-448b-b00c-600b43bbfc35', CAST(1 AS bit), CAST(0 AS bit), N'khachhang1'),
(N'7667fed0-c6f2-4dff-a12b-f4c5b064f9f2', 0, N'ce5f96d5-d52f-4039-823c-03cc9bdf85e1', N'nhanvien1@booking.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'NHANVIEN1@BOOKING.COM', N'NHANVIEN1', N'AQAAAAIAAYagAAAAECayK2xNLqsc1zsvb73kbhS/nDkAY1I3PdRbh3pzXXBAD/mxGz0KgYsBd6AkyhQA3Q==', NULL, CAST(0 AS bit), N'a652bf4d-9407-4c99-8465-b148e2e74f8d', CAST(1 AS bit), CAST(0 AS bit), N'nhanvien1'),
(N'7d328812-88db-422a-b83e-14d56dd91dde', 0, N'a3a1c257-08d9-4465-ab34-9f8d35edcf54', N'khachhang2@gmail.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'KHACHHANG2@GMAIL.COM', N'KHACHHANG2', N'AQAAAAIAAYagAAAAEHYWSeke9O9rJZSsYcx6+eOZ4RVuYO+Un1UODBRIE57gLfTk79Iiuu6yP7zbeY2CNA==', NULL, CAST(0 AS bit), N'4d08d3d4-341d-40d2-8a7c-ecf6e93a2430', CAST(1 AS bit), CAST(0 AS bit), N'khachhang2'),
(N'8e3ade7b-c691-4276-abd5-3e1fb0f02a05', 0, N'b4835b6a-3669-48ce-8496-69ede6e1aa1e', N'testuser@gmail.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'TESTUSER@GMAIL.COM', N'TESTUSER', N'AQAAAAIAAYagAAAAEDsoNw73Xg+hXDD1Fm8EMpLANvMCkU6oXJj+37wryxCNKpl6mpzJZC9oRUnizM2qgQ==', NULL, CAST(0 AS bit), N'10249c4c-755a-48e9-92b9-46b4cf9fcd3a', CAST(1 AS bit), CAST(0 AS bit), N'testuser'),
(N'c4b3adc5-a6a7-4f7e-bd1c-2806c328191b', 0, N'd384f5bf-81ff-4ec0-a2aa-4377f3d94e92', N'admin@booking.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'ADMIN@BOOKING.COM', N'ADMIN', N'AQAAAAIAAYagAAAAEHJXYAQhZDUNBWTNcgkCaOtc0r/eWeEjYGwJuD+g6pA9VwSNbM2RdpuZGFyJEIKi4g==', NULL, CAST(0 AS bit), N'dbabf772-6b59-46e1-b9b9-fa8cdb51540c', CAST(1 AS bit), CAST(0 AS bit), N'admin');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'Email', N'EmailConfirmed', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'SecurityStamp', N'TrangThai', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[AspNetUsers]'))
    SET IDENTITY_INSERT [AspNetUsers] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaKM', N'HinhAnh', N'MoTa', N'NgayBatDau', N'NgayKetThuc', N'PhanTramGiam', N'SoTienToiDaGiam', N'TenChuongTrinh', N'TrangThai') AND [object_id] = OBJECT_ID(N'[KhuyenMai]'))
    SET IDENTITY_INSERT [KhuyenMai] ON;
INSERT INTO [KhuyenMai] ([MaKM], [HinhAnh], [MoTa], [NgayBatDau], [NgayKetThuc], [PhanTramGiam], [SoTienToiDaGiam], [TenChuongTrinh], [TrangThai])
VALUES (N'DONGAM2026', N'215b54dc-a2eb-4d82-8091-e4e885928754.jpg', N'Ưu đãi sưởi ấm những chuyến đi cuối năm. Giảm giá sâu cho các tuyến tàu ra đảo nghỉ dưỡng.', '2026-11-01T00:00:00.0000000', '2026-12-25T00:00:00.0000000', 25.0E0, 250000.0, N'Mùa Đông Ấm Áp', N'Chưa diễn ra'),
(N'GIADO304', N'a64e7e7d-4495-4950-b2ec-ec62db4cfdbb.jpg', N'Ưu đãi cực lớn dành cho các tuyến tàu cao tốc du lịch trong kỳ nghỉ lễ 30/4 và 1/5.', '2026-04-25T00:00:00.0000000', '2026-05-05T00:00:00.0000000', 30.0E0, 400000.0, N'Mừng Đại Lễ - Giảm Giá Mê', N'Sắp diễn ra'),
(N'KM10', N'322b0521-7bf6-40e5-aeea-b28fcfb0c5fd.jpg', N'Chào mừng hệ thống WebAppBookingBoat đi vào hoạt động. Giảm ngay 50% cho tất cả các tuyến tàu cao tốc.', '2026-01-01T00:00:00.0000000', '2026-12-31T00:00:00.0000000', 50.0E0, 200000.0, N'Giảm giá khai trương', N'Chưa diễn ra'),
(N'SUMMER26', N'9c2fb5b6-9d1e-4a2c-91be-51c61d3d10d9.jpg', N'Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 10% khi đặt vé.', '2026-06-01T00:00:00.0000000', '2026-08-31T00:00:00.0000000', 10.0E0, 100000.0, N'Ưu đãi mùa hè rực rỡ', N'Chưa diễn ra'),
(N'SUMMER27', N'8b779b59-820c-48bd-bc11-b85e27021424.jpg', N'Tận hưởng kỳ nghỉ hè với ưu đãi cực khủng lên đến 30% khi đặt vé.', '2026-06-01T00:00:00.0000000', '2026-08-31T00:00:00.0000000', 30.0E0, 100000.0, N'Ưu đãi mùa hè hết cỡ', N'Chưa diễn ra'),
(N'TET2026', N'0b00c585-fb35-4462-9b5d-8e1aee28b429.jpg', N'Chương trình khuyến mãi đặc biệt dành cho khách hàng về quê ăn Tết hoặc du xuân cùng gia đình.', '2026-01-15T00:00:00.0000000', '2026-02-15T00:00:00.0000000', 30.0E0, 300000.0, N'Vui Tết sum vầy', N'Chưa diễn ra');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaKM', N'HinhAnh', N'MoTa', N'NgayBatDau', N'NgayKetThuc', N'PhanTramGiam', N'SoTienToiDaGiam', N'TenChuongTrinh', N'TrangThai') AND [object_id] = OBJECT_ID(N'[KhuyenMai]'))
    SET IDENTITY_INSERT [KhuyenMai] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaTau', N'HinhAnh', N'TenTau', N'TongSoGhe', N'TrangThai') AND [object_id] = OBJECT_ID(N'[Tau]'))
    SET IDENTITY_INSERT [Tau] ON;
INSERT INTO [Tau] ([MaTau], [HinhAnh], [TenTau], [TongSoGhe], [TrangThai])
VALUES (1, N'0885ba23-96cd-46c0-9cce-5b739d92c445_Tau-cao-toc-Phu-Quoc-Express-Vung-Tau-Con-Dao.jpg', N'Phú Quốc Express 1', 20, CAST(1 AS bit)),
(2, N'4f471677-b064-46f6-ba00-1840314b7226_tau-trung-trac-tren-tuyen-cao-toc-phan-thiet-phu-quy-1024x768-1.jpg', N'Phú Quốc Express 2', 20, CAST(1 AS bit)),
(3, N'ca4c374d-3d0c-48be-8b36-e79219a62796_Tau-cao-toc-Con-Dao-Express-36-1536x863.jpg', N'Phú Quốc Express 3', 20, CAST(1 AS bit)),
(4, N'dfa33889-692e-4f3a-84c2-491e2a402329_Tau-cao-toc-Trung-Nhi.jpg', N'Phú Quốc Express 4', 20, CAST(1 AS bit)),
(5, N'ca4c374d-3d0c-48be-8b36-e79219a62796_Tau-cao-toc-Con-Dao-Express-36-1536x863.jpg', N'Phú Quốc Express 5', 20, CAST(1 AS bit)),
(6, N'0885ba23-96cd-46c0-9cce-5b739d92c445_Tau-cao-toc-Phu-Quoc-Express-Vung-Tau-Con-Dao.jpg', N'Phú Quốc Express 6', 20, CAST(1 AS bit)),
(7, N'0885ba23-96cd-46c0-9cce-5b739d92c445_Tau-cao-toc-Phu-Quoc-Express-Vung-Tau-Con-Dao.jpg', N'Phú Quốc Express 7', 20, CAST(1 AS bit)),
(8, N'dfa33889-692e-4f3a-84c2-491e2a402329_Tau-cao-toc-Trung-Nhi.jpg', N'Phú Quốc Express 8', 20, CAST(1 AS bit)),
(9, N'4f471677-b064-46f6-ba00-1840314b7226_tau-trung-trac-tren-tuyen-cao-toc-phan-thiet-phu-quy-1024x768-1.jpg', N'Phú Quốc Express 9', 20, CAST(1 AS bit)),
(10, N'4f471677-b064-46f6-ba00-1840314b7226_tau-trung-trac-tren-tuyen-cao-toc-phan-thiet-phu-quy-1024x768-1.jpg', N'Phú Quốc Express 10', 20, CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaTau', N'HinhAnh', N'TenTau', N'TongSoGhe', N'TrangThai') AND [object_id] = OBJECT_ID(N'[Tau]'))
    SET IDENTITY_INSERT [Tau] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaTuyen', N'DiemDen', N'DiemDi', N'HinhAnh', N'KhoangCach', N'TenTuyen', N'ThoiGianDuKien') AND [object_id] = OBJECT_ID(N'[TuyenDuong]'))
    SET IDENTITY_INSERT [TuyenDuong] ON;
INSERT INTO [TuyenDuong] ([MaTuyen], [DiemDen], [DiemDi], [HinhAnh], [KhoangCach], [TenTuyen], [ThoiGianDuKien])
VALUES (1, N'Phú Quý', N'Phan Thiết', N'04e1440f-0c96-46c6-82b1-c10f63b4db23_phu-quy.jpg', 105.0E0, N'Phan Thiết - Phú Quý', '02:30:00'),
(2, N'Cát Bà', N'Hải Phòng', N'c31132fa-0e14-4787-8d05-fd22a23a3411_cat-ba.jpg', 30.0E0, N'Hải Phòng - Cát Bà', '00:45:00'),
(3, N'Côn Đảo', N'Vũng Tàu', N'7abf98e9-a440-4779-8a3a-529b3a400bbd_con-dao.jpg', 180.0E0, N'Vũng Tàu - Côn Đảo', '03:45:00'),
(4, N'Hòn Sơn', N'Rạch Giá', N'c051024a-ca91-4835-8751-6a6b40f5c124_hon-son.jpg', 65.0E0, N'Rạch Giá - Hòn Sơn', '01:30:00'),
(5, N'Lý Sơn', N'Sa Kỳ', N'a3c8c7b7-69df-4c73-9acd-11db5b8433e8_ly-son.jpg', 30.0E0, N'Sa Kỳ - Lý Sơn', '00:45:00'),
(6, N'Nam Du', N'Rạch Giá', N'6f1e4c0e-2db1-4540-b7a0-bdafd5a151cd_nam-du.jpg', 80.0E0, N'Rạch Giá - Nam Du', '02:15:00'),
(7, N'Phú Quốc', N'Hà Tiên', N'bcd3de60-2b13-4d14-95c5-37ca27af887d_phu-quoc.jpg', 45.0E0, N'Hà Tiên - Phú Quốc', '01:15:00');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaTuyen', N'DiemDen', N'DiemDi', N'HinhAnh', N'KhoangCach', N'TenTuyen', N'ThoiGianDuKien') AND [object_id] = OBJECT_ID(N'[TuyenDuong]'))
    SET IDENTITY_INSERT [TuyenDuong] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[AspNetUserRoles]'))
    SET IDENTITY_INSERT [AspNetUserRoles] ON;
INSERT INTO [AspNetUserRoles] ([RoleId], [UserId])
VALUES (N'2', N'7667fed0-c6f2-4dff-a12b-f4c5b064f9f2'),
(N'1', N'c4b3adc5-a6a7-4f7e-bd1c-2806c328191b');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[AspNetUserRoles]'))
    SET IDENTITY_INSERT [AspNetUserRoles] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaGhe', N'LoaiGhe', N'MaTau', N'TenGhe') AND [object_id] = OBJECT_ID(N'[Ghe]'))
    SET IDENTITY_INSERT [Ghe] ON;
INSERT INTO [Ghe] ([MaGhe], [LoaiGhe], [MaTau], [TenGhe])
VALUES (1, N'Thường', 1, N'T-01'),
(2, N'Thường', 1, N'T-02'),
(3, N'Thường', 1, N'T-03'),
(4, N'Thường', 1, N'T-04'),
(5, N'Thường', 1, N'T-05'),
(6, N'Thường', 1, N'T-06'),
(7, N'Thường', 1, N'T-07'),
(8, N'Thường', 1, N'T-08'),
(9, N'Thường', 1, N'T-09'),
(10, N'Thường', 1, N'T-10'),
(11, N'Thường', 1, N'T-11'),
(12, N'Thường', 1, N'T-12'),
(13, N'Thường', 1, N'T-13'),
(14, N'Thường', 1, N'T-14'),
(15, N'Thường', 1, N'T-15'),
(16, N'VIP', 1, N'V-16'),
(17, N'VIP', 1, N'V-17'),
(18, N'VIP', 1, N'V-18'),
(19, N'VIP', 1, N'V-19'),
(20, N'VIP', 1, N'V-20'),
(21, N'Thường', 2, N'T-01'),
(22, N'Thường', 2, N'T-02'),
(23, N'Thường', 2, N'T-03'),
(24, N'Thường', 2, N'T-04'),
(25, N'Thường', 2, N'T-05'),
(26, N'Thường', 2, N'T-06'),
(27, N'Thường', 2, N'T-07'),
(28, N'Thường', 2, N'T-08'),
(29, N'Thường', 2, N'T-09'),
(30, N'Thường', 2, N'T-10'),
(31, N'Thường', 2, N'T-11'),
(32, N'Thường', 2, N'T-12'),
(33, N'Thường', 2, N'T-13'),
(34, N'Thường', 2, N'T-14'),
(35, N'Thường', 2, N'T-15'),
(36, N'VIP', 2, N'V-16'),
(37, N'VIP', 2, N'V-17'),
(38, N'VIP', 2, N'V-18'),
(39, N'VIP', 2, N'V-19'),
(40, N'VIP', 2, N'V-20'),
(41, N'Thường', 3, N'T-01'),
(42, N'Thường', 3, N'T-02');
INSERT INTO [Ghe] ([MaGhe], [LoaiGhe], [MaTau], [TenGhe])
VALUES (43, N'Thường', 3, N'T-03'),
(44, N'Thường', 3, N'T-04'),
(45, N'Thường', 3, N'T-05'),
(46, N'Thường', 3, N'T-06'),
(47, N'Thường', 3, N'T-07'),
(48, N'Thường', 3, N'T-08'),
(49, N'Thường', 3, N'T-09'),
(50, N'Thường', 3, N'T-10'),
(51, N'Thường', 3, N'T-11'),
(52, N'Thường', 3, N'T-12'),
(53, N'Thường', 3, N'T-13'),
(54, N'Thường', 3, N'T-14'),
(55, N'Thường', 3, N'T-15'),
(56, N'VIP', 3, N'V-16'),
(57, N'VIP', 3, N'V-17'),
(58, N'VIP', 3, N'V-18'),
(59, N'VIP', 3, N'V-19'),
(60, N'VIP', 3, N'V-20'),
(61, N'Thường', 4, N'T-01'),
(62, N'Thường', 4, N'T-02'),
(63, N'Thường', 4, N'T-03'),
(64, N'Thường', 4, N'T-04'),
(65, N'Thường', 4, N'T-05'),
(66, N'Thường', 4, N'T-06'),
(67, N'Thường', 4, N'T-07'),
(68, N'Thường', 4, N'T-08'),
(69, N'Thường', 4, N'T-09'),
(70, N'Thường', 4, N'T-10'),
(71, N'Thường', 4, N'T-11'),
(72, N'Thường', 4, N'T-12'),
(73, N'Thường', 4, N'T-13'),
(74, N'Thường', 4, N'T-14'),
(75, N'Thường', 4, N'T-15'),
(76, N'VIP', 4, N'V-16'),
(77, N'VIP', 4, N'V-17'),
(78, N'VIP', 4, N'V-18'),
(79, N'VIP', 4, N'V-19'),
(80, N'VIP', 4, N'V-20');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaGhe', N'LoaiGhe', N'MaTau', N'TenGhe') AND [object_id] = OBJECT_ID(N'[Ghe]'))
    SET IDENTITY_INSERT [Ghe] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaKH', N'DiaChi', N'Email', N'HoTen', N'MaTK', N'NgaySinh', N'Sdt') AND [object_id] = OBJECT_ID(N'[KhachHang]'))
    SET IDENTITY_INSERT [KhachHang] ON;
INSERT INTO [KhachHang] ([MaKH], [DiaChi], [Email], [HoTen], [MaTK], [NgaySinh], [Sdt])
VALUES (1, NULL, N'khach.tran@gmail.com', N'Trần Thị Khách', N'8e3ade7b-c691-4276-abd5-3e1fb0f02a05', '1995-05-20T00:00:00.0000000', N'0912345678');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaKH', N'DiaChi', N'Email', N'HoTen', N'MaTK', N'NgaySinh', N'Sdt') AND [object_id] = OBJECT_ID(N'[KhachHang]'))
    SET IDENTITY_INSERT [KhachHang] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaLichTrinh', N'GiaVeCoBan', N'MaTau', N'MaTuyen', N'NgayGioCapBenDuKien', N'NgayGioKhoiHanh', N'SoGheTrong', N'TrangThai') AND [object_id] = OBJECT_ID(N'[LichTrinh]'))
    SET IDENTITY_INSERT [LichTrinh] ON;
INSERT INTO [LichTrinh] ([MaLichTrinh], [GiaVeCoBan], [MaTau], [MaTuyen], [NgayGioCapBenDuKien], [NgayGioKhoiHanh], [SoGheTrong], [TrangThai])
VALUES (1, 200000.0, 1, 1, '2026-04-18T10:30:00.0000000+07:00', '2026-04-18T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(2, 200000.0, 2, 2, '2026-04-18T10:30:00.0000000+07:00', '2026-04-18T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(3, 200000.0, 3, 3, '2026-04-19T10:30:00.0000000+07:00', '2026-04-19T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(4, 200000.0, 4, 4, '2026-04-21T10:30:00.0000000+07:00', '2026-04-21T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(5, 200000.0, 5, 5, '2026-04-22T10:30:00.0000000+07:00', '2026-04-22T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(6, 200000.0, 6, 6, '2026-04-24T10:30:00.0000000+07:00', '2026-04-24T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(7, 200000.0, 7, 7, '2026-04-24T10:30:00.0000000+07:00', '2026-04-24T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(8, 200000.0, 1, 1, '2026-04-25T10:30:00.0000000+07:00', '2026-04-25T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(9, 200000.0, 2, 2, '2026-04-26T10:30:00.0000000+07:00', '2026-04-26T08:00:00.0000000+07:00', 20, N'Sắp khởi hành'),
(10, 200000.0, 3, 3, '2026-04-26T10:30:00.0000000+07:00', '2026-04-26T08:00:00.0000000+07:00', 20, N'Sắp khởi hành');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaLichTrinh', N'GiaVeCoBan', N'MaTau', N'MaTuyen', N'NgayGioCapBenDuKien', N'NgayGioKhoiHanh', N'SoGheTrong', N'TrangThai') AND [object_id] = OBJECT_ID(N'[LichTrinh]'))
    SET IDENTITY_INSERT [LichTrinh] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaLog', N'BangTacDong', N'HanhDong', N'IpAddress', N'LoaiLog', N'MaTK', N'NoiDungChiTiet', N'ThoiGian') AND [object_id] = OBJECT_ID(N'[Logs]'))
    SET IDENTITY_INSERT [Logs] ON;
INSERT INTO [Logs] ([MaLog], [BangTacDong], [HanhDong], [IpAddress], [LoaiLog], [MaTK], [NoiDungChiTiet], [ThoiGian])
VALUES (1, N'System', N'Khởi tạo hệ thống', N'127.0.0.1', N'Info', N'c4b3adc5-a6a7-4f7e-bd1c-2806c328191b', N'Hệ thống đã khởi tạo dữ liệu mẫu (Seed Data) thành công.', '2026-04-13T09:00:00.0000000'),
(2, N'AspNetUsers', N'Cấu hình bảo mật', N'127.0.0.1', N'Info', N'c4b3adc5-a6a7-4f7e-bd1c-2806c328191b', N'Thiết lập quyền Quản trị viên (Admin) cho hệ thống.', '2026-04-13T09:00:05.0000000');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaLog', N'BangTacDong', N'HanhDong', N'IpAddress', N'LoaiLog', N'MaTK', N'NoiDungChiTiet', N'ThoiGian') AND [object_id] = OBJECT_ID(N'[Logs]'))
    SET IDENTITY_INSERT [Logs] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaNV', N'ChucVu', N'Email', N'HoTen', N'Luong', N'MaTK', N'Sdt', N'TrangThai') AND [object_id] = OBJECT_ID(N'[NhanVien]'))
    SET IDENTITY_INSERT [NhanVien] ON;
INSERT INTO [NhanVien] ([MaNV], [ChucVu], [Email], [HoTen], [Luong], [MaTK], [Sdt], [TrangThai])
VALUES (1, N'Admin', N'maihuy@booking.com', N'Mai Nhứt Huy', 0.0, N'c4b3adc5-a6a7-4f7e-bd1c-2806c328191b', N'0386747090', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaNV', N'ChucVu', N'Email', N'HoTen', N'Luong', N'MaTK', N'Sdt', N'TrangThai') AND [object_id] = OBJECT_ID(N'[NhanVien]'))
    SET IDENTITY_INSERT [NhanVien] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaHoaDon', N'GhiChu', N'MaKH', N'MaKM', N'MaNV', N'NgayLap', N'NgayThanhToan', N'PhuongThucTT', N'SoLuongVe', N'SoTienGiam', N'TamTinh', N'TongTien', N'TrangThai') AND [object_id] = OBJECT_ID(N'[HoaDon]'))
    SET IDENTITY_INSERT [HoaDon] ON;
INSERT INTO [HoaDon] ([MaHoaDon], [GhiChu], [MaKH], [MaKM], [MaNV], [NgayLap], [NgayThanhToan], [PhuongThucTT], [SoLuongVe], [SoTienGiam], [TamTinh], [TongTien], [TrangThai])
VALUES (1, N'', 1, N'KM10', 1, '2026-04-17T22:11:39.5545477+07:00', NULL, N'Tiền mặt', 1, 20000.0, 200000.0, 180000.0, N'Đã thanh toán'),
(2, N'', 1, N'KM10', 1, '2026-04-17T22:11:39.5545614+07:00', NULL, N'Tiền mặt', 1, 20000.0, 200000.0, 180000.0, N'Đã thanh toán'),
(3, N'', 1, N'KM10', 1, '2026-04-17T22:11:39.5545667+07:00', NULL, N'Tiền mặt', 1, 20000.0, 200000.0, 180000.0, N'Đã thanh toán'),
(4, N'', 1, N'KM10', 1, '2026-04-17T22:11:39.5545619+07:00', NULL, N'Tiền mặt', 1, 20000.0, 200000.0, 180000.0, N'Đã thanh toán');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaHoaDon', N'GhiChu', N'MaKH', N'MaKM', N'MaNV', N'NgayLap', N'NgayThanhToan', N'PhuongThucTT', N'SoLuongVe', N'SoTienGiam', N'TamTinh', N'TongTien', N'TrangThai') AND [object_id] = OBJECT_ID(N'[HoaDon]'))
    SET IDENTITY_INSERT [HoaDon] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaDanhGia', N'HinhAnh', N'HoaDonMaHoaDon', N'LichTrinhMaLichTrinh', N'MaHoaDon', N'NgayDanhGia', N'NgayPhanHoi', N'NoiDung', N'PhanHoiAdmin', N'SoSao', N'TrangThai') AND [object_id] = OBJECT_ID(N'[DanhGia]'))
    SET IDENTITY_INSERT [DanhGia] ON;
INSERT INTO [DanhGia] ([MaDanhGia], [HinhAnh], [HoaDonMaHoaDon], [LichTrinhMaLichTrinh], [MaHoaDon], [NgayDanhGia], [NgayPhanHoi], [NoiDung], [PhanHoiAdmin], [SoSao], [TrangThai])
VALUES (1, N'6a4b2c8d-1e5f-4a3b-9c2d-8e7f6a5b4c3d_review-phu-quoc.jpg', NULL, NULL, 1, '2026-04-10T08:30:00.0000000', '2026-04-10T14:00:00.0000000', N'Chuyến đi tuyệt vời, tàu chạy rất êm và đúng giờ. Nhân viên hỗ trợ nhiệt tình!', N'Cảm ơn bạn đã ủng hộ WebAppBookingBoat! Rất mong được phục vụ bạn trong những chuyến đi tới.', 5, N'Đã hiển thị'),
(2, N'8e7d6c5b-4a3f-4e2d-9c1b-0a9b8c7d6e5f_review-thang-long.jpg', NULL, NULL, 2, '2026-04-11T15:20:00.0000000', '2026-04-12T09:15:00.0000000', N'Chất lượng ghế VIP rất tốt, tuy nhiên đồ ăn nhẹ trên tàu hơi ít lựa chọn.', N'Chào bạn, Admin ghi nhận góp ý và sẽ làm việc với bếp tàu để cải thiện thực đơn ạ!', 4, N'Đã hiển thị'),
(3, N'2c3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5e6f_view-bien.jpg', NULL, NULL, 3, '2026-04-17T20:11:39.5546008+07:00', NULL, N'Đặt vé cực nhanh, thanh toán tiện lợi. Sẽ quay lại!', NULL, 5, N'Chờ duyệt'),
(4, N'5f4e3d2c-1b0a-4c9d-8e7f-6a5b4c3d2e1f_tau-phu-quy.jpg', NULL, NULL, 4, '2026-04-17T22:11:39.5546014+07:00', NULL, N'Gia đình mình đi tuyến Hà Tiên - Phú Quốc rất hài lòng...', NULL, 5, N'Chờ duyệt');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaDanhGia', N'HinhAnh', N'HoaDonMaHoaDon', N'LichTrinhMaLichTrinh', N'MaHoaDon', N'NgayDanhGia', N'NgayPhanHoi', N'NoiDung', N'PhanHoiAdmin', N'SoSao', N'TrangThai') AND [object_id] = OBJECT_ID(N'[DanhGia]'))
    SET IDENTITY_INSERT [DanhGia] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaVe', N'GiaVe', N'MaGhe', N'MaHoaDon', N'MaLichTrinh', N'TrangThai') AND [object_id] = OBJECT_ID(N'[Ve]'))
    SET IDENTITY_INSERT [Ve] ON;
INSERT INTO [Ve] ([MaVe], [GiaVe], [MaGhe], [MaHoaDon], [MaLichTrinh], [TrangThai])
VALUES (1, 180000.0, 2, 1, 2, N'Hợp lệ'),
(2, 180000.0, 3, 2, 1, N'Hợp lệ'),
(3, 180000.0, 4, 3, 1, N'Hợp lệ'),
(4, 180000.0, 5, 4, 2, N'Hợp lệ');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MaVe', N'GiaVe', N'MaGhe', N'MaHoaDon', N'MaLichTrinh', N'TrangThai') AND [object_id] = OBJECT_ID(N'[Ve]'))
    SET IDENTITY_INSERT [Ve] OFF;
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_DanhGia_HoaDonMaHoaDon] ON [DanhGia] ([HoaDonMaHoaDon]);
GO

CREATE INDEX [IX_DanhGia_LichTrinhMaLichTrinh] ON [DanhGia] ([LichTrinhMaLichTrinh]);
GO

CREATE UNIQUE INDEX [IX_DanhGia_MaHoaDon] ON [DanhGia] ([MaHoaDon]);
GO

CREATE UNIQUE INDEX [IX_Ghe_MaTau_TenGhe] ON [Ghe] ([MaTau], [TenGhe]);
GO

CREATE INDEX [IX_HoaDon_MaKH] ON [HoaDon] ([MaKH]);
GO

CREATE INDEX [IX_HoaDon_MaKM] ON [HoaDon] ([MaKM]);
GO

CREATE INDEX [IX_HoaDon_MaNV] ON [HoaDon] ([MaNV]);
GO

CREATE UNIQUE INDEX [IX_KhachHang_Email] ON [KhachHang] ([Email]);
GO

CREATE UNIQUE INDEX [IX_KhachHang_MaTK] ON [KhachHang] ([MaTK]) WHERE [MaTK] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_KhachHang_Sdt] ON [KhachHang] ([Sdt]);
GO

CREATE UNIQUE INDEX [IX_LichTrinh_MaTau_NgayGioKhoiHanh] ON [LichTrinh] ([MaTau], [NgayGioKhoiHanh]);
GO

CREATE INDEX [IX_LichTrinh_MaTuyen] ON [LichTrinh] ([MaTuyen]);
GO

CREATE INDEX [IX_Logs_MaTK] ON [Logs] ([MaTK]);
GO

CREATE UNIQUE INDEX [IX_NhanVien_Email] ON [NhanVien] ([Email]);
GO

CREATE UNIQUE INDEX [IX_NhanVien_MaTK] ON [NhanVien] ([MaTK]) WHERE [MaTK] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_NhanVien_Sdt] ON [NhanVien] ([Sdt]);
GO

CREATE UNIQUE INDEX [IX_Tau_TenTau] ON [Tau] ([TenTau]);
GO

CREATE UNIQUE INDEX [IX_TuyenDuong_DiemDi_DiemDen] ON [TuyenDuong] ([DiemDi], [DiemDen]);
GO

CREATE INDEX [IX_Ve_MaGhe] ON [Ve] ([MaGhe]);
GO

CREATE INDEX [IX_Ve_MaHoaDon] ON [Ve] ([MaHoaDon]);
GO

CREATE UNIQUE INDEX [IX_Ve_MaLichTrinh_MaGhe] ON [Ve] ([MaLichTrinh], [MaGhe]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260417151141_InitialCreate', N'8.0.23');
GO

COMMIT;
GO

