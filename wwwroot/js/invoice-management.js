// Invoice Management JavaScript

let invoicesTable;
let allInvoices = [];

$(document).ready(function() {
    invoicesTable = $('#invoicesTable').DataTable({
        responsive: true,
        order: [[3, 'desc']],
        columns: [
            { data: 'invoiceId' },
            { data: 'customerName' },
            { data: 'customerPhone' },
            { data: 'createdDate', render: formatDate },
            { data: 'payment', render: formatPayment },
            { data: 'status', render: formatStatus },
            { data: 'actions', orderable: false }
        ],
        language: {
            url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Vietnamese.json'
        }
    });

    loadInvoicesData();

    $('#refreshData').on('click', function() {
        loadInvoicesData();
    });

    window.shouldReloadAfterAction = true;

    setupEventListeners();
});

function setupEventListeners() {
    $(document).on('click', '.edit-payment', function() {
        const invoiceId = $(this).data('id');
        showEditPaymentModal(invoiceId);
    });

    $('#savePaymentChanges').on('click', function() {
        savePaymentChanges();
    });

    $('#editTotalAmount, #editDiscount').on('input', function() {
        calculateFinalAmount();
    });

    $(document).on('click', '.quick-action-btn', function() {
        const action = $(this).data('action');
        const invoiceId = $(this).data('id');
        if (action === 'mark-paid') {
            markInvoiceAsPaid(invoiceId);
        }
    });
}

function loadInvoicesData() {
    $.ajax({
        url: '/api/Invoice',
        type: 'GET',
        dataType: 'json',
        beforeSend: function() {
            if ($.fn.DataTable.isDataTable('#invoicesTable')) {
                invoicesTable.clear().draw();
                $('#invoicesTable tbody').html('<tr><td colspan="7" class="text-center"><div class="spinner-border text-primary" role="status"></div> Đang tải dữ liệu...</td></tr>');
            }
        },
        success: function(data) {
            allInvoices = data;
            renderInvoicesTable(data);
            updateStatisticsFromData(data); // Update statistics from invoice data
            
            // Also load statistics from dedicated endpoint for more accurate data
            loadStatistics();
        },
        error: function(xhr, status, error) {
            showToast('Lỗi', 'Không thể tải dữ liệu hóa đơn: ' + error, 'danger');
        }
    });
}

// Function to load invoice statistics
function loadStatistics() {
    fetch('/api/Invoice/Statistics')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(stats => {
            console.log('Received statistics:', stats);
            updateStatisticsUI(stats);
        })
        .catch(error => {
            console.error('Error loading statistics:', error);
            // Don't update UI on error as we already have data from updateStatisticsFromData
        });
}

// Update statistics UI with values from API
function updateStatisticsUI(stats) {
    $('#stats-total').text(`${stats.total} hóa đơn`);
    $('#stats-pending').text(`${stats.pending} hóa đơn`);
    $('#stats-paid').text(`${stats.paid} hóa đơn`);
}

// Update statistics from invoice data (fallback method)
function updateStatisticsFromData(invoices) {
    if (!invoices || !Array.isArray(invoices)) return;
    
    const total = invoices.length;
    const pending = invoices.filter(i => i.status === 'Chờ thanh toán').length;
    const paid = invoices.filter(i => i.status === 'Đã thanh toán').length;
    
    $('#stats-total').text(`${total} hóa đơn`);
    $('#stats-pending').text(`${pending} hóa đơn`);
    $('#stats-paid').text(`${paid} hóa đơn`);
}

function renderInvoicesTable(data) {
    invoicesTable.clear();

    data.forEach(invoice => {
        invoicesTable.row.add({
            'invoiceId': invoice.invoiceId,
            'customerName': invoice.customerName,
            'customerPhone': invoice.customerPhone,
            'createdDate': invoice.createdDate,
            'payment': {
                totalAmount: invoice.totalAmount,
                paidAmount: invoice.paidAmount,
                finalAmount: invoice.finalAmount,
                discount: invoice.discount
            },
            'status': invoice.status,
            'actions': `
                <div class="btn-group btn-group-sm" role="group">
                    <button type="button" class="btn btn-primary edit-payment" data-id="${invoice.invoiceId}">
                        <i class="fas fa-money-bill"></i>
                    </button>
                    <button type="button" class="btn btn-success quick-action-btn" data-action="mark-paid" data-id="${invoice.invoiceId}" 
                        ${invoice.status === 'Đã thanh toán' ? 'disabled' : ''}>
                        <i class="fas fa-check"></i>
                    </button>
                </div>
            `
        });
    });

    invoicesTable.draw();
}

