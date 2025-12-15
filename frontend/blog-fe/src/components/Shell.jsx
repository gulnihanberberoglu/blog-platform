import React from "react";

export function Shell({ children, me, isAuthed, onLogin, onHome, onNew, onLogout }) {
  return (
    <div className="gm-app">
      <header className="gm-header">
        <div className="gm-brand" role="button" tabIndex={0} onClick={onHome}>
          <span className="gm-badge">🅱</span>
          <div>
            <div className="gm-title">Blog Platform </div>
            <div className="gm-subtitle">Tech CMS • Posts / Comments / Users</div>
          </div>
        </div>

        <div className="gm-actions">
          {isAuthed ? (
            <>
              <div className="gm-user">👤 {me?.displayName ?? "User"}</div>
              <button className="gm-btn" onClick={onNew}>New Post</button>
              <button className="gm-btn gm-btn--ghost" onClick={onLogout}>Logout</button>
            </>
          ) : (
            <button className="gm-btn" onClick={onLogin}>Login</button>
          )}
        </div>
      </header>

      <main className="gm-main">
        {children}
      </main>
    </div>
  );
}
