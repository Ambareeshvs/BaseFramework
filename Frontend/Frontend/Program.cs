using Frontend.Core.HttpClients;
using Microsoft.AspNetCore.Authentication.Cookies;
using NToastNotify;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
    cookie =>
    {
        cookie.Cookie.Name = "Authentication_Cookie";
        cookie.SlidingExpiration = true;
        cookie.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddRazorPages().AddRazorRuntimeCompilation()
    .AddNToastNotifyToastr(new ToastrOptions()
    {
        TimeOut = 5000,
        NewestOnTop = true,
        CloseButton = true,
        CloseDuration = true,
        ProgressBar = true,
        PositionClass = ToastPositions.TopRight,
    });

builder.Services.AddScoped<IGenericHttpClient, GenericHttpClient>();

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

app.UseNToastNotify();

app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Account}/{action=LandingPage}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LandingPage}/{id?}");

app.Run();
