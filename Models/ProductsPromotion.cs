using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Http;

namespace TechStore.Models
{
    public class ProductsPromotion
    {
        [Key]
        public int ProductPromotionID { get; set; }
        public int ProductID { get; set; }
        public int PromotionID { get; set; }

        [ValidateNever]
        public Product Product { get; set; }

        [ValidateNever]
        public Promotion Promotion { get; set; }
    }
}