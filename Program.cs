using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QL_Spa.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using QL_Spa.Data;
using QL_Spa.Services;
using System.Text.Json.Serialization;
using QL_Spa.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        // Preserve property names
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        // Handle reference loops
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Add API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure DbContext
builder.Services.AddDbContext<SpaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)
           )
);

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<SpaDbContext>()
    .AddDefaultTokenProviders();

// Add RoleInitializer service
builder.Services.AddScoped<RoleInitializer>();

// Add this line to register AvailabilityService
builder.Services.AddScoped<QL_Spa.Services.AvailabilityService>();

// Add this line to register the script
builder.Services.AddScoped<QL_Spa.Data.Scripts.EnsureInvoicesTableScript>();

// Thêm Swagger với cấu hình tùy chỉnh
builder.Services.AddSwaggerDocumentation();

// AddScoped JwtService
builder.Services.AddScoped<JwtService>();
builder.Services.ConfigureJwt(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Use developer exception page in development
    app.UseDeveloperExceptionPage();
    
    // Sử dụng Swagger trong môi trường phát triển
    app.UseSwaggerDocumentation();
}

// Luôn kích hoạt Swagger với tham số cụ thể để kiểm soát trong production
if (!app.Environment.IsDevelopment())
{
    var showSwagger = builder.Configuration.GetValue<bool>("SwaggerSettings:ShowInProduction");
    if (showSwagger)
    {
        app.UseSwaggerDocumentation();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // Add this for API endpoints

// Initialize roles and create default admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply database migrations
        var context = services.GetRequiredService<SpaDbContext>();
        context.Database.Migrate();
        
        // Run the script to ensure Invoices table exists
        var invoicesTableScript = services.GetRequiredService<QL_Spa.Data.Scripts.EnsureInvoicesTableScript>();
        invoicesTableScript.ExecuteAsync().GetAwaiter().GetResult();
        
        // Execute script to add missing columns
        var logger = services.GetRequiredService<ILogger<AddMissingColumnsScript>>();
        var script = new AddMissingColumnsScript(context, logger);
        script.ExecuteAsync().GetAwaiter().GetResult();
        
        var roleInitializer = services.GetRequiredService<RoleInitializer>();
        // Run this synchronously to avoid issues in Program.cs
        roleInitializer.InitializeAsync().GetAwaiter().GetResult();
        
        // Optionally create a default admin (you can change these values or remove this line in production)
        roleInitializer.CreateAdminUserAsync("admin", "admin@example.com", "Admin123!").GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing roles or applying migrations");
    }
}

app.Run();