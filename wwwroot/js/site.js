// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ---------- Toast dismiss / auto-dismiss ----------
(function () {
    var AUTO_DISMISS_MS = 4400;

    function dismissToast(toastEl) {
        if (!toastEl || toastEl.dataset.dismissed === "true") {
            return;
        }
        toastEl.dataset.dismissed = "true";
        toastEl.classList.add("toast-hide");
        window.setTimeout(function () {
            toastEl.remove();
        }, 200);
    }

    function initToasts() {
        var toasts = document.querySelectorAll("[data-fo-toast]");
        toasts.forEach(function (toastEl) {
            var closeBtn = toastEl.querySelector("[data-fo-toast-close]");
            if (closeBtn) {
                closeBtn.addEventListener("click", function () {
                    dismissToast(toastEl);
                });
            }
            window.setTimeout(function () {
                dismissToast(toastEl);
            }, AUTO_DISMISS_MS);
        });
    }

    document.addEventListener("DOMContentLoaded", initToasts);
})();

// ---------- List table search + pagination ----------
(function () {
    var DEFAULT_PAGE_SIZE = 10;
    var SEARCH_DEBOUNCE_MS = 250;

    function debounce(fn, delayMs) {
        var timer = null;
        return function () {
            var args = arguments;
            var context = this;
            window.clearTimeout(timer);
            timer = window.setTimeout(function () {
                fn.apply(context, args);
            }, delayMs);
        };
    }

    function normalize(text) {
        return text.trim().toLowerCase();
    }

    function formatSummary(template, start, end, total) {
        return template
            .replace("{0}", start)
            .replace("{1}", end)
            .replace("{2}", total);
    }

    function initListSearch(container) {
        var input = container.querySelector("[data-fo-list-search-input]");
        var table = container.querySelector("table");
        var emptyMsg = container.querySelector("[data-fo-list-search-empty]");
        var paginationNav = container.querySelector("[data-fo-list-pagination]");
        if (!input || !table) {
            return;
        }

        var rows = Array.from(table.querySelectorAll("tbody tr"));
        var statusButtons = Array.from(container.querySelectorAll("[data-fo-status-filter]"));
        var paginationSummary = paginationNav
            ? paginationNav.querySelector("[data-fo-pagination-summary]")
            : null;
        var paginationPages = paginationNav
            ? paginationNav.querySelector("[data-fo-pagination-pages]")
            : null;
        var pageSize = parseInt(container.getAttribute("data-fo-page-size"), 10) || DEFAULT_PAGE_SIZE;
        var activeStatus = "all";
        var currentPage = 1;

        function rowMatchesFilter(row) {
            var query = normalize(input.value);
            var textMatch = !query || normalize(row.textContent).indexOf(query) !== -1;
            var statusMatch = activeStatus === "all"
                || row.getAttribute("data-fo-list-status") === activeStatus;
            return textMatch && statusMatch;
        }

        function renderPaginationControls(totalItems, totalPages) {
            if (!paginationNav || !paginationSummary || !paginationPages) {
                return;
            }

            if (totalItems === 0) {
                paginationNav.classList.add("d-none");
                return;
            }

            paginationNav.classList.remove("d-none");

            var start = ((currentPage - 1) * pageSize) + 1;
            var end = Math.min(currentPage * pageSize, totalItems);
            var summaryTemplate = paginationNav.getAttribute("data-fo-summary-template") || "";
            paginationSummary.textContent = formatSummary(summaryTemplate, start, end, totalItems);

            paginationPages.innerHTML = "";

            if (totalPages <= 1) {
                return;
            }

            var prevLabel = paginationNav.getAttribute("data-fo-label-prev") || "Previous";
            var nextLabel = paginationNav.getAttribute("data-fo-label-next") || "Next";

            paginationPages.appendChild(createPageItem("prev", currentPage - 1, prevLabel, currentPage === 1));
            for (var page = 1; page <= totalPages; page++) {
                paginationPages.appendChild(createPageItem("page", page, String(page), false, page === currentPage));
            }
            paginationPages.appendChild(createPageItem("next", currentPage + 1, nextLabel, currentPage === totalPages));
        }

        function createPageItem(type, page, label, disabled, active) {
            var item = document.createElement("li");
            item.className = "page-item" + (disabled ? " disabled" : "") + (active ? " active" : "");

            var button = document.createElement("button");
            button.type = "button";
            button.className = "page-link";
            button.textContent = label;
            button.setAttribute("data-fo-page", String(page));

            if (type === "prev") {
                button.setAttribute("aria-label", label);
            } else if (type === "next") {
                button.setAttribute("aria-label", label);
            } else {
                button.setAttribute("aria-label", "Page " + page);
                button.setAttribute("aria-current", active ? "page" : "false");
            }

            if (!disabled) {
                button.addEventListener("click", function () {
                    currentPage = page;
                    refreshList();
                });
            }

            item.appendChild(button);
            return item;
        }

        function refreshList() {
            var query = normalize(input.value);
            var matchingRows = rows.filter(rowMatchesFilter);
            var totalItems = matchingRows.length;
            var totalPages = Math.max(1, Math.ceil(totalItems / pageSize));

            if (currentPage > totalPages) {
                currentPage = totalPages;
            }

            rows.forEach(function (row) {
                row.classList.add("d-none");
            });

            matchingRows.forEach(function (row, index) {
                var page = Math.floor(index / pageSize) + 1;
                if (page === currentPage) {
                    row.classList.remove("d-none");
                }
            });

            if (emptyMsg) {
                var hasActiveFilter = query.length > 0 || activeStatus !== "all";
                var showEmpty = hasActiveFilter && totalItems === 0;
                emptyMsg.classList.toggle("d-none", !showEmpty);
            }

            renderPaginationControls(totalItems, totalPages);
        }

        var debouncedRefresh = debounce(function () {
            currentPage = 1;
            refreshList();
        }, SEARCH_DEBOUNCE_MS);

        input.addEventListener("input", debouncedRefresh);

        statusButtons.forEach(function (button) {
            button.addEventListener("click", function () {
                activeStatus = button.getAttribute("data-fo-status-filter") || "all";
                statusButtons.forEach(function (item) {
                    item.classList.toggle("is-active", item === button);
                });
                currentPage = 1;
                refreshList();
            });
        });

        refreshList();
    }

    function initAllListSearch() {
        document.querySelectorAll("[data-fo-list-search]").forEach(initListSearch);
    }

    document.addEventListener("DOMContentLoaded", initAllListSearch);
})();

