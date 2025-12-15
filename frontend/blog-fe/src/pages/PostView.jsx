import React, { useEffect, useState } from "react";
import { api, authStore } from "../api/authStore";

export function PostView({ id, onBack, onEdit, onDeleted }) {
  const [post, setPost] = useState(null);
  const [comments, setComments] = useState([]);
  const [body, setBody] = useState("");
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const me = authStore.getUser();
  const isAuthed = !!authStore.getAccess();

  const load = async () => {
    setErr("");
    setLoading(true);
    try {
      const p = await api.posts.get(id);
      const c = await api.comments.list(id);
      setPost(p);
      setComments(c);
    } catch (e) {
      setErr(e.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [id]);

  const delPost = async () => {
    if (!confirm("Post silinsin mi?")) return;
    try {
      await api.posts.del(id);
      onDeleted?.();
    } catch (e) {
      alert(e.message);
    }
  };

  const addComment = async () => {
    if (!isAuthed) return alert("Yorum için login ol.");
    if (!body.trim()) return;
    try {
      await api.comments.create(id, { body });
      setBody("");
      await load();
    } catch (e) {
      alert(e.message);
    }
  };

  const delComment = async (commentId) => {
    if (!confirm("Yorum silinsin mi?")) return;
    try {
      await api.comments.del(id, commentId);
      await load();
    } catch (e) {
      alert(e.message);
    }
  };

  const isOwner = post && me && (post.author?.id === me.id);

  return (
    <div className="gm-card gm-card--wide">
      <div className="gm-row gm-row--space">
        <button className="gm-btn gm-btn--ghost" onClick={onBack}>← Back</button>

        <div className="gm-row">
          {isOwner && (
            <>
              <button className="gm-btn gm-btn--ghost" onClick={onEdit}>Edit</button>
              <button className="gm-btn gm-btn--danger" onClick={delPost}>Delete</button>
            </>
          )}
        </div>
      </div>

      {loading && <div className="gm-skeleton">Loading…</div>}
      {err && <div className="gm-alert">{err}</div>}

      {!loading && post && (
        <>
          <h1 className="gm-h1">{post.title}</h1>
          <div className="gm-muted">
            ✍️ {post.author.displayName} • 🕒 {new Date(post.updatedAtUtc).toLocaleString()}
          </div>

          <article className="gm-article">
            {post.content.split("\n").map((line, idx) => (
              <p key={idx}>{line}</p>
            ))}
          </article>

          <hr className="gm-hr" />

          <h3>Comments</h3>

          {isAuthed && (
            <div className="gm-row">
              <input
                className="gm-input"
                placeholder="Write a comment…"
                value={body}
                onChange={e=>setBody(e.target.value)}
                onKeyDown={e => e.key === "Enter" && addComment()}
              />
              <button className="gm-btn" onClick={addComment}>Send</button>
            </div>
          )}

          <div className="gm-comments">
            {comments.length === 0 && <div className="gm-empty">Henüz yorum yok.</div>}
            {comments.map(c => {
              const canDel = me && (c.author.id === me.id);
              return (
                <div key={c.id} className="gm-comment">
                  <div className="gm-row gm-row--space">
                    <div className="gm-comment__meta">
                      <b>{c.author.displayName}</b> • {new Date(c.createdAtUtc).toLocaleString()}
                    </div>
                    {canDel && (
                      <button className="gm-link" onClick={() => delComment(c.id)}>Delete</button>
                    )}
                  </div>
                  <div className="gm-comment__body">{c.body}</div>
                </div>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}
