using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        public int OrderID { get; set; }
        public int ProductID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Thuộc tính Quantity
        [Required]
        public int Quantity { get; set; }

        // Mối quan hệ với bảng Orders
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        // Mối quan hệ với bảng Products
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }

}
