// JavaScript for chair management

// Global variables
let currentChairId = 0;
let currentRoomId = 0;
const apiUrl = '/api/ChairApi';

// DOM elements
const chairsContainer = document.getElementById('chairsContainer');
const chairForm = document.getElementById('chairForm');
const roomFilter = document.getElementById('roomFilter');
const currentRoomDisplay = document.getElementById('currentRoomDisplay');
const chairModal = new bootstrap.Modal(document.getElementById('chairModal'));
const deleteChairModal = new bootstrap.Modal(document.getElementById('deleteChairModal'));

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    console.log('Chair management page loaded');
    
    // Initial load of chairs
    loadChairs();
    
    // Set up event listeners
    document.getElementById('refreshChairsBtn').addEventListener('click', loadChairs);
    document.getElementById('filterChairsBtn').addEventListener('click', filterChairsByRoom);
    document.getElementById('createChairBtn').addEventListener('click', setupCreateChair);
    document.getElementById('saveChairBtn').addEventListener('click', saveChair);
    document.getElementById('confirmDeleteBtn').addEventListener('click', deleteChair);
    
    // Update room filter when changed
    roomFilter.addEventListener('change', function() {
        console.log('Room filter changed to:', this.value);
    });
});

// Load all chairs from API or chairs for a specific room
function loadChairs() {
    console.log('Loading chairs...');
    showLoading();
    
    // Reset the room filter
    roomFilter.value = "0";
    currentRoomId = 0;
    currentRoomDisplay.textContent = "Tất cả các phòng";
    
    fetch(apiUrl, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin'
    })
    .then(handleResponse)
    .then(data => {
        console.log('Chair data received:', data);
        displayChairs(data);
    })
    .catch(error => {
        console.error('Error fetching chairs:', error);
        showError(error);
    });
}

// Filter chairs by room
function filterChairsByRoom() {
    const roomId = roomFilter.value;
    console.log('Filtering chairs by room ID:', roomId);
    
    if (roomId === "0") {
        // Load all chairs
        loadChairs();
        return;
    }
    
    currentRoomId = roomId;
    const roomName = roomFilter.options[roomFilter.selectedIndex].text;
    currentRoomDisplay.textContent = roomName;
    
    showLoading();
    
    fetch(`${apiUrl}/ByRoom/${roomId}`, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin'
    })
    .then(handleResponse)
    .then(data => {
        console.log('Filtered chair data received:', data);
        displayChairs(data);
    })
    .catch(error => {
        console.error('Error fetching chairs for room:', error);
        showError(error);
    });
}

