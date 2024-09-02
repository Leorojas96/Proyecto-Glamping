using Glamping2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Agregar el contexto de la base de datos.
builder.Services.AddDbContext<GLAMPINGContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

// Configurar la autenticaci�n basada en cookies.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Ruta de la p�gina de inicio de sesi�n.
        options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta de la p�gina de acceso denegado.
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Agregar middleware de autenticaci�n y autorizaci�n.
app.UseAuthentication(); // Aseg�rate de que UseAuthentication est� antes de UseAuthorization.
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" });
app.Run();
