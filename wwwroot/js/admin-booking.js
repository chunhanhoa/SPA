$(document).ready(function() {
    // Khai báo các biến toàn cục
    let bookingsTable;
    let allBookings = [];
    
    // Khởi tạo trang
    initPage();
    
    function initPage() {
        // Tải danh sách đặt lịch
        loadBookings();
        
        // Khởi tạo sự kiện
        initEvents();
    }
    
    // Khởi tạo các sự kiện
    function initEvents() {
        // Sự kiện click lọc theo trạng thái
        $('.filter-btn').click(function() {
            $('.filter-btn').removeClass('active');
            $(this).addClass('active');
            
            const filterStatus = $(this).data('status');
            filterBookingByStatus(filterStatus);
        });
        
        // Sự kiện xóa đặt lịch
        $(document).on('click', '#btnDeleteBooking', function() {
            const bookingId = $(this).data('id');
            if (confirm('Bạn có chắc muốn xóa đặt lịch này?')) {
                deleteBooking(bookingId);
            }
        });
    }
    
    // Hàm tải danh sách đặt lịch
    function loadBookings() {
        $.ajax({
            url: '/api/Booking/All',
            type: 'GET',
            beforeSend: function() {
                $('#bookingTable tbody').html(`
                    <tr>
                        <td colspan="8" class="text-center">
                            <div class="spinner-border text-primary" role="status">
                                <span class="visually-hidden">Đang tải...</span>
                            </div>
                            <p class="mt-2">Đang tải dữ liệu...</p>
                        </td>
                    </tr>
                `);
            },
            success: function(data) {
                // Lưu dữ liệu vào biến toàn cục
                allBookings = data;
                
                // Cập nhật thống kê
                updateStatistics(data);
                
                // Hiển thị dữ liệu
                renderBookings(data);
                
                // Khởi tạo DataTable
                initDataTable();
                
                console.log("Tải dữ liệu thành công:", data);
            },
            error: function(xhr, status, error) {
                console.error("Lỗi khi tải dữ liệu:", xhr.responseText);
                $('#bookingTable tbody').html(`
                    <tr>
                        <td colspan="8" class="text-center text-danger">
                            Không thể tải dữ liệu. Lỗi: ${error}
                        </td>
                    </tr>
                `);
            }
        });
    }
    
    // Hiển thị danh sách đặt lịch
    function renderBookings(bookings) {
        if (!bookings || bookings.length === 0) {
            $('#bookingTable tbody').html(`
                <tr>
                    <td colspan="8" class="text-center">
                        Không có đặt lịch nào
                    </td>
                </tr>
            `);
            return;
        }
        
        let html = '';
        
        bookings.forEach(function(booking) {
            // Format ngày giờ
            const startTime = new Date(booking.startTime);
            const date = startTime.toLocaleDateString('vi-VN');
            const time = startTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
            
            // Xác định class cho trạng thái
            let statusClass = getStatusClass(booking.status);
            
            // Tạo danh sách dịch vụ
            let services = booking.services.map(s => s.serviceName).join(", ");
            
            html += `
                <tr>
                    <td>${booking.appointmentId}</td>
                    <td>${booking.customer.fullName}</td>
                    <td>${booking.customer.phone}</td>
                    <td>${date} ${time}</td>
                    <td>${services}</td>
                    <td>${new Intl.NumberFormat('vi-VN').format(booking.totalAmount)} VNĐ</td>
                    <td>
                        <span class="badge bg-${statusClass}">${booking.status}</span>
                    </td>
                    <td>
                        <button class="btn btn-info btn-sm" onclick="viewBookingDetail(${booking.appointmentId})">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm" onclick="showUpdateStatusModal(${booking.appointmentId}, '${booking.status}')">
                            <i class="fas fa-edit"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
        
        $('#bookingTable tbody').html(html);
    }
    
    // Khởi tạo DataTable
    function initDataTable() {
        if ($.fn.DataTable.isDataTable('#bookingTable')) {
            $('#bookingTable').DataTable().destroy();
        }
        
        bookingsTable = $('#bookingTable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/vi.json'
            },
            responsive: true,
            columnDefs: [
                { orderable: false, targets: [7] }
            ],
            order: [[0, 'desc']]
        });
    }
    
    // Lọc đặt lịch theo trạng thái
    function filterBookingByStatus(status) {
        if (status === 'all') {
            renderBookings(allBookings);
        } else {
            const filteredBookings = allBookings.filter(b => b.status === status);
            renderBookings(filteredBookings);
        }
        
        if (bookingsTable) {
            bookingsTable.draw();
        }
    }
    
    // Cập nhật thống kê
    function updateStatistics(bookings) {
        if (!bookings) return;
        
        const total = bookings.length;
        const pending = bookings.filter(b => b.status === 'Chờ xác nhận').length;
        const confirmed = bookings.filter(b => b.status === 'Đã xác nhận').length;
        const cancelled = bookings.filter(b => b.status === 'Đã hủy').length;
        
        $('#stats-total').text(total);
        $('#stats-pending').text(pending);
        $('#stats-confirmed').text(confirmed);
        $('#stats-cancelled').text(cancelled);
    }
    
    // Lấy class cho trạng thái
    function getStatusClass(status) {
        switch(status) {
            case 'Đã xác nhận': return 'success';
            case 'Đang thực hiện': return 'info';
            case 'Hoàn thành': return 'primary';
            case 'Đã hủy': return 'danger';
            default: return 'secondary';
        }
    }
    
    // Hàm xem chi tiết đặt lịch
    window.viewBookingDetail = function(id) {
        $.ajax({
            url: `/api/Booking/Details/${id}`,
            type: 'GET',
            success: function(data) {
                renderBookingDetail(data, id);
                $('#bookingDetailModal').modal('show');
            },
            error: function(xhr, status, error) {
                alert('Không thể tải chi tiết đặt lịch. Lỗi: ' + error);
            }
        });
    };
    
    // Hiển thị modal cập nhật trạng thái
    window.showUpdateStatusModal = function(id, currentStatus) {
        let availableStatuses = getAvailableStatuses(currentStatus);
        let statusOptions = '';
        
        availableStatuses.forEach(status => {
            let statusClass = getStatusClass(status);
            statusOptions += `<option value="${status}" class="text-${statusClass}">${status}</option>`;
        });
        
        const modalContent = `
            <div class="modal fade" id="updateStatusModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">Cập nhật trạng thái đặt lịch #${id}</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="updateStatusForm">
                                <div class="mb-3">
                                    <label class="form-label">Trạng thái hiện tại</label>
                                    <input type="text" class="form-control" value="${currentStatus}" readonly>
                                </div>
                                <div class="mb-3">
                                    <label class="form-label">Trạng thái mới</label>
                                    <select class="form-select" id="newStatus" name="status" required>
                                        <option value="">-- Chọn trạng thái --</option>
                                        ${statusOptions}
                                    </select>
                                </div>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                            <button type="button" class="btn btn-primary" onclick="updateBookingStatus(${id})">Cập nhật</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        // Thêm modal vào body
        $('body').append(modalContent);
        
        // Hiển thị modal
        const modal = new bootstrap.Modal(document.getElementById('updateStatusModal'));
        modal.show();
        
        // Xóa modal khi đóng
        $('#updateStatusModal').on('hidden.bs.modal', function() {
            $(this).remove();
        });
    };
    
    // Cập nhật trạng thái đặt lịch
    window.updateBookingStatus = function(id) {
        const newStatus = $('#newStatus').val();
        
        if (!newStatus) {
            alert('Vui lòng chọn trạng thái');
            return;
        }
        
        $.ajax({
            url: `/api/Booking/${id}/UpdateStatus`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ status: newStatus }),
            success: function(response) {
                if (response.success) {
                    alert('Cập nhật trạng thái thành công');
                    $('#updateStatusModal').modal('hide');
                    loadBookings(); // Tải lại dữ liệu
                } else {
                    alert('Cập nhật trạng thái không thành công: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                alert('Không thể cập nhật trạng thái. Lỗi: ' + error);
            }
        });
    };
    
    // Hiển thị chi tiết đặt lịch
    function renderBookingDetail(data, id) {
        const booking = data.appointment;
        const services = data.services;
        const chairs = data.chairs;
        const rooms = data.rooms;
        
        const startTime = new Date(booking.startTime);
        const endTime = new Date(booking.endTime);
        const date = startTime.toLocaleDateString('vi-VN');
        const startTimeStr = startTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        const endTimeStr = endTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        
        let servicesList = '';
        services.forEach(service => {
            const total = service.price * service.quantity;
            servicesList += `
                <tr>
                    <td>${service.serviceName}</td>
                    <td>${service.quantity}</td>
                    <td>${new Intl.NumberFormat('vi-VN').format(service.price)} VNĐ</td>
                    <td>${new Intl.NumberFormat('vi-VN').format(total)} VNĐ</td>
                </tr>
            `;
        });
        
        let statusClass = getStatusClass(booking.status);
        
        const detailHTML = `
            <div class="row">
                <div class="col-md-6">
                    <h5>Thông tin khách hàng</h5>
                    <table class="table table-sm">
                        <tr>
                            <th width="40%">Họ tên:</th>
                            <td>${booking.customer.fullName}</td>
                        </tr>
                        <tr>
                            <th>Điện thoại:</th>
                            <td>${booking.customer.phone}</td>
                        </tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h5>Thông tin đặt lịch</h5>
                    <table class="table table-sm">
                        <tr>
                            <th width="40%">Ngày tạo:</th>
                            <td>${new Date(booking.createdDate).toLocaleDateString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</td>
                        </tr>
                        <tr>
                            <th>Thời gian:</th>
                            <td>${date} (${startTimeStr} - ${endTimeStr})</td>
                        </tr>
                        <tr>
                            <th>Trạng thái:</th>
                            <td><span class="badge bg-${statusClass}">${booking.status}</span></td>
                        </tr>
                    </table>
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-12">
                    <h5>Dịch vụ</h5>
                    <table class="table table-striped table-bordered">
                        <thead>
                            <tr>
                                <th>Dịch vụ</th>
                                <th>Số lượng</th>
                                <th>Đơn giá</th>
                                <th>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${servicesList}
                        </tbody>
                        <tfoot>
                            <tr>
                                <th colspan="3" class="text-end">Tổng cộng:</th>
                                <th>${new Intl.NumberFormat('vi-VN').format(booking.totalAmount)} VNĐ</th>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-md-6">
                    <h5>Phòng & Ghế</h5>
                    <table class="table table-sm">
                        <tr>
                            <th width="40%">Phòng:</th>
                            <td>${rooms[0]?.roomName || 'N/A'}</td>
                        </tr>
                        <tr>
                            <th>Ghế:</th>
                            <td>${chairs[0]?.chairName || 'N/A'}</td>
                        </tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h5>Ghi chú</h5>
                    <p>${booking.notes || 'Không có ghi chú'}</p>
                </div>
            </div>
        `;
        
        $('#bookingDetailContent').html(detailHTML);
        $('#btnDeleteBooking').data('id', id);
        
        // Cập nhật tiêu đề modal
        $('#bookingDetailModalLabel').text(`Chi tiết đặt lịch #${booking.appointmentId}`);
    }
    
    // Lấy danh sách trạng thái có thể chuyển đổi
    function getAvailableStatuses(currentStatus) {
        switch(currentStatus) {
            case 'Chờ xác nhận':
                return ['Đã xác nhận', 'Đã hủy'];
            case 'Đã xác nhận':
                return ['Đang thực hiện', 'Đã hủy'];
            case 'Đang thực hiện':
                return ['Hoàn thành', 'Đã hủy'];
            case 'Hoàn thành':
                return ['Đã hủy'];
            case 'Đã hủy':
                return ['Chờ xác nhận'];
            default:
                return [];
        }
    }
    
    // Xóa đặt lịch
    function deleteBooking(id) {
        $.ajax({
            url: `/api/Booking/${id}`,
            type: 'DELETE',
            success: function(response) {
                if (response.success) {
                    alert('Xóa đặt lịch thành công');
                    $('#bookingDetailModal').modal('hide');
                    loadBookings(); // Tải lại dữ liệu
                } else {
                    alert('Xóa đặt lịch không thành công: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                alert('Không thể xóa đặt lịch. Lỗi: ' + error);
            }
        });
    }
});
