using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public required string UserID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Total => Quantity * UnitPrice;

        [ForeignKey("UserID")]
        public required ApplicationUser User { get; set; }

        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
    }
} 