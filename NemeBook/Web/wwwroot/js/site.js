// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    const notificationsDropdown = document.getElementById("notificationsDropdown");
    if (!notificationsDropdown) {
        return;
    }

    const notificationsContainer = document.getElementById("notificationsContainer");
    const notificationBadge = document.getElementById("notificationBadge");
    const markAllButton = document.getElementById("markAllNotificationsRead");
    const antiForgeryTokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    const antiForgeryToken = antiForgeryTokenElement ? antiForgeryTokenElement.value : "";

    let notifications = [];

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function formatDate(value) {
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return "";
        }

        return date.toLocaleString();
    }

    function updateBadge() {
        const unreadCount = notifications.filter(n => !n.isRead).length;
        notificationBadge.textContent = unreadCount.toString();
        if (unreadCount > 0) {
            notificationBadge.classList.remove("d-none");
        } else {
            notificationBadge.classList.add("d-none");
        }
    }

    function getNotificationUrl(notification) {
        if (notification.chatId) {
            return `/Chat?chatId=${encodeURIComponent(notification.chatId)}`;
        }

        return null;
    }

    function renderNotifications() {
        if (!notifications.length) {
            notificationsContainer.innerHTML = '<li><span class="dropdown-item-text text-muted small">Няма известия</span></li>';
            updateBadge();
            return;
        }

        const itemsHtml = notifications
            .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
            .map(notification => {
                const unreadClass = notification.isRead ? "" : "notification-item-unread";
                const url = getNotificationUrl(notification);

                if (url) {
                    return `
                        <li><a class="dropdown-item notification-item ${unreadClass}" data-notification-id="${notification.id}" href="${url}">
                            <div>${escapeHtml(notification.text || "")}</div>
                            <div class="notification-item-time">${escapeHtml(formatDate(notification.createdAt))}</div>
                        </a></li>
                    `;
                }

                return `
                    <li><button type="button" class="dropdown-item notification-item ${unreadClass}" data-notification-id="${notification.id}">
                        <div>${escapeHtml(notification.text || "")}</div>
                        <div class="notification-item-time">${escapeHtml(formatDate(notification.createdAt))}</div>
                    </button></li>
                `;
            })
            .join("");

        notificationsContainer.innerHTML = itemsHtml;
        updateBadge();
    }

    async function fetchJson(url, options) {
        const response = await fetch(url, options);
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }

        if (response.status === 204) {
            return null;
        }

        const contentType = response.headers.get("content-type") || "";
        if (contentType.includes("application/json")) {
            return await response.json();
        }

        return null;
    }

    async function loadNotifications() {
        try {
            const recent = await fetchJson("/api/notifications/recent?take=20", { credentials: "same-origin" });
            notifications = Array.isArray(recent) ? recent : [];
            renderNotifications();
        } catch {
            // ignore client-side fetch errors
        }
    }

    async function markAsRead(notificationId) {
        try {
            await fetchJson(`/api/notifications/${notificationId}/read`, {
                method: "POST",
                credentials: "same-origin",
                headers: antiForgeryToken
                    ? { "RequestVerificationToken": antiForgeryToken }
                    : undefined
            });

            notifications = notifications.map(notification => {
                if (notification.id === notificationId) {
                    return { ...notification, isRead: true };
                }

                return notification;
            });

            renderNotifications();
        } catch {
            // ignore client-side fetch errors
        }
    }

    async function markAllAsRead() {
        try {
            await fetchJson("/api/notifications/read-all", {
                method: "POST",
                credentials: "same-origin",
                headers: antiForgeryToken
                    ? { "RequestVerificationToken": antiForgeryToken }
                    : undefined
            });

            notifications = notifications.map(notification => ({ ...notification, isRead: true }));
            renderNotifications();
        } catch {
            // ignore client-side fetch errors
        }
    }

    notificationsContainer.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof Element)) {
            return;
        }

        const item = target.closest("[data-notification-id]");
        if (!item) {
            return;
        }

        const notificationId = item.getAttribute("data-notification-id");
        if (!notificationId) {
            return;
        }

        markAsRead(notificationId);
    });

    if (markAllButton) {
        markAllButton.addEventListener("click", (event) => {
            event.preventDefault();
            markAllAsRead();
        });
    }

    async function startSignalR() {
        if (typeof signalR === "undefined") {
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .withAutomaticReconnect()
            .build();

        connection.on("notificationReceived", (notification) => {
            if (!notification || !notification.id) {
                return;
            }

            notifications = [notification, ...notifications.filter(existing => existing.id !== notification.id)].slice(0, 50);
            renderNotifications();
        });

        try {
            await connection.start();
        } catch {
            // ignore client-side SignalR startup errors
        }
    }

    loadNotifications();
    startSignalR();
})();

