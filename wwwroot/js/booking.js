// Look for a function that loads user bookings
function loadUserBookings() {
    // Hiển thị trạng thái đang tải
    const container = document.getElementById('userBookingsContainer');
    if (container) {
        container.innerHTML = '<p class="text-center">Đang tải thông tin lịch hẹn...</p>';
    }

    // Make sure you're using the correct API endpoint
    fetch('/api/Booking/user')
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error('Vui lòng đăng nhập để xem lịch hẹn của bạn.');
                } else {
                    throw new Error('Lỗi khi tải dữ liệu: ' + response.status);
                }
            }
            return response.json();
        })
        .then(data => {
            console.log('My bookings data:', data); // Detailed debug logging
            
            // Check if data is empty
            if (!data || data.length === 0) {
                document.getElementById('userBookingsContainer').innerHTML = 
                    '<p class="text-center">Bạn chưa đặt lịch nào.</p>';
                return;
            }
            
            // Make sure the container element exists
            const container = document.getElementById('userBookingsContainer');
            if (!container) {
                console.error('Bookings container not found!');
                return;
            }
            
            // Generate HTML for bookings
            let html = '<div class="row">';
            
            data.forEach(booking => {
                const startTime = new Date(booking.startTime);
                const formattedDate = startTime.toLocaleDateString('vi-VN');
                const formattedTime = startTime.toLocaleTimeString('vi-VN', {hour: '2-digit', minute: '2-digit'});
                
                let servicesList = '';
                if (booking.services && booking.services.length > 0) {
                    servicesList = booking.services.map(s => `${s.ServiceName}`).join(', ');
                }
                
                html += `
                    <div class="col-md-6 mb-4">
                        <div class="card h-100">
                            <div class="card-header d-flex justify-content-between align-items-center">
                                <h5 class="mb-0">Lịch hẹn #${booking.appointmentId}</h5>
                                <span class="badge ${getStatusBadgeClass(booking.status)}">${booking.status}</span>
                            </div>
                            <div class="card-body">
                                <p><strong>Ngày giờ:</strong> ${formattedDate} ${formattedTime}</p>
                                <p><strong>Dịch vụ:</strong> ${servicesList}</p>
                                <p><strong>Tổng tiền:</strong> ${formatCurrency(booking.totalAmount)}</p>
                            </div>
                            <div class="card-footer text-end">
                                <a href="/Home/BookingConfirmation/${booking.appointmentId}" class="btn btn-sm btn-info">
                                    Xem chi tiết
                                </a>
                            </div>
                        </div>
                    </div>
                `;
            });
            
            html += '</div>';
            container.innerHTML = html;
        })
        .catch(error => {
            console.error('Error loading user bookings:', error);
            document.getElementById('userBookingsContainer').innerHTML = 
                `<p class="text-center text-danger">Đã xảy ra lỗi: ${error.message}</p>`;
        });
}

// Helper function to get appropriate badge class for status
function getStatusBadgeClass(status) {
    switch (status) {
        case 'Đã xác nhận': return 'bg-info text-white';
        case 'Đang thực hiện': return 'bg-primary text-white';
        case 'Hoàn thành': return 'bg-success text-white';
        case 'Đã hủy': return 'bg-danger text-white';
        default: return 'bg-secondary text-white';
    }
}

// Helper function to format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Gọi hàm load dữ liệu khi tab "Lịch Của Tôi" được kích hoạt
document.addEventListener('DOMContentLoaded', function() {
    // Load user bookings when "Lịch Của Tôi" tab is activated
    const tabs = document.querySelectorAll('a[data-bs-toggle="tab"], button[data-bs-toggle="tab"]');
    if (tabs) {
        tabs.forEach(tab => {
            tab.addEventListener('shown.bs.tab', function (event) {
                if (event.target.getAttribute('href') === '#my-bookings') {
                    loadUserBookings();
                }
            });
        });
    }
    
    // Also load initially if the tab is active on page load
    if (document.querySelector('.tab-pane.active#my-bookings')) {
        loadUserBookings();
    }
});
