using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using QL_Spa.Filters;
using System.Reflection;
using Swashbuckle.AspNetCore.Annotations; // Add this using directive

namespace QL_Spa.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            // Thêm service Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Cấu hình thông tin cơ bản về API
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LoanSpa API",
                    Version = "v1",
                    Description = "API hệ thống quản lý spa và đặt lịch",
                    Contact = new OpenApiContact
                    {
                        Name = "Loan Spa",
                        Email = "support@loanspa.com",
                        Url = new Uri("https://loanspa.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Bản quyền © LoanSpa 2024"
                    }
                });

                // Cấu hình xác thực JWT cho Swagger
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header sử dụng Bearer scheme",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // Phải viết thường
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };
                
                options.AddSecurityDefinition("Bearer", securityScheme);
                
                // Áp dụng yêu cầu bảo mật vào tất cả các API
                var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securityScheme, new[] { "Bearer" } }
                };
                options.AddSecurityRequirement(securityRequirement);

                // Nhóm API theo controller tags
                options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
                options.DocInclusionPredicate((docName, apiDesc) => true);

                // Thêm chú thích từ XML Documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Tùy chỉnh giao diện Swagger
                options.EnableAnnotations(); // Cho phép sử dụng annotations trong controllers

                // Thêm filter phân nhóm API
                options.DocumentFilter<SwaggerApiGroupsFilter>();
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            // Kích hoạt middleware Swagger
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
                    };
                });
            });

            // Kích hoạt Swagger UI
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api-docs/v1/swagger.json", "LoanSpa API v1");
                c.RoutePrefix = "api"; // Đường dẫn truy cập UI: /api
                
                // Tùy chỉnh giao diện Swagger UI
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(0); // Ẩn schema mặc định
                c.EnableDeepLinking(); // Cho phép liên kết trực tiếp đến operations
                c.DisplayRequestDuration(); // Hiển thị thời gian thực thi request

                // Tùy chỉnh theme
                c.InjectStylesheet("/css/swagger-theme.css");
                c.DocumentTitle = "LoanSpa API Documentation";
            });

            return app;
        }
    }
}
