using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TechStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên phải từ {2} đến {1} ký tự", MinimumLength = 2)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Địa chỉ phải từ {2} đến {1} ký tự", MinimumLength = 5)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Ảnh đại diện")]
        public string ProfilePicture { get; set; } = "~/Images/Avatars/Image_Default.png";

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new HashSet<ShoppingCart>();
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new HashSet<ShoppingCartItem>();
        
        public virtual ICollection<Wishlist> Wishlist { get; set; } = new HashSet<Wishlist>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new HashSet<WishlistItem>();
    }
}
