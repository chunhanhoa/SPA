// Functions to handle user profile interactions

document.addEventListener('DOMContentLoaded', () => {
    // Load profile data when dropdown is opened
    const profileDropdown = document.getElementById('profileDropdown');
    if (profileDropdown) {
        profileDropdown.addEventListener('click', loadUserProfile);
    }
});

function loadUserProfile() {
    const loadingElement = document.querySelector('.profile-loading');
    const dataElement = document.querySelector('.profile-data');
    
    if (loadingElement && dataElement) {
        // Show loading spinner and hide data
        loadingElement.classList.remove('d-none');
        dataElement.classList.add('d-none');
        
        fetch('/api/Profile')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to load profile data');
                }
                return response.json();
            })
            .then(data => {
                // Update UI with profile data
                updateProfileUI(data);
                
                // Hide loading spinner and show data
                loadingElement.classList.add('d-none');
                dataElement.classList.remove('d-none');
            })
            .catch(error => {
                console.error('Error loading profile:', error);
                // Show error message
                dataElement.innerHTML = `
                    <div class="alert alert-danger">
                        <p>Error loading profile data.</p>
                        <button class="btn btn-sm btn-primary" onclick="loadUserProfile()">Retry</button>
                    </div>
                `;
                dataElement.classList.remove('d-none');
                loadingElement.classList.add('d-none');
            });
    }
}

function updateProfileUI(profile) {
    // Update navbar username
    const usernameElement = document.getElementById('profileUsername');
    if (usernameElement) {
        usernameElement.textContent = profile.fullName || 'Tài khoản';
    }
    
    // Update profile details
    document.getElementById('profileFullName').textContent = profile.fullName || 'Chưa cập nhật';
    document.getElementById('profilePhone').textContent = profile.phone || 'Chưa cập nhật';
    
    // Format date
    const createdDate = new Date(profile.createdDate);
    document.getElementById('profileCreatedDate').textContent = 
        createdDate.toLocaleDateString('vi-VN');
    
    // Format currency
    document.getElementById('profileTotalAmount').textContent = 
        new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
            .format(profile.totalAmount || 0);
}
