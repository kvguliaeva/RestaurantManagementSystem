using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Пользователь")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [Display(Name = "Пользователь")]
        public virtual User User { get; set; }

        [Required]
        [Display(Name = "Дата заказа")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Общая сумма")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Статус заказа")]
        public string Status { get; set; } = "Новый";

        [Display(Name = "Комментарий")]
        [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")]
        public string? Comment { get; set; }

        public virtual ICollection<OrderItem>? OrderItems { get; set; }
        public virtual Payment? Payment { get; set; }
    }
}