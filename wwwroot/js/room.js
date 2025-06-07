// Functions to handle room API calls

function loadRooms() {
    console.log('Loading rooms...');
    
    // Sử dụng hàm setLoading từ management.js
    setLoading('roomsContainer');
    
    fetch('/api/RoomApi')
        .then(response => {
            console.log('API response status:', response.status);
            if (!response.ok) {
                throw new Error(`Network response was not ok: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Received data:', data);
            displayRooms(data);
        })
        .catch(error => {
            console.error('Error fetching rooms:', error);
            // Sử dụng hàm handleApiError từ management.js
            handleApiError(error, 'roomsContainer', 'loadRooms');
        });
}

function displayRooms(rooms) {
    const container = document.getElementById('roomsContainer');
    
    if (!rooms || rooms.length === 0) {
        container.innerHTML = '<div class="alert alert-info">Không có phòng nào.</div>';
        return;
    }
    
    let html = '<div class="row">';
    
    rooms.forEach(room => {
        // Handle both camelCase and PascalCase property names
        const roomName = room.roomName || room.RoomName;
        const isAvailable = room.isAvailable !== undefined ? room.isAvailable : room.IsAvailable;
        
        html += `
            <div class="col-md-4 mb-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">${roomName}</h5>
                        <p class="card-text">
                            Trạng thái: 
                            ${isAvailable 
                                ? '<span class="text-success">Có sẵn</span>' 
                                : '<span class="text-danger">Đã đặt</span>'}
                        </p>
                    </div>
                </div>
            </div>
        `;
    });
    
    html += '</div>';
    container.innerHTML = html;
}

// Load rooms when the page is loaded
document.addEventListener('DOMContentLoaded', () => {
    console.log('DOM loaded, initializing room data load');
    loadRooms();
});
