using EquipmentAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ������������ ��������
// ���������� DbContext � �������������� InMemoryDatabase ��� �������� ������ ������������ � ���������� � ������
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("EquipmentDB"));

// ���������� Endpoints API  ��� ��������� Swagger 
builder.Services.AddEndpointsApiExplorer();

// ���������� Swagger 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EquipmentAPI", Version = "v1" });
});

var app = builder.Build();

// ������������ Swagger ��� ��������� ���������� (Development)
// ��������� Swagger � SwaggerUI ������ ��� ������� � ������ ����������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EquipmentAPI v1"));
}

// CRUD �������� ��� ���������� ��������� Equipment (������������)
// ��������� ������ ���� ������� Equipment � ���������� ��������� Parameters
app.MapGet("/equipment", async (AppDbContext db) =>
    await db.Equipments.Include(e => e.Parameters).ToListAsync());

// ��������� ������ ������������ �� ID � ���������� ��������� Parameters
app.MapGet("/equipment/{id}", async (int id, AppDbContext db) =>
    await db.Equipments.Include(e => e.Parameters).FirstOrDefaultAsync(e => e.Id == id)
    is Equipment equipment ? Results.Ok(equipment) : Results.NotFound());

// ���������� ����� ������ Equipment
app.MapPost("/equipment", async (Equipment equipment, AppDbContext db) =>
{
    db.Equipments.Add(equipment);
    await db.SaveChangesAsync();
    return Results.Created($"/equipment/{equipment.Id}", equipment);
});

// ���������� ������������� ������������ �� ID
app.MapPut("/equipment/{id}", async (int id, Equipment updatedEquipment, AppDbContext db) =>
{
    var equipment = await db.Equipments.FindAsync(id);
    if (equipment == null) return Results.NotFound();

    // ���������� ������� Equipment
    equipment.Name = updatedEquipment.Name;
    equipment.Description = updatedEquipment.Description;
    equipment.Code = updatedEquipment.Code;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// �������� ������������ �� ID
app.MapDelete("/equipment/{id}", async (int id, AppDbContext db) =>
{
    var equipment = await db.Equipments.FindAsync(id);
    if (equipment == null) return Results.NotFound();

    db.Equipments.Remove(equipment);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


// CRUD �������� ��� ���������� ��������� Parameter (���������)
// ��������� ������ ���� ����������
app.MapGet("/parameter", async (AppDbContext db) => await db.Parameters.ToListAsync());

// ��������� ������ ��������� �� ID
app.MapGet("/parameter/{id}", async (int id, AppDbContext db) =>
    await db.Parameters.FindAsync(id) is Parameter parameter ? Results.Ok(parameter) : Results.NotFound());

// ���������� ������ ���������
app.MapPost("/parameter", async (Parameter parameter, AppDbContext db) =>
{
    db.Parameters.Add(parameter);
    await db.SaveChangesAsync();
    return Results.Created($"/parameter/{parameter.Id}", parameter);
});

// ���������� ������������� ��������� �� ID
app.MapPut("/parameter/{id}", async (int id, Parameter updatedParameter, AppDbContext db) =>
{
    var parameter = await db.Parameters.FindAsync(id);
    if (parameter == null) return Results.NotFound();

    // ���������� ������� Parameter
    parameter.Name = updatedParameter.Name;
    parameter.Description = updatedParameter.Description;
    parameter.EquipmentCode = updatedParameter.EquipmentCode;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// �������� ��������� �� ID
app.MapDelete("/parameter/{id}", async (int id, AppDbContext db) =>
{
    var parameter = await db.Parameters.FindAsync(id);
    if (parameter == null) return Results.NotFound();

    db.Parameters.Remove(parameter);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// AppDbContext: �������� ���� ������ ��� EF Core
namespace EquipmentAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ������� Equipments (������������)
        public DbSet<Equipment> Equipments { get; set; } = null!;

        // ������� Parameters (���������)
        public DbSet<Parameter> Parameters { get; set; } = null!;
    }

    // ������ ������ ��� Equipment
    public class Equipment
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Code { get; set; }

        // ����� � �����������
        public List<Parameter> Parameters { get; set; } = new();
    }

    // ������ ������ ��� Parameter
    public class Parameter
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        // ��� ������������, � ������� ������ ��������
        public required string EquipmentCode { get; set; }

        // ����� � �������� Equipments ����� ������� ����
        public int EquipmentId { get; set; }
    }
}
