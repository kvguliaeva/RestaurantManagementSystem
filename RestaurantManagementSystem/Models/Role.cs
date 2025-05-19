using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        [Display(Name = "Название роли")]
        public string Name { get; set; }

        [Display(Name = "Описание роли")]
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}