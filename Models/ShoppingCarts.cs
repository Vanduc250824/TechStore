using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class ShoppingCart
    {
        [Key]
        public int CartID { get; set; }

        [Required]
        public string UserID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<ShoppingCartItem> CartItems { get; set; } = new HashSet<ShoppingCartItem>();
    }
}
