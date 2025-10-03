using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Domain.Interfaces.Repositories;
using InventoryManagement.Domain.longerfaces.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            string connectionString)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IStockMovementRepository, StockMovementRepository>();

            // Add generic repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Add Services
            services.AddScoped<IInventoryService, InventoryService>();
            //services.AddScoped<IOrderService, OrderService>();
            //services.AddScoped<IWarehouseService, WarehouseService>();
            //services.AddScoped<IReportingService, ReportingService>();
            //services.AddScoped<IIntegrationService, IntegrationService>();

            // Add AutoMapper
            // services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
