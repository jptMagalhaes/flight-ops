(function () {
    var DEBOUNCE_MS = 200;
    var EMPTY_VALUE = "—";

    function formatNumber(value) {
        return new Intl.NumberFormat("en-US", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value);
    }

    function formatArrival(isoValue, locale) {
        var date = new Date(isoValue);
        if (Number.isNaN(date.getTime())) {
            return EMPTY_VALUE;
        }

        return new Intl.DateTimeFormat(locale, {
            dateStyle: "short",
            timeStyle: "short"
        }).format(date);
    }

    function setCalculatedFields(distanceEl, fuelEl, arrivalEl, preview, locale) {
        if (!preview) {
            distanceEl.textContent = EMPTY_VALUE;
            fuelEl.textContent = EMPTY_VALUE;
            arrivalEl.textContent = EMPTY_VALUE;
            return;
        }

        distanceEl.textContent = formatNumber(preview.distance ?? preview.Distance);
        fuelEl.textContent = formatNumber(preview.fuel ?? preview.Fuel);
        arrivalEl.textContent = formatArrival(preview.arrivalTime ?? preview.ArrivalTime, locale);
    }

    function initFlightCreate() {
        var config = window.flightCreateConfig;
        var form = document.querySelector("[data-fo-flight-create]");
        if (!config || !form || !config.calculateUrl) {
            return;
        }

        var originEl = form.querySelector("[data-fo-flight-origin]");
        var destinationEl = form.querySelector("[data-fo-flight-destination]");
        var aircraftEl = form.querySelector("[data-fo-flight-aircraft]");
        var departureEl = form.querySelector("[data-fo-flight-departure]");
        var distanceEl = form.querySelector("[data-fo-calc-distance]");
        var fuelEl = form.querySelector("[data-fo-calc-fuel]");
        var arrivalEl = form.querySelector("[data-fo-calc-arrival]");

        if (!originEl || !destinationEl || !aircraftEl || !departureEl || !distanceEl || !fuelEl || !arrivalEl) {
            return;
        }

        var debounceTimer = null;
        var requestId = 0;

        function canCalculate() {
            var originId = parseInt(originEl.value, 10);
            var destinationId = parseInt(destinationEl.value, 10);
            var aircraftId = parseInt(aircraftEl.value, 10);
            return originId > 0
                && destinationId > 0
                && aircraftId > 0
                && originId !== destinationId
                && departureEl.value.length > 0;
        }

        function buildUrl() {
            var departureDate = new Date(departureEl.value);
            var departureIso = Number.isNaN(departureDate.getTime())
                ? ""
                : departureDate.toISOString();
            var params = new URLSearchParams({
                originId: originEl.value,
                destinationId: destinationEl.value,
                aircraftId: aircraftEl.value,
                departureTime: departureIso
            });
            return config.calculateUrl + "?" + params.toString();
        }

        function refreshPreview() {
            window.clearTimeout(debounceTimer);
            debounceTimer = window.setTimeout(function () {
                if (!canCalculate()) {
                    setCalculatedFields(distanceEl, fuelEl, arrivalEl, null, config.locale);
                    return;
                }

                var currentRequest = ++requestId;
                fetch(buildUrl(), {
                    headers: { Accept: "application/json" }
                })
                    .then(function (response) {
                        if (!response.ok) {
                            throw new Error("preview failed");
                        }
                        return response.json();
                    })
                    .then(function (preview) {
                        if (currentRequest !== requestId) {
                            return;
                        }
                        setCalculatedFields(distanceEl, fuelEl, arrivalEl, preview, config.locale);
                    })
                    .catch(function () {
                        if (currentRequest !== requestId) {
                            return;
                        }
                        setCalculatedFields(distanceEl, fuelEl, arrivalEl, null, config.locale);
                    });
            }, DEBOUNCE_MS);
        }

        originEl.addEventListener("change", refreshPreview);
        destinationEl.addEventListener("change", refreshPreview);
        aircraftEl.addEventListener("change", refreshPreview);
        departureEl.addEventListener("change", refreshPreview);
        departureEl.addEventListener("input", refreshPreview);

        refreshPreview();
    }

    document.addEventListener("DOMContentLoaded", initFlightCreate);
})();
