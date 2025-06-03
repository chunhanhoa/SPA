// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Function to check if user has admin role
function checkAdminAccess() {
    return fetch('/api/RoleApi/IsAdmin')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log('Admin access check:', data);
            return data.isAdmin === true;
        })
        .catch(error => {
            console.error('Error checking admin role:', error);
            return false;
        });
}

// Display or hide admin menu based on user role
function updateAdminMenuVisibility() {
    const adminMenuItems = document.querySelectorAll('.admin-only');
    if (!adminMenuItems || adminMenuItems.length === 0) {
        return; // No admin menu items to update
    }
    
    checkAdminAccess()
        .then(isAdmin => {
            adminMenuItems.forEach(item => {
                item.style.display = isAdmin ? 'block' : 'none';
            });
        });
}

// Initialize the page
document.addEventListener('DOMContentLoaded', function() {
    // Update admin menu visibility
    updateAdminMenuVisibility();
    
    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            alert.classList.add('fade');
            setTimeout(() => {
                if (alert.parentNode) {
                    alert.parentNode.removeChild(alert);
                }
            }, 500);
        });
    }, 5000);
});
