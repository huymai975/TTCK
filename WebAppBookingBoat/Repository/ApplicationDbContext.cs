    using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using System.Linq;

namespace WebAppBookingBoat.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; } 
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
            modelBuilder.Entity<TaiKhoan>().HasIndex(tk => tk.TenDangNhap).IsUnique();
            modelBuilder.Entity<NhanVien>().HasIndex(nv => nv.Email).IsUnique();
            modelBuilder.Entity<NhanVien>().HasIndex(nv => nv.Sdt).IsUnique();
            modelBuilder.Entity<NhanVien>().HasIndex(nv => nv.MaTK).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(kh => kh.Email).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(kh => kh.Sdt).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(kh => kh.MaTK).IsUnique();
            modelBuilder.Entity<Tau>().HasIndex(t => t.TenTau).IsUnique();
            modelBuilder.Entity<Ghe>().HasIndex(g => new { g.MaTau, g.TenGhe }).IsUnique();
            modelBuilder.Entity<TuyenDuong>().HasIndex(td => new { td.DiemDi, td.DiemDen }).IsUnique();
            modelBuilder.Entity<LichTrinh>().HasIndex(lt => new { lt.MaTau, lt.NgayGioKhoiHanh }).IsUnique();
            modelBuilder.Entity<DanhGia>().HasIndex(d => d.MaVe).IsUnique();

            // --- 2. CHECK CONSTRAINTS ---

            // Khách hàng
            modelBuilder.Entity<KhachHang>().ToTable(t => {
                t.HasCheckConstraint("CK_KH_Sdt_Format", "LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'");
                t.HasCheckConstraint("CK_KH_Email_Format", "[Email] LIKE '%_@_%._%'");
            });

            // Nhân viên (Đổi tên Constraint để không trùng với Khách hàng)
            modelBuilder.Entity<NhanVien>().ToTable(t => {
                t.HasCheckConstraint("CK_NV_Sdt_Format", "LEN([Sdt]) >= 10 AND [Sdt] NOT LIKE '%[^0-9]%'");
                t.HasCheckConstraint("CK_NV_Email_Format", "[Email] LIKE '%_@_%._%'");
            });

            // Lịch trình
            modelBuilder.Entity<LichTrinh>(e => {
                e.ToTable(t => t.HasCheckConstraint("CK_LT_ThoiGian", "[NgayGioCapBenDuKien] > [NgayGioKhoiHanh]"));
                e.ToTable(t => t.HasCheckConstraint("CK_LT_GheTrong", "[SoGheTrong] >= 0"));
                e.ToTable(t => t.HasCheckConstraint("CK_LT_GiaVe", "[GiaVeCoBan] >= 0"));
            });

            // Khuyến mãi
            modelBuilder.Entity<KhuyenMai>(e => {
                e.ToTable(t => t.HasCheckConstraint("CK_KM_PhanTram", "[PhanTramGiam] >= 0 AND [PhanTramGiam] <= 100"));
                e.ToTable(t => t.HasCheckConstraint("CK_KM_ThoiGian", "[NgayKetThuc] > [NgayBatDau]"));
            });

            // Tài khoản (Thêm N cho tiếng Việt)
            modelBuilder.Entity<TaiKhoan>().ToTable(t =>
                t.HasCheckConstraint("CK_TK_VaiTro", "[VaiTro] IN (N'Admin', N'Nhân viên', N'Khách hàng')"));

            // Đánh giá (Thêm N cho tiếng Việt)
            modelBuilder.Entity<DanhGia>().ToTable(t => {
                t.HasCheckConstraint("CK_DG_SoSao", "[SoSao] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DG_TrangThai", "[TrangThai] IN (N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn')");
            });

            // Ghế (Thêm N cho tiếng Việt)
            modelBuilder.Entity<Ghe>().ToTable(t =>
                t.HasCheckConstraint("CK_Ghe_LoaiGhe", "[LoaiGhe] IN (N'Thường', N'VIP')"));

            // Hóa đơn
            modelBuilder.Entity<HoaDon>(e => {
                e.ToTable(t => t.HasCheckConstraint("CK_HD_Tien", "[TamTinh] >= 0 AND [SoTienGiam] >= 0 AND [TongTien] >= 0"));
                e.ToTable(t => t.HasCheckConstraint("CK_HD_SoLuong", "[SoLuongVe] > 0"));
            });

            // Tàu
            // Ghi chú: Ràng buộc tổng số ghế phải bằng ghế thường + ghế VIP tại tầng Database
            modelBuilder.Entity<Tau>()
                .ToTable(t => t.HasCheckConstraint("CK_Tau_TongGhe", "[TongSoGhe] = [SoGheThuong] + [SoGheVIP]"));

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

            // SeedData ở đây
            DbInitializer.Seed(modelBuilder);
        }
    }
}