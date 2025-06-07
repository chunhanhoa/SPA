using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using QL_Spa.Data;
using QL_Spa.Services; // Add this namespace for RoleInitializer
using System.Text.Json.Serialization;

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

// Thêm Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "QL_Spa API", 
        Version = "v1",
        Description = "API for QL_Spa application"
    });
});

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
}

// Thêm Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QL_Spa API V1");
    });
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