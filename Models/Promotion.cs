using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionID { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(20)]
        public string DiscountType { get; set; } = "Percentage"; // or "FixedAmount"

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        public bool IsActive { get; set; }


        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new HashSet<ProductPromotion>();
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
