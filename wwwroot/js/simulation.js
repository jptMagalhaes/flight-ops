(function () {
    const container = document.getElementById('cesiumContainer');
    if (!container || typeof Cesium === 'undefined') return;

    const config = window.simulationConfig;
    const flightListEl = document.getElementById('flightList');
    const noFlightsEl = document.getElementById('noFlightsMessage');
    const detailsEl = document.getElementById('flightDetails');

    let viewer = null;
    let flights = [];
    let selectedId = null;
    let routeEntity = null;
    const flightEntities = new Map();

    function initViewer() {
        viewer = new Cesium.Viewer('cesiumContainer', {
            baseLayerPicker: false,
            geocoder: false,
            homeButton: true,
            sceneModePicker: true,
            navigationHelpButton: false,
            animation: false,
            timeline: false,
            fullscreenButton: true,
            terrainProvider: new Cesium.EllipsoidTerrainProvider(),
            imageryProvider: false
        });

        viewer.imageryLayers.addImageryProvider(
            new Cesium.UrlTemplateImageryProvider({
                url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
                maximumLevel: 19,
                credit: 'Esri'
            })
        );

        viewer.scene.globe.enableLighting = false;
    }

    function toRadians(deg) {
        return deg * Math.PI / 180;
    }

    function toDegrees(rad) {
        return rad * 180 / Math.PI;
    }

    function interpolateGreatCircle(lat1, lon1, lat2, lon2, t) {
        if (t <= 0) return { lat: lat1, lon: lon1 };
        if (t >= 1) return { lat: lat2, lon: lon2 };

        const phi1 = toRadians(lat1);
        const lambda1 = toRadians(lon1);
        const phi2 = toRadians(lat2);
        const lambda2 = toRadians(lon2);

        const deltaLambda = lambda2 - lambda1;
        const sinPhi1 = Math.sin(phi1);
        const cosPhi1 = Math.cos(phi1);
        const sinPhi2 = Math.sin(phi2);
        const cosPhi2 = Math.cos(phi2);

        const cosAngle = sinPhi1 * sinPhi2 + cosPhi1 * cosPhi2 * Math.cos(deltaLambda);
        const angle = Math.acos(Math.min(1, Math.max(-1, cosAngle)));

        if (angle < 1e-10) return { lat: lat1, lon: lon1 };

        const a = Math.sin((1 - t) * angle) / Math.sin(angle);
        const b = Math.sin(t * angle) / Math.sin(angle);

        const x = a * cosPhi1 * Math.cos(lambda1) + b * cosPhi2 * Math.cos(lambda2);
        const y = a * cosPhi1 * Math.sin(lambda1) + b * cosPhi2 * Math.sin(lambda2);
        const z = a * sinPhi1 + b * sinPhi2;

        return {
            lat: toDegrees(Math.atan2(z, Math.sqrt(x * x + y * y))),
            lon: toDegrees(Math.atan2(y, x))
        };
    }

    function buildArcPositions(flight, steps = 64) {
        const positions = [];
        for (let i = 0; i <= steps; i++) {
            const t = i / steps;
            const { lat, lon } = interpolateGreatCircle(
                flight.originLatitude,
                flight.originLongitude,
                flight.destinationLatitude,
                flight.destinationLongitude,
                t
            );
            const alt = 8000 + Math.sin(t * Math.PI) * 2000;
            positions.push(Cesium.Cartesian3.fromDegrees(lon, lat, alt));
        }
        return positions;
    }

    function computeProgress(flight) {
        const start = new Date(flight.departureTime).getTime();
        const end = new Date(flight.arrivalTime).getTime();
        const now = Date.now();
        if (now <= start) return 0;
        if (now >= end) return 1;
        return (now - start) / (end - start);
    }

    function getFlightPosition(flight) {
        const progress = computeProgress(flight);
        const { lat, lon } = interpolateGreatCircle(
            flight.originLatitude,
            flight.originLongitude,
            flight.destinationLatitude,
            flight.destinationLongitude,
            progress
        );
        const alt = 8000 + Math.sin(progress * Math.PI) * 2000;
        return Cesium.Cartesian3.fromDegrees(lon, lat, alt);
    }

    function formatDuration(seconds) {
        const mins = Math.floor(seconds / 60);
        const hrs = Math.floor(mins / 60);
        const remMins = mins % 60;
        return hrs > 0 ? `${hrs}h ${remMins}m` : `${remMins}m`;
    }

    function computeFuelConsumed(flight, progress) {
        const takeOff = flight.takeOffFuel ?? 0;
        const cruiseFuel = Math.max(0, flight.totalFuel - takeOff);
        return Math.min(flight.totalFuel, takeOff + cruiseFuel * progress);
    }

    function formatFuel(value) {
        return `${value.toFixed(2)} ${config.labels.fuel}`;
    }

    function clearRoute() {
        if (routeEntity) {
            viewer.entities.remove(routeEntity);
            routeEntity = null;
        }
    }

    function drawRoute(flight) {
        clearRoute();
        routeEntity = viewer.entities.add({
            polyline: {
                positions: buildArcPositions(flight),
                width: 3,
                material: Cesium.Color.CORNFLOWERBLUE.withAlpha(0.85),
                arcType: Cesium.ArcType.NONE
            }
        });
    }

    function upsertFlightEntity(flight) {
        const position = getFlightPosition(flight);
        const label = `${flight.originIata} → ${flight.destinationIata}`;
        const isSelected = flight.id === selectedId;

        let entity = flightEntities.get(flight.id);
        if (!entity) {
            entity = viewer.entities.add({
                id: `flight-${flight.id}`,
                position,
                point: {
                    pixelSize: isSelected ? 16 : 12,
                    color: Cesium.Color.GOLD,
                    outlineColor: Cesium.Color.BLACK,
                    outlineWidth: 2
                },
                label: {
                    text: label,
                    font: '13px sans-serif',
                    fillColor: Cesium.Color.WHITE,
                    outlineColor: Cesium.Color.BLACK,
                    outlineWidth: 2,
                    style: Cesium.LabelStyle.FILL_AND_OUTLINE,
                    pixelOffset: new Cesium.Cartesian2(0, -22),
                    show: isSelected
                }
            });
            flightEntities.set(flight.id, entity);
        } else {
            entity.position = position;
            entity.label.text = label;
            entity.label.show = isSelected;
            entity.point.pixelSize = isSelected ? 16 : 12;
        }
    }

    function removeStaleEntities(activeIds) {
        for (const [id, entity] of flightEntities) {
            if (!activeIds.has(id)) {
                viewer.entities.remove(entity);
                flightEntities.delete(id);
            }
        }
    }

    function updateDetails(flight) {
        const progress = computeProgress(flight);
        const remaining = Math.max(0, (new Date(flight.arrivalTime) - Date.now()) / 1000);
        const fuelConsumed = computeFuelConsumed(flight, progress);
        const position = getFlightPosition(flight);
        const cartographic = Cesium.Cartographic.fromCartesian(position);

        document.getElementById('detailAircraft').textContent =
            `${flight.aircraftName} (${flight.aircraftModel})`;
        document.getElementById('detailRoute').textContent =
            `${flight.originIata} → ${flight.destinationIata}`;
        document.getElementById('detailDeparture').textContent =
            new Date(flight.departureTime).toLocaleString();
        document.getElementById('detailArrival').textContent =
            new Date(flight.arrivalTime).toLocaleString();
        document.getElementById('detailRemaining').textContent = formatDuration(remaining);
        document.getElementById('detailProgress').textContent = `${Math.round(progress * 100)}%`;
        document.getElementById('detailFuelConsumed').textContent = formatFuel(fuelConsumed);
        document.getElementById('detailFuelRate').textContent =
            `${flight.fuelBurnRatePerHour.toFixed(2)} ${config.labels.fuel}${config.labels.perHour}`;
        document.getElementById('detailTotalFuel').textContent = formatFuel(flight.totalFuel);

        detailsEl.classList.remove('d-none');
        drawRoute(flight);

        viewer.camera.flyTo({
            destination: Cesium.Cartesian3.fromRadians(
                cartographic.longitude,
                cartographic.latitude,
                1_800_000
            ),
            duration: 1.2
        });
    }

    function renderFlightList() {
        flightListEl.innerHTML = '';
        const hasFlights = flights.length > 0;
        noFlightsEl.classList.toggle('d-none', hasFlights);

        flights.forEach((flight) => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'list-group-item list-group-item-action' +
                (flight.id === selectedId ? ' active' : '');
            btn.innerHTML = `<strong>${flight.originIata} → ${flight.destinationIata}</strong>
                <span class="d-block small ${flight.id === selectedId ? 'text-white-50' : 'text-muted'}">${flight.aircraftName}</span>`;
            btn.addEventListener('click', () => selectFlight(flight.id));
            flightListEl.appendChild(btn);
        });
    }

    function selectFlight(id) {
        selectedId = id;
        renderFlightList();
        const flight = flights.find((f) => f.id === id);
        if (flight) updateDetails(flight);
        flightEntities.forEach((entity, entityId) => {
            const selected = entityId === selectedId;
            entity.label.show = selected;
            entity.point.pixelSize = selected ? 16 : 12;
        });
    }

    async function tryCompleteFlight(flight) {
        if (computeProgress(flight) < 1) return;
        await fetch(`${config.completeFlightUrl}?id=${flight.id}`, { method: 'POST' });
        await loadFlights();
    }

    function refreshEntities() {
        const activeIds = new Set(flights.map((f) => f.id));
        flights.forEach(upsertFlightEntity);
        removeStaleEntities(activeIds);

        if (selectedId) {
            const flight = flights.find((f) => f.id === selectedId);
            if (flight) {
                const progress = computeProgress(flight);
                if (progress >= 1) {
                    tryCompleteFlight(flight).catch(console.error);
                    return;
                }
                document.getElementById('detailRemaining').textContent =
                    formatDuration(Math.max(0, (new Date(flight.arrivalTime) - Date.now()) / 1000));
                document.getElementById('detailProgress').textContent = `${Math.round(progress * 100)}%`;
                document.getElementById('detailFuelConsumed').textContent =
                    formatFuel(computeFuelConsumed(flight, progress));
            }
        }
    }

    async function loadFlights() {
        const response = await fetch(config.activeFlightsUrl);
        if (!response.ok) return;

        flights = await response.json();
        renderFlightList();

        if (!selectedId && flights.length > 0) {
            selectFlight(flights[0].id);
        } else if (selectedId && !flights.find((f) => f.id === selectedId)) {
            selectedId = flights[0]?.id ?? null;
            detailsEl.classList.toggle('d-none', !selectedId);
            clearRoute();
            if (selectedId) selectFlight(selectedId);
        } else {
            refreshEntities();
        }
    }

    initViewer();
    loadFlights().catch(console.error);
    setInterval(() => loadFlights().catch(console.error), 30000);
    setInterval(refreshEntities, 1000);
})();
