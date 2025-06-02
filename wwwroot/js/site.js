// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Function to check if user has admin role
function checkAdminAccess() {
    fetch('/api/Role/IsAdmin')
        .then(response => {
            if (response.ok) {
                return response.json();
            }
            throw new Error('Failed to fetch admin status');
        })
        .then(isAdmin => {
            // Show/hide elements based on admin status
            const adminElements = document.querySelectorAll('.admin-only');
            adminElements.forEach(element => {
                if (isAdmin) {
                    element.classList.remove('d-none');
                } else {
                    element.classList.add('d-none');
                }
            });
        })
        .catch(error => {
            console.error('Error checking admin status:', error);
        });
}

// Run on page load if user is authenticated
document.addEventListener('DOMContentLoaded', function() {
    // Only run if user is logged in (check for logout button as an indicator)
    if (document.querySelector('form[asp-controller="Account"][asp-action="Logout"]')) {
        checkAdminAccess();
    }
});