function formatDate(data) {
    if (!data) {
        return 'N/A';
    }
    const date = new Date(data);
    return date.toLocaleString('vi-VN');
}

function formatPayment(data) {
    const { totalAmount, paidAmount, finalAmount, discount } = data;
    const percentage = finalAmount > 0 ? (paidAmount / finalAmount * 100) : 0;

    return `
        <div>
            <div class="small">Tổng: ${formatCurrency(totalAmount)}</div>
            <div class="small">Giảm giá: ${discount}%</div>
            <div class="small">Thanh toán: ${formatCurrency(finalAmount)}</div>
            <div class="small">Còn lại: ${formatCurrency(finalAmount - paidAmount)}</div>
            <div class="progress mt-1">
                <div class="progress-bar ${percentage === 100 ? 'bg-success' : 'bg-warning'}" 
                    role="progressbar" style="width: ${percentage}%" 
                    aria-valuenow="${percentage}" aria-valuemin="0" aria-valuemax="100">
                </div>
            </div>
        </div>
    `;
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

function formatStatus(status) {
    let badgeClass = 'bg-secondary';
    switch (status) {
        case 'Chờ thanh toán':
            badgeClass = 'bg-warning';
            break;
        case 'Đã thanh toán':
            badgeClass = 'bg-success';
            break;
        case 'Đã hủy':
            badgeClass = 'bg-danger';
            break;
        case 'Hoàn tiền':
            badgeClass = 'bg-info';
            break;
    }
    return `<span class="badge ${badgeClass}">${status}</span>`;
}

function updateStatistics(data) {
    const totalInvoices = data.length;
    const totalPending = data.filter(i => i.status === 'Chờ thanh toán').length;
    const totalPaid = data.filter(i => i.status === 'Đã thanh toán').length;

    let totalRevenue = 0;
    let pendingRevenue = 0;

    data.forEach(invoice => {
        const finalAmount = invoice.finalAmount;
        const paidAmount = invoice.paidAmount;
        totalRevenue += paidAmount;
        if (invoice.status === 'Chờ thanh toán') {
            pendingRevenue += (finalAmount - paidAmount);
        }
    });

    $('#totalInvoices').text(totalInvoices);
    $('#totalPending').text(totalPending);
    $('#totalPaid').text(totalPaid);
    $('#totalRevenue').text(formatCurrency(totalRevenue));
    $('#pendingRevenue').text(formatCurrency(pendingRevenue));
}

function showInvoiceDetails(invoiceId) {
    const invoice = allInvoices.find(i => i.invoiceId === invoiceId);
    if (!invoice) return;

    $('#detailInvoiceId').text(invoice.invoiceId);
    $('#detailCustomerName').text(invoice.customerName);
    $('#detailCustomerPhone').text(invoice.customerPhone);
    $('#detailCreatedDate').text(formatDate(invoice.createdDate));
    $('#detailTotalAmount').text(formatCurrency(invoice.totalAmount));
    $('#detailDiscount').text(`${invoice.discount}%`);
    $('#detailFinalAmount').text(formatCurrency(invoice.finalAmount));
    $('#detailPaidAmount').text(formatCurrency(invoice.paidAmount));
    $('#detailStatus').html(formatStatus(invoice.status));

    $('#invoiceDetailsModal').modal('show');
}

function showUpdateStatusModal(invoiceId, currentStatus) {
    $('#statusInvoiceId').val(invoiceId);
    $('#invoiceStatus').val(currentStatus);
    $('#updateStatusModal').modal('show');
}

function showEditPaymentModal(invoiceId) {
    const invoice = allInvoices.find(i => i.invoiceId === invoiceId);
    if (!invoice) return;

    $('#editInvoiceId').val(invoice.invoiceId);
    $('#editTotalAmount').val(invoice.totalAmount);
    $('#editDiscount').val(invoice.discount);
    $('#editFinalAmount').val(invoice.finalAmount);
    $('#editPaidAmount').val(invoice.paidAmount);

    $('#editPaymentModal').modal('show');
}

function calculateFinalAmount() {
    const totalAmount = parseFloat($('#editTotalAmount').val()) || 0;
    const discount = parseFloat($('#editDiscount').val()) || 0;
    const finalAmount = totalAmount - (totalAmount * discount / 100);
    $('#editFinalAmount').val(finalAmount.toFixed(2));
}

function savePaymentChanges() {
    const invoiceId = $('#editInvoiceId').val();
    const totalAmount = parseFloat($('#editTotalAmount').val());
    const discount = parseFloat($('#editDiscount').val());
    const paidAmount = parseFloat($('#editPaidAmount').val());

    if (!totalAmount || totalAmount <= 0) {
        showToast('Lỗi', 'Tổng tiền phải lớn hơn 0', 'danger');
        return;
    }
    if (discount < 0 || discount > 100) {
        showToast('Lỗi', 'Giảm giá phải từ 0% đến 100%', 'danger');
        return;
    }
    if (paidAmount < 0) {
        showToast('Lỗi', 'Số tiền đã thanh toán không thể âm', 'danger');
        return;
    }

    const saveButton = $('#savePaymentChanges');
    saveButton.prop('disabled', true);
    saveButton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

    $.ajax({
        url: `/api/Invoice/${invoiceId}/UpdatePayment`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            totalAmount: totalAmount,
            discount: discount,
            paidAmount: paidAmount
        }),
        success: function(response) {
            if (response.success) {
                showToast('Thành công', response.message, 'success');
                $('#editPaymentModal').modal('hide');
                const updatedInvoice = allInvoices.find(i => i.invoiceId == invoiceId);
                if (updatedInvoice) {
                    updatedInvoice.totalAmount = totalAmount;
                    updatedInvoice.discount = discount;
                    updatedInvoice.finalAmount = response.data.finalAmount;
                    updatedInvoice.paidAmount = paidAmount;
                    updatedInvoice.status = response.data.status;
                    renderInvoicesTable(allInvoices);
                    updateStatistics(allInvoices);
                }
                if (window.shouldReloadAfterAction) {
                    loadInvoicesData();
                }
            } else {
                showToast('Lỗi', response.message, 'danger');
            }
        },
        error: function(xhr, status, error) {
            let errorMessage = 'Đã xảy ra lỗi khi cập nhật thông tin thanh toán';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            showToast('Lỗi', errorMessage, 'danger');
        },
        complete: function() {
            saveButton.prop('disabled', false);
            saveButton.html('Lưu thay đổi');
        }
    });
}

