using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            // Исправлено: добавлена проверка на null для IsAvailable
            ViewData["Dishes"] = await _context.Dishes
                .Where(d => d.IsAvailable == true)
                .ToListAsync();

            ViewData["Users"] = new SelectList(await _context.Users.ToListAsync(), "UserId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            Console.WriteLine("=== Начало обработки создания заказа ===");

            // Получаем данные из формы
            var userId = int.Parse(form["UserId"]);
            var comment = form["Comment"];
            var dishIds = form["dishIds"].Select(int.Parse).ToArray();
            var quantities = form["quantities"].Select(int.Parse).ToArray();

            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"Comment: {comment}");
            Console.WriteLine($"dishIds: {string.Join(",", dishIds)}");
            Console.WriteLine($"quantities: {string.Join(",", quantities)}");

            var order = new Order
            {
                UserId = userId,
                Comment = comment,
                OrderDate = DateTime.Now,
                Status = "Новый",
                TotalAmount = 0,
                OrderItems = new List<OrderItem>()
            };

            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            foreach (var state in ModelState)
            {
                Console.WriteLine($"{state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            if (ModelState.IsValid && dishIds != null && quantities != null && dishIds.Length == quantities.Length)
            {
                try
                {
                    // Инициализация заказа
                    order.OrderDate = DateTime.Now;
                    order.Status = "Новый";
                    order.TotalAmount = 0;
                    order.OrderItems = new List<OrderItem>();

                    Console.WriteLine("Начало обработки блюд...");

                    for (int i = 0; i < dishIds.Length; i++)
                    {
                        Console.WriteLine($"Обработка блюда #{i + 1}: DishId={dishIds[i]}, Quantity={quantities[i]}");

                        var dish = await _context.Dishes.FindAsync(dishIds[i]);
                        if (dish == null)
                        {
                            Console.WriteLine($"Блюдо с ID {dishIds[i]} не найдено!");
                            continue;
                        }

                        if (quantities[i] <= 0)
                        {
                            Console.WriteLine($"Некорректное количество для блюда {dishIds[i]}: {quantities[i]}");
                            continue;
                        }

                        Console.WriteLine($"Добавляем блюдо: {dish.Name} (ID: {dish.DishId}), Цена: {dish.Price}, Количество: {quantities[i]}");

                        order.OrderItems.Add(new OrderItem
                        {
                            DishId = dish.DishId,
                            Quantity = quantities[i],
                            Price = dish.Price,
                            Comment = $"Добавлено {DateTime.Now:g}"
                        });

                        order.TotalAmount += dish.Price * quantities[i];
                    }

                    if (!order.OrderItems.Any())
                    {
                        Console.WriteLine("Ошибка: не добавлено ни одного блюда в заказ");
                        ModelState.AddModelError("", "Необходимо добавить хотя бы одно блюдо");
                    }
                    else
                    {
                        Console.WriteLine($"Итоговая сумма заказа: {order.TotalAmount}");
                        Console.WriteLine($"Количество позиций в заказе: {order.OrderItems.Count}");

                        // Проверка пользователя
                        var userExists = await _context.Users.AnyAsync(u => u.UserId == order.UserId);
                        if (!userExists)
                        {
                            Console.WriteLine($"Ошибка: пользователь с ID {order.UserId} не найден");
                            ModelState.AddModelError("UserId", "Указанный пользователь не существует");
                        }
                        else
                        {
                            try
                            {
                                _context.Orders.Add(order);
                                await _context.SaveChangesAsync();
                                Console.WriteLine("Заказ успешно создан! ID: " + order.OrderId);
                                return RedirectToAction(nameof(Index));
                            }
                            catch (DbUpdateException dbEx)
                            {
                                Console.WriteLine($"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
                                ModelState.AddModelError("", "Ошибка при сохранении заказа в базу данных");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Критическая ошибка: {ex.ToString()}");
                    ModelState.AddModelError("", "Произошла непредвиденная ошибка при создании заказа");
                }
            }

            // Повторная загрузка данных для формы
            ViewData["Dishes"] = await _context.Dishes
                .Where(d => d.IsAvailable == true)
                .ToListAsync();
            ViewData["Users"] = new SelectList(await _context.Users.ToListAsync(), "UserId", "Name", order.UserId);

            Console.WriteLine("=== Конец обработки создания заказа ===");
            return View(order);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewData["Users"] = new SelectList(_context.Users, "UserId", "Name", order.UserId);
            ViewData["Statuses"] = new SelectList(new[] {
        "Новый",
        "В обработке",
        "Готовится",
        "Готов",
        "Доставлен",
        "Отменен"
    }, order.Status);

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,Status,Comment")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Получаем существующий заказ из базы
                    var existingOrder = await _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    // Обновляем только изменяемые поля
                    existingOrder.UserId = order.UserId;
                    existingOrder.Status = order.Status;
                    existingOrder.Comment = order.Comment;

                    _context.Update(existingOrder);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Если что-то пошло не так, заново загружаем данные для формы
            ViewData["Users"] = new SelectList(_context.Users, "UserId", "Name", order.UserId);
            ViewData["Statuses"] = new SelectList(new[] {
                "Новый",
                "В обработке",
                "Готовится",
                "Готов",
                "Доставлен",
                "Отменен"
            }, order.Status);

            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                // Удаляем связанные элементы
                _context.OrderItems.RemoveRange(order.OrderItems);

                if (order.Payment != null)
                {
                    _context.Payments.Remove(order.Payment);
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}