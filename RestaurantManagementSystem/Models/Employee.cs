using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmployeeId { get; set; }

        [Required]
        [Display(Name = "Пользователь")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [Display(Name = "Должность")]
        public string Position { get; set; }

        [Required]
        [Display(Name = "Дата найма")]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime HireDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Зарплата")]
        public decimal Salary { get; set; }

        [Display(Name = "Дата увольнения")]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? TerminationDate { get; set; }

        [Display(Name = "Причина увольнения")]
        [StringLength(500, ErrorMessage = "Причина не должна превышать 500 символов")]
        public string? TerminationReason { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Активен";

        [Display(Name = "Номер банковского счета")]
        [StringLength(50, ErrorMessage = "Номер счета не должен превышать 50 символов")]
        public string? BankAccountNumber { get; set; }
    }
}