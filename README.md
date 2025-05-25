# LoanSpa - Hệ thống quản lý spa và đặt lịch

## Giới thiệu
LoanSpa là ứng dụng quản lý spa và đặt lịch sử dụng kiến trúc Web API kết hợp với giao diện người dùng hiện đại. Ứng dụng cung cấp các tính năng cho phép khách hàng xem thông tin về các dịch vụ spa, đặt lịch hẹn và quản lý lịch hẹn của mình.

## Công nghệ sử dụng
- **Backend**: ASP.NET Core 9.0
- **Xác thực**: JWT (JSON Web Token)
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Cơ sở dữ liệu**: Entity Framework Core (đã cấu hình nhưng hiện tại dữ liệu lưu tạm trong bộ nhớ)
- **API Client**: Axios

## Tính năng chính
- **Quản lý dịch vụ spa**: Xem danh sách, chi tiết của các dịch vụ
- **Đặt lịch hẹn**: Khách hàng có thể đặt lịch sử dụng dịch vụ
- **Xác thực người dùng**: Đăng nhập, đăng ký tài khoản
- **Quản lý lịch hẹn**: Xem, cập nhật và hủy lịch hẹn
- **Hồ sơ người dùng**: Quản lý thông tin cá nhân

## Cấu trúc dự án
```
SPA/
├── Controllers/                # Controllers xử lý logic
│   ├── HomeController.cs       # Controller cho các trang giao diện
│   └── API/                    # API Controllers
│       ├── AuthController.cs   # Xử lý xác thực
│       ├── BookingsController.cs # Quản lý đặt lịch
│       └── ServicesController.cs # Quản lý dịch vụ
├── Models/                     # Định nghĩa các model dữ liệu
│   ├── AppUser.cs              # Model người dùng
│   ├── Service.cs              # Model dịch vụ spa
│   └── Booking.cs              # Model đặt lịch
├── Views/                      # Giao diện Razor
│   ├── Home/                   # Các trang chính
│   └── Shared/                 # Layout và partial views
├── ClientApp/                  # Mã nguồn frontend
│   ├── src/
│   │   ├── services/           # Logic gọi API
│   │   └── utils/              # Công cụ hỗ trợ
├── Services/                   # Các dịch vụ phía máy chủ
│   └── JwtService.cs           # Dịch vụ xử lý JWT
└── Program.cs                  # Khởi tạo ứng dụng
```

## Hướng dẫn cài đặt

### Yêu cầu
- .NET 9.0 SDK
- Node.js và npm (cho phát triển frontend)
- Visual Studio 2022 hoặc Visual Studio Code

### Các bước cài đặt
1. Clone repository về máy:
```
git clone <repository-url>
```

2. Mở dự án trong Visual Studio hoặc VS Code

3. Khôi phục các gói NuGet:
```
dotnet restore
```

4. Cài đặt các gói npm cho frontend:
```
cd ClientApp
npm install
```

5. Chạy ứng dụng:
```
dotnet run
```

6. Truy cập ứng dụng tại địa chỉ: `https://localhost:5001` hoặc `http://localhost:5000`

## API Endpoints

### Xác thực
- `POST /api/Auth/login` - Đăng nhập
- `POST /api/Auth/register` - Đăng ký
- `GET /api/Auth/profile` - Lấy thông tin cá nhân

### Dịch vụ
- `GET /api/Services` - Lấy danh sách dịch vụ
- `GET /api/Services/{id}` - Lấy chi tiết dịch vụ

### Đặt lịch
- `GET /api/Bookings` - Lấy tất cả lịch đặt (yêu cầu quyền Admin)
- `GET /api/Bookings/user/{userId}` - Lấy lịch đặt của người dùng
- `POST /api/Bookings` - Tạo lịch đặt mới
- `PUT /api/Bookings/{id}` - Cập nhật lịch đặt
- `DELETE /api/Bookings/{id}` - Hủy lịch đặt

## Người đóng góp
- Nguyễn Thị Loan - Người sáng lập

## Giấy phép
© 2024 LoanSpa. All rights reserved.