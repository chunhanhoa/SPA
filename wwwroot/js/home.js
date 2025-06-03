document.addEventListener('DOMContentLoaded', function() {
    // Load featured services
    loadFeaturedServices();
    
    // Setup booking form if on booking page
    if (document.getElementById('bookingForm')) {
        setupBookingForm();
    }
});

// Load featured services for the homepage
function loadFeaturedServices() {
    // Kiểm tra xem phần tử chứa dịch vụ có tồn tại không
    const servicesContainer = document.getElementById('featuredServicesContainer');
    if (!servicesContainer) return;

    fetch('/api/ServiceApi/Featured')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok: ' + response.statusText);
            }
            return response.json();
        })
        .then(data => {
            console.log("Featured services data:", data);
            if (data && data.length > 0) {
                let html = '';
                data.forEach(service => {
                    // Điều chỉnh tên thuộc tính để phù hợp với API trả về
                    const serviceId = service.id || service.serviceId || service.ServiceId;
                    const serviceName = service.title || service.serviceName || service.ServiceName;
                    const serviceDesc = service.description || service.Description;
                    const servicePrice = service.price || service.Price;
                    const servicePicture = service.picture || service.Picture;
                    
                    html += `
                        <div class="col-lg-3 col-md-6 mb-4">
                            <div class="card service-item h-100">
                                <img src="${servicePicture || '/images/service-default.jpg'}" class="card-img-top" alt="${serviceName}" 
                                    onerror="this.src='/images/service-default.jpg'">
                                <div class="card-body">
                                    <h5 class="card-title">${serviceName}</h5>
                                    <p class="card-text">${serviceDesc?.substring(0, 100) || ''}...</p>
                                    <div class="d-flex justify-content-between align-items-center">
                                        <span class="price-badge">${new Intl.NumberFormat('vi-VN').format(servicePrice)} VNĐ</span>
                                        <a href="/Home/ServiceDetails/${serviceId}" class="btn btn-sm btn-outline-primary">Chi tiết</a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                });
                servicesContainer.innerHTML = html;
            } else {
                servicesContainer.innerHTML = '<div class="col-12"><p class="text-center">Không có dịch vụ nổi bật.</p></div>';
            }
        })
        .catch(error => {
            console.error("Error loading featured services:", error);
            servicesContainer.innerHTML = '<div class="col-12"><p class="text-center text-danger">Không thể tải dịch vụ nổi bật.</p></div>';
        });
}

// Setup booking form functionality
function setupBookingForm() {
    // Code for booking form setup (already handled in Booking.cshtml)
}