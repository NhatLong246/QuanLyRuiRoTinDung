using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Services;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Kestrel để sử dụng HTTP/1.1 (tránh lỗi HTTP/2 protocol error)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

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

// Add HttpClient for ZaloPay
builder.Services.AddHttpClient("ZaloPay", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ICicService, CicService>();
builder.Services.AddScoped<IRuiRoService, RuiRoService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IZaloPayService, ZaloPayService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Background Service for payment reminders
builder.Services.AddHostedService<PaymentReminderBackgroundService>();

// Add Background Service for overdue payment processing (CIC penalty, interest increase)
builder.Services.AddHostedService<OverduePaymentBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
    
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}")
    //pattern: "{controller=LanhDao}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
