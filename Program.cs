using Microsoft.EntityFrameworkCore;
using ProductService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80);
});

builder.Services.AddControllers();
builder.Services.AddDbContext<ProductDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

var app = builder.Build();
app.MapControllers();

//// Minimal API
//app.MapGet("/", () => "Product Service is running!");

//// Auto-migrate
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
//    db.Database.Migrate();
//}

//app.MapGet("/products", async (ProductDbContext db) => await db.Products.ToListAsync());
//app.MapGet("/api/product", async (ProductDbContext db) => await db.Products.ToListAsync());

//app.MapGet("/products/{id}", async (int id, ProductDbContext db) =>
//    await db.Products.FindAsync(id) is Product product ? Results.Ok(product) : Results.NotFound());

//app.MapPost("/products", async (Product product, ProductDbContext db) =>
//{
//    db.Products.Add(product);
//    await db.SaveChangesAsync();
//    return Results.Created($"/products/{product.Id}", product);
//});

//app.MapPut("/products/{id}", async (int id, Product inputProduct, ProductDbContext db) =>
//{
//    var product = await db.Products.FindAsync(id);
//    if (product is null) return Results.NotFound();

//    product.Name = inputProduct.Name;
//    product.Price = inputProduct.Price;
//    product.Quantity = inputProduct.Quantity;

//    await db.SaveChangesAsync();
//    return Results.NoContent();
//});

//app.MapDelete("/products/{id}", async (int id, ProductDbContext db) =>
//{
//    var product = await db.Products.FindAsync(id);
//    if (product is null) return Results.NotFound();

//    db.Products.Remove(product);
//    await db.SaveChangesAsync();
//    return Results.Ok(product);
//});

app.Run();
