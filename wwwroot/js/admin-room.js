// JavaScript for room management

// Global variables
let currentRoomId = 0;
const apiUrl = '/api/RoomApi';

// DOM elements
const roomsContainer = document.getElementById('roomsContainer');
const roomForm = document.getElementById('roomForm');
const roomModal = new bootstrap.Modal(document.getElementById('roomModal'));
const deleteRoomModal = new bootstrap.Modal(document.getElementById('deleteRoomModal'));

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    console.log('Room management page loaded');
    
    // Initial load of rooms
    loadRooms();
    
    // Set up event listeners
    document.getElementById('refreshRoomsBtn').addEventListener('click', loadRooms);
    document.getElementById('createRoomBtn').addEventListener('click', setupCreateRoom);
    document.getElementById('saveRoomBtn').addEventListener('click', saveRoom);
    document.getElementById('confirmDeleteBtn').addEventListener('click', deleteRoom);
});

// Load all rooms from API
function loadRooms() {
    console.log('Loading rooms...');
    showLoading();
    
    fetch(apiUrl, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin'
    })
    .then(response => {
        console.log('API response status:', response.status);
        if (!response.ok) {
            return response.text().then(text => {
                console.error('API error response:', text);
                try {
                    // Try to parse as JSON
                    const json = JSON.parse(text);
                    throw new Error(json.message || `HTTP error ${response.status}`);
                } catch (e) {
                    // If parsing fails, use the raw text
                    throw new Error(`HTTP error ${response.status}: ${text}`);
                }
            });
        }
        return response.json();
    })
    .then(data => {
        console.log('Room data received:', data);
        displayRooms(data);
    })
    .catch(error => {
        console.error('Error fetching rooms:', error);
        showError(error);
    });
}

// Display rooms in the table
function displayRooms(rooms) {
    if (!rooms || rooms.length === 0) {
        roomsContainer.innerHTML = `
            <div class="alert alert-info">
                <h5>Không có phòng nào</h5>
                <p>Hãy thêm phòng mới bằng cách nhấn nút "Thêm phòng mới" ở trên.</p>
            </div>
        `;
        return;
    }
    
    let html = `
        <div class="table-responsive">
            <table class="table table-striped table-hover">
                <thead class="table-light">
                    <tr>
                        <th>ID</th>
                        <th>Tên phòng</th>
                        <th>Trạng thái</th>
                        <th>Thao tác</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    rooms.forEach(room => {
        const roomId = room.roomId || room.RoomId;
        const roomName = room.roomName || room.RoomName;
        const isAvailable = room.isAvailable !== undefined ? room.isAvailable : (room.IsAvailable !== undefined ? room.IsAvailable : true);
        
        const statusClass = isAvailable ? 'success' : 'danger';
        const statusText = isAvailable ? 'Có sẵn' : 'Không có sẵn';
        
        html += `
            <tr>
                <td>${roomId}</td>
                <td>${roomName}</td>
                <td><span class="badge bg-${statusClass}">${statusText}</span></td>
                <td>
                    <button class="btn btn-sm btn-primary edit-room-btn" data-id="${roomId}" data-name="${roomName}" data-available="${isAvailable}">
                        <i class="bi bi-pencil"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-danger delete-room-btn" data-id="${roomId}" data-name="${roomName}">
                        <i class="bi bi-trash"></i> Xóa
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
    
    roomsContainer.innerHTML = html;
    
    // Add event listeners for the edit and delete buttons
    document.querySelectorAll('.edit-room-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const roomId = this.getAttribute('data-id');
            const roomName = this.getAttribute('data-name');
            const isAvailable = this.getAttribute('data-available') === 'true';
            openEditRoomModal(roomId, roomName, isAvailable);
        });
    });
    
    document.querySelectorAll('.delete-room-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const roomId = this.getAttribute('data-id');
            const roomName = this.getAttribute('data-name');
            openDeleteRoomModal(roomId, roomName);
        });
    });
}

// Setup the modal for creating a new room
function setupCreateRoom() {
    currentRoomId = 0;
    document.getElementById('roomModalLabel').textContent = 'Thêm phòng mới';
    roomForm.reset();
    
    // Cho phép tạo phòng với trạng thái "Có sẵn" (IsAvailable = true)
    document.getElementById('isAvailable').checked = true;
    
    // Set default values
    document.getElementById('roomType').value = 'Standard';
    document.getElementById('price').value = '0';
}

// Setup the modal for editing an existing room
function setupEditRoom(roomId) {
    console.log('Setting up edit for room ID:', roomId);
    currentRoomId = roomId;
    
    document.getElementById('roomModalLabel').textContent = 'Chỉnh sửa phòng';
    
    fetch(`${apiUrl}/${roomId}`)
        .then(handleResponse)
        .then(room => {
            document.getElementById('roomId').value = room.roomId || room.RoomId;
            document.getElementById('roomName').value = room.roomName || room.RoomName;
            document.getElementById('roomType').value = room.roomType || room.RoomType || 'Standard';
            document.getElementById('price').value = room.price || room.Price || 0;
            document.getElementById('isAvailable').checked = room.isAvailable !== undefined ? room.isAvailable : (room.IsAvailable !== undefined ? room.IsAvailable : true);
            
            roomModal.show();
        })
        .catch(error => {
            console.error('Error fetching room details:', error);
            alert('Không thể tải thông tin phòng. Vui lòng thử lại.');
        });
}

