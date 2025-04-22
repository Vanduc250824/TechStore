using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class ProductPromotion
    {
        [Key]
        public int ProductPromotionID { get; set; }

        public int ProductID { get; set; }
        public int PromotionID { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Promotion Promotion { get; set; } = null!;
    }

}