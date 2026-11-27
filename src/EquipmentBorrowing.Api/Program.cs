using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var messages = context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message));

        var message = string.Join(" ", messages);
        if (string.IsNullOrWhiteSpace(message))
            message = "The request is invalid.";

        return new BadRequestObjectResult(
            new ApiError("validation_error", message));
    };
});
builder.Services.AddOpenApi();

string connectionString =
    Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_CONNECTION_STRING")
    ?? string.Empty;

// Do not connect or migrate at host startup. Database failures should be
// returned by API requests, while the API process stays available.
builder.Services.AddDbContextFactory<EquipmentBorrowingDbContext>(
    options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IStudentRepository, EfStudentRepository>();
builder.Services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
builder.Services.AddSingleton<IBorrowingRepository, EfBorrowingRepository>();

builder.Services.AddTransient<BorrowEquipmentService>();
builder.Services.AddTransient<ReturnEquipmentService>();
builder.Services.AddTransient<EquipmentCatalogService>();
builder.Services.AddTransient<StudentCatalogService>();
builder.Services.AddTransient<ActiveBorrowingsService>();
builder.Services.AddTransient<StudentManagementService>();
builder.Services.AddTransient<EquipmentManagementService>();
builder.Services.AddTransient<BorrowingQueryService>();

var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsJsonAsync(
        new ApiError("server_error", "An unexpected server error occurred."));
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();

public partial class Program;