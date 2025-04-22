using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class WishlistItem
    {
        [Key]
        public int WishlistItemID { get; set; }

        [Required]
        public int WishlistID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("WishlistID")]
        public virtual Wishlist Wishlist { get; set; } = null!;

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; } = null!;
    }
}