// ---------- Browser timezone + UTC datetime formatting ----------
(function () {
    var COOKIE_MAX_AGE_SECONDS = 31536000;

    function getLocale() {
        return document.documentElement.lang || undefined;
    }

    function setTimezoneOffsetCookie() {
        var offsetMinutes = String(new Date().getTimezoneOffset());
        document.cookie = "fo_tz_offset=" + offsetMinutes
            + ";path=/;max-age=" + COOKIE_MAX_AGE_SECONDS
            + ";SameSite=Lax";
    }

    function formatUtcDateTimes() {
        var locale = getLocale();
        var timeFormatter = new Intl.DateTimeFormat(locale, {
            hour: "2-digit",
            minute: "2-digit"
        });
        var dateFormatter = new Intl.DateTimeFormat(locale, {
            day: "numeric",
            month: "short",
            year: "numeric"
        });

        document.querySelectorAll("[data-fo-utc-datetime]").forEach(function (element) {
            var isoValue = element.getAttribute("datetime");
            if (!isoValue) {
                return;
            }

            var utcDate = new Date(isoValue);
            if (Number.isNaN(utcDate.getTime())) {
                return;
            }

            var primary = element.querySelector("[data-fo-datetime-primary]");
            var secondary = element.querySelector("[data-fo-datetime-secondary]");
            if (primary && secondary) {
                primary.textContent = timeFormatter.format(utcDate);
                secondary.textContent = dateFormatter.format(utcDate);
                return;
            }

            element.textContent = new Intl.DateTimeFormat(locale, {
                dateStyle: "medium",
                timeStyle: "short"
            }).format(utcDate);
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        setTimezoneOffsetCookie();
        formatUtcDateTimes();
    });
})();

// ---------- Navbar UTC clock ----------
(function () {
    function pad(value) {
        return String(value).padStart(2, "0");
    }

    function formatUtcClock(date) {
        return pad(date.getUTCHours()) + ":"
            + pad(date.getUTCMinutes()) + ":"
            + pad(date.getUTCSeconds()) + " UTC";
    }

    function updateUtcClock() {
        var clock = document.querySelector("[data-fo-utc-clock]");
        var timeEl = clock ? clock.querySelector("[data-fo-utc-time]") : null;
        if (!timeEl) {
            return;
        }

        var now = new Date();
        timeEl.textContent = formatUtcClock(now);
        timeEl.setAttribute("datetime", now.toISOString());
    }

    document.addEventListener("DOMContentLoaded", function () {
        updateUtcClock();
        window.setInterval(updateUtcClock, 1000);
    });
})();
