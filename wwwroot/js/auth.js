// Auth handler for SPA
document.addEventListener('DOMContentLoaded', function() {
    // Kiểm tra trạng thái đăng nhập
    checkAuthStatus();
    
    // Xử lý form đăng nhập từ modal
    document.getElementById('loginForm')?.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const username = document.getElementById('loginUsername').value;
        const password = document.getElementById('loginPassword').value;
        const rememberMe = document.getElementById('loginRememberMe')?.checked || false;
        
        // Thêm thông báo để debug
        console.log("Đang gửi yêu cầu đăng nhập cho:", username);
        
        // Gọi API đăng nhập
        fetch('/api/AccountApi/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                password: password,
                rememberMe: rememberMe
            })
        })
        .then(response => {
            console.log("Nhận phản hồi:", response.status);
            if (!response.ok) {
                return response.json().then(data => {
                    throw new Error(data.message || 'Đăng nhập thất bại');
                });
            }
            return response.json();
        })
        .then(data => {
            console.log("Đăng nhập thành công:", data);
            
            // Lưu token và thông tin người dùng
            localStorage.setItem('token', data.token);
            localStorage.setItem('user', JSON.stringify({
                id: data.userId,
                customerId: data.customerId,
                username: username
            }));
            
            // Đóng modal
            const loginModal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
            if (loginModal) loginModal.hide();
            
            // Cập nhật UI
            updateAuthUI();
            
            // Hiển thị thông báo
            alert('Đăng nhập thành công!');
            
            // Tải lại trang sau khi đăng nhập thành công
            window.location.reload();
        })
        .catch(error => {
            console.error('Lỗi đăng nhập:', error);
            alert('Đăng nhập thất bại. Vui lòng kiểm tra thông tin đăng nhập.');
        });
    });
    
    // Xử lý form đăng ký từ modal - tương tự như trên
    document.getElementById('registerForm')?.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const username = document.getElementById('registerUsername').value;
        const email = document.getElementById('registerEmail').value;
        const password = document.getElementById('registerPassword').value;
        const confirmPassword = document.getElementById('registerConfirmPassword').value;
        
        // Kiểm tra mật khẩu khớp nhau
        if (password !== confirmPassword) {
            alert('Mật khẩu không khớp!');
            return;
        }
        
        console.log("Đang gửi yêu cầu đăng ký:", username, email);
        
        // Gọi API đăng ký
        fetch('/api/AccountApi/Register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                email: email,
                password: password,
                confirmPassword: confirmPassword,
                fullName: username, // Thêm fullName
                phone: "" // Thêm phone
            })
        })
        .then(response => {
            console.log("Nhận phản hồi đăng ký:", response.status);
            if (!response.ok) {
                return response.json().then(data => {
                    throw new Error(data.message || 'Đăng ký thất bại');
                });
            }
            return response.json();
        })
        .then(data => {
            console.log("Đăng ký thành công:", data);
            
            // Lưu token và thông tin người dùng
            localStorage.setItem('token', data.token);
            localStorage.setItem('user', JSON.stringify({
                id: data.userId,
                customerId: data.customerId,
                username: username
            }));
            
            // Đóng modal
            const registerModal = bootstrap.Modal.getInstance(document.getElementById('registerModal'));
            if (registerModal) registerModal.hide();
            
            // Cập nhật UI
            updateAuthUI();
            
            // Hiển thị thông báo
            alert('Đăng ký thành công!');
            
            // Tải lại trang sau khi đăng ký thành công
            window.location.reload();
        })
        .catch(error => {
            console.error('Lỗi đăng ký:', error);
            alert('Đăng ký thất bại. Vui lòng thử lại: ' + error.message);
        });
    });
    
    // Xử lý đăng xuất
    document.getElementById('logoutLink')?.addEventListener('click', function(e) {
        e.preventDefault();
        
        const token = localStorage.getItem('token');
        
        // Gọi API đăng xuất
        fetch('/api/AccountApi/Logout', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        .then(response => {
            // Xóa thông tin người dùng
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            
            // Cập nhật UI
            updateAuthUI();
            
            // Hiển thị thông báo
            showToast('Đăng xuất thành công!', 'success');
        })
        .catch(error => {
            console.error('Lỗi đăng xuất:', error);
        });
    });
    
    const token = localStorage.getItem('token');
    // Chỉ cập nhật thông tin người dùng nếu chưa đăng nhập trong phiên hiện tại
    // (phải kiểm tra xem người dùng đã được hiển thị bởi server-side chưa)
    if (token && document.getElementById('userDisplayName').textContent === 'Tài khoản') {
        // Cập nhật thông tin người dùng từ token
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            if (payload.name) {
                document.getElementById('userDisplayName').textContent = payload.name;
            }
        } catch (e) {
            console.error('Lỗi khi xử lý token:', e);
        }
    }
});

// Kiểm tra trạng thái đăng nhập
function checkAuthStatus() {
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');
    
    if (token && user) {
        // Token tồn tại, cập nhật UI
        updateAuthUI();
    }
}

// Cập nhật UI dựa trên trạng thái đăng nhập
function updateAuthUI() {
    const user = JSON.parse(localStorage.getItem('user'));
    
    if (user) {
        // Đã đăng nhập
        document.getElementById('userDisplayName').textContent = user.username;
        
        // Hiển thị các menu item cho người dùng đã đăng nhập
        document.getElementById('loginMenuItem')?.classList.add('d-none');
        document.getElementById('registerMenuItem')?.classList.add('d-none');
        document.getElementById('profileMenuItem')?.classList.remove('d-none');
        document.getElementById('bookingMenuItem')?.classList.remove('d-none');
        document.getElementById('logoutMenuItem')?.classList.remove('d-none');
        
        // Cập nhật href cho profile link
        document.getElementById('profileLink').href = '/Account/Profile';
        document.getElementById('myBookingsLink').href = '/Account/Bookings';
    } else {
        // Chưa đăng nhập
        document.getElementById('userDisplayName').textContent = 'Tài khoản';
        
        // Hiển thị các menu item cho người dùng chưa đăng nhập
        document.getElementById('loginMenuItem')?.classList.remove('d-none');
        document.getElementById('registerMenuItem')?.classList.remove('d-none');
        document.getElementById('profileMenuItem')?.classList.add('d-none');
        document.getElementById('bookingMenuItem')?.classList.add('d-none');
        document.getElementById('logoutMenuItem')?.classList.add('d-none');
    }
}

// Hiển thị thông báo toast
function showToast(message, type = 'info') {
    // Check if we already have a toast container
    let toastContainer = document.querySelector('.toast-container');
    
    if (!toastContainer) {
        // Create toast container if it doesn't exist
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }
    
    // Create the toast element
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type} border-0`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    
    toastContainer.appendChild(toastEl);
    
    // Initialize the toast
    const toast = new bootstrap.Toast(toastEl);
    toast.show();
    
    // Remove toast after it's hidden
    toastEl.addEventListener('hidden.bs.toast', function() {
        toastEl.remove();
    });
}

// Không cần sửa gì ở đây vì chúng ta sẽ sử dụng Identity trực tiếp từ server
// Nếu chỉ là SPA không có server-side rendering, cần thêm code JS để xử lý JWT và cập nhật UI

// Có thể thêm code tùy chỉnh ở đây nếu cần
