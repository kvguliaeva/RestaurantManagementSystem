using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        [Required]
        [Display(Name = "Дата отчета")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Тип отчета")]
        public string ReportType { get; set; }

        [Required]
        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Общая выручка")]
        public decimal TotalRevenue { get; set; }

        [Required]
        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Общая зарплата")]
        public decimal TotalSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Итог")]
        public decimal Total { get; set; }

        [Display(Name = "Период начала")]
        public DateTime PeriodStart { get; set; }

        [Display(Name = "Период окончания")]
        public DateTime PeriodEnd { get; set; }

        [Display(Name = "Комментарий")]
        [StringLength(2000, ErrorMessage = "Комментарий не должен превышать 2000 символов")]
        public string Comment { get; set; }
    }
}