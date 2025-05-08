/**
 * Generates passenger forms based on the selected count
 * @param {number} count - The number of passenger forms to generate
 * @param {Array} mealChoices - Array of meal choice objects from the database
 */
function generatePassengerForms(count, mealChoices) {
    const container = document.getElementById('passengersContainer');
    const countDisplay = document.getElementById('passengerCountDisplay');

    // Update the passenger count display
    countDisplay.textContent = `Total passengers: ${count}`;

    // Clear the container
    container.innerHTML = '';

    console.log("Generating forms with meal choices:", mealChoices);

    // Build meal choice options
    let mealChoiceOptions = '<option value="">Select a meal option</option>';
    if (mealChoices && mealChoices.length) {
        mealChoices.forEach(meal => {
            
            mealChoiceOptions += `<option value="${meal.MealChoiceId}">${meal.Type}</option>`;
        });
    }

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
                        <input type="text" name="passengers[${i}].FirstName" class="form-control passenger-firstname" required onchange="checkDuplicateNames()" onkeyup="checkDuplicateNames()" />
                        <div class="invalid-feedback">First name is required.</div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Last Name</label>
                        <input type="text" name="passengers[${i}].LastName" class="form-control passenger-lastname" required onchange="checkDuplicateNames()" onkeyup="checkDuplicateNames()" />
                        <div class="invalid-feedback">Last name is required.</div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Email</label>
                        <input type="email" name="passengers[${i}].Email" class="form-control" required />
                        <div class="invalid-feedback">Valid email is required.</div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Date of Birth</label>
                        <input type="date" name="passengers[${i}].DateOfBirth" class="form-control" required />
                        <div class="invalid-feedback">Date of birth is required.</div>
                    </div>
                    <div class="col-md-12 mb-3">
                        <label class="form-label">Meal Preference</label>
                        <select name="passengers[${i}].MealChoiceId" class="form-select" required>
                            ${mealChoiceOptions}
                        </select>
                        <div class="invalid-feedback">Meal preference is required.</div>
                    </div>
                </div>
                <div class="duplicate-name-error text-danger" style="display: none;">
                    Passenger name must be unique. Another passenger has the same name.
                </div>
            </div>
        `;

        container.appendChild(passengerCard);
    }
}

/**
 * Check for duplicate passenger names in the form
 * Returns true if duplicates are found, false otherwise
 */
function checkDuplicateNames() {
    const firstNames = document.querySelectorAll('.passenger-firstname');
    const lastNames = document.querySelectorAll('.passenger-lastname');
    const warningElement = document.getElementById('duplicateNamesWarning');

    const nameMap = new Map();
    let duplicateFound = false;

    // Reset all duplicate indicators
    document.querySelectorAll('.duplicate-name-error').forEach(el => {
        el.style.display = 'none';
    });

    // Check for duplicates
    for (let i = 0; i < firstNames.length; i++) {
        const firstName = firstNames[i].value.trim().toLowerCase();
        const lastName = lastNames[i].value.trim().toLowerCase();

        if (firstName && lastName) {
            const fullName = `${firstName}-${lastName}`;

            if (nameMap.has(fullName)) {
                // Mark both the current and previous duplicate as invalid
                firstNames[i].closest('.card-body').querySelector('.duplicate-name-error').style.display = 'block';
                firstNames[nameMap.get(fullName)].closest('.card-body').querySelector('.duplicate-name-error').style.display = 'block';
                duplicateFound = true;
            } else {
                nameMap.set(fullName, i);
            }
        }
    }

    // Show or hide the global warning
    if (warningElement) {
        warningElement.style.display = duplicateFound ? 'block' : 'none';
    }

    return duplicateFound;
}