(function () {
    const openButtons = Array.from(document.querySelectorAll("[data-nb-modal-open]"));
    const modals = Array.from(document.querySelectorAll("[data-nb-modal]"));
    let activeTrigger = null;

    function closeModal(modal) {
        if (!modal) {
            return;
        }

        modal.hidden = true;
        modal.classList.remove("is-visible");
        document.body.classList.remove("nb-modal-open");

        if (activeTrigger) {
            activeTrigger.focus();
            activeTrigger = null;
        }
    }

    function openModal(id, trigger) {
        const modal = document.querySelector(`[data-nb-modal="${id}"]`);
        if (!modal) {
            return;
        }

        activeTrigger = trigger;
        modal.hidden = false;
        modal.classList.add("is-visible");
        document.body.classList.add("nb-modal-open");
        modal.querySelector(".nb-modal")?.focus();
    }

    openButtons.forEach((button) => {
        button.addEventListener("click", (event) => {
            event.preventDefault();
            openModal(button.dataset.nbModalOpen, button);
        });
    });

    modals.forEach((modal) => {
        modal.addEventListener("click", (event) => {
            if (event.target === modal || event.target.closest("[data-nb-modal-close]")) {
                closeModal(modal);
            }
        });
    });

    document.addEventListener("keydown", (event) => {
        if (event.key !== "Escape") {
            return;
        }

        closeModal(document.querySelector("[data-nb-modal]:not([hidden])"));
    });
})();

