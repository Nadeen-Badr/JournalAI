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

async function send() {
    const input = document.getElementById("msg");
    const text = input.value;

    if (!text) return;

    messages.push({ role: "user", content: text });
    input.value = "";

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

    messages.push({ role: "ai", content: data.response });

    renderChat();
}

function renderChat() {
    const chat = document.getElementById("chat");
    chat.innerHTML = "";

    messages.forEach(m => {
        const div = document.createElement("div");

        const isUser = m.role === "user";

        div.style.alignSelf = isUser ? "flex-end" : "flex-start";
        div.style.maxWidth = "70%";
        div.style.padding = "10px 14px";
        div.style.borderRadius = "14px";
        div.style.margin = "5px 0";
        div.style.background = isUser
            ? "linear-gradient(135deg, #6d5efc, #4a90e2)"
            : "rgba(255,255,255,0.07)";

        div.innerText = m.content;

        chat.appendChild(div);
    });

    chat.scrollTop = chat.scrollHeight;
}

/* =========================
   AUTO INIT
========================= */

if (document.getElementById("list")) {
    loadJournals();
}