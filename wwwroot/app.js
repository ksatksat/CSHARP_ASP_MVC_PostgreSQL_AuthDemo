const $ = (id) => document.getElementById(id);
const TOKEN_KEY = "token";

// sessionStorage: cleared when the tab closes. NOT sent automatically (unlike cookies).
const getToken = () => sessionStorage.getItem(TOKEN_KEY);
const setToken = (t) => t ? sessionStorage.setItem(TOKEN_KEY, t) : sessionStorage.removeItem(TOKEN_KEY);

function show(text, isError = false) {
  const el = $("msg");
  el.textContent = text;           // textContent (not innerHTML) => no XSS
  el.className = "msg" + (isError ? " error" : "");
  el.hidden = !text;
}

// Every request manually attaches the token in the Authorization header
async function api(url, method = "GET", body) {
  const headers = { "Content-Type": "application/json" };
  if (getToken()) headers["Authorization"] = "Bearer " + getToken();
  const res = await fetch(url, { method, headers, body: body ? JSON.stringify(body) : undefined });
  const data = res.status === 204 ? null : await res.json().catch(() => null);
  if (!res.ok) {
    if (res.status === 403) throw new Error("403 Forbidden: you don't have permission.");
    const validation = data?.errors && Object.values(data.errors).flat().join(" ");
    throw new Error(data?.error || validation || `Request failed (${res.status})`);
  }
  return data;
}

function render(user) {
  $("guest").hidden = !!user;
  $("user").hidden = !user;
  if (user) {
    $("u-name").textContent = user.name;
    $("u-email").textContent = user.email;
    $("u-role").textContent = user.role;
  }
}

async function loadMe() {
  if (!getToken()) return render(null);
  try { render(await api("/api/account/me")); }
  catch { setToken(null); render(null); }   // expired/invalid token
}

// Tabs
$("tab-login").onclick = () => { $("login-form").hidden = false; $("register-form").hidden = true;
  $("tab-login").classList.add("active"); $("tab-register").classList.remove("active"); show(""); };
$("tab-register").onclick = () => { $("login-form").hidden = true; $("register-form").hidden = false;
  $("tab-register").classList.add("active"); $("tab-login").classList.remove("active"); show(""); };

async function submitAuth(e, url) {
  e.preventDefault();
  try {
    const data = await api(url, "POST", Object.fromEntries(new FormData(e.target)));
    setToken(data.token);
    e.target.reset();
    show("Welcome!");
    await loadMe();
  } catch (err) { show(err.message, true); }
}
$("login-form").onsubmit = (e) => submitAuth(e, "/api/auth/login");
$("register-form").onsubmit = (e) => submitAuth(e, "/api/auth/register");

$("btn-logout").onclick = () => { setToken(null); render(null); show("Logged out."); };

$("btn-delete").onclick = async () => {
  if (!confirm("Delete your account permanently?")) return;
  try {
    await api("/api/account", "DELETE");
    setToken(null); render(null); show("Account deleted.");
  } catch (err) { show(err.message, true); }
};

$("btn-admin").onclick = async () => {
  try { show((await api("/api/account/admin-only")).message); }
  catch (err) { show(err.message, true); }
};

loadMe();
