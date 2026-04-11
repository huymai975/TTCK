using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.Models
{
    public class AppUser : IdentityUser
    {
        [Display(Name = "Trạng thái tài khoản")]
        public bool TrangThai { get; set; } = true; // Mặc định tạo tài khoản là hoạt động
    }
}
