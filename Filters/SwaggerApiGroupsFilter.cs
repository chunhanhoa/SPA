using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace QL_Spa.Filters
{
    public class SwaggerApiGroupsFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var apiGroups = new Dictionary<string, OpenApiTag>
            {
                { "Account", new OpenApiTag { Name = "Account", Description = "Quản lý tài khoản và xác thực người dùng" } },
                { "Booking", new OpenApiTag { Name = "Booking", Description = "Quản lý đặt lịch và lịch hẹn" } },
                { "Admin", new OpenApiTag { Name = "Admin", Description = "Các chức năng quản trị hệ thống" } },
                { "Profile", new OpenApiTag { Name = "Profile", Description = "Quản lý hồ sơ người dùng" } },
                { "Role", new OpenApiTag { Name = "Role", Description = "Quản lý vai trò và phân quyền" } },
                { "Token", new OpenApiTag { Name = "Token", Description = "Quản lý token xác thực" } },
                { "AccountManagement", new OpenApiTag { Name = "AccountManagement", Description = "Quản lý tài khoản (dành cho Admin)" } },
                // Bạn có thể thêm nhiều nhóm API khác tại đây
            };
            
            swaggerDoc.Tags = apiGroups.Values.ToList();

            // Phân nhóm tất cả path theo controller
            foreach (var path in swaggerDoc.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    var controllerName = operation.Value.Tags.FirstOrDefault()?.Name;
                    if (controllerName != null && apiGroups.ContainsKey(controllerName))
                    {
                        // Thêm mô tả cho operation tag
                        operation.Value.Tags = new List<OpenApiTag>
                        {
                            apiGroups[controllerName]
                        };
                    }
                }
            }
        }
    }
}
