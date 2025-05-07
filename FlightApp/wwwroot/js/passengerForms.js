/**
 * Generates passenger forms based on the selected count
 * @param {number} count - The number of passenger forms to generate
 */
function generatePassengerForms(count) {
    const container = document.getElementById('passengersContainer');
    const countDisplay = document.getElementById('passengerCountDisplay');
    
    // Update the passenger count display
    countDisplay.textContent = `Total passengers: ${count}`;
    
    // Clear the container
    container.innerHTML = '';
    
    // Generate forms for each passenger
    for (let i = 0; i < count; i++) {
        const passengerCard = document.createElement('div');
        passengerCard.className = 'card mb-3';
        
        passengerCard.innerHTML = `
            <div class="card-header">
                <h5>Passenger ${i + 1}</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">First Name</label>
                        <input type="text" name="passengers[${i}].FirstName" class="form-control" required />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Last Name</label>
                        <input type="text" name="passengers[${i}].LastName" class="form-control" required />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Email</label>
                        <input type="email" name="passengers[${i}].Email" class="form-control" required />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Date of Birth</label>
                        <input type="date" name="passengers[${i}].DateOfBirth" class="form-control" required />
                    </div>
                </div>
            </div>
        `;
        
        container.appendChild(passengerCard);
    }
}

// Event listener for passenger count dropdown changes
document.addEventListener('DOMContentLoaded', function() {
    const passengerCountDropdown = document.getElementById('passengerCount');
    
    passengerCountDropdown.addEventListener('change', function() {
        const selectedCount = parseInt(this.value, 10);
        generatePassengerForms(selectedCount);
    });
});
