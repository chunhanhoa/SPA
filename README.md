# LoanSpa - Hệ thống quản lý spa và đặt lịch

## Giới thiệu
LoanSpa là ứng dụng web quản lý spa và đặt lịch toàn diện được phát triển bằng ASP.NET Core 9.0. Hệ thống cung cấp giải pháp hoàn chỉnh cho việc quản lý spa bao gồm đặt lịch trực tuyến, quản lý khách hàng, nhân viên, dịch vụ, phòng, ghế và hệ thống thanh toán.

## Công nghệ sử dụng

### Backend
- **Framework**: ASP.NET Core 9.0 MVC
- **Cơ sở dữ liệu**: Entity Framework Core với SQL Server
- **Xác thực**: ASP.NET Core Identity + JWT Authentication
- **API Documentation**: Swagger/OpenAPI 3.0

### Frontend
- **UI Framework**: Bootstrap 5
- **JavaScript Libraries**: jQuery, DataTables
- **CSS**: Custom CSS với theme tùy chỉnh
- **Icons**: Bootstrap Icons

### Packages chính
- Microsoft.AspNetCore.Authentication.JwtBearer 9.0.5
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.5
- Microsoft.EntityFrameworkCore.SqlServer 9.0.5
- Swashbuckle.AspNetCore 6.5.0
- System.IdentityModel.Tokens.Jwt 8.12.0
- iTextSharp 5.5.13.3 (cho xuất PDF)

## Tính năng chính

### 🏠 Dashboard và Quản lý
- Dashboard tổng quan với thống kê real-time
- Quản lý lịch hẹn theo thời gian thực
- Báo cáo doanh thu và thống kê chi tiết

### 👥 Quản lý Người dùng
- **Hệ thống vai trò**: Admin, Manager, User
- **Xác thực**: Đăng ký/Đăng nhập với JWT
- **Quản lý hồ sơ**: Cập nhật thông tin cá nhân
- **Phân quyền**: Kiểm soát truy cập theo vai trò

### 📅 Quản lý Đặt lịch
- **Đặt lịch trực tuyến**: Khách hàng có thể đặt lịch 24/7
- **Quản lý trạng thái**: Chờ xác nhận → Đã xác nhận → Đang thực hiện → Hoàn thành
- **Kiểm tra tính khả dụng**: Tự động kiểm tra ghế và thời gian trống
- **Xác nhận tự động**: Gửi thông báo xác nhận qua email

### 🏢 Quản lý Cơ sở vật chất
- **Quản lý phòng**: Thêm, sửa, xóa phòng
- **Quản lý ghế**: Theo dõi trạng thái ghế (trống/đã đặt)
- **Bố trí tối ưu**: Tự động phân bổ ghế cho khách hàng

### 💼 Quản lý Dịch vụ
- **Danh mục dịch vụ**: Massage, chăm sóc da, nail art, v.v.
- **Giá và thời gian**: Quản lý giá dịch vụ và thời gian thực hiện
- **Combo dịch vụ**: Tạo gói dịch vụ ưu đãi

### 👤 Quản lý Khách hàng
- **Hồ sơ khách hàng**: Lưu trữ thông tin chi tiết
- **Lịch sử sử dụng**: Theo dõi lịch sử đặt lịch và thanh toán
- **Chương trình khách hàng thân thiết**: Tích điểm và ưu đãi

### 💰 Hệ thống Thanh toán
- **Hóa đơn tự động**: Tự động tạo hóa đơn sau khi đặt lịch
- **Quản lý thanh toán**: Theo dõi trạng thái thanh toán
- **Báo cáo doanh thu**: Thống kê theo ngày, tháng, năm
- **Xuất PDF**: Xuất hóa đơn định dạng PDF

## API Documentation

Dự án được trang bị Swagger UI đầy đủ để test và document các API:

### Các nhóm API chính:
- **Account API**: Đăng ký, đăng nhập, quản lý tài khoản
- **Booking API**: Đặt lịch, quản lý lịch hẹn
- **Admin API**: Các chức năng quản trị hệ thống
- **Profile API**: Quản lý hồ sơ người dùng
- **Role API**: Quản lý vai trò và phân quyền

### Truy cập Swagger UI:
- Development: `http://localhost:5000/swagger`
- Production: `https://domain.com/swagger` (nếu được cấu hình)

## Cấu trúc Dự án

```
LoanSpa/
├── Controllers/                    # MVC Controllers
│   ├── HomeController.cs          # Trang chủ và các trang chính
│   ├── AccountController.cs       # Xác thực và quản lý tài khoản
│   ├── AdminController.cs         # Các chức năng admin
│   └── API/                       # API Controllers
│       ├── AccountApiController.cs
│       ├── BookingApiController.cs
│       ├── ProfileApiController.cs
│       └── RoleApiController.cs
├── Models/                        # Data Models
│   ├── Appointment.cs             # Model lịch hẹn
│   ├── Customer.cs                # Model khách hàng
│   ├── Service.cs                 # Model dịch vụ
│   ├── Room.cs                    # Model phòng
│   ├── Chair.cs                   # Model ghế
│   └── Invoice.cs                 # Model hóa đơn
├── Views/                         # Razor Views
│   ├── Home/                      # Trang chủ
│   ├── Account/                   # Xác thực
│   ├── Admin/                     # Quản trị
│   └── Shared/                    # Layout và partial views
├── wwwroot/                       # Static Files
│   ├── css/                       # CSS files
│   ├── js/                        # JavaScript files
│   └── images/                    # Images
├── Data/                          # Database Context
│   ├── SpaDbContext.cs           # Entity Framework Context
│   └── Scripts/                   # Database scripts
├── Services/                      # Business Logic Services
│   ├── JwtService.cs             # JWT token service
│   ├── RoleInitializer.cs        # Role initialization
│   └── AvailabilityService.cs    # Kiểm tra tính khả dụng
├── Extensions/                    # Extension Methods
│   └── SwaggerExtensions.cs      # Swagger configuration
└── Migrations/                    # EF Core Migrations
```

## Hướng dẫn Cài đặt

### Yêu cầu Hệ thống
- **.NET 9.0 SDK**
- **SQL Server 2019** trở lên (hoặc SQL Server Express)
- **Visual Studio 2022** hoặc **VS Code**
- **Node.js** (tùy chọn, cho các tool frontend)

### Các Bước Cài đặt

1. **Clone Repository**
```bash
git clone <repository-url>
cd SPA
```

2. **Khôi phục Packages**
```bash
dotnet restore
```

3. **Cấu hình Database**
   - Mở `appsettings.json`
   - Cập nhật connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LoanSpaDb;Trusted_Connection=true;"
  }
}
```

4. **Chạy Migration**
```bash
dotnet ef database update
```

5. **Chạy Ứng dụng**
```bash
dotnet run
```

6. **Truy cập Ứng dụng**
   - Web: `https://localhost:5001`
   - API Documentation: `https://localhost:5001/swagger`

### Tài khoản Mặc định
Sau khi chạy lần đầu, hệ thống sẽ tự động tạo:
- **Username**: admin
- **Email**: admin@example.com  
- **Password**: Admin123!
- **Role**: Admin

## Cấu hình Môi trường

### Development
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "SwaggerSettings": {
    "ShowInProduction": false
  }
}
```

### Production
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "SwaggerSettings": {
    "ShowInProduction": true
  }
}
```

## Tính năng Nâng cao

### JWT Authentication
- Token tự động refresh
- Lưu trữ token trong HttpOnly cookies
- Thời gian hết hạn có thể cấu hình

### Real-time Updates
- Cập nhật trạng thái lịch hẹn theo thời gian thực
- Thông báo tức thì cho admin và khách hàng

### Responsive Design
- Giao diện thân thiện trên mobile
- Tối ưu cho tablet và desktop


