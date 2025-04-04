using System;
using System.ComponentModel.DataAnnotations;

namespace TechStore.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionID { get; set; }

        [Required(ErrorMessage = "Tên khuyến mãi không được để trống.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giảm giá không được để trống.")]
        [Range(0, 100, ErrorMessage = "Giảm giá phải từ 0 đến 100%.")]
        public decimal DiscountPercent { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        public DateTime EndDate { get; set; }

        public required string Description { get; set; }
    }
}
