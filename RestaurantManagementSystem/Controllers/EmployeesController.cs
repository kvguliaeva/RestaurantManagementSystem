using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using System;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.User)
                .ThenInclude(u => u.Role) // Добавлено для отображения роли
                .OrderBy(e => e.Position)
                .ThenBy(e => e.User.Name)
                .ToListAsync();

            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .ThenInclude(u => u.Role) // Добавлено для отображения роли
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            // Получаем только тех пользователей, которые ещё не являются сотрудниками
            var availableUsers = await _context.Users
                .Where(u => !_context.Employees.Any(e => e.UserId == u.UserId))
                .ToListAsync();

            if (!availableUsers.Any())
            {
                ViewBag.Message = "Нет доступных пользователей для добавления в сотрудники";
            }

            ViewData["UserId"] = new SelectList(availableUsers, "UserId", "Name");
            ViewData["Positions"] = GetPositionSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee) // Убрали [Bind]
        {
            try
            {
                // Логируем входящие данные
                Console.WriteLine($"Received: UserId={employee.UserId}, Position={employee.Position}, Salary={employee.Salary}");

                // Очистка ошибок для необязательных полей
                ModelState.Remove("TerminationReason");
                ModelState.Remove("TerminationDate");
                ModelState.Remove("EmployeeId");
                ModelState.Remove("User");

                // Проверка существующего сотрудника
                if (await _context.Employees.AnyAsync(e => e.UserId == employee.UserId))
                {
                    ModelState.AddModelError("UserId", "Этот пользователь уже является сотрудником");
                }

                if (employee.Salary <= 0)
                {
                    ModelState.AddModelError("Salary", "Зарплата должна быть больше нуля");
                }

                // Логирование состояния модели
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState errors:");
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        if (errors.Any())
                        {
                            Console.WriteLine($"{key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    await RepopulateDropdowns(employee.UserId, employee.Position);
                    return View(employee);
                }

                // Установка значений
                employee.HireDate = DateTime.Now;
                employee.Status = "Активен";
                employee.TerminationReason = null;
                employee.TerminationDate = null;

                // Логирование перед сохранением
                Console.WriteLine($"Saving employee: {JsonSerializer.Serialize(employee)}");

                _context.Add(employee);
                await _context.SaveChangesAsync();

                Console.WriteLine("Employee saved successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
                ModelState.AddModelError("", "Ошибка сохранения. Проверьте данные.");
                await RepopulateDropdowns(employee.UserId, employee.Position);
                return View(employee);
            }
        }

        private async Task RepopulateDropdowns(int? userId = null, string selectedPosition = null)
        {
            var availableUsers = await _context.Users
                .Where(u => !_context.Employees.Any(e => e.UserId == u.UserId) || (userId.HasValue && u.UserId == userId.Value))
                .ToListAsync();

            ViewData["UserId"] = new SelectList(availableUsers, "UserId", "Name", userId);
            ViewData["Positions"] = GetPositionSelectList(selectedPosition);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Разрешаем выбрать текущего пользователя, даже если он уже привязан
            var availableUsers = await _context.Users
                .Where(u => !_context.Employees.Any(e => e.UserId == u.UserId && e.EmployeeId != id) || u.UserId == employee.UserId)
                .ToListAsync();

            ViewData["UserId"] = new SelectList(availableUsers, "UserId", "Name", employee.UserId);
            ViewData["Positions"] = GetPositionSelectList(employee.Position);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee) // Убрали [Bind]
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            try
            {
                // Очистка ошибок для необязательных полей
                ModelState.Remove("User");

                // Дополнительная валидация
                if (employee.Salary < 0)
                {
                    ModelState.AddModelError("Salary", "Зарплата не может быть отрицательной");
                }

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState errors:");
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        if (errors.Any())
                        {
                            Console.WriteLine($"{key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                        }
                    }

                    await RepopulateDropdowns(employee.UserId, employee.Position);
                    return View(employee);
                }

                // Получаем существующего сотрудника из базы
                var existingEmployee = await _context.Employees.FindAsync(id);
                if (existingEmployee == null)
                {
                    return NotFound();
                }

                // Обновляем только необходимые поля
                existingEmployee.UserId = employee.UserId;
                existingEmployee.Position = employee.Position;
                existingEmployee.Salary = employee.Salary;
                existingEmployee.Status = employee.Status;
                existingEmployee.BankAccountNumber = employee.BankAccountNumber;

                // Обработка дат
                if (employee.TerminationDate.HasValue)
                {
                    existingEmployee.TerminationDate = employee.TerminationDate.Value.ToUniversalTime();
                }
                else
                {
                    existingEmployee.TerminationDate = null;
                }

                existingEmployee.TerminationReason = employee.TerminationReason;

                // Логирование перед сохранением
                Console.WriteLine($"Updating employee: {JsonSerializer.Serialize(existingEmployee)}");

                _context.Update(existingEmployee);
                await _context.SaveChangesAsync();

                Console.WriteLine("Employee updated successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency error: {ex}");
                if (!EmployeeExists(employee.EmployeeId))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "Не удалось сохранить изменения. Данные могли быть изменены другим пользователем.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                ModelState.AddModelError("", "Произошла ошибка при сохранении изменений.");
            }

            await RepopulateDropdowns(employee.UserId, employee.Position);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Проверка на связанные смены
            var hasShifts = await _context.Shifts.AnyAsync(s => s.UserId == employee.UserId);
            if (hasShifts)
            {
                ViewBag.ErrorMessage = "Невозможно удалить сотрудника, так как у него есть связанные смены.";
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Проверка на связанные смены
            var hasShifts = await _context.Shifts.AnyAsync(s => s.UserId == employee.UserId);
            if (hasShifts)
            {
                ModelState.AddModelError(string.Empty, "Невозможно удалить сотрудника, так как у него есть связанные смены.");
                return View("Delete", employee);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }

        // Вспомогательный метод для получения списка должностей
        private SelectList GetPositionSelectList(string selectedPosition = null)
        {
            var positions = new[]
            {
                "Главный администратор",
                "Старший официант",
                "Старший менеджер",
                "Шеф-повар",
                "Повар",
                "Бармен",
                "Уборщик",
                "Охранник"
            };

            return new SelectList(positions, selectedPosition);
        }
    }
}