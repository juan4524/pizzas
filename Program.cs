using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) MVC
builder.Services.AddControllersWithViews();

// 2) DbContext (usa tu cadena del appsettings.json)
builder.Services.AddDbContext<PizzeriaContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("PizzeriaDB")));

// 👇 Sesiones en memoria
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});


var app = builder.Build();


// 3) Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// activar sesión ANTES de Authorization
app.UseSession();

app.UseAuthorization();

// 4) Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Productos}/{action=Index}/{id?}");

// 5) (Opcional) Sembrar datos iniciales
// DbInicializador.Sembrar(app);
DbInicializador.Sembrar(app);
app.Run();