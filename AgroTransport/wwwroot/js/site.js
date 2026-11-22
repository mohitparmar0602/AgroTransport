// AgroTransport - Client Side Utilities (INT402 Unit III & VI)

document.addEventListener("DOMContentLoaded", function () {
    // 1. Live Dynamic Cost Calculation for Booking Page
    const qtyInput = document.getElementById("bookingQuantityInput");
    const costPerKgElem = document.getElementById("tripCostPerKg");
    const totalCostDisplay = document.getElementById("calculatedTotalCostDisplay");
    const savingsDisplay = document.getElementById("calculatedSavingsDisplay");
    const totalTripCostElem = document.getElementById("tripTotalFullCost");

    if (qtyInput && costPerKgElem && totalCostDisplay) {
        const costPerKg = parseFloat(costPerKgElem.dataset.costPerKg || 0);
        const fullTripCost = parseFloat(totalTripCostElem?.dataset.totalCost || 0);

        function updateCost() {
            let qty = parseFloat(qtyInput.value) || 0;
            if (qty < 0) qty = 0;
            const cost = (qty * costPerKg).toFixed(2);
            totalCostDisplay.textContent = "₹" + Number(cost).toLocaleString("en-IN");

            if (savingsDisplay && fullTripCost > 0) {
                const savings = Math.max(0, fullTripCost - cost).toFixed(2);
                savingsDisplay.textContent = "₹" + Number(savings).toLocaleString("en-IN");
            }
        }

        qtyInput.addEventListener("input", updateCost);
        updateCost(); // Initial run
    }

    // 2. Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 6000);
    });
});
