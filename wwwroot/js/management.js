// Common JavaScript functions for management pages

function handleApiError(error, containerId, retryFn) {
    console.error('API Error:', error);
    const container = document.getElementById(containerId);
    if (container) {
        container.innerHTML = `
            <div class="alert alert-danger">
                <h5>Lỗi khi tải dữ liệu</h5>
                <p>${error.message || 'Không thể kết nối với máy chủ'}</p>
                <button class="btn btn-sm btn-primary mt-2" onclick="${retryFn}()">Thử lại</button>
            </div>
        `;
    }
}

function setLoading(containerId) {
    const container = document.getElementById(containerId);
    if (container) {
        container.innerHTML = `
            <div class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Đang tải...</span>
                </div>
                <p class="mt-2">Đang tải dữ liệu...</p>
            </div>
        `;
    }
}

// Account management functions
function loadAccounts() {
    console.log('Loading accounts...');
    
    // Display loading state
    setLoading('accountsContainer');
    
    fetch('/api/AccountManagement')
        .then(response => {
            console.log('API response status:', response.status);
            if (!response.ok) {
                throw new Error(`Network response was not ok: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Received account data:', data);
            displayAccounts(data);
        })
        .catch(error => {
            console.error('Error fetching accounts:', error);
            handleApiError(error, 'accountsContainer', 'loadAccounts');
        });
}

function displayAccounts(accounts) {
    const container = document.getElementById('accountsContainer');
    
    if (!accounts || accounts.length === 0) {
        container.innerHTML = '<div class="alert alert-info">Không có tài khoản nào.</div>';
        return;
    }
    
    let html = `
        <div class="table-responsive">
            <table class="table table-striped table-hover">
                <thead class="table-light">
                    <tr>
                        <th>Tên đăng nhập</th>
                        <th>Email</th>
                        <th>Họ và tên</th>
                        <th>Ngày tạo</th>
                        <th>Quyền</th>
                        <th>Thao tác</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    accounts.forEach(account => {
        const createdDate = account.createdDate 
            ? new Date(account.createdDate).toLocaleDateString('vi-VN') 
            : 'N/A';
            
        html += `
            <tr>
                <td>${account.userName || 'N/A'}</td>
                <td>${account.email || 'N/A'}</td>
                <td>${account.fullName || 'N/A'}</td>
                <td>${createdDate}</td>
                <td>
                    ${account.roles && account.roles.length > 0 
                        ? account.roles.map(role => `<span class="badge bg-primary me-1">${role}</span>`).join('') 
                        : '<span class="badge bg-secondary">Không có quyền</span>'}
                </td>
                <td>
                    <button class="btn btn-sm btn-outline-primary" onclick="viewAccountDetails('${account.id}')">
                        <i class="bi bi-eye"></i>
                    </button>
                </td>
            </tr>
        `;
    });
    
    html += `
                </tbody>
            </table>
        </div>
    `;
    
    container.innerHTML = html;
}

function viewAccountDetails(accountId) {
    console.log(`Viewing details for account ID: ${accountId}`);
    // This function can be implemented later to show a modal with account details
    alert(`Thông tin chi tiết của tài khoản sẽ được hiển thị ở đây (ID: ${accountId})`);
}

// Chair management functions can be added here

// Room management functions can be added here
