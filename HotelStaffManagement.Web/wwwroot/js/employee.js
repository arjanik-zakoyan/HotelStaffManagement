// Notification
document.addEventListener("DOMContentLoaded", () => {
    if ("Notification" in window && Notification.permission !== "granted") {
        Notification.requestPermission().then(permission => {
            if (permission === "granted") {
                console.log("Ծանուցումները միացված են։");
            }
        });
    }
});
function showPushNotification(title, message) {
    if (Notification.permission === "granted") {
        new Notification(title, {
            body: message,
            icon: "/images/notification-icon.png"
        });
    }
}
// Unread
document.addEventListener("DOMContentLoaded", () => {
    if ("Notification" in window && Notification.permission !== "granted") {
        Notification.requestPermission();
    }

    const latestUnread = @Html.Raw(Json.Serialize(unread));
    if (latestUnread && Notification.permission === "granted") {
        new Notification(latestUnread.title, {
            body: latestUnread.message,
            icon: "/images/notification-icon.png"
        });
    }
});