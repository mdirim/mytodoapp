using Microsoft.EntityFrameworkCore;
using MyTodoApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Veritabanı: Production'da PostgreSQL (Render), lokalde SQLite
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Render.com'un verdiği URL'yi Npgsql bağlantı string'ine çevir
    // Örnek: postgresql://user:pass@host/db
    var connStr = ConvertPostgresUrlToConnString(databaseUrl);
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connStr));
    Console.WriteLine("✅ PostgreSQL bağlantısı kullanılıyor (Production)");
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=todos.db";
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
    Console.WriteLine("✅ SQLite bağlantısı kullanılıyor (Local)");
}

var app = builder.Build();

// Otomatik olarak veritabanını oluştur
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        var created = db.Database.EnsureCreated();
        Console.WriteLine(created
            ? "✅ Veritabanı oluşturuldu"
            : "✅ Veritabanı zaten mevcut");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Veritabanı hatası: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todos}/{action=Index}/{id?}");

app.Run();

// postgresql://user:password@host:port/dbname?sslmode=require
static string ConvertPostgresUrlToConnString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    var db = uri.AbsolutePath.TrimStart('/');
    var query = uri.Query;

    var connStr =
        $"Host={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={db};" +
        $"Username={Uri.UnescapeDataString(userInfo[0])};" +
        $"Password={Uri.UnescapeDataString(userInfo[1])};" +
        $"Ssl Mode=Require;" +
        (query.Length > 1 ? $"Trust Server Certificate=true;" : "");

    return connStr;
}
