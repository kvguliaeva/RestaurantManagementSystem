using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Shift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShiftId { get; set; }

        [Required]
        [Display(Name = "Сотрудник")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [Display(Name = "Дата смены")]
        public DateTime ShiftDate { get; set; }

        [Required]
        [Display(Name = "Время начала")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "Время окончания")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Запланирована";

        [Display(Name = "Комментарий")]
        [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов")]
        public string Comment { get; set; }

        [Display(Name = "Отработанные часы")]
        public double WorkedHours { get; set; }
    }
}