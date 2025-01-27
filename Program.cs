using TaskManagerAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי מסד נתונים בזיכרון (In-Memory)
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseInMemoryDatabase("TaskDb"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// הגדרת Swagger עם פרטי תיעוד
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager API",
        Version = "v1",
        Description = "API for managing tasks and assignments",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@taskmanager.com"
        }
    });

    // Include OpenAPI YAML file from task-manager-docs directory
    var yamlPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)?.FullName ?? string.Empty, @"..\task-manager-docs\task-api-v1.0.0.yaml");
    if (File.Exists(yamlPath))
    {
        c.IncludeXmlComments(yamlPath);
        // Attach the YAML file 
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAllOrigins");

// יצירת מסד הנתונים עם נתוני דמו
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Manager API V1");
        c.RoutePrefix = string.Empty;  // מאפשר גישה ישירה מכתובת השורש
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Middleware לניהול שגיאות גלובליות
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        var response = new
        {
            Status = context.Response.StatusCode,
            Message = "server error, please try again later.",
            Details = ex.Message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.Run();
