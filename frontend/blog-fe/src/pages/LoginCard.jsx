import React, { useState } from "react";
import { api, authStore } from "../api/authStore";

export function LoginCard({ onSuccess }) {
  const [mode, setMode] = useState("login"); // login | register
  const [email, setEmail] = useState("demo@ghost.local");
  const [displayName, setDisplayName] = useState("Demo");
  const [password, setPassword] = useState("Demo123!");
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState("");

  const submit = async () => {
    setErr("");
    setLoading(true);
    try {
      const payload = mode === "login"
        ? { email, password }
        : { email, displayName, password };

      const res = mode === "login"
        ? await api.auth.login(payload)
        : await api.auth.register(payload);

      authStore.set({ accessToken: res.accessToken, user: res.user });
      onSuccess?.(res.user);
    } catch (e) {
      setErr(e.message || "İşlem başarısız.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="gm-card">
      <div className="gm-card__head">
        <h2>{mode === "login" ? "Login" : "Register"}</h2>
      </div>

      <div className="gm-form">
        <label className="gm-label">Email</label>
        <input className="gm-input" value={email} onChange={e=>setEmail(e.target.value)} />

        {mode === "register" && (
          <>
            <label className="gm-label">Display Name</label>
            <input className="gm-input" value={displayName} onChange={e=>setDisplayName(e.target.value)} />
          </>
        )}

        <label className="gm-label">Password</label>
        <input className="gm-input" type="password" value={password} onChange={e=>setPassword(e.target.value)} />

        {err && <div className="gm-alert">{err}</div>}

        <button className="gm-btn gm-btn--wide" onClick={submit} disabled={loading}>
          {loading ? "..." : (mode === "login" ? "Login" : "Create account")}
        </button>

        <button className="gm-link" onClick={() => setMode(m => m === "login" ? "register" : "login")}>
          {mode === "login" ? "Hesabın yok mu? Register" : "Zaten hesabın var mı? Login"}
        </button>
      </div>
    </div>
  );
}
