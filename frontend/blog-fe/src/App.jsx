import React, { useEffect, useMemo, useState } from "react";
import { api } from "./api/client";
import { authStore } from "./api/authStore";
import { Shell } from "./components/Shell";
import { LoginCard } from "./pages/LoginCard";
import { PostEditor } from "./pages/PostEditor";
import { PostList } from "./pages/PostList";
import { PostView } from "./pages/PostView";

export default function App() {
  const [route, setRoute] = useState({ name: "list", id: null });
  const [me, setMe] = useState(authStore.getUser());
  const [refreshKey, setRefreshKey] = useState(0);

  const isAuthed = !!authStore.getAccess();

  useEffect(() => {
    const onStorage = () => setMe(authStore.getUser());
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const go = (name, id = null) => setRoute({ name, id });

  const topActions = useMemo(() => {
    return {
      onNew: () => go("new"),
      onHome: () => go("list"),
      onLogout: () => { authStore.clear(); setMe(null); go("list"); setRefreshKey(k => k + 1); }
    };
  }, []);

  return (
    <Shell
      me={me}
      isAuthed={isAuthed}
      onLogin={() => go("login")}
      onHome={topActions.onHome}
      onNew={topActions.onNew}
      onLogout={topActions.onLogout}
    >
      {route.name === "login" && (
        <LoginCard
          onSuccess={(user) => { setMe(user); go("list"); setRefreshKey(k => k + 1); }}
        />
      )}

      {route.name === "new" && (
        <PostEditor
          mode="create"
          onCancel={() => go("list")}
          onSaved={(id) => { go("view", id); setRefreshKey(k => k + 1); }}
        />
      )}

      {route.name === "edit" && (
        <PostEditor
          mode="edit"
          id={route.id}
          onCancel={() => go("view", route.id)}
          onSaved={(id) => { go("view", id); setRefreshKey(k => k + 1); }}
        />
      )}

      {route.name === "view" && (
        <PostView
          key={`view-${route.id}-${refreshKey}`}
          id={route.id}
          onBack={() => go("list")}
          onEdit={() => go("edit", route.id)}
          onDeleted={() => { go("list"); setRefreshKey(k => k + 1); }}
        />
      )}

      {route.name === "list" && (
        <PostList
          key={`list-${refreshKey}`}
          onOpen={(id) => go("view", id)}
          onLogin={() => go("login")}
          onNew={() => go("new")}
          onRefetch={() => setRefreshKey(k => k + 1)}
        />
      )}
    </Shell>
  );
}
