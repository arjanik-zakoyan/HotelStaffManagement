// For All Calendars
document.addEventListener('DOMContentLoaded', function () {
    const dateInputs = document.querySelectorAll('input.flatpickr-date, input[type="date"]');
    dateInputs.forEach(function (input) {
        input.setAttribute('type', 'text');
        flatpickr(input, {
            dateFormat: "Y-m-d",
            locale: 'hy',
            allowInput: true,
        });
    });

    const chatToggleBtn = document.getElementById("chatToggleBtn");
    if (chatToggleBtn) {
        chatToggleBtn.addEventListener("click", () => {
            const chatBox = document.getElementById("chatBox");
            chatBox.classList.toggle("d-none");

            if (!chatBox.classList.contains("d-none") && !chatDataLoaded) {
                fetchUsers("Manager", "managersTab");
                fetchUsers("Employee", "employeesTab");
                chatDataLoaded = true;
            }
            if (!chatBox.classList.contains("d-none")) {
                loadUnreadCounts();
            }
        });
    }

    loadUnreadCounts(); 
    startSignalR(); 
});

// ===============================
// Chat Functionality
// ===============================
let currentChatUserId = null;
let chatDataLoaded = false;
let connection = null;

const fetchUsers = async (role, containerId) => {
    const response = await fetch(`/Chat/GetUsers?role=${role}`);
    const data = await response.json();

    const container = document.getElementById(containerId);
    container.innerHTML = "";

    data.forEach(user => {
        const div = document.createElement("div");
        div.className = "user-entry";
        div.dataset.userId = user.userID;
        div.dataset.fullName = user.fullName;
        div.dataset.username = user.username.toLowerCase();
        div.dataset.position = user.position.toLowerCase();

        div.innerHTML = `
              <strong>${user.fullName}</strong><br>
              <small>${user.username} <span class="text-muted">(${user.position})</span></small>
              ${user.unreadCount > 0 ? `<span class="per-user-badge">${user.unreadCount}</span>` : ""}
          `;

        container.appendChild(div);
    });
};

const loadUnreadCounts = async () => {
    const res = await fetch('/Chat/GetUnreadCounts');
    const counts = await res.json();

    let total = 0;

    document.querySelectorAll(".per-user-badge").forEach(b => b.remove());

    counts.forEach(c => {
        total += c.count;
        const userEntry = document.querySelector(`.user-entry[data-user-id='${c.userId}']`);
        if (userEntry) {
            const badge = document.createElement("span");
            badge.className = "per-user-badge";
            badge.textContent = c.count;
            userEntry.appendChild(badge);
        }
    });

    const toggleBadge = document.getElementById("chatToggleBadge");
    if (toggleBadge) {
        toggleBadge.textContent = total;
        toggleBadge.classList.remove("d-none");
        toggleBadge.style.display = total === 0 ? "none" : "flex";
    }

    if (currentChatUserId) {
        const currentBadge = document.querySelector(`.user-entry[data-user-id='${currentChatUserId}'] .per-user-badge`);
        if (currentBadge) currentBadge.remove();
    }
};

window.addEventListener("load", loadUnreadCounts);
window.addEventListener("focus", loadUnreadCounts);

// SignalR Setup for real-time updates
function startSignalR() {
    connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();

    connection.on("ReceiveMessage", (senderId, message, time, isRead) => {
        if (parseInt(senderId) === parseInt(currentChatUserId)) {
            const container = document.getElementById("chatMessages");
            const div = document.createElement("div");
            div.className = "message other";
            div.innerHTML = `
                  <div>${message}</div>
                  <div class="message-meta">${time}</div>
              `;
            container.appendChild(div);
            container.scrollTop = container.scrollHeight;

            fetch(`/Chat/MarkAsRead?senderId=${senderId}`, { method: 'POST' })
                .then(() => {
                    const currentBadge = document.querySelector(`.user-entry[data-user-id='${senderId}'] .per-user-badge`);
                    if (currentBadge) currentBadge.remove();
                    loadUnreadCounts();
                });
        } else {
            loadUnreadCounts();
        }
    });

    connection.start().catch(err => console.error(err));
}


document.querySelectorAll(".chat-header .tab").forEach(tab => {
    tab.addEventListener("click", () => {
        document.querySelectorAll(".chat-header .tab").forEach(t => t.classList.remove("active"));
        tab.classList.add("active");
        const id = tab.dataset.tab;
        document.querySelectorAll(".tab-content").forEach(tc => tc.classList.add("d-none"));
        document.getElementById(id + "Tab").classList.remove("d-none");
    });
});