function markInvoiceAsPaid(invoiceId) {
    const invoice = allInvoices.find(i => i.invoiceId == invoiceId);
    if (!invoice) return;

    const button = $(`.quick-action-btn[data-id="${invoiceId}"]`);
    button.prop('disabled', true);
    button.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');

    $.ajax({
        url: `/api/Invoice/${invoiceId}/UpdateStatus`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            status: 'Đã thanh toán'
        }),
        success: function(response) {
            if (response.success) {
                showToast('Thành công', 'Hóa đơn đã được đánh dấu là đã thanh toán', 'success');
                
                if (invoice) {
                    invoice.status = 'Đã thanh toán';
                    invoice.paidAmount = invoice.finalAmount;
                    renderInvoicesTable(allInvoices);
                    updateStatistics(allInvoices);
                }
                
                if (window.shouldReloadAfterAction) {
                    loadInvoicesData();
                }
            } else {
                button.prop('disabled', false);
                button.html('<i class="fas fa-check"></i>');
                showToast('Lỗi', response.message, 'danger');
            }
        },
        error: function(xhr, status, error) {
            button.prop('disabled', false);
            button.html('<i class="fas fa-check"></i>');
            showToast('Lỗi', 'Không thể cập nhật trạng thái hóa đơn', 'danger');
        }
    });
}

function showToast(title, message, type) {
    const toastId = 'toast-' + Date.now();
    const toast = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="5000">
            <div class="toast-header bg-${type} text-white">
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    $('#toastContainer').append(toast);
    const toastElement = document.getElementById(toastId);
    const bsToast = new bootstrap.Toast(toastElement);
    bsToast.show();
    $(toastElement).on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// Show toast notification
function showToast(title, message, type) {
    const toastId = 'toast-' + Date.now();
    const toast = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="5000">
            <div class="toast-header bg-${type} text-white">
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    
    $('#toastContainer').append(toast);
    const toastElement = document.getElementById(toastId);
    const bsToast = new bootstrap.Toast(toastElement);
    bsToast.show();
    
    // Remove toast after it's hidden
    $(toastElement).on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// Function to update invoice status
function updateInvoiceStatus(invoiceId, status) {
    $.ajax({
        url: `/api/Invoice/${invoiceId}/UpdateStatus`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ status: status }),
        success: function(response) {
            if (response.success) {
                showToast('Thành công', 'Đã đánh dấu hóa đơn là ' + status, 'success');
                if (window.shouldReloadAfterAction) {
                    setTimeout(function() {
                        loadInvoicesData();
                    }, 1000);
                }
            } else {
                showToast('Lỗi', response.message || 'Cập nhật không thành công', 'danger');
            }
        },
        error: function(xhr, status, error) {
            showToast('Lỗi', 'Không thể cập nhật trạng thái hóa đơn', 'danger');
        }
    });
}
