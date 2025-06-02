// JavaScript for Home page

document.addEventListener('DOMContentLoaded', function() {
    loadFeaturedServices();
});

// Load featured services from API
function loadFeaturedServices() {
    const container = document.getElementById('featuredServicesContainer');
    
    fetch('/api/ServiceApi?limit=3')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Featured services data:', data);
            displayFeaturedServices(data.slice(0, 3)); // Ensure we only get 3 services
        })
        .catch(error => {
            console.error('Error loading featured services:', error);
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-danger">
                        <h4>Không thể tải dịch vụ</h4>
                        <p>${error.message}</p>
                    </div>
                </div>
            `;
        });
}

// Display featured services in a grid
function displayFeaturedServices(services) {
    const container = document.getElementById('featuredServicesContainer');
    
    if (!services || services.length === 0) {
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-info">
                    <h4>Không có dịch vụ nào</h4>
                    <p>Hiện tại chúng tôi chưa có dịch vụ nào. Vui lòng quay lại sau.</p>
                </div>
            </div>
        `;
        return;
    }
    
    let html = '';
    
    services.forEach(service => {
        // Handle both camelCase and PascalCase property names
        const id = service.serviceId || service.ServiceId;
        const name = service.serviceName || service.ServiceName;
        const description = service.description || service.Description;
        const price = service.price || service.Price || 0;
        const duration = service.duration || service.Duration || 0;
        const picture = service.picture || service.Picture || '/images/default-service.jpg';
        
        // Format price as VND
        const formattedPrice = new Intl.NumberFormat('vi-VN', { 
            style: 'currency', 
            currency: 'VND' 
        }).format(price);
        
        // Format duration
        const formattedDuration = `${duration} phút`;
        
        html += `
            <div class="col-lg-4 col-md-6 mb-4">
                <div class="service-card text-center p-4 shadow-sm rounded h-100">
                    <img src="${picture}" class="card-img-top service-image" alt="${name}" 
                         onerror="this.src='/images/default-service.jpg'" style="height: 200px; object-fit: cover;">
                    <div class="card-body d-flex flex-column">
                        <h3 class="h4 mb-3">${name}</h3>
                        <p class="mb-3 flex-grow-1">${description.length > 100 ? description.substring(0, 100) + '...' : description}</p>
                        <div class="d-flex justify-content-between mb-3">
                            <span class="text-primary fw-bold">${formattedPrice}</span>
                            <span class="text-muted"><i class="bi bi-clock"></i> ${formattedDuration}</span>
                        </div>
                        <a href="/Home/ServiceDetails/${id}" class="btn btn-outline-primary">Xem chi tiết</a>
                    </div>
                </div>
            </div>
        `;
    });
    
    container.innerHTML = html;
}
