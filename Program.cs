using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpContextAccessor for accessing HttpContext in views
builder.Services.AddHttpContextAccessor();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add DbContext - MUST be before builder.Build()
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 2,
                maxRetryDelay: TimeSpan.FromSeconds(3),
                errorNumbersToAdd: null);
        }
    );
    
    // Tối ưu performance
    if (!builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(false);
        options.EnableDetailedErrors(false);
    }
    
    options.EnableServiceProviderCaching(); // Cache service provider
});

// Add Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}")
    .WithStaticAssets();


app.Run();
