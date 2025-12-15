# Blog Platform API

Minimal, sade ve öğretici bir **Blog Platform** projesi.  
Post / Comment / User akışını **JWT Authentication**, **EF Core (SQLite)** ve temiz API tasarımıyla ele alır.

Bu repo öğrenme ve backend pratikleri amacıyla hazırlanmıştır.

---

## Kullanılan Teknolojiler

### Backend
- .NET 9 / C# 12
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- JWT Authentication
- Swagger (OpenAPI)

### Frontend
- React
- Vite
- Fetch API

---

## Proje Yapısı

### Backend

```
backend/
 └─ Blog.Api
    ├─ Controllers
    │  ├─ AuthController.cs
    │  ├─ PostsController.cs
    │  └─ CommentsController.cs
    ├─ Data
    │  └─ AppDbContext.cs
    ├─ Models
    ├─ Services
    │  ├─ JwtService.cs
    │  └─ PasswordService.cs
    └─ Program.cs
```

### Frontend

```
frontend/
 ├─ src
 │  ├─ api
 │  │  └─ client.js
 │  ├─ pages
 │  │  ├─ Login.jsx
 │  │  ├─ Posts.jsx
 │  │  └─ PostDetail.jsx
 │  ├─ components
 │  │  ├─ PostList.jsx
 │  │  ├─ CommentList.jsx
 │  │  └─ Header.jsx
 │  ├─ styles
 │  │  └─ main.css
 │  ├─ App.jsx
 │  └─ main.jsx
 └─ index.html
```

---

## Kimlik Doğrulama (JWT)

- JWT tabanlı authentication kullanılır
- Token header üzerinden gönderilir:

```
Authorization: Bearer {token}
```

- Yetkili endpoint’ler `[Authorize]` ile korunur

### Demo Kullanıcı

```
Email : demo@ghost.local
Şifre : Demo123!
```

---

## İş Kuralları

- Kullanıcı sadece **kendi postunu**:
  - güncelleyebilir
  - silebilir
- Kullanıcı sadece **kendi yorumunu** silebilir
- Post silinirse:
  - bağlı yorumlar otomatik silinir (**Cascade**)
- Kullanıcı silinirse:
  - post ve yorumlar silinmez (**Restrict**)

---

## Veritabanı

- SQLite kullanılır
- Veritabanı dosyası:

```
backend/Blog.Api/App_Data/blog.db
```

- İlk çalıştırmada:
  - Veritabanı otomatik oluşturulur
  - Demo kullanıcı ve örnek post eklenir

> Demo amaçlı `EnsureCreated` kullanılmıştır.  
> Gerçek projelerde **Migration** önerilir.

---

## API Endpoint’leri

### Auth
- POST `/api/auth/register`
- POST `/api/auth/login`

### Posts
- GET    `/api/posts`
- GET    `/api/posts/{id}`
- POST   `/api/posts`
- PUT    `/api/posts/{id}`
- DELETE `/api/posts/{id}`
- DELETE `/api/posts/clear`

### Comments
- GET    `/api/posts/{postId}/comments`
- POST   `/api/posts/{postId}/comments`
- DELETE `/api/posts/{postId}/comments/{commentId}`

---

## Swagger

```
http://localhost:5099/swagger
```

JWT ile test etmek için:
1. `/api/auth/login`
2. Dönen `accessToken`’ı kopyala
3. Swagger → **Authorize** → `Bearer {token}`

---

## Projeyi Çalıştırma

### Backend
```
cd backend/Blog.Api
dotnet run
```

### Frontend
```
cd frontend
npm install
npm run dev
```

---