// Display chairs in the table
function displayChairs(chairs) {
    if (!chairs || chairs.length === 0) {
        chairsContainer.innerHTML = `
            <div class="alert alert-info">
                <h5>Không có ghế nào</h5>
                <p>Hãy thêm ghế mới bằng cách nhấn nút "Thêm ghế mới" ở trên.</p>
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
                        <th>Tên ghế</th>
                        <th>Phòng</th>
                        <th>Trạng thái</th>
                        <th>Thao tác</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    chairs.forEach(chair => {
        const chairId = chair.chairId || chair.ChairId;
        const chairName = chair.chairName || chair.ChairName;
        const roomName = chair.roomName || chair.RoomName;
        const isAvailable = chair.isAvailable !== undefined ? chair.isAvailable : (chair.IsAvailable !== undefined ? chair.IsAvailable : true);
        
        html += `
            <tr>
                <td>${chairId}</td>
                <td>${chairName}</td>
                <td>${roomName}</td>
                <td>
                    <span class="badge ${isAvailable ? 'bg-success' : 'bg-danger'}">
                        ${isAvailable ? 'Có sẵn' : 'Đã đặt'}
                    </span>
                </td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="setupEditChair(${chairId})">
                        <i class="bi bi-pencil"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="setupDeleteChair(${chairId}, '${chairName}')">
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
    
    chairsContainer.innerHTML = html;
}

// Setup the modal for creating a new chair
function setupCreateChair() {
    currentChairId = 0;
    document.getElementById('chairModalLabel').textContent = 'Thêm ghế mới';
    chairForm.reset();
    
    // Set default values
    document.getElementById('isAvailable').checked = true;
    
    // If we're currently filtering by room, pre-select that room
    if (currentRoomId > 0) {
        document.getElementById('roomId').value = currentRoomId;
    }
}

// Setup the modal for editing an existing chair
function setupEditChair(chairId) {
    console.log('Setting up edit for chair ID:', chairId);
    currentChairId = chairId;
    
    document.getElementById('chairModalLabel').textContent = 'Chỉnh sửa ghế';
    
    fetch(`${apiUrl}/${chairId}`)
        .then(handleResponse)
        .then(chair => {
            document.getElementById('chairId').value = chair.chairId || chair.ChairId;
            document.getElementById('chairName').value = chair.chairName || chair.ChairName;
            document.getElementById('roomId').value = chair.roomId || chair.RoomId;
            document.getElementById('isAvailable').checked = chair.isAvailable !== undefined ? chair.isAvailable : (chair.IsAvailable !== undefined ? chair.IsAvailable : true);
            
            chairModal.show();
        })
        .catch(error => {
            console.error('Error fetching chair details:', error);
            alert('Không thể tải thông tin ghế. Vui lòng thử lại.');
        });
}

// Setup the modal for deleting a chair
function setupDeleteChair(chairId, chairName) {
    console.log('Setting up delete for chair ID:', chairId);
    currentChairId = chairId;
    document.getElementById('deleteChairName').textContent = chairName;
    deleteChairModal.show();
}

// Save a chair (create or update)
function saveChair() {
    console.log('Saving chair...');
    
    // Get form values
    const chairId = parseInt(document.getElementById('chairId').value) || 0;
    const chairName = document.getElementById('chairName').value;
    const roomId = parseInt(document.getElementById('roomId').value);
    const isAvailable = document.getElementById('isAvailable').checked;
    
    // Log form values for debugging
    console.log('Form values:', { chairId, chairName, roomId, isAvailable });
    
    // Validate form
    if (!chairName) {
        alert('Vui lòng điền tên ghế.');
        return;
    }
    
    if (!roomId) {
        alert('Vui lòng chọn phòng.');
        return;
    }
    
    // Prepare data
    const chairData = {
        ChairName: chairName,
        RoomId: roomId,
        IsAvailable: isAvailable
    };
    
    // For update operations, include the ID
    if (chairId > 0) {
        chairData.ChairId = chairId;
    }
    
    // Show saving indicator
    const saveButton = document.getElementById('saveChairBtn');
    saveButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang lưu...';
    saveButton.disabled = true;
    
    console.log('Sending data:', chairData);
    
    // Use direct fetch with simplified error handling
    const isCreate = chairId === 0;
    const url = isCreate ? apiUrl : `${apiUrl}/${chairId}`;
    const method = isCreate ? 'POST' : 'PUT';
    
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(chairData)
    })
    .then(response => {
        console.log('Response status:', response.status);
        
        // Log full response for debugging
        return response.text().then(text => {
            console.log('Response text:', text);
            
            if (!response.ok) {
                throw new Error(`Error: ${response.status} - ${text}`);
            }
            
            try {
                return text ? JSON.parse(text) : {};
            } catch (e) {
                console.log('Response is not JSON but that\'s OK');
                return {};
            }
        });
    })
    .then(data => {
        console.log('Success:', data);
        alert(`Ghế đã được ${isCreate ? 'tạo' : 'cập nhật'} thành công!`);
        chairModal.hide();
        loadChairs();
    })
    .catch(error => {
        console.error('Error:', error);
        alert(`Không thể ${isCreate ? 'tạo' : 'cập nhật'} ghế: ${error.message}`);
    })
    .finally(() => {
        saveButton.innerHTML = 'Lưu';
        saveButton.disabled = false;
    });
}

// Delete a chair
function deleteChair() {
    console.log('Deleting chair:', currentChairId);
    
    fetch(`${apiUrl}/${currentChairId}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin'
    })
    .then(handleResponse)
    .then(() => {
        console.log('Chair deleted successfully');
        deleteChairModal.hide();
        
        // Refresh the current view (filtered by room or all)
        if (currentRoomId > 0) {
            filterChairsByRoom();
        } else {
            loadChairs();
        }
    })
    .catch(error => {
        console.error('Error deleting chair:', error);
        alert('Không thể xóa ghế. Vui lòng thử lại.\n\nLỗi: ' + error.message);
    });
}

// Helper functions
function showLoading() {
    chairsContainer.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
            <p class="mt-2">Đang tải dữ liệu ghế...</p>
        </div>
    `;
}

function showError(error) {
    chairsContainer.innerHTML = `
        <div class="alert alert-danger">
            <h5>Lỗi khi tải dữ liệu</h5>
            <p>${error.message || 'Đã xảy ra lỗi không xác định'}</p>
            <div class="mt-2">
                <button class="btn btn-primary" onclick="loadChairs()">Thử lại</button>
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
        // Clone the response to log it for debugging
        const clonedResponse = response.clone();
        
        return clonedResponse.text().then(text => {
            console.error('API error response:', text);
            try {
                // Try to parse as JSON
                const json = JSON.parse(text);
                throw new Error(json.message || json.title || `HTTP error ${response.status}`);
            } catch (e) {
                // If parsing fails, use the raw text
                if (e instanceof SyntaxError) {
                    throw new Error(`HTTP error ${response.status}: ${text}`);
                }
                throw e;
            }
        });
    }
    
    // For DELETE operations that might not return JSON
    if (response.status === 204) {
        return {};
    }
    
    // Try to parse the response as JSON
    return response.json().catch(error => {
        console.error('Error parsing JSON response:', error);
        return {}; // Return empty object if parsing fails
    });
}
