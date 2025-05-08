/**
 * Generates passenger forms based on the selected count
 * @param {number} count - The number of passenger forms to generate
 * @param {Array} mealChoices - Array of meal choice objects from the database
 */
function generatePassengerForms(count, mealChoices) {
    const container = document.getElementById('passengersContainer');
    const countDisplay = document.getElementById('passengerCountDisplay');

    console.log('generatePassengerForms called with count:', count);
    console.log('mealChoices:', mealChoices);

    // Update the passenger count display
    countDisplay.textContent = `Total passengers: ${count}`;

    // Clear the container
    container.innerHTML = '';

    // Generate forms for each passenger
    for (let i = 0; i < count; i++) {
        const passengerCard = document.createElement('div');
        passengerCard.className = 'card mb-3';

        let mealOptionsHtml = '<option value="">Select a meal option</option>';
        if (Array.isArray(mealChoices) && mealChoices.length > 0) {
            mealChoices.forEach(meal => {
                mealOptionsHtml += `<option value="${meal.MealChoiceId}">${meal.Type}</option>`;
            });
        } else {
            console.warn('No meal choices available');
        }

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
                    <div class="col-md-12 mb-3">
                        <label class="form-label">Meal Preference</label>
                        <select name="passengers[${i}].MealChoiceId" class="form-select" required>
                            ${mealOptionsHtml}
                        </select>
                    </div>
                </div>
            </div>
        `;

        container.appendChild(passengerCard);
    }
}