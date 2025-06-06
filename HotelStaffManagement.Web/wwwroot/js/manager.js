// Alert For Cancel Schedule
document.addEventListener("DOMContentLoaded", function () {
    const links = document.querySelectorAll(".employee-name-link");
    const popup = document.getElementById("cancelPopup");
    const popupName = document.getElementById("popupEmployeeName");
    const popupShiftDate = document.getElementById("popupShiftDate");
    const popupShiftTime = document.getElementById("popupShiftTime");
    const nameInput = document.getElementById("popupEmployeeNameInput");
    const dateInput = document.getElementById("popupDateInput");
    const shiftInput = document.getElementById("popupShiftInput");

    links.forEach(link => {
        link.addEventListener("click", function () {
            const employeeName = this.dataset.employee;
            const date = this.dataset.date;
            const shift = this.dataset.shift;

            const now = new Date();
            const shiftParts = shift.split(" - ");
            if (shiftParts.length !== 2) return;

            const shiftStart = new Date(`${date}T${shiftParts[0]}:00`);
            const shiftEnd = new Date(`${date}T${shiftParts[1]}:00`);
            if (shiftEnd <= shiftStart) {
                shiftEnd.setDate(shiftEnd.getDate() + 1);
            }

            if (now >= shiftStart) {
                showCustomAlert("Այս հերթափոխը արդեն սկսվել է և չի կարող չեղարկվել։");
                return;
            }

            popupName.textContent = employeeName;
            popupShiftDate.textContent = new Date(date).toLocaleDateString("hy-AM");
            popupShiftTime.textContent = shift;
            nameInput.value = employeeName;
            dateInput.value = date;
            shiftInput.value = shift;

            popup.style.display = "flex";
        });
    });
});

function closePopup() {
    document.getElementById("cancelPopup").style.display = "none";
}

function showCustomAlert(message) {
    const alertMessage = document.getElementById("customAlertMessage");
    alertMessage.innerText = message;

    const modal = new bootstrap.Modal(document.getElementById("customAlertModal"));
    modal.show();
}

// Schedule
document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("createScheduleForm");
    const positionSelect = document.getElementById("positionSelect");

    if (positionSelect.value) {
        loadEmployeesAndShifts(positionSelect.value);
    }

    positionSelect.addEventListener("change", function () {
        loadEmployeesAndShifts(this.value);
    });

    form.addEventListener("submit", function (e) {
        const checkboxes = document.querySelectorAll("input[name='SelectedEmployeeIds']:checked");
        const errorSpan = document.getElementById("employeeValidationMessage");

        if (checkboxes.length === 0) {
            e.preventDefault(); 
            errorSpan.textContent = "Խնդրում ենք ընտրել գոնե մեկ աշխատակից։";
            errorSpan.classList.add("field-validation-error");
        } else {
            errorSpan.textContent = "";
            errorSpan.classList.remove("field-validation-error");
        }
    });

    function loadEmployeesAndShifts(position) {
        fetch(`/Manager/GetEmployeesAndShifts?position=${position}`)
            .then(res => res.json())
            .then(data => {
                const shiftsContainer = document.getElementById("shiftsContainer");
                shiftsContainer.innerHTML = "<div class='custom-toggle-group'>";
                data.shifts.forEach((shift, index) => {
                    const id = `shift-${index}`;
                    shiftsContainer.innerHTML += `
                        <div class="custom-toggle">
                            <input type="radio" name="SelectedShift" id="${id}" value="${shift.value}">
                            <label for="${id}">${shift.text}</label>
                        </div>`;
                });
                shiftsContainer.innerHTML += "</div>";

                const employeesContainer = document.getElementById("employeesContainer");
                employeesContainer.innerHTML = "<div class='custom-toggle-group'>";
                data.employees.forEach((emp, index) => {
                    const id = `employee-${index}`;
                    employeesContainer.innerHTML += `
                        <div class="custom-toggle">
                            <input type="checkbox" name="SelectedEmployeeIds" id="${id}" value="${emp.value}">
                            <label for="${id}">${emp.text}</label>
                        </div>`;
                });
                employeesContainer.innerHTML += "</div>";
            });
    }
});
