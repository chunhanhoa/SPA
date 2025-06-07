// Admin JS

window.addEventListener('DOMContentLoaded', event => {
    // Toggle sidebar
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }

    // Apply sidebar toggle state from localStorage
    if (localStorage.getItem('sb|sidebar-toggle') === 'true') {
        document.body.classList.add('sb-sidenav-toggled');
    }
    
    // Auto-close alerts after 5 seconds
    let alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            let bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});

// Utility functions for admin

// Format number as currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Format date and time
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', { 
        year: 'numeric', 
        month: '2-digit', 
        day: '2-digit',
        hour: '2-digit', 
        minute: '2-digit'
    }).format(date);
}

// Format date only
function formatDate(dateString) {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', { 
        year: 'numeric', 
        month: '2-digit', 
        day: '2-digit'
    }).format(date);
}

// Get status class based on status text
function getStatusClass(status) {
    switch(status) {
        case 'Đã xác nhận': return 'success';
        case 'Đang thực hiện': return 'info';
        case 'Hoàn thành': return 'primary';
        case 'Đã hủy': return 'danger';
        default: return 'secondary';
    }
}

// Show a toast/notification
function showToast(message, type = 'success') {
    if (typeof bootstrap !== 'undefined') {
        const toastEl = document.getElementById('toast');
        if (!toastEl) return;
        
        const toast = new bootstrap.Toast(toastEl);
        
        document.getElementById('toastMessage').textContent = message;
        toastEl.className = toastEl.className.replace(/bg-\w+/, `bg-${type}`);
        
        toast.show();
    } else {
        alert(message);
    }
}
