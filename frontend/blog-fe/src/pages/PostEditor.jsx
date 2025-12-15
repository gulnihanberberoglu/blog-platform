import React, { useEffect, useState } from "react";
import { api } from "../api/authStore";

export function PostEditor({ mode, id, onCancel, onSaved }) {
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [loading, setLoading] = useState(mode === "edit");
  const [err, setErr] = useState("");

  useEffect(() => {
    if (mode !== "edit" || !id) return;
    (async () => {
      setErr("");
      setLoading(true);
      try {
        const p = await api.posts.get(id);
        setTitle(p.title);
        setContent(p.content);
      } catch (e) {
        setErr(e.message);
      } finally {
        setLoading(false);
      }
    })();
  }, [mode, id]);

  const save = async () => {
    setErr("");
    try {
      if (!title.trim() || !content.trim()) {
        setErr("Title ve Content zorunlu.");
        return;
      }
      if (mode === "create") {
        const res = await api.posts.create({ title, content });
        onSaved?.(res.id);
      } else {
        await api.posts.update(id, { title, content });
        onSaved?.(id);
      }
    } catch (e) {
      setErr(e.message);
    }
  };

  return (
    <div className="gm-card gm-card--wide">
      <div className="gm-row gm-row--space">
        <div>
          <h2>{mode === "create" ? "New Post" : "Edit Post"}</h2>
        </div>
        <div className="gm-row">
          <button className="gm-btn gm-btn--ghost" onClick={onCancel}>Cancel</button>
          <button className="gm-btn" onClick={save}>Save</button>
        </div>
      </div>

      {loading && <div className="gm-skeleton">Loading…</div>}
      {err && <div className="gm-alert">{err}</div>}

      {!loading && (
        <div className="gm-form">
          <label className="gm-label">Title</label>
          <input className="gm-input" value={title} onChange={e=>setTitle(e.target.value)} />

          <label className="gm-label">Content</label>
          <textarea className="gm-textarea" rows={14} value={content} onChange={e=>setContent(e.target.value)} />
        </div>
      )}
    </div>
  );
}