document.getElementById("chatSearch").addEventListener("input", function () {
    const searchValue = this.value.toLowerCase();
    document.querySelectorAll(".tab-content:not(.d-none) .user-entry").forEach(entry => {
        const match =
            entry.dataset.fullName.toLowerCase().includes(searchValue) ||
            entry.dataset.username.includes(searchValue) ||
            entry.dataset.position.includes(searchValue);
        entry.style.display = match ? "block" : "none";
    });
});

document.addEventListener("click", function (e) {
    const entry = e.target.closest(".user-entry");
    if (entry) {
        const userId = entry.dataset.userId;
        const fullName = entry.dataset.fullName || "Օգտատեր";

        currentChatUserId = userId;

        document.getElementById("chatWindow").classList.remove("d-none");
        document.getElementById("chatReceiverName").textContent = fullName;
        document.getElementById("chatMessages").innerHTML = "<div class='text-muted'>Բեռնվում է...</div>";

        fetch(`/Chat/MarkAsRead?senderId=${userId}`, { method: 'POST' }).then(() => loadUnreadCounts());

        fetch(`/Chat/GetMessages?userId=${userId}`)
            .then(res => res.json())
            .then(messages => {
                const container = document.getElementById("chatMessages");
                container.innerHTML = "";
                let lastDate = "";

                messages.forEach((msg, index) => {
                    const div = document.createElement("div");
                    const msgDate = msg.sentAt.split(' ')[1];
                    const showDate = index === messages.length - 1 || messages[index + 1].sentAt.split(' ')[1] !== msgDate;
                    const checkColor = msg.isRead ? "#3F51B5" : "#212121";
                    const side = msg.isMine ? "mine" : "other";

                    div.className = `message ${side}`;
                    div.innerHTML = `
                        <div>${msg.text}</div>
                        ${msg.isMine && showDate ? `<div class="message-meta">${msg.sentAt}<span class="read-status ${msg.isRead ? 'read' : ''}"> ✔</span></div>` : showDate ? `<div class="message-meta">${msg.sentAt}</div>` : ""}
                    `;

                    container.appendChild(div);
                });
                container.scrollTop = container.scrollHeight;
            });
    }
});

document.getElementById("backToListBtn").addEventListener("click", () => {
    document.getElementById("chatWindow").classList.add("d-none");
    currentChatUserId = null;
});

document.getElementById("sendMessageBtn").addEventListener("click", function () {
    const input = document.getElementById("messageInput");
    const text = input.value.trim();
    if (!text || !currentChatUserId) return;

    if (text.length > 255) {
        showError("Նամակի առավելագույն չափը կարող է լինել 255 սիմվոլ");
        return;
    }
    fetch('/Chat/SendMessage', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ receiverID: currentChatUserId, text: text })
    })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                const container = document.getElementById("chatMessages");
                const div = document.createElement("div");
                div.className = "message mine";
                div.innerHTML = `
                    <div>${text}</div>
                    <div class="message-meta">${result.sentAt}<span class="read-status"> ✔</span></div>
                `;
                container.appendChild(div);
                container.scrollTop = container.scrollHeight;
                input.value = "";
            }
        });
});
function showError(message) {
    const errorDiv = document.getElementById("errorContainer");
    errorDiv.innerText = message;
    errorDiv.style.display = "block";

    setTimeout(() => {
        errorDiv.style.display = "none";
    }, 6000);
}

connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();

connection.on("ReceiveMessage", function (senderId, message, time, isRead) {
    if (parseInt(senderId) === parseInt(currentChatUserId)) {
        const container = document.getElementById("chatMessages");
        const div = document.createElement("div");
        div.className = "message other";
        div.innerHTML = `
            <div>${message}</div>
            <div class="message-meta">${time}</div>
        `;
        container.appendChild(div);
        container.scrollTop = container.scrollHeight;
    } else {
        loadUnreadCounts();
    }
});
connection.on("ReceiveOwnMessage", function (receiverId, message, time, isRead) {
    if (parseInt(receiverId) === parseInt(currentChatUserId)) {
        const container = document.getElementById("chatMessages");
        const div = document.createElement("div");
        div.className = "message mine";
        div.innerHTML = `
            <div>${message}</div>
            <div class="message-meta">${time}<span class="read-status read"> ✔</span></div>
        `;
        container.appendChild(div);
        container.scrollTop = container.scrollHeight;
    }
});

connection.start().then(() => {
    loadUnreadCounts();
}).catch(err => console.error(err));
