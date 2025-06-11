/**
 * Admin Booking Management JavaScript
 * Handles all booking management functionality including:
 * - Loading and displaying bookings
 * - Filtering and searching
 * - Status updates
 * - Viewing booking details
 */

$(document).ready(function() {
    // Global variables
    let bookingsTable;
    let allBookings = [];
    
    // Initialize page
    initPage();
    
    function initPage() {
        // Load booking data
        loadBookingData();
        
        // Initialize events
        initEvents();
    }
    
    // Initialize event handlers
    function initEvents() {
        $('#refresh-data').click(loadBookingData);
        $('#reset-filters').click(resetFilters);
        $('#status-filter, #date-filter').change(applyFilters);
        $('#search-input').on('keyup', applyFilters);
        
        // Edit status button click
        $(document).on('click', '#edit-status-btn', function() {
            const id = $('#detail-id').text();
            const currentStatus = $('#booking-current-status').text();
            
            $('#update-id').text(id);
            $('#current-status').val(currentStatus);
            $('#new-status').val(currentStatus);
            $('#status-update-message').addClass('d-none');
            
            $('#bookingDetailModal').modal('hide');
            $('#updateStatusModal').modal('show');
        });
        
        // Save status button click
        $('#save-status-btn').click(function() {
            const id = $('#update-id').text();
            const newStatus = $('#new-status').val();
            
            updateBookingStatus(id, newStatus);
        });
        
        // View booking button click
        $(document).on('click', '.view-booking', function() {
            const id = $(this).data('id');
            viewBookingDetail(id);
        });
        
        // Edit status button click
        $(document).on('click', '.edit-status', function() {
            const id = $(this).data('id');
            const status = $(this).data('status');
            
            $('#update-id').text(id);
            $('#current-status').val(status);
            $('#new-status').val(status);
            $('#status-update-message').addClass('d-none');
            
            $('#updateStatusModal').modal('show');
        });
        
        // Export to Excel button click
        $('#export-excel').click(function() {
            alert('Tính năng xuất Excel đang được phát triển');
            // Future implementation will go here
        });
    }
    
    // Load booking data from API
    function loadBookingData() {
        $.ajax({
            url: '/api/Booking/ManagementData',
            type: 'GET',
            beforeSend: function() {
                $('#bookingsTable tbody').html('<tr><td colspan="9" class="text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"></div> Đang tải dữ liệu...</td></tr>');
            },
            success: function(data) {
                allBookings = data;
                renderBookingsTable(data);
                updateStats(data);
            },
            error: function(xhr, status, error) {
                $('#bookingsTable tbody').html(`<tr><td colspan="9" class="text-center text-danger">Lỗi: ${error}</td></tr>`);
                console.error('Error loading booking data:', error);
            }
        });
    }
    
    // Render bookings data in table
    function renderBookingsTable(bookings) {
        if (bookingsTable) {
            bookingsTable.destroy();
        }
        
        let tableBody = '';
        
        if (bookings.length === 0) {
            tableBody = '<tr><td colspan="9" class="text-center">Không có dữ liệu</td></tr>';
            $('#bookingsTable tbody').html(tableBody);
            return;
        }
        
        bookings.forEach(function(booking) {
            // Format dates and times
            const createdDate = new Date(booking.createdDate).toLocaleDateString('vi-VN');
            const startDate = new Date(booking.startTime).toLocaleDateString('vi-VN');
            const startTime = new Date(booking.startTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
            const endTime = new Date(booking.endTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
            
            // Format status with appropriate badge
            let statusBadge = getStatusBadge(booking.status);
            
            // Format services
            const services = booking.services.join(', ') || 'Không có dịch vụ';
            
            // Format notes (truncate if too long)
            const notes = booking.notes ? (booking.notes.length > 30 ? booking.notes.substring(0, 30) + '...' : booking.notes) : '';
            
            // Format chairs - add better null checking
            let chairs = 'Không có ghế';
            if (booking.chairs && booking.chairs.length > 0) {
                chairs = booking.chairs.map(c => {
                    const chairName = c.chairName || 'N/A';
                    const roomName = c.roomName || 'N/A';
                    return `${chairName} (${roomName})`;
                }).join(', ');
            }
            
            tableBody += `
                <tr>
                    <td>${booking.appointmentId}</td>
                    <td>${booking.customerName}</td>
                    <td>${createdDate}</td>
                    <td>${startTime} - ${endTime}</td>
                    <td>${services}</td>
                    <td>${new Intl.NumberFormat('vi-VN').format(booking.totalAmount)} VNĐ</td>
                    <td>${statusBadge}</td>
                    <td>${notes}</td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-info view-booking" data-id="${booking.appointmentId}">
                                <i class="bi bi-eye"></i>
                            </button>
                            <button type="button" class="btn btn-primary edit-status" data-id="${booking.appointmentId}" data-status="${booking.status}">
                                <i class="bi bi-pencil"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        });
        
        $('#bookingsTable tbody').html(tableBody);
        
        // Initialize DataTable
        bookingsTable = $('#bookingsTable').DataTable({
            responsive: true,
            language: {
                url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/vi.json'
            },
            order: [[0, 'desc']]
        });
    }
    
    // Get status badge HTML
    function getStatusBadge(status) {
        switch(status) {
            case 'Chờ xác nhận':
                return '<span class="badge bg-warning">Chờ xác nhận</span>';
            case 'Đã xác nhận':
                return '<span class="badge bg-success">Đã xác nhận</span>';
            case 'Đang thực hiện':
                return '<span class="badge bg-info">Đang thực hiện</span>';
            case 'Hoàn thành':
                return '<span class="badge bg-primary">Hoàn thành</span>';
            case 'Đã hủy':
                return '<span class="badge bg-danger">Đã hủy</span>';
            default:
                return `<span class="badge bg-secondary">${status}</span>`;
        }
    }
    
    // View booking detail
    function viewBookingDetail(id) {
        $.ajax({
            url: `/api/Booking/Details/${id}`,
            type: 'GET',
            beforeSend: function() {
                $('#detail-id').text(id);
                $('#booking-detail-content').html(`
                    <div class="text-center py-5">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Đang tải...</span>
                        </div>
                        <p class="mt-2">Đang tải chi tiết đặt lịch...</p>
                    </div>
                `);
                $('#bookingDetailModal').modal('show');
            },
            success: function(data) {
                renderBookingDetail(data, id);
            },
            error: function(xhr, status, error) {
                $('#booking-detail-content').html(`
                    <div class="alert alert-danger">
                        Không thể tải thông tin chi tiết. Lỗi: ${error}
                    </div>
                `);
            }
        });
    }
    
    // Render booking detail in modal
    function renderBookingDetail(data, id) {
        const appointment = data.appointment;
        const services = data.services;
        const chairs = data.chairs;
        
        // Format dates
        const createdDate = new Date(appointment.CreatedDate).toLocaleDateString('vi-VN');
        const startDate = new Date(appointment.StartTime).toLocaleDateString('vi-VN');
        const startTime = new Date(appointment.StartTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        const endTime = new Date(appointment.EndTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        
        // Services list
        let servicesHtml = '';
        if (services && services.length > 0) {
            servicesHtml = '<ul class="list-group">';
            services.forEach(service => {
                servicesHtml += `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        ${service.ServiceName}
                        <span class="badge bg-primary rounded-pill">${new Intl.NumberFormat('vi-VN').format(service.Price)} VNĐ</span>
                    </li>
                `;
            });
            servicesHtml += '</ul>';
        } else {
            servicesHtml = '<p class="text-muted">Không có dịch vụ</p>';
        }
        
        // Add chairs to the detail view
        let chairsHtml = '';
        if (chairs && chairs.length > 0) {
            chairsHtml = '<ul class="list-group mt-3">';
            chairs.forEach(chair => {
                chairsHtml += `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        ${chair.ChairName || chair.chairName || 'N/A'}
                        <span class="badge bg-info rounded-pill">Phòng: ${chair.RoomName || chair.roomName || 'N/A'}</span>
                    </li>
                `;
            });
            chairsHtml += '</ul>';
        } else {
            chairsHtml = '<p class="text-muted mt-3">Không có ghế</p>';
        }
        
        const html = `
            <div class="row">
                <div class="col-md-6">
                    <h5 class="border-bottom pb-2">Thông tin đặt lịch</h5>
                    <dl class="row">
                        <dt class="col-sm-4">Mã đặt lịch:</dt>
                        <dd class="col-sm-8">#${appointment.AppointmentId}</dd>
                        
                        <dt class="col-sm-4">Ngày tạo:</dt>
                        <dd class="col-sm-8">${createdDate}</dd>
                        
                        <dt class="col-sm-4">Thời gian:</dt>
                        <dd class="col-sm-8">${startDate} (${startTime} - ${endTime})</dd>
                        
                        <dt class="col-sm-4">Tổng tiền:</dt>
                        <dd class="col-sm-8">${new Intl.NumberFormat('vi-VN').format(appointment.TotalAmount)} VNĐ</dd>
                        
                        <dt class="col-sm-4">Trạng thái:</dt>
                        <dd class="col-sm-8" id="booking-current-status">${appointment.Status}</dd>
                        
                        <dt class="col-sm-4">Ghi chú:</dt>
                        <dd class="col-sm-8">${appointment.Notes || 'Không có ghi chú'}</dd>
                    </dl>
                </div>
                <div class="col-md-6">
                    <h5 class="border-bottom pb-2">Thông tin khách hàng</h5>
                    <dl class="row">
                        <dt class="col-sm-4">Họ tên:</dt>
                        <dd class="col-sm-8">${appointment.Customer.FullName}</dd>
                        
                        <dt class="col-sm-4">Số điện thoại:</dt>
                        <dd class="col-sm-8">${appointment.Customer.Phone}</dd>
                    </dl>
                    
                    <h5 class="border-bottom pb-2 mt-4">Dịch vụ đã đặt</h5>
                    ${servicesHtml}
                    
                    <h5 class="border-bottom pb-2 mt-4">Ghế đã đặt</h5>
                    ${chairsHtml}
                </div>
            </div>
        `;
        
        $('#booking-detail-content').html(html);
    }
    
    // Update booking status
    function updateBookingStatus(id, status) {
        $.ajax({
            url: `/api/Booking/${id}/UpdateStatus`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ status: status }),
            beforeSend: function() {
                $('#save-status-btn').prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');
                $('#status-update-message').removeClass('alert-success alert-danger').addClass('d-none');
            },
            success: function(response) {
                $('#status-update-message').removeClass('d-none alert-danger').addClass('alert-success').text('Cập nhật trạng thái thành công');
                
                setTimeout(function() {
                    $('#updateStatusModal').modal('hide');
                    loadBookingData(); // Reload data
                }, 1500);
            },
            error: function(xhr, status, error) {
                $('#status-update-message').removeClass('d-none alert-success').addClass('alert-danger').text(`Lỗi: ${xhr.responseJSON?.message || error}`);
            },
            complete: function() {
                $('#save-status-btn').prop('disabled', false).text('Lưu thay đổi');
            }
        });
    }
    
    // Update statistics
    function updateStats(bookings) {
        const total = bookings.length;
        const pending = bookings.filter(b => b.status === 'Chờ xác nhận').length;
        const confirmed = bookings.filter(b => b.status === 'Đã xác nhận').length;
        const cancelled = bookings.filter(b => b.status === 'Đã hủy').length;
        
        $('#total-bookings').text(`${total} lịch hẹn`);
        $('#pending-bookings').text(`${pending} lịch hẹn`);
        $('#confirmed-bookings').text(`${confirmed} lịch hẹn`);
        $('#cancelled-bookings').text(`${cancelled} lịch hẹn`);
    }
    
    // Apply filters
    function applyFilters() {
        const statusFilter = $('#status-filter').val();
        const dateFilter = $('#date-filter').val();
        const searchTerm = $('#search-input').val().toLowerCase();
        
        let filteredBookings = [...allBookings];
        
        // Apply status filter
        if (statusFilter) {
            filteredBookings = filteredBookings.filter(b => b.status === statusFilter);
        }
        
        // Apply date filter
        if (dateFilter) {
            const filterDate = new Date(dateFilter);
            filteredBookings = filteredBookings.filter(b => {
                const bookingDate = new Date(b.startTime);
                return bookingDate.toDateString() === filterDate.toDateString();
            });
        }
        
        // Apply search filter
        if (searchTerm) {
            filteredBookings = filteredBookings.filter(b => 
                b.customerName.toLowerCase().includes(searchTerm) || 
                (b.notes && b.notes.toLowerCase().includes(searchTerm))
            );
        }
        
        renderBookingsTable(filteredBookings);
    }
    
    // Reset filters
    function resetFilters() {
        $('#status-filter').val('');
        $('#date-filter').val('');
        $('#search-input').val('');
        
        renderBookingsTable(allBookings);
    }
});
