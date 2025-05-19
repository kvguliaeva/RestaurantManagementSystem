using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Таблицы базы данных
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Настройка таблицы БЛЮДА
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.HasIndex(d => d.Name).IsUnique();
                entity.Property(d => d.Price).HasColumnType("decimal(10,2)");
                entity.Property(d => d.IsAvailable).HasDefaultValue(true);
                entity.Property(d => d.Description).HasMaxLength(1000);
                entity.Property(d => d.ImagePath).HasMaxLength(500);
            });

            // 2. Настройка таблицы КАТЕГОРИЯ
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.Property(c => c.Description).HasMaxLength(500);
            });

            // 3. Настройка таблицы ПОЛЬЗОВАТЕЛИ
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Phone).HasMaxLength(20);
                entity.Property(u => u.Email).HasMaxLength(255);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // 4. Настройка таблицы РОЛИ
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Description).HasMaxLength(500);
            });

            // 5. Настройка таблицы ЗАКАЗЫ
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(o => o.Status).HasDefaultValue("Новый");
                entity.Property(o => o.Comment).HasMaxLength(1000);
                entity.Property(o => o.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // 6. Настройка таблицы СОСТАВ_ЗАКАЗА
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.Price).HasColumnType("decimal(10,2)");
                entity.Property(oi => oi.Comment).HasMaxLength(500);
                entity.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
            });

            // 7. Настройка таблицы ПЛАТЕЖИ
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount).HasColumnType("decimal(10,2)");
                entity.Property(p => p.PaymentMethod).HasDefaultValue("Наличные");
                entity.Property(p => p.Status).HasDefaultValue("Завершено");
                entity.Property(p => p.Comment).HasMaxLength(500);
                entity.Property(p => p.PaymentDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // 8. Настройка таблицы ОТЧЕТЫ
            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(r => r.TotalRevenue).HasColumnType("decimal(12,2)");
                entity.Property(r => r.TotalSalary).HasColumnType("decimal(12,2)");
                entity.Property(r => r.Total).HasColumnType("decimal(12,2)");
                entity.Property(r => r.Comment).HasMaxLength(2000);
                entity.Property(r => r.ReportDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // 9. Настройка таблицы СМЕНЫ
            modelBuilder.Entity<Shift>(entity =>
            {
                entity.Property(s => s.Status).HasDefaultValue("Запланирована");
                entity.Property(s => s.Comment).HasMaxLength(500);
            });

            // 10. Настройка таблицы СОТРУДНИКИ
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Salary)
                    .HasColumnType("decimal(10,2)")
                    .HasAnnotation("CheckConstraint", "\"Salary\" >= 0");

                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.TerminationReason).HasMaxLength(500);
                entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValue("Активен");
                entity.Property(e => e.HireDate).HasDefaultValueSql("CURRENT_DATE");
            });

            // Настройка отношений
            modelBuilder.Entity<Dish>()
                .HasOne(d => d.Category)
                .WithMany(c => c.Dishes)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Dish)
                .WithMany()
                .HasForeignKey(oi => oi.DishId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.User)
                .WithMany(u => u.Shifts)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}