using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Название категории обязательно")]
        [StringLength(255, ErrorMessage = "Название не должно превышать 255 символов")]
        [Display(Name = "Название категории")]
        public string Name { get; set; }

        [Display(Name = "Описание категории")]
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }

        public virtual ICollection<Dish> Dishes { get; set; }
    }
}