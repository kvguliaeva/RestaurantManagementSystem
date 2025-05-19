using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }

        [Required]
        [Display(Name = "Заказ")]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Required]
        [Display(Name = "Блюдо")]
        public int DishId { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Цена за единицу")]
        public decimal Price { get; set; }

        [Display(Name = "Комментарий")]
        [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов")]
        public string? Comment { get; set; }
    }
}