document.addEventListener('DOMContentLoaded', function() {
    const contactForm = document.getElementById('contactForm');
    
    if (contactForm) {
        contactForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Get form values
            const formData = {
                fullName: document.getElementById('fullName').value,
                email: document.getElementById('email').value,
                phoneNumber: document.getElementById('phoneNumber').value,
                subject: document.getElementById('subject').value,
                message: document.getElementById('message').value,
                agreeTerms: document.getElementById('agreeTerms').checked
            };
            
            // Disable submit button and show loading state
            const submitButton = contactForm.querySelector('button[type="submit"]');
            const originalButtonText = submitButton.innerHTML;
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang gửi...';
            
            // Send data to API
            fetch('/api/ContactApi/Submit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(formData)
            })
            .then(response => response.json())
            .then(data => {
                // Show success message
                const resultContainer = document.getElementById('formResult');
                resultContainer.innerHTML = `
                    <div class="alert alert-success alert-dismissible fade show" role="alert">
                        <strong>Thành công!</strong> ${data.message}
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>
                `;
                
                // Reset form
                contactForm.reset();
            })
            .catch(error => {
                // Show error message
                const resultContainer = document.getElementById('formResult');
                resultContainer.innerHTML = `
                    <div class="alert alert-danger alert-dismissible fade show" role="alert">
                        <strong>Lỗi!</strong> Đã xảy ra lỗi khi gửi biểu mẫu. Vui lòng thử lại sau.
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>
                `;
                console.error('Error submitting form:', error);
            })
            .finally(() => {
                // Re-enable submit button
                submitButton.disabled = false;
                submitButton.innerHTML = originalButtonText;
            });
        });
    }
});
