using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SalesReport()
        {
            // Устанавливаем корректные начальные даты
            var model = new Report
            {
                PeriodStart = DateTime.Today.AddDays(-7).Date,
                PeriodEnd = DateTime.Today.Date
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SalesReport([Bind("PeriodStart,PeriodEnd")] Report report)
        {
            // Нормализация дат (убираем время)
            report.PeriodStart = report.PeriodStart.Date;
            report.PeriodEnd = report.PeriodEnd.Date.AddDays(1).AddSeconds(-1); // До конца дня

            if (report.PeriodStart > report.PeriodEnd)
            {
                ModelState.AddModelError("", "Дата начала не может быть позже даты окончания");
                return View(report);
            }

            Console.WriteLine($"Поиск заказов с {report.PeriodStart} по {report.PeriodEnd}");

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= report.PeriodStart && o.OrderDate <= report.PeriodEnd)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .ToListAsync();

            report.ReportDate = DateTime.Now;
            report.ReportType = "Отчет по продажам";
            report.TotalRevenue = orders.Sum(o => o.TotalAmount);
            report.TotalSalary = 0;
            report.Total = report.TotalRevenue;
            report.Comment = $"Отчет по продажам за период с {report.PeriodStart:dd.MM.yyyy} по {report.PeriodEnd:dd.MM.yyyy}. " +
                           $"Всего заказов: {orders.Count}, Общая сумма: {report.TotalRevenue:C}";

            return View("ReportResult", report);
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeWorkReport()
        {
            var employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.Status == "Активен")
                .Select(e => new { e.UserId, e.User.Name })
                .ToListAsync();

            ViewBag.Employees = new SelectList(employees, "UserId", "Name");

            return View(new Report
            {
                PeriodStart = DateTime.Today.AddMonths(-1).Date,
                PeriodEnd = DateTime.Today.Date
            });
        }

        [HttpPost]
        public async Task<IActionResult> EmployeeWorkReport([Bind("PeriodStart,PeriodEnd")] Report report, int employeeId)
        {
            // Нормализация дат
            report.PeriodStart = report.PeriodStart.Date;
            report.PeriodEnd = report.PeriodEnd.Date.AddDays(1).AddSeconds(-1);

            if (report.PeriodStart > report.PeriodEnd)
            {
                ModelState.AddModelError("", "Дата начала не может быть позже даты окончания");

                // Повторно загружаем список сотрудников
                var employees = await _context.Employees
                    .Include(e => e.User)
                    .Where(e => e.Status == "Активен")
                    .Select(e => new { e.UserId, e.User.Name })
                    .ToListAsync();

                ViewBag.Employees = new SelectList(employees, "UserId", "Name");
                return View(report);
            }

            var shifts = await _context.Shifts
                .Where(s => s.UserId == employeeId &&
                           s.ShiftDate >= report.PeriodStart &&
                           s.ShiftDate <= report.PeriodEnd)
                .Include(s => s.User)
                .ToListAsync();

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            decimal totalSalary = shifts.Sum(s => (decimal)s.WorkedHours * (employee.Salary / 160m));

            report.ReportDate = DateTime.Now;
            report.ReportType = "Отчет по отработанным сменам";
            report.TotalRevenue = 0;
            report.TotalSalary = totalSalary;
            report.Total = totalSalary;
            report.Comment = $"Отчет по сменам сотрудника {employee.User.Name} за период с {report.PeriodStart:dd.MM.yyyy} по {report.PeriodEnd:dd.MM.yyyy}.\n" +
                           $"Всего смен: {shifts.Count}, Отработано часов: {shifts.Sum(s => s.WorkedHours):F2},\n" +
                           $"Зарплата за период: {totalSalary:C}";

            return View("ReportResult", report);
        }

        public IActionResult ReportResult(Report report)
        {
            return View(report);
        }
    }
}