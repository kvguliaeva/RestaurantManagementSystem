using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        [Display(Name = "Заказ")]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Required]
        [Display(Name = "Дата платежа")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Сумма")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Метод оплаты")]
        public string PaymentMethod { get; set; } = "Наличные";

        [Display(Name = "Статус оплаты")]
        public string Status { get; set; } = "Завершено";

        [Display(Name = "Комментарий")]
        [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов")]
        public string Comment { get; set; }
    }
}