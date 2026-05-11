# DepEd SDO Accounting — Digital Receiving Logbook
## ASP.NET Core MVC Web Application
## FOR Local host only(Same Wifi Server)

### 🔐 Login Credentials
| Field    | Value            |
|----------|-----------------|
| Username | `AccountingUnit` |
| Password | `Accounting2026` |

---

### 🛠 Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (v17.8+) **or** VS Code with C# extension

---

### 🚀 Running the Application

#### Option A — Visual Studio 2022
1. Open `DepEdLogbook.sln`
2. Press **F5** or click **IIS Express** / **https**
3. Navigate to `https://localhost:7183` (or the port shown)
4. Log in with credentials above

#### Option B — .NET CLI
```bash
cd DepEdLogbook
dotnet restore
dotnet run
```
Then open `http://localhost:5232` in your browser.

---

### 📦 NuGet Packages Required
The project will automatically restore on first build. Packages used:
- `Microsoft.EntityFrameworkCore` 8.0.0
- `Microsoft.EntityFrameworkCore.Sqlite` 8.0.0
- `Microsoft.EntityFrameworkCore.Tools` 8.0.0
- `Microsoft.AspNetCore.Authentication.Cookies` 2.2.0

---

### 🗄 Database
- Uses **SQLite** (`depedlogbook.db`) — auto-created on first run
- No migrations needed; `EnsureCreated()` handles schema
- Database file stored in the project root

---

### ✨ Features
- 🔐 Cookie-based authentication (single user)
- 📋 Create, Read, Update, Delete logbook entries
- 📄 Multiple document rows per entry (Doc Code, Particulars, Remarks)
- 🏢 Department dropdown + free-text input (combobox)
- 🔍 Date-range and department filters
- 📄 Pagination (10 records per page)
- 🕵️ Audit Trail (logs all login/logout/CRUD actions)
- 📊 Stats dashboard (Total Entries, Today's, Departments, Documents)
- 🎨 DepEd color scheme: `#003087` blue + `#FDB913` gold + `#C8102E` red

---

### 🏗 Project Structure
```
DepEdLogbook/
├── Controllers/
│   ├── AccountController.cs   (Login/Logout)
│   └── HomeController.cs      (CRUD + Audit Trail)
├── Data/
│   └── AppDbContext.cs        (EF Core DbContext)
├── Models/
│   ├── LogbookEntry.cs        (Domain models)
│   └── ViewModels.cs          (View models)
├── Views/
│   ├── Account/Login.cshtml
│   ├── Home/Index.cshtml      (Dashboard + logbook)
│   ├── Home/Edit.cshtml
│   ├── Home/AuditTrail.cshtml
│   └── Shared/_Layout.cshtml
├── wwwroot/css/site.css
├── Program.cs
└── appsettings.json
```

---

*© 2026 Department of Education — Schools Division Office, Accounting Office*
