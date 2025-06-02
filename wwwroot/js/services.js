// JavaScript for services page

document.addEventListener('DOMContentLoaded', function() {
    loadServices();
});

// Load services from API
function loadServices() {
    const container = document.getElementById('servicesContainer');
    
    fetch('/api/ServiceApi')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Services data:', data);
            displayServices(data);
        })
        .catch(error => {
            console.error('Error loading services:', error);
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-danger">
                        <h4>Không thể tải dịch vụ</h4>
                        <p>${error.message}</p>
                        <button class="btn btn-primary mt-2" onclick="loadServices()">Thử lại</button>
                    </div>
                </div>
            `;
        });
}

// Display services in a grid
function displayServices(services) {
    const container = document.getElementById('servicesContainer');
    
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
            <div class="col-md-4 mb-4">
                <div class="card h-100 service-card">
                    <img src="${picture}" class="card-img-top service-image" alt="${name}" onerror="this.src='/images/default-service.jpg'">
                    <div class="card-body">
                        <h5 class="card-title">${name}</h5>
                        <p class="card-text text-truncate">${description}</p>
                        <div class="d-flex justify-content-between">
                            <span class="text-primary fw-bold">${formattedPrice}</span>
                            <span class="text-muted"><i class="bi bi-clock"></i> ${formattedDuration}</span>
                        </div>
                    </div>
                    <div class="card-footer bg-white border-top-0">
                        <button class="btn btn-primary w-100" onclick="viewServiceDetails(${id})">
                            Xem chi tiết
                        </button>
                    </div>
                </div>
            </div>
        `;
    });
    
    container.innerHTML = html;
}

// View service details function
function viewServiceDetails(serviceId) {
    console.log(`Navigating to service details page for ID: ${serviceId}`);
    window.location.href = `/Home/ServiceDetails/${serviceId}`;
}