// Setup the modal for deleting a room
function setupDeleteRoom(roomId, roomName) {
    console.log('Setting up delete for room ID:', roomId);
    currentRoomId = roomId;
    document.getElementById('deleteRoomName').textContent = roomName;
    deleteRoomModal.show();
}

// Save a room (create or update)
function saveRoom() {
    console.log('Saving room...');
    
    // Get form values
    const roomId = parseInt(document.getElementById('roomId').value) || 0;
    const roomName = document.getElementById('roomName').value;
    
    // Get checkbox state and ensure it's a boolean
    const isAvailableElement = document.getElementById('isAvailable');
    const isAvailable = isAvailableElement ? isAvailableElement.checked : true;
    
    console.log('Form values:', {
        roomId,
        roomName,
        isAvailable,
        isAvailableElementExists: !!isAvailableElement,
        checkboxType: isAvailableElement ? typeof isAvailableElement.checked : 'N/A'
    });
    
    // Validate form
    if (!roomName) {
        alert('Vui lòng điền tên phòng.');
        return;
    }
    
    // Prepare data - use PascalCase for property names to match C# model
    const roomData = {
        RoomId: roomId,
        RoomName: roomName,
        IsAvailable: isAvailable // Send as a boolean
    };
    
    console.log('Room data to send:', roomData);
    console.log('JSON to send:', JSON.stringify(roomData));
    
    const isCreate = roomId === 0;
    const url = isCreate ? apiUrl : `${apiUrl}/${roomId}`;
    const method = isCreate ? 'POST' : 'PUT';
    
    // Show saving indicator
    const saveButton = document.getElementById('saveRoomBtn');
    const originalText = saveButton.textContent;
    saveButton.textContent = 'Đang lưu...';
    saveButton.disabled = true;
    
    // Send request
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin',
        body: JSON.stringify(roomData)
    })
    .then(response => {
        console.log('API response status:', response.status);
        
        // Clone the response to log the raw content for debugging
        const clonedResponse = response.clone();
        clonedResponse.text().then(text => {
            console.log('Raw API response:', text);
        });
        
        if (!response.ok) {
            return response.text().then(text => {
                try {
                    const json = JSON.parse(text);
                    throw new Error(json.message || `HTTP error ${response.status}`);
                } catch (e) {
                    throw new Error(`HTTP error ${response.status}: ${text}`);
                }
            });
        }
        return response.json();
    })
    .then(data => {
        console.log(`Room ${isCreate ? 'created' : 'updated'} successfully:`, data);
        roomModal.hide();
        loadRooms();
    })
    .catch(error => {
        console.error(`Error ${isCreate ? 'creating' : 'updating'} room:`, error);
        alert(`Không thể ${isCreate ? 'tạo' : 'cập nhật'} phòng. Vui lòng thử lại.\n\nLỗi: ${error.message}`);
    })
    .finally(() => {
        // Reset save button
        saveButton.textContent = originalText;
        saveButton.disabled = false;
    });
}

// Delete a room
function deleteRoom() {
    console.log('Deleting room:', currentRoomId);
    
    fetch(`${apiUrl}/${currentRoomId}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin'
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => {
                try {
                    const json = JSON.parse(text);
                    throw new Error(json.message || `HTTP error ${response.status}`);
                } catch (e) {
                    throw new Error(`HTTP error ${response.status}: ${text}`);
                }
            });
        }
        console.log('Room deleted successfully');
        deleteRoomModal.hide();
        loadRooms();
    })
    .catch(error => {
        console.error('Error deleting room:', error);
        alert('Không thể xóa phòng. Vui lòng thử lại.\n\nLỗi: ' + error.message);
    });
}

// Helper functions
function showLoading() {
    roomsContainer.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
            <p class="mt-2">Đang tải dữ liệu phòng...</p>
        </div>
    `;
}

function showError(error) {
    roomsContainer.innerHTML = `
        <div class="alert alert-danger">
            <h5>Lỗi khi tải dữ liệu</h5>
            <p>${error.message || 'Đã xảy ra lỗi không xác định'}</p>
            <div class="mt-2">
                <button class="btn btn-primary" onclick="loadRooms()">Thử lại</button>
                <button class="btn btn-outline-secondary ms-2" onclick="showErrorDetails('${error}')">Chi tiết lỗi</button>
            </div>
        </div>
        <div class="mt-3" id="errorDetails" style="display: none;">
            <div class="card">
                <div class="card-header">Chi tiết lỗi</div>
                <div class="card-body">
                    <pre class="mb-0" id="errorDetailsText"></pre>
                </div>
            </div>
        </div>
    `;
}

function showErrorDetails(errorString) {
    console.error('Error details:', errorString);
    
    const errorDetails = document.getElementById('errorDetails');
    const errorDetailsText = document.getElementById('errorDetailsText');
    
    if (errorDetails && errorDetailsText) {
        errorDetailsText.textContent = errorString;
        errorDetails.style.display = 'block';
    } else {
        alert('Chi tiết lỗi đã được ghi lại trong console. Vui lòng mở DevTools để kiểm tra (F12).');
    }
}

function handleResponse(response) {
    console.log('API response status:', response.status);
    if (!response.ok) {
        return response.text().then(text => {
            console.error('API error response:', text);
            try {
                // Try to parse as JSON
                const json = JSON.parse(text);
                throw new Error(json.message || `HTTP error ${response.status}`);
            } catch (e) {
                // If parsing fails, use the raw text
                throw new Error(`HTTP error ${response.status}: ${text}`);
            }
        });
    }
    return response.json();
}
