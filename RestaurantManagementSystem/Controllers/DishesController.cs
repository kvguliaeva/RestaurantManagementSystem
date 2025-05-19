using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace RestaurantManagementSystem.Controllers
{
    public class DishesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DishesController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Dishes
        public async Task<IActionResult> Index()
        {
            var dishes = await _context.Dishes
                .Include(d => d.Category)
                .OrderBy(d => d.Name)
                .ToListAsync();
            return View(dishes);
        }

        // GET: Dishes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.DishId == id);

            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }

        // GET: Dishes/Create
        public IActionResult Create()
        {
            var categories = _context.Categories.ToList();
            if (!categories.Any())
            {
                TempData["Error"] = "Сначала добавьте категории";
                return RedirectToAction("Index");
            }

            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name");
            return View(new Dish { IsAvailable = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dish dish)
        {
            // Обработка IsAvailable
            dish.IsAvailable = Request.Form["IsAvailable"].Count > 0 &&
                             bool.TryParse(Request.Form["IsAvailable"][0], out var available) &&
                             available;

            // Валидация цены
            if (dish.Price <= 0)
            {
                ModelState.AddModelError("Price", "Цена должна быть больше нуля");
            }

            // Обработка изображения
            if (dish.ImageFile != null && dish.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "dishes");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dish.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dish.ImageFile.CopyToAsync(fileStream);
                }

                dish.ImagePath = $"/images/dishes/{uniqueFileName}";
            }

            // Убираем проверку обязательности ImagePath
            ModelState.Remove("ImagePath");

            // Убираем проверку обязательности Category, так как она уже есть в модели
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(dish);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
                    ModelState.AddModelError("", "Не удалось сохранить изменения. Попробуйте еще раз.");
                }
            }

            // Логирование ошибок валидации
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Ошибка в {state.Key}: {error.ErrorMessage}");
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", dish.CategoryId);
            return View(dish);
        }

        // GET: Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            var categories = _context.Categories.ToList();
            if (!categories.Any())
            {
                TempData["Error"] = "Сначала добавьте категории";
                return RedirectToAction("Index");
            }

            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", dish.CategoryId);
            return View(dish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dish dish)
        {
            if (id != dish.DishId)
            {
                return NotFound();
            }

            // Получаем текущее блюдо из БД
            var existingDish = await _context.Dishes.FindAsync(id);
            if (existingDish == null)
            {
                return NotFound();
            }

            // Обработка IsAvailable
            existingDish.IsAvailable = Request.Form["IsAvailable"].Count > 0 &&
                                    bool.TryParse(Request.Form["IsAvailable"][0], out var available) &&
                                    available;

            // Валидация цены
            if (dish.Price <= 0)
            {
                ModelState.AddModelError("Price", "Цена должна быть больше нуля");
            }

            // Обработка изображения (только если загружено)
            if (dish.ImageFile != null && dish.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "dishes");
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dish.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dish.ImageFile.CopyToAsync(fileStream);
                }

                // Удаляем старое изображение
                if (!string.IsNullOrEmpty(existingDish.ImagePath))
                {
                    var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, existingDish.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                existingDish.ImagePath = $"/images/dishes/{uniqueFileName}";
            }

            // Обновляем остальные поля
            existingDish.Name = dish.Name;
            existingDish.Price = dish.Price;
            existingDish.CategoryId = dish.CategoryId;
            existingDish.Description = dish.Description;

            // Убираем проверки для необязательных полей
            ModelState.Remove("ImageFile");
            ModelState.Remove("ImagePath");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(existingDish);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DishExists(dish.DishId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Не удалось сохранить изменения. Попробуйте еще раз.");
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", dish.CategoryId);
            return View(dish);
        }

        // GET: Dishes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.DishId == id);

            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            // Удаление связанного изображения
            if (!string.IsNullOrEmpty(dish.ImagePath))
            {
                var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, dish.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Dishes.Remove(dish);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.DishId == id);
        }
    }
}