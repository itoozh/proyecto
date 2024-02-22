using AppWeb.Models;
using Microsoft.EntityFrameworkCore;
using AppWeb.Servicios.Contrato;
using AppWeb.Servicios.Implementacion;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppWeb.Recursos;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("IdRolClaim", "1");
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsuarioPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("IdRolClaim", "2"); 
    });
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GerenciaPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("IdRolClaim", "3"); 
    });
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CombinePolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            return context.User.HasClaim("IdRolClaim", "1") || context.User.HasClaim("IdRolClaim", "3");
        });
    });
});




// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ProyectoContext>(opciones =>
{
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL"));
});

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Inicio/IniciarSesion";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
         
    });

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(
        new ResponseCacheAttribute {
            NoStore = true,
            Location=ResponseCacheLocation.None,
        }
        );
});

builder.Services.AddScoped<ServicioUtilidades>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();



app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
