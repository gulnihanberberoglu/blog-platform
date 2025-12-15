import React, { useEffect, useMemo, useState } from "react";
import { api, authStore } from "../api/authStore";

export function PostList({ onOpen, onLogin, onNew, onRefetch }) {
  const [search, setSearch] = useState("");
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const isAuthed = !!authStore.getAccess();

  const fetchList = async () => {
    setErr("");
    setLoading(true);
    try {
      const res = await api.posts.list({ search, page: 1, pageSize: 20 });
      setData(res);
    } catch (e) {
      setErr(e.message || "Liste alınamadı.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchList(); }, []); // first load

  const onSearch = () => fetchList();

  const onClear = async () => {
    if (!isAuthed) return onLogin?.();
    if (!confirm("Kendi postlarının tamamı silinsin mi?")) return;
    try {
      await api.posts.clear();
      onRefetch?.();
      await fetchList();
    } catch (e) {
      alert(e.message);
    }
  };

  return (
    <div className="gm-grid">
      <div className="gm-card gm-card--wide">
        <div className="gm-row gm-row--space">
          <div>
            <h2>Posts</h2>
            <div className="gm-muted">Arama, silme ve temizle destekli minimal CMS listesi.</div>
          </div>

          <div className="gm-row">
            <input
              className="gm-input gm-input--search"
              placeholder="Search title/content…"
              value={search}
              onChange={e=>setSearch(e.target.value)}
              onKeyDown={e=> e.key === "Enter" && onSearch()}
            />
            <button className="gm-btn gm-btn--ghost" onClick={onSearch}>Search</button>
            <button className="gm-btn gm-btn--danger" onClick={onClear}>Clear</button>
            <button className="gm-btn" onClick={() => isAuthed ? onNew?.() : onLogin?.()}>New</button>
          </div>
        </div>

        {loading && <div className="gm-skeleton">Loading…</div>}
        {err && <div className="gm-alert">{err}</div>}

        {!loading && data?.items?.length === 0 && (
          <div className="gm-empty">Henüz post yok.</div>
        )}

        <div className="gm-list">
          {data?.items?.map(p => (
            <div key={p.id} className="gm-item" onClick={() => onOpen?.(p.id)} role="button" tabIndex={0}>
              <div className="gm-item__title">{p.title}</div>
              <div className="gm-item__meta">
                <span>✍️ {p.author.displayName}</span>
                <span>💬 {p.commentCount}</span>
                <span>🕒 {new Date(p.updatedAtUtc).toLocaleString()}</span>
              </div>
              <div className="gm-item__excerpt">{p.excerpt}</div>
            </div>
          ))}
        </div>
      </div>

      <aside className="gm-card">
        <h3>Quick</h3>
        <ul className="gm-ul">
          <li><span className="gm-kbd">Search</span> title/content</li>
          <li><span className="gm-kbd">Clear</span> my posts (auth)</li>
          <li><span className="gm-kbd">New</span> create post (auth)</li>
          <li>Demo login: <b>demo@ghost.local</b> / Password: <b> Demo123!</b></li>
        </ul>
      </aside>
    </div>
  );
}
