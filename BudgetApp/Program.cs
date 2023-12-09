using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using BudgetApp.Configurations;
using BudgetApp.Data;
using BudgetApp.DataServices;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.EmailServices;
using BudgetApp.Logging;
using BudgetApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Identity.Client;
using MySql.Data.MySqlClient;
using System.Configuration;
using static BudgetApp.EmailServices.GmailSender;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//[NEW | OPTIONAL]
//Add Custom Configuration Manager to handle configurations from secrets file
AppConfigurationManager.Initialize(builder.Configuration);

//[REPLACE]
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var connectionString = Connections.ConnectionString ?? throw new InvalidOperationException("Connection String Not Found");

//[NEW]
builder.Services.AddTransient<MySqlConnection>(_ => new MySqlConnection(connectionString));

//[REPLACE]
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySQL(connectionString), ServiceLifetime.Scoped);
//[NEW] Must include lazy loading proxies explicitly (Install proxie nuget package)

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//[REPLACE]
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<User, Role>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

//[NEW]
//new (optional: clears the logging providers, and add custom logger)
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddProvider(new AppLoggerProvider(new AppLoggerConfiguration()));
});

//[NEW] Add any services that are being injected into constructors
builder.Services.AddScoped<UserServiceManager>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<IncomeService>();
builder.Services.AddScoped<FixedCostService>();
builder.Services.AddScoped<VariableCostService>();
builder.Services.AddScoped<CostCategoryService>();
builder.Services.AddScoped<EntityServiceManager>();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>(); //helper to wrap the IHttpContextAccessor
builder.Services.Configure<GmailSettings>(builder.Configuration.GetSection("GmailSettings")); //injected as IOptions<GmailSettings> in the GmailSender class
builder.Services.AddTransient<IEmailSender, GmailSender>();

//[NEW]
builder.Services.AddRazorPages();

var app = builder.Build();

//[NEW | OPTIONAL] (for seeding data)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    await SeedData.InitializeAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
