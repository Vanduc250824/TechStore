using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TechStore.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Danh mục không được để trống.")]
        public int CategoryID { get; set; }

        public virtual Category Category { get; set; }

        [Required(ErrorMessage = "Giá không được để trống.")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Giá không được để trống.")]
        public decimal DiscountedPrice { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Hình ảnh không được để trống.")]

        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        public required string Description { get; set; }

        [NotMapped] // Không lưu vào database
        public required IFormFile ImageFile { get; set; }


    }
}