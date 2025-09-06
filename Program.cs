using CarInsurance.Api.Data;
using CarInsurance.Api.ScheduledTasks.PolicyExpiry;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<CarService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Task 4: Scheduled Task
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddHostedService<PolicyExpiryLogger>();

var app = builder.Build();

// Ensure DB and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }