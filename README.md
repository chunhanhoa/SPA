# LoanSpa - Hệ thống quản lý spa và đặt lịch

## Giới thiệu
LoanSpa là ứng dụng web quản lý spa và đặt lịch sử dụng ASP.NET Core. Hệ thống giúp khách hàng dễ dàng đặt lịch sử dụng các dịch vụ spa và hỗ trợ quản lý hiệu quả các hoạt động của spa gồm: quản lý lịch hẹn, khách hàng, nhân viên, dịch vụ, phòng và ghế.

## Công nghệ sử dụng
- **Framework**: ASP.NET Core 9.0 MVC
- **Front-end**: HTML, CSS, JavaScript, Bootstrap 5, jQuery, DataTables
- **Cơ sở dữ liệu**: Entity Framework Core với SQL Server
- **Xác thực**: ASP.NET Core Identity
- **API**: RESTful API với Swagger

## Tính năng chính
- **Quản lý đặt lịch**: Theo dõi, cập nhật trạng thái lịch hẹn (chờ xác nhận, đã xác nhận)
- **Quản lý dịch vụ**: Thêm, sửa, xóa các dịch vụ spa
- **Quản lý khách hàng**: Theo dõi thông tin và lịch sử sử dụng dịch vụ của khách hàng
- **Quản lý phòng và ghế**: Bố trí và quản lý hiệu quả không gian làm việc
- **Báo cáo và thống kê**: Thống kê doanh thu, lịch hẹn theo thời gian
- **Hệ thống vai trò**: Phân quyền Admin và Nhân viên

## Hướng dẫn cài đặt

### Yêu cầu hệ thống
- .NET 9.0 SDK
- SQL Server 2019 trở lên
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt
1. Clone repository:
```bash
git clone <đường-dẫn-repository>
```

2. Khôi phục các gói NuGet:
```bash
dotnet restore
```

3. Cấu hình kết nối cơ sở dữ liệu trong `appsettings.json`

4. Chạy migration để tạo cơ sở dữ liệu:
```bash
dotnet ef database update
```

5. Chạy ứng dụng:
```bash
dotnet run
```

6. Truy cập ứng dụng tại địa chỉ: `https://localhost:5001` hoặc `http://localhost:5000`

## Cấu trúc ứng dụng
```
LoanSpa/
├── Controllers/                # Các controllers xử lý logic
│   ├── AdminController.cs      # Controller quản trị
│   ├── BookingController.cs    # Controller đặt lịch
│   └── Api/                    # Controllers API
├── Models/                     # Các model dữ liệu
├── Views/                      # Giao diện người dùng
│   ├── Admin/                  # Giao diện quản trị
│   ├── Booking/                # Giao diện đặt lịch
│   └── Shared/                 # Các layout và partial views
├── wwwroot/                    # Tài nguyên tĩnh (CSS, JS, images)
└── Data/                       # Cấu hình cơ sở dữ liệu
```

## API Endpoints

### Xác thực
- `POST /api/Account/login` - Đăng nhập
- `POST /api/Account/register` - Đăng ký
- `GET /api/Account/profile` - Lấy thông tin cá nhân

### Dịch vụ
- `GET /api/Services` - Lấy danh sách dịch vụ
- `GET /api/Services/{id}` - Lấy chi tiết dịch vụ

### Đặt lịch
- `GET /api/Bookings` - Lấy tất cả lịch đặt (yêu cầu quyền Admin)
- `GET /api/Bookings/user/{userId}` - Lấy lịch đặt của người dùng
- `POST /api/Bookings` - Tạo lịch đặt mới
- `PUT /api/Bookings/{id}` - Cập nhật lịch đặt
- `DELETE /api/Bookings/{id}` - Hủy lịch đặt
