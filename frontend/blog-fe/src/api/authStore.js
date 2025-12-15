const API_BASE = import.meta.env.VITE_API_BASE ?? "http://localhost:5099";

export const authStore = {
  getAccess() { return localStorage.getItem("accessToken") || ""; },
  getUser() { try { return JSON.parse(localStorage.getItem("user") || "null"); } catch { return null; } },
  set({ accessToken, user }) {
    localStorage.setItem("accessToken", accessToken);
    localStorage.setItem("user", JSON.stringify(user));
  },
  clear() {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("user");
  }
};

async function rawRequest(path, { method="GET", body, auth=false } = {}) {
  const headers = { "Content-Type": "application/json" };
  if (auth) {
    const t = authStore.getAccess();
    if (t) headers.Authorization = `Bearer ${t}`;
  }

  const res = await fetch(`${API_BASE}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined
  });

  const isJson = (res.headers.get("content-type") || "").includes("application/json");
  const data = isJson ? await res.json().catch(() => null) : await res.text().catch(() => "");

  if (!res.ok) {
    const msg = typeof data === "string" && data ? data : (data?.message || data || `HTTP ${res.status}`);
    const err = new Error(msg);
    err.status = res.status;
    err.data = data;
    throw err;
  }

  return data;
}

export const api = {
  auth: {
    register: (payload) => rawRequest("/api/auth/register", { method:"POST", body: payload }),
    login: (payload) => rawRequest("/api/auth/login", { method:"POST", body: payload }),
  },
  posts: {
    list: ({ search="", page=1, pageSize=10 }={}) =>
      rawRequest(`/api/posts?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`),
    get: (id) => rawRequest(`/api/posts/${id}`),
    create: (payload) => rawRequest("/api/posts", { method:"POST", body: payload, auth:true }),
    update: (id, payload) => rawRequest(`/api/posts/${id}`, { method:"PUT", body: payload, auth:true }),
    del: (id) => rawRequest(`/api/posts/${id}`, { method:"DELETE", auth:true }),
    clear: () => rawRequest("/api/posts/clear", { method:"DELETE", auth:true })
  },
  comments: {
    list: (postId) => rawRequest(`/api/posts/${postId}/comments`),
    create: (postId, payload) => rawRequest(`/api/posts/${postId}/comments`, { method:"POST", body: payload, auth:true }),
    del: (postId, commentId) => rawRequest(`/api/posts/${postId}/comments/${commentId}`, { method:"DELETE", auth:true })
  }
};
