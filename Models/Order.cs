using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public int? PromotionID { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("PromotionID")]
        public virtual Promotion? Promotion { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
    }
}
