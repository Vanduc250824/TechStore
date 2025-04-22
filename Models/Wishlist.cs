using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class Wishlist
    {
        [Key]
        public int WishlistID { get; set; }

        [Required]
        public string UserID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new HashSet<WishlistItem>();
    }
}
