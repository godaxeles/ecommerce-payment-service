using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.PaymentGateways;
using PaymentService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/payment-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Payment Gateway (Mock or Stripe based on configuration)
var gatewayMode = builder.Configuration["PaymentGateway:Mode"];
if (gatewayMode == "Stripe")
{
    builder.Services.AddScoped<IPaymentGateway, StripeGateway>();
}
else
{
    builder.Services.AddScoped<IPaymentGateway, MockGateway>();
}

builder.Services.AddScoped<IPaymentService, PaymentService.Services.PaymentService>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Payment Service started");
app.Run();