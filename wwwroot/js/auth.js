document.addEventListener('DOMContentLoaded', function() {
    // Kiểm tra trạng thái đăng nhập khi tải trang
    checkAuthStatus();

    // Biến để theo dõi trạng thái gửi request
    let isSubmitting = false;

    // Xử lý form đăng nhập
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.removeEventListener('submit', handleLoginSubmit); // Xóa sự kiện cũ
        loginForm.addEventListener('submit', handleLoginSubmit);
    }

    function handleLoginSubmit(e) {
        e.preventDefault();
        e.stopPropagation(); // Ngăn sự kiện lan truyền

        // Nếu đang gửi request, bỏ qua
        if (isSubmitting) return;
        isSubmitting = true;

        const username = document.getElementById('loginUsername').value;
        const password = document.getElementById('loginPassword').value;
        const rememberMe = document.getElementById('loginRememberMe')?.checked || false;

        console.log("Đang gửi yêu cầu đăng nhập cho:", username);

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
                    throw new Error(data.message || 'Đăng nhập thất bại, sai tên đăng nhập hoặc mật khẩu');
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
                username: data.username,
                fullName: data.fullName
            }));

            // Cập nhật UI
            document.getElementById('userDisplayName').textContent = data.fullName || data.username;
            updateAuthUI();

            // Hiển thị thông báo toast duy nhất
            showToast('Đăng nhập thành công!', 'success');

            // Chuyển hướng về trang chủ
            setTimeout(() => {
                window.location.href = '/';
            }, 1000);
        })
        .catch(error => {
            console.error('Lỗi đăng nhập:', error);
            // Chỉ hiển thị một toast
            showToast(error.message, 'error');
        })
        .finally(() => {
            isSubmitting = false; // Reset trạng thái
        });
    }

    // Xử lý form đăng ký
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.removeEventListener('submit', handleRegisterSubmit); // Xóa sự kiện cũ
        registerForm.addEventListener('submit', handleRegisterSubmit);
    }

    function handleRegisterSubmit(e) {
        e.preventDefault();
        e.stopPropagation(); // Ngăn sự kiện lan truyền

        // Nếu đang gửi request, bỏ qua
        if (isSubmitting) return;
        isSubmitting = true;

        const username = document.getElementById('registerUsername').value;
        const fullName = document.getElementById('registerFullName').value;
        const phoneNumber = document.getElementById('registerPhoneNumber').value;
        const email = document.getElementById('registerEmail').value;
        const password = document.getElementById('registerPassword').value;
        const confirmPassword = document.getElementById('registerConfirmPassword').value;

        if (password !== confirmPassword) {
            showToast('Mật khẩu và xác nhận mật khẩu không khớp!', 'error');
            isSubmitting = false;
            return;
        }

        console.log("Đang gửi yêu cầu đăng ký:", username);

        fetch('/api/AccountApi/Register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                fullName: fullName,
                phoneNumber: phoneNumber,
                email: email,
                password: password
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
                username: data.username,
                fullName: data.fullName
            }));

            // Cập nhật UI
            document.getElementById('userDisplayName').textContent = data.fullName || data.username;
            updateAuthUI();

            // Hiển thị thông báo toast duy nhất
            showToast('Đăng ký thành công!', 'success');

            // Chuyển hướng về trang chủ
            setTimeout(() => {
                window.location.href = '/';
            }, 1000);
        })
        .catch(error => {
            console.error('Lỗi đăng ký:', error);
            // Chỉ hiển thị một toast
            showToast('Đăng ký thất bại: ' + error.message, 'error');
        })
        .finally(() => {
            isSubmitting = false; // Reset trạng thái
        });
    }

    // Xử lý đăng xuất
    const logoutLink = document.getElementById('logoutLink');
    if (logoutLink) {
        logoutLink.removeEventListener('click', handleLogoutClick); // Xóa sự kiện cũ
        logoutLink.addEventListener('click', handleLogoutClick);
    }

    function handleLogoutClick(e) {
        e.preventDefault();
        e.stopPropagation(); // Ngăn sự kiện lan truyền

        // Nếu đang gửi request, bỏ qua
        if (isSubmitting) return;
        isSubmitting = true;

        const token = localStorage.getItem('token');

        fetch('/api/AccountApi/Logout', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Đăng xuất thất bại');
            }
            // Xóa thông tin người dùng
            localStorage.removeItem('token');
            localStorage.removeItem('user');

            // Cập nhật UI
            updateAuthUI();

            // Hiển thị thông báo toast duy nhất
            showToast('Đăng xuất thành công!', 'success');

            // Chuyển hướng về trang chủ
            window.location.href = '/';
        })
        .catch(error => {
            console.error('Lỗi đăng xuất:', error);
            // Chỉ hiển thị một toast
            showToast('Đăng xuất thất bại: ' + error.message, 'error');
        })
        .finally(() => {
            isSubmitting = false; // Reset trạng thái
        });
    }
});

function checkAuthStatus() {
    const token = localStorage.getItem('token');
    const userJson = localStorage.getItem('user');

    if (token && userJson) {
        try {
            const user = JSON.parse(userJson);
            const payload = JSON.parse(atob(token.split('.')[1]));
            const expTime = payload.exp * 1000;

            if (Date.now() > expTime) {
                console.log("Token đã hết hạn, đăng xuất tự động");
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                updateAuthUI();
                return;
            }

            // Hiển thị tên đầy đủ hoặc username
            const displayName = user.fullName || user.username || payload.sub || 'Người dùng';
            document.getElementById('userDisplayName').textContent = displayName;
            updateAuthUI();
        } catch (e) {
            console.error('Token không hợp lệ, đăng xuất tự động:', e);
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            updateAuthUI();
        }
    } else {
        updateAuthUI();
    }
}

function updateAuthUI() {
    const userJson = localStorage.getItem('user');

    if (userJson) {
        const user = JSON.parse(userJson);
        document.getElementById('userDisplayName').textContent = user.fullName || user.username || 'Người dùng';

        document.getElementById('loginMenuItem')?.classList.add('d-none');
        document.getElementById('registerMenuItem')?.classList.add('d-none');
        document.getElementById('profileMenuItem')?.classList.remove('d-none');
        document.getElementById('bookingMenuItem')?.classList.remove('d-none');
        document.querySelectorAll('#logoutMenuItem').forEach(item => item.classList.remove('d-none'));

        document.getElementById('profileLink').href = '/Account/Profile';
        document.getElementById('myBookingsLink').href = '/Account/Bookings';
    } else {
        document.getElementById('userDisplayName').textContent = 'Tài khoản';

        document.getElementById('loginMenuItem')?.classList.remove('d-none');
        document.getElementById('registerMenuItem')?.classList.remove('d-none');
        document.getElementById('profileMenuItem')?.classList.add('d-none');
        document.getElementById('bookingMenuItem')?.classList.add('d-none');
        document.querySelectorAll('#logoutMenuItem').forEach(item => item.classList.add('d-none'));
    }
}

function showToast(message, type = 'info') {
    // Kiểm tra xem đã có toast nào đang hiển thị chưa
    const existingToasts = document.querySelectorAll('.toast');
    if (existingToasts.length > 0) {
        existingToasts.forEach(toast => toast.remove()); // Xóa toast cũ
    }

    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

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

    const toast = new bootstrap.Toast(toastEl);
    toast.show();

    toastEl.addEventListener('hidden.bs.toast', function() {
        toastEl.remove();
    });
}