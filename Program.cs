using EquipmentAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация сервисов
// Добавление DbContext с использованием InMemoryDatabase для хранения данных оборудования и параметров в памяти
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("EquipmentDB"));

// Добавление Endpoints API  для генерации Swagger 
builder.Services.AddEndpointsApiExplorer();

// Добавление Swagger 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EquipmentAPI", Version = "v1" });
});

var app = builder.Build();

// Конфигурация Swagger для окружения разработки (Development)
// Включение Swagger и SwaggerUI только при запуске в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EquipmentAPI v1"));
}

// CRUD операции для управления сущностью Equipment (Оборудование)
// Получение списка всех записей Equipment с включением связанных Parameters
app.MapGet("/equipment", async (AppDbContext db) =>
    await db.Equipments.Include(e => e.Parameters).ToListAsync());

// Получение одного оборудования по ID с включением связанных Parameters
app.MapGet("/equipment/{id}", async (int id, AppDbContext db) =>
    await db.Equipments.Include(e => e.Parameters).FirstOrDefaultAsync(e => e.Id == id)
    is Equipment equipment ? Results.Ok(equipment) : Results.NotFound());

// Добавление новой записи Equipment
app.MapPost("/equipment", async (Equipment equipment, AppDbContext db) =>
{
    db.Equipments.Add(equipment);
    await db.SaveChangesAsync();
    return Results.Created($"/equipment/{equipment.Id}", equipment);
});

// Обновление существующего оборудования по ID
app.MapPut("/equipment/{id}", async (int id, Equipment updatedEquipment, AppDbContext db) =>
{
    var equipment = await db.Equipments.FindAsync(id);
    if (equipment == null) return Results.NotFound();

    // Обновление свойств Equipment
    equipment.Name = updatedEquipment.Name;
    equipment.Description = updatedEquipment.Description;
    equipment.Code = updatedEquipment.Code;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Удаление оборудования по ID
app.MapDelete("/equipment/{id}", async (int id, AppDbContext db) =>
{
    var equipment = await db.Equipments.FindAsync(id);
    if (equipment == null) return Results.NotFound();

    db.Equipments.Remove(equipment);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


// CRUD операции для управления сущностью Parameter (Параметры)
// Получение списка всех параметров
app.MapGet("/parameter", async (AppDbContext db) => await db.Parameters.ToListAsync());

// Получение одного параметра по ID
app.MapGet("/parameter/{id}", async (int id, AppDbContext db) =>
    await db.Parameters.FindAsync(id) is Parameter parameter ? Results.Ok(parameter) : Results.NotFound());

// Добавление нового параметра
app.MapPost("/parameter", async (Parameter parameter, AppDbContext db) =>
{
    db.Parameters.Add(parameter);
    await db.SaveChangesAsync();
    return Results.Created($"/parameter/{parameter.Id}", parameter);
});

// Обновление существующего параметра по ID
app.MapPut("/parameter/{id}", async (int id, Parameter updatedParameter, AppDbContext db) =>
{
    var parameter = await db.Parameters.FindAsync(id);
    if (parameter == null) return Results.NotFound();

    // Обновление свойств Parameter
    parameter.Name = updatedParameter.Name;
    parameter.Description = updatedParameter.Description;
    parameter.EquipmentCode = updatedParameter.EquipmentCode;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Удаление параметра по ID
app.MapDelete("/parameter/{id}", async (int id, AppDbContext db) =>
{
    var parameter = await db.Parameters.FindAsync(id);
    if (parameter == null) return Results.NotFound();

    db.Parameters.Remove(parameter);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// AppDbContext: Контекст базы данных для EF Core
namespace EquipmentAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Таблица Equipments (Оборудование)
        public DbSet<Equipment> Equipments { get; set; } = null!;

        // Таблица Parameters (Параметры)
        public DbSet<Parameter> Parameters { get; set; } = null!;
    }

    // Модель данных для Equipment
    public class Equipment
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Code { get; set; }

        // Связь с параметрами
        public List<Parameter> Parameters { get; set; } = new();
    }

    // Модель данных для Parameter
    public class Parameter
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        // Код оборудования, с которым связан параметр
        public required string EquipmentCode { get; set; }

        // Связь с таблицей Equipments через внешний ключ
        public int EquipmentId { get; set; }
    }
}
