using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(255, ErrorMessage = "Имя не должно превышать 255 символов")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [StringLength(255, ErrorMessage = "Email не должен превышать 255 символов")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Роль")]
        public int? RoleId { get; set; }

        [ForeignKey("RoleId")]
        [Display(Name = "Роль")]
        public virtual Role Role { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Shift> Shifts { get; set; }
        public virtual Employee Employee { get; set; }
    }
}