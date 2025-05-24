// Service giả để xử lý các cuộc gọi API trong quá trình phát triển frontend

// Dữ liệu mẫu cho ứng dụng
const mockData = {
  services: [
    { id: 1, name: 'Dịch vụ spa 1', price: 500000, description: 'Mô tả dịch vụ spa 1' },
    { id: 2, name: 'Dịch vụ spa 2', price: 700000, description: 'Mô tả dịch vụ spa 2' },
    { id: 3, name: 'Dịch vụ spa 3', price: 1000000, description: 'Mô tả dịch vụ spa 3' },
  ],
  bookings: []
};

export const mockApiService = {
  // Lấy danh sách dịch vụ
  getServices: () => {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(mockData.services);
      }, 300);
    });
  },
  
  // Lấy chi tiết dịch vụ
  getServiceById: (id) => {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        const service = mockData.services.find(s => s.id === parseInt(id));
        if (service) {
          resolve(service);
        } else {
          reject('Không tìm thấy dịch vụ');
        }
      }, 300);
    });
  },
  
  // Đặt lịch
  createBooking: (bookingData) => {
    return new Promise((resolve) => {
      setTimeout(() => {
        const newBooking = {
          id: Math.floor(Math.random() * 1000),
          ...bookingData,
          status: 'pending',
          createdAt: new Date().toISOString()
        };
        mockData.bookings.push(newBooking);
        resolve(newBooking);
      }, 500);
    });
  },
  
  // Lấy lịch sử đặt lịch
  getUserBookings: (userId) => {
    return new Promise((resolve) => {
      setTimeout(() => {
        const userBookings = mockData.bookings.filter(b => b.userId === userId);
        resolve(userBookings);
      }, 300);
    });
  }
};
