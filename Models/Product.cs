using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CategoryID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal OriginalPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountedPrice { get; set; }

        [NotMapped]
        public decimal CurrentPrice => DiscountedPrice ?? OriginalPrice;

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;


        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new HashSet<ProductPromotion>();
    }
}
