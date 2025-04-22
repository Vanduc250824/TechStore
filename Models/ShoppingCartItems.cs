using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int CartItemID { get; set; }

        [Required]
        public int CartID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [ForeignKey("CartID")]
        public virtual ShoppingCart ShoppingCart { get; set; } = null!;

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; } = null!;
    }
}
