using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Dish
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DishId { get; set; }

        [Required(ErrorMessage = "Название блюда обязательно")]
        [StringLength(255, ErrorMessage = "Название не должно превышать 255 символов")]
        [Display(Name = "Название блюда")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, 10000, ErrorMessage = "Цена должна быть между 0.01 и 10000")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [Display(Name = "Категория")]
        public virtual Category Category { get; set; }

        [Display(Name = "Описание")]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; }

        [Display(Name = "Доступно")]
        public bool? IsAvailable { get; set; } = true;

        [Display(Name = "Изображение")]
        public string ImagePath { get; set; }

        [NotMapped] // Это свойство не будет сохраняться в БД
        [Display(Name = "Изображение блюда")]
        public IFormFile ImageFile { get; set; }
    }
}