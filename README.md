# Reem Library — API

ASP.NET Core Web API for the Reem Digital Library project.

## Stack

- ASP.NET Core 10 · EF Core · SQLite
- JWT Authentication · SignalR Realtime
- Repository + Service layers · Swagger (dev)

## Run locally

```bash
dotnet restore
dotnet run
```

- API: http://localhost:5080
- Swagger: http://localhost:5080/swagger
- Health: http://localhost:5080/api/public/health

## Default login (seeded)

| Email | Password |
|-------|----------|
| admin@elibrary.com | Admin@123 |

## Environment variables (production)

| Variable | Description |
|----------|-------------|
| `PORT` | HTTP port (set by Render) |
| `Jwt__Key` | JWT signing key (32+ chars) |
| `Cors__Origins` | Comma-separated frontend URLs |
| `ConnectionStrings__Default` | SQLite path or connection string |

## Deploy on Render

1. Push this folder to GitHub repo `reem-library-api`
2. [Render](https://render.com) → New → Blueprint → connect repo
3. Set `Cors__Origins` to your Netlify admin + site URLs
4. After deploy, update `js/config.js` in admin & site with the API URL

> **Note:** SQLite on free hosting resets on redeploy. Fine for portfolio demo.

## Related projects

- **Admin dashboard:** `../admin` — static SPA
- **Public site:** `../site` — static site
