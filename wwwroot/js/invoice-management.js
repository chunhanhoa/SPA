$(document).ready(function() {
    // Khai báo các biến toàn cục
    let invoicesTable;
    let allInvoices = [];
    
    // Khởi tạo trang
    initPage();
    
    function initPage() {
        // Tải danh sách hóa đơn
        loadInvoices();
        
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
            filterInvoicesByStatus(filterStatus);
        });
        
        // Sự kiện xem chi tiết hóa đơn
        $(document).on('click', '.view-invoice', function() {
            const invoiceId = $(this).data('id');
            viewInvoiceDetail(invoiceId);
        });
        
        // Sự kiện cập nhật trạng thái hóa đơn
        $(document).on('click', '.update-status', function() {
            const invoiceId = $(this).data('id');
            const currentStatus = $(this).data('status');
            showUpdateStatusModal(invoiceId, currentStatus);
        });
        
        // Sự kiện cập nhật thanh toán
        $(document).on('click', '.update-payment', function() {
            const invoiceId = $(this).data('id');
            const discount = $(this).data('discount');
            const paidAmount = $(this).data('paid');
            const totalAmount = $(this).data('total');
            showUpdatePaymentModal(invoiceId, discount, paidAmount, totalAmount);
        });
        
        // Thêm xử lý cho các nút thao tác trực tiếp trên bảng (nếu có)
        $(document).on('click', '.status-option', function(e) {
            e.preventDefault();
            const invoiceId = $(this).data('id');
            const newStatus = $(this).data('status');
            
            if (confirm(`Bạn có chắc muốn cập nhật trạng thái hóa đơn thành "${newStatus}"?`)) {
                // Hiển thị spinner trong nút
                const btn = $(this);
                const originalHtml = btn.html();
                btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');
                btn.prop('disabled', true);
                
                // Gọi API cập nhật trạng thái
                $.ajax({
                    url: `/api/Invoice/${invoiceId}/UpdateStatus`,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ status: newStatus }),
                    success: function(response) {
                        if (response.success) {
                            showToast('Thành công', 'Cập nhật trạng thái hóa đơn thành công', 'success');
                            
                            // Đợi 1 giây để người dùng thấy thông báo, sau đó tải lại trang
                            setTimeout(function() {
                                window.location.reload();
                            }, 1000);
                        } else {
                            btn.html(originalHtml);
                            btn.prop('disabled', false);
                            showToast('Lỗi', response.message || 'Cập nhật trạng thái không thành công', 'danger');
                        }
                    },
                    error: function(xhr, status, error) {
                        btn.html(originalHtml);
                        btn.prop('disabled', false);
                        showToast('Lỗi', 'Không thể cập nhật trạng thái. Lỗi: ' + error, 'danger');
                    }
                });
            }
        });
        
        // Add a direct handler for quick action buttons in the table
        $(document).on('click', '.quick-action-btn', function() {
            const action = $(this).data('action');
            const invoiceId = $(this).data('id');
            
            // Display spinner in the button
            const btn = $(this);
            const originalHtml = btn.html();
            btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');
            btn.prop('disabled', true);
            
            if (action === 'mark-paid') {
                $.ajax({
                    url: `/api/Invoice/${invoiceId}/UpdateStatus`,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ status: 'Đã thanh toán' }),
                    success: function(response) {
                        if (response.success) {
                            showToast('Thành công', 'Đã đánh dấu hóa đơn là đã thanh toán', 'success');
                            
                            // Wait 1 second then reload the page
                            setTimeout(function() {
                                window.location.reload();
                            }, 1000);
                        } else {
                            btn.html(originalHtml);
                            btn.prop('disabled', false);
                            showToast('Lỗi', response.message || 'Cập nhật không thành công', 'danger');
                        }
                    },
                    error: function(xhr, status, error) {
                        btn.html(originalHtml);
                        btn.prop('disabled', false);
                        showToast('Lỗi', 'Không thể cập nhật. Lỗi: ' + error, 'danger');
                    }
                });
            } else if (action === 'cancel') {
                $.ajax({
                    url: `/api/Invoice/${invoiceId}/UpdateStatus`,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ status: 'Đã hủy' }),
                    success: function(response) {
                        if (response.success) {
                            showToast('Thành công', 'Đã hủy hóa đơn', 'success');
                            
                            // Wait 1 second then reload the page
                            setTimeout(function() {
                                window.location.reload();
                            }, 1000);
                        } else {
                            btn.html(originalHtml);
                            btn.prop('disabled', false);
                            showToast('Lỗi', response.message || 'Hủy không thành công', 'danger');
                        }
                    },
                    error: function(xhr, status, error) {
                        btn.html(originalHtml);
                        btn.prop('disabled', false);
                        showToast('Lỗi', 'Không thể hủy. Lỗi: ' + error, 'danger');
                    }
                });
            }
        });
    }
    
    // Hàm tải danh sách hóa đơn
    function loadInvoices() {
        $.ajax({
            url: '/api/Invoice',
            type: 'GET',
            beforeSend: function() {
                $('#invoicesTable tbody').html(`
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
                allInvoices = data;
                
                // Cập nhật thống kê
                updateStatistics(data);
                
                // Hiển thị dữ liệu
                renderInvoices(data);
                
                // Khởi tạo DataTable
                initDataTable();
                
                console.log("Tải dữ liệu thành công:", data);
            },
            error: function(xhr, status, error) {
                console.error("Lỗi khi tải dữ liệu:", xhr.responseText);
                $('#invoicesTable tbody').html(`
                    <tr>
                        <td colspan="8" class="text-center text-danger">
                            Không thể tải dữ liệu. Lỗi: ${error}
                        </td>
                    </tr>
                `);
            }
        });
    }
    
    // Hiển thị danh sách hóa đơn
    function renderInvoices(invoices) {
        if (!invoices || invoices.length === 0) {
            $('#invoicesTable tbody').html(`
                <tr>
                    <td colspan="8" class="text-center">
                        Không có hóa đơn nào
                    </td>
                </tr>
            `);
            return;
        }
        
        let html = '';
        
        invoices.forEach(function(invoice) {
            // Format ngày giờ
            const createdDate = new Date(invoice.createdDate);
            const date = createdDate.toLocaleDateString('vi-VN');
            const time = createdDate.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
            
            // Xác định class cho trạng thái
            let statusClass = getStatusClass(invoice.status);
            
            // Số tiền đã thanh toán / tổng
            const paymentPercent = invoice.totalAmount > 0 
                ? Math.round((invoice.paidAmount / invoice.finalAmount) * 100) 
                : 0;
            
            // Tạo thanh tiến trình thanh toán
            const progressClass = paymentPercent >= 100 ? 'bg-success' : 
                                   paymentPercent > 0 ? 'bg-info' : 'bg-warning';
            
            html += `
                <tr>
                    <td>${invoice.invoiceId}</td>
                    <td>${invoice.customerName}</td>
                    <td>${invoice.customerPhone}</td>
                    <td>${date} ${time}</td>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="progress flex-grow-1 me-2" style="height: 10px;">
                                <div class="progress-bar ${progressClass}" role="progressbar" 
                                    style="width: ${paymentPercent}%" 
                                    aria-valuenow="${paymentPercent}" aria-valuemin="0" aria-valuemax="100">
                                </div>
                            </div>
                            <span class="small">${paymentPercent}%</span>
                        </div>
                        <div class="small mt-1">
                            <span>${new Intl.NumberFormat('vi-VN').format(invoice.paidAmount)}</span> / 
                            <span>${new Intl.NumberFormat('vi-VN').format(invoice.finalAmount)}</span> VNĐ
                        </div>
                    </td>
                    <td>
                        <span class="badge bg-${statusClass}">${invoice.status}</span>
                    </td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-primary update-payment" 
                                data-id="${invoice.invoiceId}"
                                data-discount="${invoice.discount}"
                                data-paid="${invoice.paidAmount}"
                                data-total="${invoice.totalAmount}">
                                <i class="fas fa-money-bill-wave"></i>
                            </button>
                            <button class="btn btn-warning update-status" 
                                data-id="${invoice.invoiceId}" 
                                data-status="${invoice.status}">
                                <i class="fas fa-edit"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        });
        
        $('#invoicesTable tbody').html(html);
    }
    
    // Khởi tạo DataTable
    function initDataTable() {
        if ($.fn.DataTable.isDataTable('#invoicesTable')) {
            $('#invoicesTable').DataTable().destroy();
        }
        
        invoicesTable = $('#invoicesTable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/vi.json'
            },
            responsive: true,
            columnDefs: [
                { orderable: false, targets: [6] }
            ],
            order: [[0, 'desc']]
        });
    }
    
    // Lọc hóa đơn theo trạng thái
    function filterInvoicesByStatus(status) {
        if (status === 'all') {
            renderInvoices(allInvoices);
        } else {
            const filteredInvoices = allInvoices.filter(i => i.status === status);
            renderInvoices(filteredInvoices);
        }
        
        if (invoicesTable) {
            invoicesTable.draw();
        }
    }
    
    // Cập nhật thống kê
    function updateStatistics(invoices) {
        if (!invoices) return;
        
        const total = invoices.length;
        const pending = invoices.filter(i => i.status === 'Chờ thanh toán').length;
        const paid = invoices.filter(i => i.status === 'Đã thanh toán').length;
        const cancelled = invoices.filter(i => i.status === 'Đã hủy').length;
        
        $('#stats-total').text(total);
        $('#stats-pending').text(pending);
        $('#stats-paid').text(paid);
        $('#stats-cancelled').text(cancelled);
    }
    
    // Lấy class cho trạng thái
    function getStatusClass(status) {
        switch(status) {
            case 'Đã thanh toán':
                return 'success';
            case 'Chờ thanh toán':
                return 'warning';
            case 'Đã hủy':
                return 'danger';
            case 'Hoàn tiền':
                return 'info';
            default:
                return 'secondary';
        }
    }
    
    // Hàm xem chi tiết hóa đơn
    function viewInvoiceDetail(id) {
        // Show loading state in the modal
        $('#invoiceDetailContent').html(`
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Đang tải...</span>
                </div>
                <p class="mt-3">Đang tải thông tin hóa đơn...</p>
            </div>
        `);
        $('#invoiceDetailModal').modal('show');
        $('#invoiceDetailModalLabel').text(`Đang tải chi tiết hóa đơn...`);
        
        $.ajax({
            url: `/api/Invoice/${id}`,
            type: 'GET',
            success: function(data) {
                console.log("Loaded invoice data:", data);
                
                // Format ngày giờ
                const createdDate = new Date(data.createdDate);
                const formattedDate = createdDate.toLocaleDateString('vi-VN');
                const formattedTime = createdDate.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
                
                // Tạo HTML hiển thị dịch vụ
                let servicesHtml = '';
                let totalServices = 0;
                
                if (data.services && data.services.length > 0) {
                    servicesHtml = '<div class="table-responsive"><table class="table table-striped table-bordered">';
                    servicesHtml += '<thead class="table-light"><tr><th>Dịch vụ</th><th class="text-center">Số lượng</th><th class="text-end">Đơn giá</th><th class="text-end">Thành tiền</th></tr></thead><tbody>';
                    
                    data.services.forEach(service => {
                        const subtotal = service.price * service.quantity;
                        totalServices += subtotal;
                        servicesHtml += `<tr>
                            <td><strong>${service.serviceName}</strong></td>
                            <td class="text-center">${service.quantity}</td>
                            <td class="text-end">${new Intl.NumberFormat('vi-VN').format(service.price)} VNĐ</td>
                            <td class="text-end">${new Intl.NumberFormat('vi-VN').format(subtotal)} VNĐ</td>
                        </tr>`;
                    });
                    
                    servicesHtml += '</tbody></table></div>';
                } else {
                    servicesHtml = '<div class="alert alert-info">Không có dịch vụ nào trong hóa đơn này</div>';
                }
                
                // Tính toán số tiền còn thiếu
                const remaining = data.finalAmount - data.paidAmount;
                const isPaid = remaining <= 0;
                const paymentStatus = isPaid ? 
                    '<span class="badge bg-success">Đã thanh toán đủ</span>' : 
                    `<span class="badge bg-warning">Còn thiếu ${new Intl.NumberFormat('vi-VN').format(remaining)} VNĐ</span>`;
                
                // Hiển thị trạng thái hóa đơn
                const statusClass = getStatusClass(data.status);
                const statusBadge = `<span class="badge bg-${statusClass}">${data.status}</span>`;
                
                // Tạo HTML cho chi tiết hóa đơn
                const detailHtml = `
                    <div class="row mb-4">
                        <div class="col-md-6">
                            <h5 class="border-bottom pb-2 mb-3">Thông tin hóa đơn</h5>
                            <table class="table table-bordered">
                                <tr>
                                    <th class="table-light" style="width: 40%">Mã hóa đơn:</th>
                                    <td><strong>#${data.invoiceId}</strong></td>
                                </tr>
                                <tr>
                                    <th class="table-light">Ngày tạo:</th>
                                    <td>${formattedDate} ${formattedTime}</td>
                                </tr>
                                <tr>
                                    <th class="table-light">Trạng thái:</th>
                                    <td>${statusBadge}</td>
                                </tr>
                            </table>
                        </div>
                        <div class="col-md-6">
                            <h5 class="border-bottom pb-2 mb-3">Thông tin khách hàng</h5>
                            <table class="table table-bordered">
                                <tr>
                                    <th class="table-light" style="width: 40%">Họ tên:</th>
                                    <td><strong>${data.customer ? data.customer.fullName : 'N/A'}</strong></td>
                                </tr>
                                <tr>
                                    <th class="table-light">Số điện thoại:</th>
                                    <td>${data.customer ? data.customer.phone : 'N/A'}</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                    
                    <h5 class="border-bottom pb-2 mb-3">Chi tiết dịch vụ</h5>
                    ${servicesHtml}
                    
                    <div class="row mt-4">
                        <div class="col-md-6 ms-auto">
                            <h5 class="border-bottom pb-2 mb-3">Tóm tắt thanh toán</h5>
                            <table class="table table-bordered">
                                <tr>
                                    <th class="table-light">Tổng tiền dịch vụ:</th>
                                    <td class="text-end">${new Intl.NumberFormat('vi-VN').format(data.totalAmount)} VNĐ</td>
                                </tr>
                                <tr>
                                    <th class="table-light">Giảm giá (${data.discount}%):</th>
                                    <td class="text-end">- ${new Intl.NumberFormat('vi-VN').format(data.totalAmount * data.discount / 100)} VNĐ</td>
                                </tr>
                                <tr class="table-primary">
                                    <th>Thành tiền:</th>
                                    <td class="text-end"><strong>${new Intl.NumberFormat('vi-VN').format(data.finalAmount)} VNĐ</strong></td>
                                </tr>
                                <tr>
                                    <th class="table-light">Đã thanh toán:</th>
                                    <td class="text-end">${new Intl.NumberFormat('vi-VN').format(data.paidAmount)} VNĐ</td>
                                </tr>
                                <tr>
                                    <th class="table-light">Trạng thái thanh toán:</th>
                                    <td class="text-end">${paymentStatus}</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                    
                    <div class="alert alert-light border mt-3">
                        <i class="fas fa-info-circle text-primary"></i> 
                        Bạn có thể cập nhật trạng thái hoặc thông tin thanh toán bằng các nút bên dưới.
                    </div>
                `;
                
                // Hiển thị modal với thông tin chi tiết
                $('#invoiceDetailContent').html(detailHtml);
                $('#invoiceDetailModalLabel').text(`Chi tiết hóa đơn #${data.invoiceId}`);
                
                // Cập nhật data attributes cho các nút
                $('.update-status-modal').data('id', data.invoiceId).data('status', data.status);
                $('.update-payment-modal').data('id', data.invoiceId)
                    .data('discount', data.discount)
                    .data('paid', data.paidAmount)
                    .data('total', data.totalAmount);
            },
            error: function(xhr, status, error) {
                console.error("Error loading invoice details:", error);
                console.error("Status:", status);
                console.error("Response:", xhr.responseText);
                
                // Show error message in modal
                $('#invoiceDetailContent').html(`
                    <div class="alert alert-danger">
                        <h5><i class="fas fa-exclamation-triangle"></i> Không thể tải chi tiết hóa đơn</h5>
                        <p>${error || 'Lỗi kết nối đến máy chủ'}</p>
                        <button class="btn btn-outline-danger mt-2" onclick="viewInvoiceDetail(${id})">
                            <i class="fas fa-sync"></i> Thử lại
                        </button>
                    </div>
                `);
                $('#invoiceDetailModalLabel').text(`Lỗi tải chi tiết hóa đơn #${id}`);
            }
        });
    }
    
    // Hiển thị modal cập nhật trạng thái
    function showUpdateStatusModal(id, currentStatus) {
        let availableStatuses = ['Chờ thanh toán', 'Đã thanh toán'];
        let statusOptions = '';
        
        availableStatuses.forEach(status => {
            const selected = status === currentStatus ? 'selected' : '';
            const statusClass = getStatusClass(status);
            statusOptions += `<option value="${status}" ${selected}>${status}</option>`;
        });
        
        const modalContent = `
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title">Cập nhật trạng thái hóa đơn #${id}</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label for="currentStatus" class="form-label">Trạng thái hiện tại</label>
                    <input type="text" class="form-control" id="currentStatus" value="${currentStatus}" readonly>
                </div>
                <div class="mb-3">
                    <label for="newStatus" class="form-label">Trạng thái mới</label>
                    <select class="form-select" id="newStatus">
                        ${statusOptions}
                    </select>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                <button type="button" class="btn btn-primary" id="saveStatusBtn">Lưu thay đổi</button>
            </div>
        `;
        
        $('#modalContent').html(modalContent);
        $('#actionModal').modal('show');
        
        $('#saveStatusBtn').click(function() {
            updateInvoiceStatus(id, $('#newStatus').val());
        });
    }
    
    // Cập nhật trạng thái hóa đơn
    function updateInvoiceStatus(id, newStatus) {
        $.ajax({
            url: `/api/Invoice/${id}/UpdateStatus`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ status: newStatus }),
            beforeSend: function() {
                $('#saveStatusBtn').prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');
            },
            success: function(response) {
                if (response.success) {
                    $('#actionModal').modal('hide');
                    // Hiển thị thông báo thành công
                    showToast('Thành công', 'Cập nhật trạng thái hóa đơn thành công', 'success');
                    
                    // Đợi 1 giây để người dùng thấy thông báo, sau đó tải lại trang
                    setTimeout(function() {
                        window.location.reload();
                    }, 1000);
                } else {
                    showToast('Lỗi', response.message || 'Cập nhật trạng thái không thành công', 'danger');
                }
            },
            error: function(xhr, status, error) {
                showToast('Lỗi', 'Không thể cập nhật trạng thái. Lỗi: ' + error, 'danger');
            },
            complete: function() {
                $('#saveStatusBtn').prop('disabled', false).text('Lưu thay đổi');
            }
        });
    }
    
    // Hiển thị modal cập nhật thanh toán
    function showUpdatePaymentModal(id, discount, paidAmount, totalAmount) {
        const finalAmount = totalAmount - (totalAmount * discount / 100);
        const remainingAmount = finalAmount - paidAmount;
        
        const modalContent = `
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title">Cập nhật thanh toán hóa đơn #${id}</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <div class="row mb-3">
                    <div class="col-md-6">
                        <label for="totalAmount" class="form-label">Tổng tiền dịch vụ</label>
                        <input type="text" class="form-control" id="totalAmount" value="${new Intl.NumberFormat('vi-VN').format(totalAmount)} VNĐ" readonly>
                    </div>
                    <div class="col-md-6">
                        <label for="discountPercent" class="form-label">Giảm giá (%)</label>
                        <input type="number" class="form-control" id="discountPercent" value="${discount}" min="0" max="100">
                    </div>
                </div>
                <div class="row mb-3">
                    <div class="col-md-6">
                        <label for="finalAmount" class="form-label">Thành tiền</label>
                        <input type="text" class="form-control" id="finalAmount" value="${new Intl.NumberFormat('vi-VN').format(finalAmount)} VNĐ" readonly>
                    </div>
                    <div class="col-md-6">
                        <label for="paidAmount" class="form-label">Số tiền đã thanh toán</label>
                        <input type="number" class="form-control" id="paidAmount" value="${paidAmount}" min="0">
                    </div>
                </div>
                <div class="alert alert-info">
                    <strong>Còn lại:</strong> ${new Intl.NumberFormat('vi-VN').format(remainingAmount)} VNĐ
                </div>
                <div class="mb-3">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="payFullAmount">
                        <label class="form-check-label" for="payFullAmount">
                            Thanh toán đủ
                        </label>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                <button type="button" class="btn btn-primary" id="savePaymentBtn">Lưu thay đổi</button>
            </div>
        `;
        
        $('#modalContent').html(modalContent);
        $('#actionModal').modal('show');
        
        // Tính toán lại số tiền khi thay đổi giảm giá
        $('#discountPercent').change(function() {
            const newDiscount = parseFloat($(this).val()) || 0;
            const newFinalAmount = totalAmount - (totalAmount * newDiscount / 100);
            $('#finalAmount').val(new Intl.NumberFormat('vi-VN').format(newFinalAmount) + ' VNĐ');
            const newRemainingAmount = newFinalAmount - paidAmount;
            $('.alert-info').html(`<strong>Còn lại:</strong> ${new Intl.NumberFormat('vi-VN').format(newRemainingAmount)} VNĐ`);
        });
        
        // Cập nhật khi tick vào thanh toán đủ
        $('#payFullAmount').change(function() {
            if ($(this).is(':checked')) {
                const currentDiscount = parseFloat($('#discountPercent').val()) || 0;
                const currentFinalAmount = totalAmount - (totalAmount * currentDiscount / 100);
                $('#paidAmount').val(currentFinalAmount);
            }
        });
        
        // Lưu thay đổi thanh toán
        $('#savePaymentBtn').click(function() {
            const newDiscount = parseFloat($('#discountPercent').val()) || 0;
            const newPaidAmount = parseFloat($('#paidAmount').val()) || 0;
            
            updateInvoicePayment(id, newDiscount, newPaidAmount);
        });
    }
    
    // Cập nhật thông tin thanh toán
    function updateInvoicePayment(id, discount, paidAmount) {
        $.ajax({
            url: `/api/Invoice/${id}/UpdatePayment`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                discount: discount,
                paidAmount: paidAmount
            }),
            beforeSend: function() {
                $('#savePaymentBtn').prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');
            },
            success: function(response) {
                if (response.success) {
                    $('#actionModal').modal('hide');
                    // Hiển thị thông báo thành công
                    showToast('Thành công', 'Cập nhật thanh toán thành công', 'success');
                    
                    // Đợi 1 giây để người dùng thấy thông báo, sau đó tải lại trang
                    setTimeout(function() {
                        window.location.reload();
                    }, 1000);
                } else {
                    showToast('Lỗi', response.message || 'Cập nhật thanh toán không thành công', 'danger');
                }
            },
            error: function(xhr, status, error) {
                showToast('Lỗi', 'Không thể cập nhật thanh toán. Lỗi: ' + error, 'danger');
            },
            complete: function() {
                $('#savePaymentBtn').prop('disabled', false).text('Lưu thay đổi');
            }
        });
    }
    
    // Hiển thị toast thông báo
    function showToast(title, message, type) {
        const toastId = 'toast-' + Date.now();
        const toast = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <strong>${title}:</strong> ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
        
        $('#toastContainer').append(toast);
        
        const toastEl = document.getElementById(toastId);
        const bsToast = new bootstrap.Toast(toastEl, { delay: 3000 });
        bsToast.show();
        
        $(toastEl).on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }
    
    // Utility function to reload page after action
    function reloadAfterAction(delay = 1000) {
        if (window.shouldReloadAfterAction !== false) {
            setTimeout(function() {
                window.location.reload();
            }, delay);
            console.log(`Page will reload in ${delay}ms`);
            return true;
        }
        console.log('Auto-reload is disabled');
        return false;
    }
});
