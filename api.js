const API = "https://localhost:7288/api";

/* =========================
   AUTH HELPERS
========================= */

function getToken() {
    return localStorage.getItem("token");
}

/* =========================
   AUTH (LOGIN / REGISTER)
========================= */

async function login() {
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    const res = await fetch(`${API}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password })
    });

    const data = await res.json();

    if (data.token) {
        localStorage.setItem("token", data.token);
        window.location.href = "dashboard.html";
    } else {
        alert("Login failed");
    }
}

async function register() {
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    const res = await fetch(`${API}/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            username: email.split("@")[0],
            email,
            password
        })
    });

    if (res.ok) {
        alert("Registered! Now login.");
    }
}

/* =========================
   JOURNAL DASHBOARD
========================= */

async function loadJournals() {
    const res = await fetch(`${API}/journal`, {
        headers: {
            "Authorization": "Bearer " + getToken()
        }
    });

    const data = await res.json();

    const list = document.getElementById("list");
    list.innerHTML = data.map(renderJournalCard).join("");
}

function renderJournalCard(j) {
    return `
    <div class="glass" style="cursor:pointer;" onclick="openChat(${j.id})">
        <h3>${j.title}</h3>
        <p style="opacity:0.7;">Mood: ${j.mood}</p>
        <p style="opacity:0.5; font-size:14px;">
            ${j.content.substring(0, 80)}...
        </p>
        <span style="opacity:0.4; font-size:12px;">
            Open AI Chat →
        </span>
    </div>
    `;
}

function openChat(id) {
    window.location.href = `chat.html?id=${id}`;
}

async function createJournal() {
    const title = prompt("Title");
    const content = prompt("Content");
    const mood = prompt("Mood");

    await fetch(`${API}/journal`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + getToken()
        },
        body: JSON.stringify({ title, content, mood })
    });

    loadJournals();
}

/* =========================
   CHAT SYSTEM
========================= */

const urlParams = new URLSearchParams(window.location.search);
const journalId = urlParams.get("id");

let messages = [];

async function loadHistory() {
    try {
        const res = await fetch(`${API}/ai/history/${journalId}`, {
            headers: {
                "Authorization": "Bearer " + getToken()
            }
        });

        messages = await res.json();

        renderChat();
    }
    catch (err) {
        console.error(err);
    }
}

async function send() {
    const input = document.getElementById("msg");
    const text = input.value.trim();

    if (!text) return;

    // ADD USER MESSAGE
    messages.push({
        role: "user",
        content: text
    });

    input.value = "";

    renderChat();

    // SHOW TYPING
    showTyping();

    try {

        const res = await fetch(`${API}/ai/chat`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + getToken()
            },
            body: JSON.stringify({
                journalId: parseInt(journalId),
                messages
            })
        });

        const data = await res.json();

        hideTyping();

        // ADD AI MESSAGE
        messages.push({
            role: "ai",
            content: data.response
        });

        renderChat();
    }
    catch (err) {

        hideTyping();

        messages.push({
            role: "ai",
            content: "Something went wrong."
        });

        renderChat();

        console.error(err);
    }
}

function renderChat() {

    const chat = document.getElementById("chat");

    if (!chat) return;

    chat.innerHTML = "";

    messages.forEach(m => {

        const div = document.createElement("div");

        const isUser = m.role === "user";

        div.className = "message";

        div.style.alignSelf =
            isUser ? "flex-end" : "flex-start";

        div.style.maxWidth = "70%";

        div.style.padding = "12px 16px";

        div.style.borderRadius = "18px";

        div.style.margin = "8px 0";

        div.style.lineHeight = "1.5";

        div.style.animation = "fadeIn 0.25s ease";

        div.style.background = isUser
            ? "linear-gradient(135deg, #6d5efc, #4a90e2)"
            : "rgba(255,255,255,0.08)";

        div.innerText = m.content;

        chat.appendChild(div);
    });

    chat.scrollTop = chat.scrollHeight;
}

function showTyping() {

    const chat = document.getElementById("chat");

    const typing = document.createElement("div");

    typing.id = "typing";

    typing.style.alignSelf = "flex-start";

    typing.style.padding = "12px 16px";

    typing.style.borderRadius = "18px";

    typing.style.margin = "8px 0";

    typing.style.background = "rgba(255,255,255,0.08)";

    typing.style.opacity = "0.7";

    typing.innerText = "AI is thinking...";

    chat.appendChild(typing);

    chat.scrollTop = chat.scrollHeight;
}

function hideTyping() {

    const typing = document.getElementById("typing");

    if (typing) {
        typing.remove();
    }
}

/* =========================
   AUTO INIT
========================= */

if (document.getElementById("list")) {
    loadJournals();
}

if (document.getElementById("chat")) {
    loadHistory();
}