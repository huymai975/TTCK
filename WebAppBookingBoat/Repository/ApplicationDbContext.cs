
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;

namespace WebAppBookingBoat.Repository
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<Tau> Taus { get; set; }
        public DbSet<Ghe> Ghes { get; set; }
        public DbSet<TuyenDuong> TuyenDuongs { get; set; }
        public DbSet<LichTrinh> LichTrinhs { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<Ve> Ves { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. RÀNG BUỘC UNIQUE ---
            modelBuilder.Entity<NhanVien>().HasIndex(nv => nv.Email).IsUnique();
            modelBuilder.Entity<NhanVien>().HasIndex(nv => nv.Sdt).IsUnique();
            modelBuilder.Entity<NhanVien>().HasIndex(nv => nv.MaTK).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(kh => kh.Email).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(kh => kh.Sdt).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(kh => kh.MaTK).IsUnique().HasFilter("[MaTK] IS NOT NULL");
            modelBuilder.Entity<Tau>().HasIndex(t => t.TenTau).IsUnique();
            modelBuilder.Entity<Ghe>().HasIndex(g => new { g.MaTau, g.TenGhe }).IsUnique();
            modelBuilder.Entity<TuyenDuong>().HasIndex(td => new { td.DiemDi, td.DiemDen }).IsUnique();
            modelBuilder.Entity<LichTrinh>().HasIndex(lt => new { lt.MaTau, lt.NgayGioKhoiHanh }).IsUnique();
            modelBuilder.Entity<DanhGia>().HasIndex(d => d.MaHoaDon).IsUnique();

            // --- 2. CHECK CONSTRAINTS ---

            // Khách hàng
            modelBuilder.Entity<KhachHang>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_KH_Sdt_Format", "LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'");
                t.HasCheckConstraint("CK_KH_Email_Format", "[Email] LIKE '%_@_%._%'");
            });

            // Nhân viên
            modelBuilder.Entity<NhanVien>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_NV_Sdt_Format", "LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'");
                t.HasCheckConstraint("CK_NV_Email_Format", "[Email] LIKE '%_@_%._%'");
            });

            // Tuyến đường
            modelBuilder.Entity<TuyenDuong>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_TD_DiemKhacNhau", "[DiemDi] <> [DiemDen]");
            });

            // Lịch trình
            modelBuilder.Entity<LichTrinh>(e =>
            {
                e.ToTable(t => t.HasCheckConstraint("CK_LT_ThoiGian", "[NgayGioCapBenDuKien] > [NgayGioKhoiHanh]"));
                e.ToTable(t => t.HasCheckConstraint("CK_LT_GheTrong", "[SoGheTrong] >= 0"));
                e.ToTable(t => t.HasCheckConstraint("CK_LT_GiaVe", "[GiaVeCoBan] >= 0"));
            });

            modelBuilder.Entity<LichTrinh>().ToTable(t =>
            {
                // Trạng thái chuyến đi
                t.HasCheckConstraint("CK_LT_TrangThai", "[TrangThai] IN (N'Sắp khởi hành', N'Đang vận hành', N'Hoàn thành', N'Đã hủy')");
            });

            //Khuyến mãi(Đảm bảo ngày kết thúc sau ngày bắt đầu và các giá trị không âm)
            modelBuilder.Entity<KhuyenMai>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_KM_PhanTram", "[PhanTramGiam] >= 0 AND [PhanTramGiam] <= 100");
                t.HasCheckConstraint("CK_KM_ThoiGian", "[NgayKetThuc] > [NgayBatDau]");
                t.HasCheckConstraint("CK_KM_SoTienToiDa", "[SoTienToiDaGiam] >= 0");
            });

            // Đánh giá (Cập nhật ràng buộc cho trường mới)
            modelBuilder.Entity<DanhGia>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_DG_SoSao", "[SoSao] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DG_TrangThai", "[TrangThai] IN (N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn')");
                // Kiểm tra logic: Nếu đã có phản hồi admin thì ngày phản hồi không được để trống (Tùy chọn)
                t.HasCheckConstraint("CK_DG_NgayPhanHoi", "[NgayPhanHoi] IS NULL OR [NgayPhanHoi] >= [NgayDanhGia]");
            });

            // Ghế (Thêm N cho tiếng Việt)
            modelBuilder.Entity<Ghe>().ToTable(t =>
                t.HasCheckConstraint("CK_Ghe_LoaiGhe", "[LoaiGhe] IN (N'Thường', N'VIP')"));

            // Hóa đơn
            modelBuilder.Entity<HoaDon>(e =>
            {
                e.ToTable(t => t.HasCheckConstraint("CK_HD_Tien", "[TamTinh] >= 0 AND [SoTienGiam] >= 0 AND [TongTien] >= 0"));
                e.ToTable(t => t.HasCheckConstraint("CK_HD_SoLuong", "[SoLuongVe] > 0"));
            });

            // Thêm ràng buộc Unique cho bảng Ve: Một ghế chỉ được xuất hiện 1 lần trong 1 lịch trình
            modelBuilder.Entity<Ve>()
    .HasIndex(v => new { v.MaLichTrinh, v.MaGhe })
    .IsUnique()
    .HasFilter("[TrangThai] <> N'Đã hủy'");

            modelBuilder.Entity<Ve>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_Ve_GiaVe", "[GiaVe] >= 0");
                t.HasCheckConstraint("CK_Ve_TrangThai", "[TrangThai] IN (N'Đang chờ', N'Hợp lệ', N'Đã hủy')");
            });

            // --- 3. CẤU HÌNH DECIMAL ---
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // --- 4. SIẾT CHẶT VIỆC XÓA (RESTRICT) ---
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // 1 Hóa đơn <-> 1 Đánh giá
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.HoaDon)
                .WithOne(h => h.DanhGia) // Bạn cần đảm bảo trong class HoaDon có: public virtual DanhGia? DanhGia { get; set; }
                .HasForeignKey<DanhGia>(d => d.MaHoaDon) // Chỉ định MaHoaDon trong bảng DanhGia là Foreign Key
                .OnDelete(DeleteBehavior.Restrict); // Tránh xóa dây chuyền nếu không cần thiết


            // --- 5. CẤU HÌNH BẢNG LOG ---
            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasOne(l => l.AppUser)
                      .WithMany()
                      .HasForeignKey(l => l.MaTK)
                      .OnDelete(DeleteBehavior.SetNull); // Quan trọng: Tránh lỗi Restrict khi xóa User
            });
            modelBuilder.Entity<Log>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_Log_Loai", "[LoaiLog] IN ('Info', 'Warning', 'Error', 'Critical')");
            });

            // --- CẤU HÌNH QUAN HỆ 1-1 CHI TIẾT ---

            // 1 tài khoản AppUser <-> 1 hồ sơ NhanVien
            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.AppUser)
                .WithOne()
                .HasForeignKey<NhanVien>(nv => nv.MaTK)
                .OnDelete(DeleteBehavior.Restrict);

            // 1 tài khoản AppUser <-> 1 hồ sơ KhachHang
            modelBuilder.Entity<KhachHang>()
                .HasOne(kh => kh.AppUser)
                .WithOne()
                .HasForeignKey<KhachHang>(kh => kh.MaTK)
                .OnDelete(DeleteBehavior.Restrict);

            // SeedData ở đây
            DbInitializer.Seed(modelBuilder);
        }
    }
}