(function () {
    const notificationButton = document.querySelector("[data-notification-open]");
    if (!notificationButton || window.NemeBookNotificationsInitialized) {
        return;
    }

    window.NemeBookNotificationsInitialized = true;

    const token = document.querySelector(".nb-notification-token-form input[name='__RequestVerificationToken']")?.value;
    const gradesUrl = "/Student/MyGrades";
    let notifications = [];

    function ensureNotificationModal() {
        let panel = document.querySelector('[data-nb-modal="student-notifications-modal"]');
        if (panel) {
            return panel;
        }

        panel = document.createElement("div");
        panel.className = "nb-modal-backdrop";
        panel.dataset.nbModal = "student-notifications-modal";
        panel.hidden = true;
        panel.innerHTML = `
            <section class="nb-modal" role="dialog" aria-modal="true" aria-labelledby="student-notifications-title" tabindex="-1">
                <header class="nb-modal-header">
                    <div>
                        <h2 id="student-notifications-title">Известия</h2>
                        <p data-notification-summary>Непрочетени известия</p>
                    </div>
                    <div class="nb-notification-modal-actions">
                        <button class="nb-notification-clear" type="button" data-notification-clear-all>
                            <i class="bi bi-check2-all" aria-hidden="true"></i>
                            <span>Изчисти всички</span>
                        </button>
                        <button class="nb-modal-close" type="button" data-nb-modal-close aria-label="Затвори">
                            <i class="bi bi-x-lg" aria-hidden="true"></i>
                        </button>
                    </div>
                </header>
                <div class="nb-modal-body">
                    <div class="nb-notification-list" data-notification-list></div>
                    <div class="nb-modal-empty" data-notification-empty hidden>Няма нови известия.</div>
                </div>
            </section>`;

        document.body.appendChild(panel);
        panel.addEventListener("click", (event) => {
            if (event.target === panel || event.target.closest("[data-nb-modal-close]")) {
                closeNotificationModal();
            }
        });

        return panel;
    }

    const notificationPanel = ensureNotificationModal();
    const notificationBadge = document.querySelector("[data-notification-count]");
    const notificationList = notificationPanel.querySelector("[data-notification-list]");
    const notificationEmpty = notificationPanel.querySelector("[data-notification-empty]");
    const notificationSummary = notificationPanel.querySelector("[data-notification-summary]");
    const clearAllButton = notificationPanel.querySelector("[data-notification-clear-all]");

    if (!notificationBadge || !notificationList || !notificationEmpty || !clearAllButton) {
        return;
    }

    function readValue(notification, key) {
        return notification[key] ?? notification[key.charAt(0).toUpperCase() + key.slice(1)];
    }

    function getTypeLabel(type) {
        if (type === 1 || type === "Grade") {
            return "Оценка";
        }

        if (type === 2 || type === "Absence") {
            return "Отсъствие";
        }

        if (type === 3 || type === "Feedback") {
            return "Отзив";
        }

        if (type === 4 || type === "Message") {
            return "Съобщение";
        }

        return "Известие";
    }

    function getIconClass(notification) {
        const type = readValue(notification, "type");
        if (type === 1 || type === "Grade") {
            return "bi bi-mortarboard-fill";
        }

        if (type === 2 || type === "Absence") {
            return "bi bi-calendar-x-fill";
        }

        if (type === 3 || type === "Feedback") {
            return "bi bi-chat-square-text-fill";
        }

        if (type === 4 || type === "Message") {
            return "bi bi-chat-dots-fill";
        }

        return "bi bi-bell-fill";
    }

    function isGradeNotification(notification) {
        const type = readValue(notification, "type");
        return type === 1 || type === "Grade";
    }

    function formatDate(value) {
        if (!value) {
            return "";
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return "";
        }

        return date.toLocaleString("bg-BG", {
            day: "2-digit",
            month: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        });
    }

    function updateBadge(count) {
        notificationBadge.textContent = count > 99 ? "99+" : count.toString();
        notificationBadge.hidden = count === 0;
    }

    function updateSummary(count) {
        if (notificationSummary) {
            notificationSummary.textContent = count === 1
                ? "1 непрочетено известие"
                : `${count} непрочетени известия`;
        }

        clearAllButton.disabled = count === 0;
    }

    function renderNotifications(items) {
        notificationList.replaceChildren();

        items.forEach((notification) => {
            notificationList.appendChild(createNotificationItem(notification));
        });

        notificationEmpty.hidden = items.length > 0;
        updateSummary(items.length);
    }

    function createNotificationItem(notification) {
        const id = readValue(notification, "id");
        const type = readValue(notification, "type");
        const text = readValue(notification, "text") || "Ново известие";
        const createdAt = readValue(notification, "createdAt");

        const item = document.createElement("button");
        item.className = "nb-notification-item";
        item.type = "button";
        item.dataset.notificationId = id;

        const icon = document.createElement("span");
        icon.className = "nb-notification-icon";
        icon.setAttribute("aria-hidden", "true");

        const iconGlyph = document.createElement("i");
        iconGlyph.className = getIconClass(notification);
        icon.appendChild(iconGlyph);

        const copy = document.createElement("span");
        copy.className = "nb-notification-copy";

        const title = document.createElement("strong");
        title.textContent = text;

        const typeLabel = document.createElement("span");
        typeLabel.textContent = getTypeLabel(type);

        const date = document.createElement("span");
        date.className = "nb-notification-date";
        date.textContent = formatDate(createdAt);

        copy.append(title, typeLabel);
        item.append(icon, copy, date);

        item.addEventListener("click", async () => {
            item.disabled = true;

            const response = await markAsRead(id);
            if (!response.ok) {
                item.disabled = false;
                return;
            }

            const gradeId = readValue(notification, "gradeId");
            await loadUnreadNotifications();

            if (isGradeNotification(notification) && gradeId) {
                window.location.href = `${gradesUrl}?gradeId=${encodeURIComponent(gradeId)}`;
            }
        });

        return item;
    }

    async function fetchJson(url, options = {}) {
        const response = await fetch(url, {
            credentials: "same-origin",
            headers: {
                "Accept": "application/json",
                ...(options.headers || {})
            },
            ...options
        });

        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }

        return await response.json();
    }

    function markAsRead(id) {
        return fetch(`/notifications/${id}/read`, {
            method: "POST",
            credentials: "same-origin",
            headers: {
                "RequestVerificationToken": token || ""
            }
        });
    }

    async function loadUnreadNotifications() {
        try {
            notifications = await fetchJson("/notifications/unread?take=50");
            renderNotifications(notifications);
            updateBadge(notifications.length);
        } catch {
            notifications = [];
            renderNotifications(notifications);
            updateBadge(0);
        }
    }

    async function refreshUnreadCount() {
        try {
            const result = await fetchJson("/notifications/unread-count");
            updateBadge(result.count ?? result.Count ?? 0);
        } catch {
            updateBadge(0);
        }
    }

    function openNotificationModal() {
        notificationPanel.hidden = false;
        notificationPanel.classList.add("is-visible");
        document.body.classList.add("nb-modal-open");
        notificationPanel.querySelector(".nb-modal")?.focus();
        void loadUnreadNotifications();
    }

    function closeNotificationModal() {
        notificationPanel.hidden = true;
        notificationPanel.classList.remove("is-visible");
        document.body.classList.remove("nb-modal-open");
        notificationButton.focus();
    }

    async function startLiveNotifications() {
        if (!window.signalR) {
            return;
        }

        const connection = new window.signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .withAutomaticReconnect()
            .build();

        connection.on("notificationReceived", (notification) => {
            if (!notification || !readValue(notification, "id")) {
                return;
            }

            notifications = [
                notification,
                ...notifications.filter((current) => readValue(current, "id") !== readValue(notification, "id"))
            ].slice(0, 50);

            if (!notificationPanel.hidden) {
                renderNotifications(notifications);
            }

            void refreshUnreadCount();
        });

        try {
            await connection.start();
        } catch {
            // Notifications still refresh through normal HTTP calls.
        }
    }

    notificationButton.addEventListener("click", (event) => {
        event.preventDefault();
        openNotificationModal();
    });

    clearAllButton.addEventListener("click", async () => {
        clearAllButton.disabled = true;

        const response = await fetch("/notifications/read-all", {
            method: "POST",
            credentials: "same-origin",
            headers: {
                "RequestVerificationToken": token || ""
            }
        });

        if (!response.ok) {
            clearAllButton.disabled = false;
            return;
        }

        await loadUnreadNotifications();
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && !notificationPanel.hidden) {
            closeNotificationModal();
        }
    });

    void refreshUnreadCount();
    void startLiveNotifications();
})();
