// Smart Attendance Management System - Interactive Frontend Logic

document.addEventListener("DOMContentLoaded", function () {
    // 1. Sidebar Toggle for Mobile / Small Screens
    const sidebarToggle = document.getElementById("sidebarToggle");
    const appSidebar = document.getElementById("appSidebar");
    if (sidebarToggle && appSidebar) {
        sidebarToggle.addEventListener("click", function () {
            appSidebar.classList.toggle("show");
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener("click", function (e) {
            if (window.innerWidth <= 992 &&
                appSidebar.classList.contains("show") &&
                !appSidebar.contains(e.target) &&
                !sidebarToggle.contains(e.target)) {
                appSidebar.classList.remove("show");
            }
        });
    }

    // 2. Auto-dismiss Bootstrap Alerts after 5 seconds
    const alerts = document.querySelectorAll(".alert-dismissible");
    alerts.forEach(function (alert) {
        setTimeout(function () {
            try {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            } catch (e) { }
        }, 5000);
    });

    // 3. Attendance Counters Initialization
    updateAttendanceCounters();
});

// Calculate live counts of Present, Absent, and Late in the mark attendance table
function updateAttendanceCounters() {
    const presentInputs = document.querySelectorAll('input[type="radio"][value="1"]:checked');
    const absentInputs = document.querySelectorAll('input[type="radio"][value="2"]:checked');
    const lateInputs = document.querySelectorAll('input[type="radio"][value="3"]:checked');

    const pCountEl = document.getElementById("countPresent");
    const aCountEl = document.getElementById("countAbsent");
    const lCountEl = document.getElementById("countLate");
    const pctEl = document.getElementById("liveAttendancePct");

    const pCount = presentInputs.length;
    const aCount = absentInputs.length;
    const lCount = lateInputs.length;
    const total = pCount + aCount + lCount;

    if (pCountEl) pCountEl.textContent = pCount;
    if (aCountEl) aCountEl.textContent = aCount;
    if (lCountEl) lCountEl.textContent = lCount;

    if (pctEl && total > 0) {
        // Late counts as 0.5
        const pct = Math.round(((pCount + (lCount * 0.5)) / total) * 1000) / 10;
        pctEl.textContent = pct + "%";
    }
}

// Quick Mark All helpers
function markAllStatus(statusValue) {
    const radios = document.querySelectorAll(`input[type="radio"][value="${statusValue}"]`);
    radios.forEach(r => {
        r.checked = true;
    });
    updateAttendanceCounters();
}

// Quick login credential auto-filler for viva demo
function fillLogin(email, password) {
    const emailField = document.getElementById("Email");
    const passwordField = document.getElementById("Password");
    if (emailField) emailField.value = email;
    if (passwordField) passwordField.value = password;
}
