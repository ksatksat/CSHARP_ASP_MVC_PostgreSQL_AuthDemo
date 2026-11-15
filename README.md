# AuthDemo – ASP.NET Core + plain HTML + JWT + PostgreSQL

## Run
1. Install .NET 8 SDK and PostgreSQL.
2. Edit `ConnectionStrings:Default` and `Jwt:Key` in `appsettings.json` (32 symbol long password).
3. `dotnet run`  -> open the URL printed in the console (DB + table are created automatically).

## Make yourself Admin (to test authorization)
    UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'you@example.com';
Then log out and log in again (the role lives inside the token).

## API
POST   /api/auth/register   {name,email,password}
POST   /api/auth/login      {email,password}
GET    /api/account/me            (Bearer token)
DELETE /api/account               (Bearer token)
GET    /api/account/admin-only    (Bearer token + Admin role)
