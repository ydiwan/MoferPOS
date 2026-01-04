using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Application.Menu;
using MoferPOS.Backend.Application.Orders;
using MoferPOS.Backend.Application.Orders.Queries;
using MoferPOS.Backend.Application.Payments;
using MoferPOS.Backend.Application.Pricing;
using MoferPOS.Backend.Application.Reports;
using MoferPOS.Backend.Data;
using MoferPOS.Backend.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("MoferDb")));

builder.Services.AddScoped<IPricingCalculator, PricingCalculator>();
builder.Services.AddScoped<IMenuService, MenuService>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderQueryService, OrderQueryService>();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

await app.MigrateAndSeedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
