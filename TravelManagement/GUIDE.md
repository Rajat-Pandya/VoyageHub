# Building a CRUD API in .NET — Travel Management System

### A step-by-step guide for freshers

We will build **one complete feature** of the Travel Management System:
**Travel Requests**. An employee creates a request (destination, dates, purpose, estimated budget),
can view it, edit it and delete it. That is **CRUD**:

| Letter | Means | Example in our project |
|---|---|---|
| **C** | Create | Employee creates a new travel request |
| **R** | Read | See all requests, or one request |
| **U** | Update | Change the destination or dates |
| **D** | Delete | Remove a request |

**How to use this guide:** go top to bottom. Every step builds on the previous one, and every new
idea is explained the first time it appears (look for the **"C# concept"** and **".NET concept"** boxes).
At the end of most steps there is a **Checkpoint** so you know things are working before moving on.

---

## Contents

- **Part 0 — Before we start:** tools, HTTP basics, the big picture
- **Part 1 — Create the solution** (Step 1)
- **Part 2 — `Program.cs`: where the application starts** (Step 2)
- **Part 3 — The Data layer:** Entity, `AppDbContext`, connection string, migrations (Steps 3–7)
- **Part 4 — The Repository** (Steps 8–10)
- **Part 5 — The Business layer (Service)** (Steps 11–13)
- **Part 6 — The API layer:** DTOs, Mapper (AutoMapper), Controller (Steps 14–18)
- **Part 7 — Run and test** (Step 19)
- **Part 8 — The big picture again**
- **Part 9 — Common errors and how to fix them**
- **Appendix:** cheat sheets, glossary, trainer notes

---

# Part 0 — Before we start

## 0.1 The tools and what each one is

| Name | What it is (in one line) |
|---|---|
| **C#** | The programming language we write in. |
| **.NET** | The platform that runs C# code (like the JVM for Java). We use **.NET 8**. |
| **ASP.NET Core** | The part of .NET for building web applications and web **APIs**. |
| **Web API** | A program that waits for HTTP requests and answers with data (JSON), not web pages. |
| **Entity Framework Core (EF Core)** | A library that lets us work with database tables as C# classes, so we don't write SQL by hand. This kind of library is called an **ORM** (Object-Relational Mapper). |
| **SQL Server** | The database where the data is actually stored. |
| **Visual Studio** | The editor (IDE) we write, build and run code in. |
| **NuGet** | The "app store" for .NET libraries. EF Core, AutoMapper and Swagger come from NuGet. |
| **AutoMapper** | A library that copies values from one class to another (e.g. DTO → Entity) automatically, matching properties by name. |
| **Swagger** | A web page generated automatically from our API, where we can click buttons to test it. |

## 0.2 HTTP in two minutes

Every time a client (a browser, a mobile app, Postman, Swagger) talks to our API, it sends an
**HTTP request** and gets back an **HTTP response**.

A **request** has:
- a **verb** (what you want to do): `GET`, `POST`, `PUT`, `DELETE`
- a **URL** (what you want to do it to): `https://localhost:7201/api/TravelRequest/5`
- optionally a **body** (data you send), written in **JSON**:
  ```json
  { "destination": "Mumbai", "estimatedBudget": 45000 }
  ```

A **response** has:
- a **status code** (did it work?): `200`, `201`, `204`, `400`, `404`, `500`
- optionally a **body** (data sent back), also JSON

The verbs map onto CRUD:

| CRUD | HTTP verb | URL | Meaning |
|---|---|---|---|
| Create | `POST` | `/api/TravelRequest` | "Here is a new request, save it" |
| Read all | `GET` | `/api/TravelRequest` | "Give me all requests" |
| Read one | `GET` | `/api/TravelRequest/5` | "Give me request number 5" |
| Update | `PUT` | `/api/TravelRequest/5` | "Replace request 5 with this data" |
| Delete | `DELETE` | `/api/TravelRequest/5` | "Remove request 5" |

Status codes we will use:

| Code | Name | Meaning |
|---|---|---|
| **200** | OK | Worked, here is the data |
| **201** | Created | A new record was created |
| **204** | No Content | Worked, nothing to send back (used after delete) |
| **400** | Bad Request | Your data is wrong (e.g. missing destination) |
| **404** | Not Found | That id does not exist |
| **500** | Internal Server Error | Our code crashed (a bug on our side) |

## 0.3 The big picture — a restaurant

Our API is split into **layers**. Each layer has one job and only talks to the layer directly below it.
Think of a restaurant:

| Restaurant | Our code | Job |
|---|---|---|
| Customer | **Client** (Swagger, Postman, an Angular app) | Places an order |
| Order slip / the plated dish | **DTOs** (Input DTO / Response DTO) | The order slip lists only what a customer *may* ask for; the plate shows only what they *should* see (not the kitchen's notes) |
| Waiter | **Controller** | Takes the order, checks it's readable, brings the result back |
| Waiter copying the slip into a kitchen ticket, and plating the dish | **Mapper** (AutoMapper) | Converts the order slip ↔ kitchen format, so the chef only ever works with kitchen tickets |
| Chef | **Service** (Business layer) | Applies the recipe = business rules |
| Storekeeper | **Repository** | Fetches and stores ingredients. Doesn't care about recipes |
| Storekeeper's logbook | **DbContext** | Records what was taken / added, and has the key to the storeroom |
| Storeroom | **SQL Server database** | Where everything is actually kept |
| Manager who assigns staff | **Dependency Injection container** | Makes sure every waiter has a chef, every chef has a storekeeper |
| Opening checklist | **`Program.cs`** | Hire staff, open the doors, start serving |

The waiter never goes into the storeroom. The chef never talks to the customer. That separation is
the whole point of layers: each piece is simple, and you can change one without breaking the others.

```
  Client  ──HTTP──►  Controller  ──►  Service  ──►  Repository  ──►  DbContext  ──►  SQL Server
  (JSON = DTOs)     (TMS.Api)        (TMS.Business)  (TMS.Data)      (TMS.Data)
                    + Mapper
                    DTO ⇄ Entity
```

## 0.4 What we will build — the final folder structure

```
TravelManagement/
├── TMS.sln                                  ← the "solution": a container for the 3 projects
│
├── TMS.Api/                                 ← API layer (HTTP)            — built in Part 2 and Part 6
│   ├── Program.cs                               app startup + wiring
│   ├── appsettings.json                         settings (connection string)
│   ├── Dtos/
│   │   ├── TravelRequestInputDto.cs             JSON the client SENDS (create / update)
│   │   └── TravelRequestResponseDto.cs          JSON the API SENDS BACK
│   ├── Mapping/MapperProfile.cs                 AutoMapper rules: DTO ⇄ Entity
│   └── Controllers/TravelRequestController.cs   the 5 endpoints
│
├── TMS.Business/                            ← Business layer (rules)      — built in Part 5
│   └── Services/
│       ├── ITravelRequestService.cs
│       └── TravelRequestService.cs
│
└── TMS.Data/                                ← Data layer (database)       — built in Part 3 and Part 4
    ├── Entity/TravelRequest.cs                  the table
    ├── AppDbContext.cs                          EF Core's connection to the DB
    └── Repository/
        ├── IRepository.cs
        └── Repository.cs
```

---

# Part 1 — Create the solution

## Step 1: Create one solution with three projects

> **.NET concept — Solution vs Project**
> A **project** (`.csproj`) is one unit of code that builds into one `.dll` (a compiled library) or `.exe`.
> A **solution** (`.sln`) is just a folder-like container that groups several projects so Visual Studio
> can open them together. We use **3 projects = 3 layers**, so the layers are physically separated:
> the Data project *cannot* accidentally use controller code, because it doesn't even reference it.

**In Visual Studio:**

1. **File → New → Project → "Blank Solution"**. Name: `TMS`. Location: wherever you like.
2. Right-click the solution → **Add → New Project → "ASP.NET Core Web API"**.
   - Name: `TMS.Api`
   - Framework: **.NET 8.0**
   - Authentication: **None**
   - ✔ Configure for HTTPS
   - ✔ Enable OpenAPI support (this gives us Swagger)
   - ✔ Use controllers
3. Right-click the solution → **Add → New Project → "Class Library"**. Name: `TMS.Business`, .NET 8.0.
4. Same again: **Class Library** named `TMS.Data`, .NET 8.0.
5. **Delete the sample files** we don't need:
   - `Class1.cs` in `TMS.Business` and `TMS.Data`
   - `WeatherForecast.cs` and `Controllers/WeatherForecastController.cs` in `TMS.Api`

> **.NET concept — Web API project vs Class Library**
> `TMS.Api` is a **Web API** project: it can *run* (it starts a web server).
> `TMS.Business` and `TMS.Data` are **Class Libraries**: they can't run on their own. They are code that
> the API project *uses*.

**Add the project references** (who is allowed to use whom):

6. Right-click `TMS.Business` → **Add → Project Reference** → tick `TMS.Data`.
7. Right-click `TMS.Api` → **Add → Project Reference** → tick `TMS.Business` and `TMS.Data`.

```
TMS.Api  ──uses──►  TMS.Business  ──uses──►  TMS.Data
   └──────────────────uses────────────────────►  TMS.Data
```

References only point **downwards**. `TMS.Data` uses nothing of ours. This is what stops the
"storekeeper" from ever calling the "waiter".

> Why does `TMS.Api` also reference `TMS.Data` directly? Only because `Program.cs` (in `TMS.Api`) is where
> we register `AppDbContext` and the Repository (Step 6 and Step 10). The controller itself never uses them.

✅ **Checkpoint:** **Build → Build Solution** (`Ctrl+Shift+B`). It should say *Build succeeded*.

---

# Part 2 — `Program.cs`: where the application starts

## Step 2: Understand `Program.cs`

`Program.cs` is the **first code that runs** when you start the API. It is the "opening checklist" of
the restaurant. Everything else we build will be **plugged in here**, so we learn it first.

This is what Visual Studio generated for you:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

> **C# concept — top-level statements**
> Normally C# code must live inside a `class` with a `Main` method. In `Program.cs`, .NET lets us skip that
> and write statements directly. The compiler wraps them in a hidden `Main` for us. So this file just runs
> from the first line to the last.

> **C# concept — `var`**
> `var builder = ...` means "create a variable called `builder`; the compiler works out its type from
> the right-hand side". It is still strongly typed. `builder` is a `WebApplicationBuilder`; we just
> didn't have to type that.

The file has **two halves**, split by `builder.Build()`:

```
┌────────────────────────────────────────────┐
│ 1. REGISTER SERVICES  (before Build)       │  "Hire the staff"
│    builder.Services.Add...                 │
├────────────────────────────────────────────┤
│ var app = builder.Build();                 │  "Open the restaurant"
├────────────────────────────────────────────┤
│ 2. CONFIGURE THE PIPELINE  (after Build)   │  "Decide what happens to every customer
│    app.Use... / app.Map...                 │   who walks through the door"
├────────────────────────────────────────────┤
│ app.Run();                                 │  "Start serving. Wait for requests forever."
└────────────────────────────────────────────┘
```

### Line by line

**`var builder = WebApplication.CreateBuilder(args);`**
Creates a *builder*: an object that collects everything the app needs before it starts. It
automatically:
- reads **`appsettings.json`** into `builder.Configuration` (we'll put the DB connection string there),
- prepares **`builder.Services`**, the list of services (explained below),
- knows the **environment** (`Development` on your machine, `Production` on a server).

`args` are command-line arguments passed when the app starts. You can ignore them.

### Half 1 — Registering services

> **.NET concept — Dependency Injection (DI)**
> Our controller will need a service. The service will need a repository. The repository will need a
> `DbContext`. We could create each one ourselves with `new`, but then every class would have to know how
> to build everything below it.
>
> Instead, .NET has a **DI container**: a registry where we write down, once, *"if anybody needs X, give
> them Y"*. Then, when a class says in its constructor *"I need X"*, the container **creates and hands
> over** a Y automatically. We never write `new` for our services.
>
> `builder.Services` is that registry. Every `builder.Services.Add...(...)` line is one entry in it.

| Line | What it registers |
|---|---|
| `builder.Services.AddControllers();` | Everything needed for **controllers**: finding controller classes, reading JSON into C# objects, turning results back into JSON, validation. |
| `builder.Services.AddEndpointsApiExplorer();` | Lets the app **list its own endpoints** (URLs + verbs). Swagger needs this list. |
| `builder.Services.AddSwaggerGen();` | The **Swagger generator**: builds a description of our API from that list. |

Later in this guide we will add **four more registrations** to this half:

| Added in | Line | Meaning |
|---|---|---|
| Step 6 | `AddDbContext<AppDbContext>(...)` | "Here is how to connect to the database" |
| Step 10 | `AddScoped(typeof(IRepository<>), typeof(Repository<>))` | "If someone needs a repository, give them ours" |
| Step 13 | `AddScoped<ITravelRequestService, TravelRequestService>()` | "If someone needs the travel service, give them ours" |
| Step 16 | `AddAutoMapper(cfg => { }, typeof(MapperProfile))` | "If someone needs a mapper, give them one that knows our mapping rules" |

**`var app = builder.Build();`**
Takes everything registered so far and builds the actual application. After this line you can't
register more services.

### Half 2 — The request pipeline

> **.NET concept — Middleware and the pipeline**
> Every HTTP request passes through a **chain of steps** before it reaches our controller, and the response
> passes back through the same chain. Each step is called **middleware**. Think of it as the restaurant
> door → security check → host → table. The order of `app.Use...` lines **is** the order of that chain.

| Line | What it does to every request |
|---|---|
| `if (app.Environment.IsDevelopment())` | Only on a developer machine… |
| `app.UseSwagger();` | …serve the API description as JSON at `/swagger/v1/swagger.json` |
| `app.UseSwaggerUI();` | …serve the clickable test page at `/swagger` |
| `app.UseHttpsRedirection();` | If someone calls `http://`, redirect them to the secure `https://` |
| `app.UseAuthorization();` | Checks permissions (`[Authorize]`). We have none yet, but it's ready for later |
| `app.MapControllers();` | Looks at all controller classes and their `[Route]`/`[HttpGet]` attributes, and connects each **URL + verb** to the right **method** |

**`app.Run();`**
Starts the web server and **waits for requests forever** (until you stop it). Any code after this line
would never run.

### The two settings files next to `Program.cs`

**`appsettings.json`**: settings that can change without changing code. `CreateBuilder` reads it
automatically. In Step 6 we add the database connection string here.

**`Properties/launchSettings.json`**: only used when you press **F5** in Visual Studio. It says which
URL to use (e.g. `https://localhost:7201`), to open the browser at `swagger`, and to set
`ASPNETCORE_ENVIRONMENT=Development`. That last setting is why `IsDevelopment()` is true on your machine.

✅ **Checkpoint:** press **F5**. A browser opens on the Swagger page. It says *"No operations defined in
spec!"* because we deleted the sample controller. That's correct: the engine is running, it just has no
endpoints yet.

---

# Part 3 — The Data layer

We start at the **bottom** (the database) and build upwards. A controller is useless without data, so
data comes first.

## Step 3: Install the EF Core packages (NuGet)

Right-click the project → **Manage NuGet Packages → Browse**, search, pick version **8.0.26**, Install.

| Package | Install into | Why |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | `TMS.Data` **and** `TMS.Api` | EF Core itself + the ability to talk to **SQL Server** |
| `Microsoft.EntityFrameworkCore.Tools` | `TMS.Data` **and** `TMS.Api` | Adds the **migration commands** (`Add-Migration`, `Update-Database`) to Visual Studio |

> Why both projects? `TMS.Data` contains the EF Core code. `TMS.Api` is the project that *runs*, and the
> migration tools need EF Core in the running (startup) project to read its settings.
> `TMS.Business` needs no packages; it only uses our own `TMS.Data` code.
> (Swagger's package `Swashbuckle.AspNetCore` was already added to `TMS.Api` by the template.)

## Step 4: The Entity — `TMS.Data/Entity/TravelRequest.cs`

An **entity** is a C# class that represents **one database table**. Each **property** becomes a
**column**. Each **object** of this class is one **row**.

Create a folder `Entity` in `TMS.Data`, then add a class `TravelRequest`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.Data.Entity
{
    public class TravelRequest
    {
        [Key]
        public int TravelRequestId { get; set; }

        [Required, MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required, MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedBudget { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedOn { get; set; }
    }
}
```

> **C# concept — `namespace` and `using`**
> A **namespace** is like a folder name for code: the full name of this class is
> `TMS.Data.Entity.TravelRequest`. Two classes can have the same name if they're in different namespaces.
> **`using X;`** at the top means "I want to use classes from namespace X without typing the full name".
> We need `System.ComponentModel.DataAnnotations` because that's where `[Key]`, `[Required]` and
> `[MaxLength]` live.

> **C# concept — `class`**
> A **class** is a blueprint. `TravelRequest` describes *what a travel request looks like*. Each actual
> request (Asha's trip to Mumbai, Ravi's trip to Pune) is an **object** made from that blueprint.
> `public` means other projects are allowed to use it.

> **C# concept — properties `{ get; set; }`**
> `public string Destination { get; set; }` is a **property**: a value stored in the object that others can
> read (`get`) and change (`set`). Common types:
> `int` = whole number, `string` = text, `decimal` = exact number for money,
> `DateTime` = date and time, `bool` = true/false.

> **C# concept — default values `= string.Empty;`**
> Our projects have **nullable reference types** turned on (`<Nullable>enable</Nullable>` in the `.csproj`).
> That makes the compiler warn us whenever a `string` *might* be `null` (= no value at all). Writing
> `= string.Empty` gives it a starting value of `""`, so it is never null. `Status = "Draft"` works the same
> way: every new `TravelRequest` object starts as `"Draft"`.

> **C# concept — attributes `[ ... ]`**
> The things in square brackets are **attributes**: labels that add extra information to the next line.
> They don't do anything by themselves. Other code (here EF Core) reads them and reacts.

How EF Core reads each attribute when it creates the table:

| C# | Database column |
|---|---|
| `[Key] public int TravelRequestId` | **Primary key**. Because it is an `int`, SQL Server makes it **IDENTITY** (auto-numbered 1, 2, 3…). We never set it ourselves. |
| `[Required]` | `NOT NULL`: the column must always have a value |
| `[MaxLength(100)]` | `nvarchar(100)`: text up to 100 characters (without it: `nvarchar(max)`) |
| `[Column(TypeName = "decimal(18,2)")]` | Exact SQL type: up to 18 digits, 2 after the decimal point (money!) |
| `DateTime` with no attribute | `datetime2 NOT NULL`. Value types like `int`, `decimal`, `DateTime` can never be null, so they are automatically `NOT NULL` |

> **Why `decimal` and not `double` for money?** `double` stores numbers in binary, so `0.1 + 0.2` becomes
> `0.30000000000000004`. `decimal` stores them exactly. Always use `decimal` for money.

## Step 5: The DbContext — `TMS.Data/AppDbContext.cs`

The entity describes *one table*. The **DbContext** is the class that represents *the whole database
connection* and lists which tables exist.

```csharp
using Microsoft.EntityFrameworkCore;
using TMS.Data.Entity;

namespace TMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TravelRequest> TravelRequests { get; set; }
    }
}
```

> **C# concept — inheritance `: DbContext`**
> `class AppDbContext : DbContext` means *AppDbContext **is a** DbContext, plus whatever we add*.
> `DbContext` is written by Microsoft and already knows how to connect, run queries and save data.
> We **inherit** all of that for free and only add our tables.

> **C# concept — constructor**
> A **constructor** is a special method with the **same name as the class** and no return type. It runs
> once, when an object is created, to give it what it needs.
> `public AppDbContext(DbContextOptions<AppDbContext> options)` says: *"to create me, you must give me
> options"*. The options contain **which database type** (SQL Server) and **which connection string**.
> Who gives them? The DI container, using what we write in `Program.cs` in Step 6.
>
> `: base(options)` passes those options straight up to the parent `DbContext` constructor, because
> that's the class that actually uses them. `{ }` is empty because we have nothing else to do.

**`public DbSet<TravelRequest> TravelRequests { get; set; }`**
A `DbSet<T>` represents **one table**. This line tells EF Core: *"there is a table of `TravelRequest`
rows, called `TravelRequests`"*. The property name becomes the table name.
When we add more features (expenses, itineraries), we add one `DbSet` line per table here.

### What does a DbContext actually do? (important for later)

1. **Connection**: opens and closes the connection to SQL Server for us.
2. **Translation**: turns C# calls into SQL (`ToListAsync()` → `SELECT ...`).
3. **Change tracking**: it remembers every object it has loaded or been given, and what happened to it:

| State | Meaning | What `SaveChanges` will do |
|---|---|---|
| `Added` | New object, not in the DB yet | `INSERT` |
| `Unchanged` | Loaded from the DB, not touched | nothing |
| `Modified` | Loaded, then a property was changed | `UPDATE` |
| `Deleted` | Marked for removal | `DELETE` |

Nothing goes to the database until we call **`SaveChangesAsync()`**. Then EF Core looks at all the
states and runs all the SQL **in one transaction**: either everything succeeds, or nothing is saved.
Remember this. It explains how the Repository works in Part 4.

## Step 6: Connect to the database (connection string + `Program.cs`)

### 6a. The connection string — `TMS.Api/appsettings.json`

Add a `ConnectionStrings` section:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TravelDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

A **connection string** is the "address + login" for the database, as `key=value` pairs separated by `;`:

| Part | Meaning |
|---|---|
| `Server=localhost\\SQLEXPRESS` | Computer name + SQL Server instance name. **Change this to your own.** (`\\` is how you write one `\` inside JSON.) In SSMS, the "Server name" on the login screen is exactly this value. |
| `Database=TravelDB` | Database name. It does **not** need to exist yet; Step 7 creates it. |
| `Trusted_Connection=True` | Log in with your **Windows account**, so no username or password is written in the file. |
| `MultipleActiveResultSets=true` | Allows more than one query open at once on the same connection. |
| `TrustServerCertificate=True` | Accept the local server's self-made security certificate. Needed on dev machines. |

### 6b. Register the DbContext — `TMS.Api/Program.cs`

Add this line in **Half 1** (after `AddSwaggerGen()`, before `builder.Build()`), plus two `using` lines at
the top of the file:

```csharp
using Microsoft.EntityFrameworkCore;
using TMS.Data;

// ...

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Read it from the inside out:

| Piece | Meaning |
|---|---|
| `builder.Configuration` | Everything that was read from `appsettings.json` |
| `.GetConnectionString("DefaultConnection")` | Shortcut for: go into `"ConnectionStrings"`, return the value of `"DefaultConnection"` |
| `options.UseSqlServer(...)` | "The database is **SQL Server**, and here is its address" |
| `AddDbContext<AppDbContext>(...)` | Register `AppDbContext` in the DI container. Whenever someone needs one, build it with these options. Those options are exactly what arrive in the `AppDbContext` constructor from Step 5. |

> **C# concept — generics `<AppDbContext>`**
> The `<...>` after a method or class name is a **type parameter**: *"this method works for any type; this
> time, use `AppDbContext`"*. `DbSet<TravelRequest>` = "a table **of** TravelRequest". You'll see `<T>` a lot.

> **C# concept — lambda `options => ...`**
> `options => options.UseSqlServer(...)` is a **lambda**: a tiny function without a name.
> Read `=>` as *"goes to"*: "given `options`, call `UseSqlServer` on it". We hand this little function to
> `AddDbContext`, and .NET calls it later, each time it needs to configure a new `AppDbContext`.

> **.NET concept — service lifetime "Scoped"**
> `AddDbContext` registers `AppDbContext` as **Scoped**: **one new `AppDbContext` per HTTP request**,
> thrown away when the request ends. So two users calling the API at the same time never share the same
> change tracker. We'll use the same lifetime for the Repository and Service in Steps 10 and 13.

✅ **Checkpoint:** build the solution. It should succeed.

## Step 7: Migrations — creating the database from our C# code

We have a C# class (`TravelRequest`) but **no table** yet. We will **not** write `CREATE TABLE` by hand.
EF Core will generate it.

> **EF Core concept — Migration**
> A **migration** is a C# file, generated by EF Core, that describes **how to change the database** so it
> matches our entity classes. EF Core compares our classes with what it generated last time, and writes
> only the difference. Think of migrations as **version control for the database structure**: each one is
> a numbered step, and the database remembers which steps it has already applied.

### The steps (Visual Studio → Package Manager Console)

Open **Tools → NuGet Package Manager → Package Manager Console**.

1. In Solution Explorer, right-click **`TMS.Api` → Set as Startup Project**.
   *Why:* EF reads `Program.cs` and `appsettings.json` of the **startup project** to find the connection string.
2. In the console, set the **Default project** dropdown to **`TMS.Data`**.
   *Why:* `AppDbContext` lives there, so the `Migrations` folder should be created there.
3. Run:
   ```powershell
   Add-Migration InitialCreate
   ```
4. Run:
   ```powershell
   Update-Database
   ```

**Alternative — command line** (from the solution folder), if you're not using Visual Studio:

```bash
dotnet tool install --global dotnet-ef
```
```bash
dotnet ef migrations add InitialCreate --project TMS.Data --startup-project TMS.Api
```
```bash
dotnet ef database update --project TMS.Data --startup-project TMS.Api
```

### What `Add-Migration InitialCreate` does

It **only creates files**. It **does not touch the database**. In `TMS.Data/Migrations/` you get:

| File | What's inside |
|---|---|
| `2026xxxxxxxxxx_InitialCreate.cs` | Two methods. **`Up()`** = how to apply this change (`CreateTable("TravelRequests", ...)`). **`Down()`** = how to undo it (`DropTable("TravelRequests")`). The number in front is the date/time, so migrations stay in order. |
| `2026xxxxxxxxxx_InitialCreate.Designer.cs` | Metadata EF Core uses to identify this migration. Don't edit it. |
| `AppDbContextModelSnapshot.cs` | A full picture of all entities **as they are now**. Next time you run `Add-Migration`, EF compares against this snapshot to find what changed. |

`InitialCreate` is just a name we chose. Name each migration after what it does.

### What `Update-Database` does

It connects using the connection string, then:

1. Creates the database **`TravelDB`** if it doesn't exist.
2. Creates a small table **`__EFMigrationsHistory`**: EF Core's notebook of which migrations have already run.
3. Runs the `Up()` method of every migration that is **not** in that notebook yet.
4. Writes `InitialCreate` into `__EFMigrationsHistory`, so it never runs twice.

The table it creates looks like this:

```sql
CREATE TABLE [TravelRequests] (
    [TravelRequestId] int           NOT NULL IDENTITY,   -- [Key] on an int
    [EmployeeName]    nvarchar(100) NOT NULL,            -- [Required, MaxLength(100)]
    [Destination]     nvarchar(100) NOT NULL,
    [StartDate]       datetime2     NOT NULL,            -- DateTime can't be null
    [EndDate]         datetime2     NOT NULL,
    [Purpose]         nvarchar(500) NOT NULL,
    [EstimatedBudget] decimal(18,2) NOT NULL,            -- [Column(TypeName = ...)]
    [Status]          nvarchar(20)  NOT NULL,
    [CreatedOn]       datetime2     NOT NULL,
    CONSTRAINT [PK_TravelRequests] PRIMARY KEY ([TravelRequestId])
);
```

Compare it with the entity in Step 4: every property → a column, every attribute → a rule.

### Changing the table later

Say you add `public string? TravelClass { get; set; }` to `TravelRequest`. Then run:

```powershell
Add-Migration AddTravelClass
Update-Database
```

EF compares against the snapshot, sees one new property, and generates **only** an `AddColumn`.
Existing data is kept.

| Command | Use it to… |
|---|---|
| `Add-Migration Name` | Generate a new migration from your entity changes (files only) |
| `Update-Database` | Apply all pending migrations to the database |
| `Update-Database PreviousName` | Roll the database back to an earlier migration (runs `Down()`) |
| `Remove-Migration` | Delete the last migration's files, **only if it hasn't been applied yet** |

✅ **Checkpoint:** in **SQL Server Management Studio (SSMS)**, connect to your server, expand
**Databases → TravelDB → Tables**. You should see `dbo.TravelRequests` and `dbo.__EFMigrationsHistory`.

---

# Part 4 — The Repository

## Why have a Repository at all?

We *could* use `AppDbContext` directly in the service. But then every service would contain EF Core code,
and the same "get all / get by id / add / delete" logic would be copied for every table.

A **Repository** wraps those common operations in one place, so:
- the Business layer says *"give me request 5"* without knowing **how** (EF Core, SQL, a file, a fake list
  in a test…),
- we write it **once** and reuse it for every table (that's what the `<TEntity>` is for).

## Step 8: The contract — `TMS.Data/Repository/IRepository.cs`

```csharp
namespace TMS.Data.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity?> GetByIdAsync(int id);

        Task AddAsync(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        Task SaveAsync();
    }
}
```

> **C# concept — `interface`**
> An **interface** is a **contract**: a list of methods that a class *promises* to have, **without any code
> inside them**. It answers "*what* can you do?", not "*how*?". By convention its name starts with **`I`**.
> Other layers will depend on `IRepository`, the promise, and not on `Repository`, the actual code.
> That means we can swap the real repository for a fake one in tests without changing the service at all.

> **C# concept — generic type `<TEntity>` and `where TEntity : class`**
> `TEntity` is a **placeholder** for "some entity type, decided later". `IRepository<TravelRequest>` is a
> repository for travel requests. `IRepository<Expense>` would be one for expenses. Same code, different table.
> `where TEntity : class` is a **rule** on the placeholder: it must be a class (EF Core entities always are).

> **C# concept — `Task`, `async` and `await`**
> Talking to a database is **slow** compared to normal code (milliseconds vs nanoseconds). Instead of making
> the thread sit and wait, .NET methods return a **`Task`**: a *"promise of a result, later"*.
> - `Task` = a promise of *finishing* (no value comes back).
> - `Task<int>` = a promise of an `int`. `Task<TEntity?>` = a promise of an entity (or nothing).
>
> When we `await` a Task, our method pauses there, and the thread is **freed to serve other users'
> requests**. When the database answers, our method continues on the next line.
> A method that uses `await` must be marked **`async`**. By convention its name ends with **`Async`**.
> **Rule of thumb:** if you call a method ending in `Async`, put `await` in front of it.

> **C# concept — nullable `?`**
> `TEntity?` means *"may be **null**"*, i.e. "may return nothing". `GetByIdAsync(99)` returns `null` when
> there is no row with id 99. The `?` warns whoever calls it: **check for null before using the result.**

> **C# concept — `IEnumerable<T>`**
> "A sequence of T that you can loop over": a list, an array, etc. We use it for "many rows".

**Read the contract like a sentence:**

| Method | Returns | Meaning |
|---|---|---|
| `GetAllAsync()` | promise of many entities | "Get every row" |
| `GetByIdAsync(int id)` | promise of one entity **or null** | "Get the row with this id" |
| `AddAsync(entity)` | promise of finishing | "**Stage** this new row" |
| `Update(entity)` | `void` (nothing) | "**Stage** this row as changed" |
| `Delete(entity)` | `void` | "**Stage** this row for removal" |
| `SaveAsync()` | promise of finishing | "**Actually write** all staged changes to the DB" |

Why are `Update` and `Delete` not `async`? Remember Step 5: they only **change the state** in the
DbContext's memory (`Modified` / `Deleted`). No database call, so nothing to wait for.
Only `SaveAsync()` (and the reads) talk to SQL Server.

## Step 9: The implementation — `TMS.Data/Repository/Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace TMS.Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext _appDbContext;

        public Repository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _appDbContext.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _appDbContext.Set<TEntity>().FindAsync(id);
        }

        public async Task AddAsync(TEntity entity)
        {
            await _appDbContext.Set<TEntity>().AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _appDbContext.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _appDbContext.Set<TEntity>().Remove(entity);
        }

        public async Task SaveAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}
```

> **C# concept — implementing an interface `: IRepository<TEntity>`**
> `class Repository<TEntity> : IRepository<TEntity>` = *"this class keeps the promises of IRepository"*.
> The compiler **forces** us to write every method listed in the interface, with exactly the same signature.
> Forget one and the build fails.

> **C# concept — constructor injection + `private readonly` field**
> The constructor says *"to create a Repository you must give me an `AppDbContext`"*. We never call this
> constructor ourselves: the **DI container** sees the request and passes in the `AppDbContext` it registered
> in Step 6. This is called **constructor injection**.
> We store it in a **field** `_appDbContext` so every method can use it.
> - `private` = only this class can see it.
> - `readonly` = it can only be set in the constructor, never replaced later (safety).
> - the `_` prefix is just a naming habit for private fields.

### Each method, explained

**`Set<TEntity>()`**: gets the `DbSet` (table) for whatever type `TEntity` currently is.
For `Repository<TravelRequest>` this is exactly `_appDbContext.TravelRequests`. This one call is what
makes the repository work for **any** table.

| Method | EF Core call | DB call? | SQL that runs |
|---|---|---|---|
| `GetAllAsync` | `.ToListAsync()` | ✅ yes | `SELECT ... FROM [TravelRequests]` |
| `GetByIdAsync` | `.FindAsync(id)` | ✅ usually | `SELECT TOP(1) ... WHERE [TravelRequestId] = @id` |
| `AddAsync` | `.AddAsync(entity)` | ❌ no | none yet. State becomes `Added` |
| `Update` | `.Update(entity)` | ❌ no | none yet. State becomes `Modified` |
| `Delete` | `.Remove(entity)` | ❌ no | none yet. State becomes `Deleted` |
| `SaveAsync` | `.SaveChangesAsync()` | ✅ yes | all pending `INSERT` / `UPDATE` / `DELETE`, in one transaction |

Details worth knowing:
- **`ToListAsync()`**: a query in EF Core is only a *description* until you ask for the results. Methods
  like `ToListAsync`, `FindAsync` and `FirstOrDefaultAsync` are what actually **run** it.
- **`FindAsync(id)`**: searches by **primary key**. If the DbContext is already tracking that object, it
  returns it from memory without asking the DB. If nothing is found, it returns **`null`**.
- **`@id`**: EF Core always sends values as **parameters**, never by gluing text into the SQL. That
  protects us from **SQL injection** attacks automatically.
- **`SaveChangesAsync()` after an `Added` entity**: EF Core runs the `INSERT`, reads back the new
  IDENTITY value, and **writes it into `entity.TravelRequestId`**. That's how we learn the new id.

## Step 10: Register the Repository — `Program.cs`

Add (with `using TMS.Data.Repository;` at the top):

```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

Meaning: *"Whenever any class asks for `IRepository<Something>`, create a `Repository<Something>`."*

- `typeof(...)` gives the **type** itself (not an object of it).
- The **empty** `<>` makes it an **open generic**: one line covers `IRepository<TravelRequest>`,
  `IRepository<Expense>`, and every future entity.
- **`AddScoped`** = one per HTTP request, the same lifetime as `AppDbContext`.

| Registration method | One object per… | Typical use |
|---|---|---|
| `AddSingleton` | the whole app, forever | shared, stateless things (settings, caches) |
| `AddScoped` | **HTTP request** | anything that uses a `DbContext` ✅ |
| `AddTransient` | every single time it's asked for | small, stateless helpers |

✅ **Checkpoint:** build. Still no endpoints. We're building the foundations.

---

# Part 5 — The Business layer (Service)

## What belongs here?

The **rules of the business**, the things that would still be true if the API were replaced by a
mobile app or a nightly batch job:
- *A new travel request always starts as **Draft**.*
- *The **creation time** is set by the system, not by the user.*
- *A trip **cannot end before it starts**.*
- *When editing, the user may change the trip details but **not** the id, status or creation time.*

The Service knows nothing about HTTP (no status codes) and nothing about SQL. It talks to the
Repository's interface only.

## Step 11: The contract — `TMS.Business/Services/ITravelRequestService.cs`

```csharp
using TMS.Data.Entity;

namespace TMS.Business.Services
{
    public interface ITravelRequestService
    {
        Task<IEnumerable<TravelRequest>> GetAllAsync();

        Task<TravelRequest?> GetByIdAsync(int id);

        Task<TravelRequest> CreateAsync(TravelRequest travelRequest);

        Task<TravelRequest?> UpdateAsync(int id, TravelRequest travelRequest);

        Task<bool> DeleteAsync(int id);
    }
}
```

The return types tell the controller what can happen:

| Method | Returns | `null` / `false` means |
|---|---|---|
| `GetByIdAsync` | the request **or null** | not found |
| `CreateAsync` | the saved request (with its new id) | (never null) |
| `UpdateAsync` | the updated request **or null** | not found |
| `DeleteAsync` | **`bool`**: `true` / `false` | `false` = not found |

## Step 12: The implementation — `TMS.Business/Services/TravelRequestService.cs`

```csharp
using TMS.Data.Entity;
using TMS.Data.Repository;

namespace TMS.Business.Services
{
    public class TravelRequestService : ITravelRequestService
    {
        private readonly IRepository<TravelRequest> _travelRequestRepository;

        public TravelRequestService(IRepository<TravelRequest> travelRequestRepository)
        {
            _travelRequestRepository = travelRequestRepository;
        }

        public async Task<IEnumerable<TravelRequest>> GetAllAsync()
        {
            return await _travelRequestRepository.GetAllAsync();
        }

        public async Task<TravelRequest?> GetByIdAsync(int id)
        {
            return await _travelRequestRepository.GetByIdAsync(id);
        }

        public async Task<TravelRequest> CreateAsync(TravelRequest travelRequest)
        {
            ValidateDates(travelRequest);

            travelRequest.Status = "Draft";
            travelRequest.CreatedOn = DateTime.UtcNow;

            await _travelRequestRepository.AddAsync(travelRequest);
            await _travelRequestRepository.SaveAsync();

            return travelRequest;
        }

        public async Task<TravelRequest?> UpdateAsync(int id, TravelRequest travelRequest)
        {
            var existing = await _travelRequestRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            ValidateDates(travelRequest);

            existing.EmployeeName = travelRequest.EmployeeName;
            existing.Destination = travelRequest.Destination;
            existing.StartDate = travelRequest.StartDate;
            existing.EndDate = travelRequest.EndDate;
            existing.Purpose = travelRequest.Purpose;
            existing.EstimatedBudget = travelRequest.EstimatedBudget;

            _travelRequestRepository.Update(existing);
            await _travelRequestRepository.SaveAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _travelRequestRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            _travelRequestRepository.Delete(existing);
            await _travelRequestRepository.SaveAsync();

            return true;
        }

        private static void ValidateDates(TravelRequest travelRequest)
        {
            if (travelRequest.EndDate < travelRequest.StartDate)
            {
                throw new ArgumentException("EndDate cannot be before StartDate.");
            }
        }
    }
}
```

Notice the constructor asks for **`IRepository<TravelRequest>`** (the interface). Because of Step 10, the DI
container hands it a `Repository<TravelRequest>`, which in turn gets an `AppDbContext` (Step 6).
The service never knows EF Core exists.

### Each method, explained

**`GetAllAsync` / `GetByIdAsync`**: simply pass through to the repository. That's fine. This is the place
where read rules would go later ("employees only see their own requests", "show only Drafts").

**`CreateAsync`**, step by step:
1. `ValidateDates(...)`: check the business rule. If it fails, an exception is thrown and **nothing below runs**.
2. `Status = "Draft"`: the system decides the status, whatever the client sent.
3. `CreatedOn = DateTime.UtcNow`: the current time in **UTC** (universal time). We store UTC and convert to
   local time only when displaying, so users in different time zones don't get confused.
4. `AddAsync`: **stage** the new row (state `Added`).
5. `SaveAsync`: **run the `INSERT`**. After this line, `travelRequest.TravelRequestId` holds the new id.
6. `return travelRequest`: give the saved object back to the controller.

**`UpdateAsync`**, step by step:
1. **Load the current row first** with `GetByIdAsync(id)`. Why?
   - If it doesn't exist → `return null` → the controller will answer **404**.
   - So we keep the DB's own `TravelRequestId`, `Status` and `CreatedOn`. We only copy the **editable** fields
     onto the loaded object. A client can't sneak in `"status": "Approved"`.
2. Validate dates (same rule as Create).
3. Copy the 6 editable fields from the incoming object onto `existing`.
4. `Update(existing)`: mark it `Modified`. *(Because `existing` was loaded by this same DbContext, EF Core
   is already tracking it and would notice the changes even without this call. We keep the call because it
   makes the intention obvious, and it's required when the object did **not** come from the DbContext.)*
5. `SaveAsync()`: runs `UPDATE [TravelRequests] SET ... WHERE [TravelRequestId] = @id`.

**`DeleteAsync`**, step by step:
1. Load the row. Not found → `return false` → controller answers **404**.
2. `Delete(existing)`: mark it `Deleted`. EF Core needs the object (or at least its key) to know what to delete.
3. `SaveAsync()`: runs `DELETE FROM [TravelRequests] WHERE [TravelRequestId] = @id`.
4. `return true`.

> **C# concept — `if` and `null` checks, early `return`**
> `if (existing == null) { return null; }` means: "if nothing was found, stop here and hand back nothing".
> Returning early keeps the "happy path" below un-indented and easy to read.

> **C# concept — exceptions: `throw`**
> `throw new ArgumentException("...")` **stops the method immediately** and sends an error object up to
> whoever called it, and up again, until some code **catches** it (we'll catch it in the controller). If
> nobody catches it, the API returns **500**. `ArgumentException` is a built-in type that means "an
> argument you gave me is invalid".

> **C# concept — `private static` method**
> `private` = only usable inside this class. `static` = doesn't need any of the object's fields; it only
> works with what you pass in. `void` = returns nothing. We made it a separate method because both
> Create and Update use the same rule (write once, use twice).

## Step 13: Register the Service — `Program.cs`

Add (with `using TMS.Business.Services;` at the top):

```csharp
builder.Services.AddScoped<ITravelRequestService, TravelRequestService>();
```

*"Whenever a class asks for `ITravelRequestService`, create a `TravelRequestService`."*
`<Interface, Implementation>`: the asked-for type first, the actual class second.

✅ **Checkpoint:** build. Data and Business layers are done. Only the "waiter" is left.

---


# Part 6 — The API layer

The API layer has three jobs, and we build them in this order:

1. **DTOs**: define what JSON comes **in** and what JSON goes **out** (Step 14)
2. **Mapper**: convert between DTOs and the Entity automatically (Steps 15–16)
3. **Controller**: the endpoints that tie everything together (Step 17)

## Step 14: The DTOs — `TMS.Api/Dtos/`

> **.NET concept — DTO (Data Transfer Object)**
> A **DTO** is a simple class whose only job is to **carry data across a boundary**, here between the
> client and our API. It has properties and nothing else: no logic, no database attributes.
> It describes **the shape of the JSON**.

### Why not just send and receive the Entity?

The entity `TravelRequest` is the **shape of our database table**. If we use it directly as the
API's input and output, two problems appear:

| Problem | Example |
|---|---|
| **Coming in:** the client can set fields it must never control | A client POSTs `"status": "Approved"` or `"createdOn": "2020-01-01"` and skips the approval workflow |
| **Going out:** every column is exposed, and the API's JSON changes whenever the table changes | Tomorrow we add an internal column `ApprovedByUserId` or `IsDeleted`, and it suddenly appears in every response |

So we use **two DTOs**, one for each direction:

```
               INPUT DTO                                          RESPONSE DTO
  Client  ──── TravelRequestInputDto ────►  API  ── ... ──►  ──── TravelRequestResponseDto ────►  Client
               (what the client may send)                         (what the client is allowed to see)
```

Create a folder **`Dtos`** in `TMS.Api`.

### 14a. Input DTO — `TMS.Api/Dtos/TravelRequestInputDto.cs`

Used for **both Create and Update**, because the client sends the same fields in both cases.

```csharp
using System.ComponentModel.DataAnnotations;

namespace TMS.Api.Dtos
{
    public class TravelRequestInputDto
    {
        [Required, MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        [Required, MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Range(1, 10000000)]
        public decimal EstimatedBudget { get; set; }
    }
}
```

There is **no** `TravelRequestId`, `Status` or `CreatedOn` here. The client physically cannot send them.
If they put them in the JSON anyway, ASP.NET Core simply ignores them.

The attributes here are **input checks**, read by ASP.NET Core (Step 17), not by EF Core. The **same
attribute** means different things depending on who reads it:

| Attribute | On the Input DTO (read by ASP.NET Core) | On the Entity (read by EF Core) |
|---|---|---|
| `[Required]` | **400** if the field is missing | column is `NOT NULL` |
| `[MaxLength(100)]` | **400** if longer than 100 characters | `nvarchar(100)` |
| `[Range(1, 10000000)]` | **400** if outside 1 – 1 crore | (not used) |

> **Why `DateTime?` (nullable) for the dates?**
> A plain `DateTime` can **never** be empty. If the client forgets `startDate`, it silently becomes
> `01-01-0001`, and `[Required]` sees "there is a value" and lets it pass. With `DateTime?`, a missing date
> stays **`null`**, and `[Required]` correctly rejects it with 400.
> `EstimatedBudget` doesn't need this trick: if it's missing it becomes `0`, and `[Range(1, ...)]` rejects `0`.

### 14b. Response DTO — `TMS.Api/Dtos/TravelRequestResponseDto.cs`

What the API **sends back** from every endpoint.

```csharp
namespace TMS.Api.Dtos
{
    public class TravelRequestResponseDto
    {
        public int TravelRequestId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TripDays { get; set; }

        public string Purpose { get; set; } = string.Empty;

        public decimal EstimatedBudget { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
    }
}
```

- It **does** include `TravelRequestId`, `Status` and `CreatedOn`. The client may *see* them, just not *set* them.
- It has **no attributes**: nobody validates data we send out ourselves.
- **`TripDays`** doesn't exist in the database. It's a value **calculated for the client** (2 Nov → 5 Nov =
  4 days). The Response DTO is the right place for "presentation" values like this. The table stays clean.

### The three classes side by side

| Property | `TravelRequestInputDto` (in) | `TravelRequest` (Entity / table) | `TravelRequestResponseDto` (out) |
|---|---|---|---|
| `TravelRequestId` | ❌ DB generates it | ✅ | ✅ |
| `EmployeeName`, `Destination`, `Purpose` | ✅ | ✅ | ✅ |
| `StartDate`, `EndDate` | ✅ (`DateTime?`) | ✅ | ✅ |
| `EstimatedBudget` | ✅ | ✅ | ✅ |
| `Status` | ❌ system sets it | ✅ | ✅ |
| `CreatedOn` | ❌ system sets it | ✅ | ✅ |
| `TripDays` | ❌ | ❌ not stored | ✅ calculated |
| Validation attributes | ✅ input checks | ✅ column rules | ❌ none |
| Lives in project | `TMS.Api` | `TMS.Data` | `TMS.Api` |

> **Why do the DTOs live in `TMS.Api` and not in `TMS.Data`?** Because they describe the **HTTP JSON**, and
> HTTP is the API layer's business. The Service and Repository keep working with the Entity only, and
> never know DTOs exist.

## Step 15: The Mapper — AutoMapper and `MapperProfile`

### The problem

We now have three classes with almost the same properties. On every request we must copy values
between them:

```
POST:   TravelRequestInputDto ──copy──► TravelRequest ──(save)──► TravelRequest ──copy──► TravelRequestResponseDto
```

By hand, converting one entity to a response DTO looks like this:

```csharp
var response = new TravelRequestResponseDto
{
    TravelRequestId = entity.TravelRequestId,
    EmployeeName    = entity.EmployeeName,
    Destination     = entity.Destination,
    StartDate       = entity.StartDate,
    EndDate         = entity.EndDate,
    TripDays        = (entity.EndDate.Date - entity.StartDate.Date).Days + 1,
    Purpose         = entity.Purpose,
    EstimatedBudget = entity.EstimatedBudget,
    Status          = entity.Status,
    CreatedOn       = entity.CreatedOn
};
```

…plus the same again for Input → Entity, plus a loop for lists. That's boring, repetitive code. Add a
property and forget one line, and the value is silently missing from the JSON.

### The solution: AutoMapper

**AutoMapper** is a NuGet library that does this copying for us. The rule it follows:
**if the source and destination have a property with the same name, copy it.** We only describe the
exceptions.

### 15a. Install AutoMapper

Right-click **`TMS.Api` → Manage NuGet Packages → Browse** → `AutoMapper` → version **15.1.3** → Install.
Only `TMS.Api` needs it, because only the API layer maps DTOs.

> **Version note:** use **15.1.3** or newer. **15.1.0** (the version in the EMS project) has a
> **known high-severity security issue** (GitHub advisory GHSA-rvv3-g6hj-g44x), and Visual Studio will
> warn you about it.
> **Licence note:** from version 15, AutoMapper is a commercial product. It still works without a licence
> key, but logs a licence warning at startup. Companies above a revenue limit need a paid licence. Check
> with your team lead before using it in a real project. For learning, the warning is harmless.

### 15b. The Profile — `TMS.Api/Mapping/MapperProfile.cs`

Create a folder **`Mapping`** in `TMS.Api` and add:

```csharp
using AutoMapper;
using TMS.Api.Dtos;
using TMS.Data.Entity;

namespace TMS.Api.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // Input DTO -> Entity (Create / Update)
            CreateMap<TravelRequestInputDto, TravelRequest>()
                .ForMember(dest => dest.TravelRequestId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore());

            // Entity -> Response DTO (every response)
            CreateMap<TravelRequest, TravelRequestResponseDto>()
                .ForMember(dest => dest.TripDays,
                           opt => opt.MapFrom(src => (src.EndDate.Date - src.StartDate.Date).Days + 1));
        }
    }
}
```

> **AutoMapper concept — `Profile`**
> A **Profile** is a class where we write down **which class can be converted into which**. We inherit from
> AutoMapper's `Profile` (`: Profile`) and put the rules inside the **constructor**. At startup, AutoMapper
> creates our profile once, reads the rules, and prepares fast copying code for each pair.

Line by line:

| Code | Meaning |
|---|---|
| `CreateMap<TravelRequestInputDto, TravelRequest>()` | "I will need to turn an **Input DTO** (source) into an **Entity** (destination)." Read the generics as `<From, To>`. All same-named properties (`EmployeeName`, `Destination`, `StartDate`…) are now copied automatically. |
| `.ForMember(dest => dest.TravelRequestId, opt => opt.Ignore())` | "For the destination's `TravelRequestId`, **do nothing**." The Input DTO doesn't have these three properties anyway, so this line documents an intention: *"these are system-owned. Never map into them."* It also keeps AutoMapper's configuration check happy (see Part 9). |
| `CreateMap<TravelRequest, TravelRequestResponseDto>()` | "I will need to turn an **Entity** into a **Response DTO**." All same-named properties are copied. |
| `.ForMember(dest => dest.TripDays, opt => opt.MapFrom(src => ...))` | "`TripDays` has no matching property on the entity, so **calculate it from the source like this**." |
| `(src.EndDate.Date - src.StartDate.Date).Days + 1` | Subtracting two dates gives a `TimeSpan` (a duration). `.Date` drops the time part, so 2 Nov 18:00 → 5 Nov 09:00 still counts as whole days. `.Days` = number of whole days (3), and `+ 1` counts both the first and last day (4). |

> **C# concept — lambdas again: `dest => dest.TripDays`**
> You met lambdas in Step 6. Here AutoMapper uses them to **point at a property**: `dest => dest.TripDays`
> means "on the destination object, the `TripDays` property". `src => ...` means "given the source object,
> compute this". The names `dest`, `opt`, `src` are just parameter names we chose. You could write `x => x.TripDays`.

> **Why `DateTime?` → `DateTime` just works:** the Input DTO's `StartDate` is `DateTime?` and the entity's
> is `DateTime`. AutoMapper converts between nullable and non-nullable automatically. `[Required]` has already
> guaranteed the value isn't null by the time mapping runs.

**Why only one direction per pair?** EMS uses `.ReverseMap()`, which also creates the opposite mapping
(Entity → Input DTO). We don't need that direction, and we don't want a Response DTO to ever be turned back
into an Entity. **Only create the maps you actually use.**

## Step 16: Register AutoMapper — `Program.cs`

Add this line in **Half 1** (with `using TMS.Api.Mapping;` at the top):

```csharp
builder.Services.AddAutoMapper(cfg => { }, typeof(MapperProfile));
```

| Piece | Meaning |
|---|---|
| `AddAutoMapper(...)` | Register AutoMapper in the DI container. After this, any class can ask for **`IMapper`** in its constructor. |
| `cfg => { }` | A lambda for extra global settings. We have none, so the body `{ }` is empty. (This is also where a licence key would go: `cfg => { cfg.LicenseKey = "..."; }`, read from configuration, never typed into code.) |
| `typeof(MapperProfile)` | "Look in the project that contains `MapperProfile` and load **every** `Profile` you find there." Add more profiles later (e.g. `ExpenseProfile`) and they're picked up automatically. |

> **Trainer note:** EMS has this line commented out:
> `new MapperConfiguration(x => x.AddProfile(new MapperProfile())).CreateMapper()`.
> From AutoMapper 15, `MapperConfiguration` also needs a logger, so that line no longer compiles as written.
> `AddAutoMapper(...)` handles all of that for us and is the recommended way.

`IMapper` is registered as **Scoped** by `AddAutoMapper`, the same lifetime as our other services.

✅ **Checkpoint:** build the solution. It should succeed.

## Step 17: The Controller — `TMS.Api/Controllers/TravelRequestController.cs`

The controller is the "waiter". Each **public method** is one **endpoint** (one URL + verb). Each one now
does the same four things:

1. **Receive** the request (the URL `id` and/or the Input DTO from the JSON body)
2. **Map** Input DTO → Entity
3. **Call** the service (which works with Entities only)
4. **Map** Entity → Response DTO and wrap it in an **HTTP status code**

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.Api.Dtos;
using TMS.Business.Services;
using TMS.Data.Entity;

namespace TMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TravelRequestController : ControllerBase
    {
        private readonly ITravelRequestService _travelRequestService;
        private readonly IMapper _mapper;

        public TravelRequestController(ITravelRequestService travelRequestService, IMapper mapper)
        {
            _travelRequestService = travelRequestService;
            _mapper = mapper;
        }

        // ... the 5 endpoints, explained one by one below ...
    }
}
```

### The class-level parts

| Piece | Meaning |
|---|---|
| `[Route("api/[controller]")]` | Base URL for every endpoint in this class. `[controller]` is replaced by the class name **minus the word "Controller"**, so the URL is **`/api/TravelRequest`**. |
| `[ApiController]` | Switches on helpful API behaviour: **(1)** automatically checks the Input DTO's attributes and returns **400** if they fail, *before your method even runs*; **(2)** automatically reads complex objects (our Input DTO) from the **JSON body** and simple values (`id`) from the **URL**. |
| `: ControllerBase` | Inherit from Microsoft's base class for APIs. It gives us the helper methods `Ok()`, `NotFound()`, `BadRequest()`, `CreatedAtAction()`, `NoContent()`. |
| Constructor | Asks for **two** things: `ITravelRequestService` (registered in Step 13) and **`IMapper`** (registered in Step 16). The DI container provides both. **One controller object is created per request.** |

> **C# concept — `IActionResult`**
> Every endpoint returns `Task<IActionResult>`: a promise of "some HTTP result". `Ok(...)`, `NotFound()`
> etc. all produce an `IActionResult`, which ASP.NET Core turns into a status code + JSON body.

> **AutoMapper concept — `_mapper.Map<Destination>(source)`**
> This is the only AutoMapper method we call. You give it a **source object**, and in `< >` you name the
> **type you want back**. AutoMapper finds the matching `CreateMap` from the profile and builds the new object.
> - `_mapper.Map<TravelRequest>(input)` → uses `CreateMap<TravelRequestInputDto, TravelRequest>`
> - `_mapper.Map<TravelRequestResponseDto>(entity)` → uses `CreateMap<TravelRequest, TravelRequestResponseDto>`
> - `_mapper.Map<IEnumerable<TravelRequestResponseDto>>(list)` → maps **every item** of a list using the same
>   rule. No `CreateMap` needed for lists.

### READ ALL: `GET /api/TravelRequest`

```csharp
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var travelRequests = await _travelRequestService.GetAllAsync();
    var response = _mapper.Map<IEnumerable<TravelRequestResponseDto>>(travelRequests);
    return Ok(response);
}
```

- `[HttpGet]`: this method answers **GET** on the base URL.
- The service returns **entities**. We map the whole list to **Response DTOs** in one call.
- `Ok(response)`: **200** + the list turned into a JSON array.
- An empty table returns `[]` with **200**, not 404. "There are zero requests" is a valid answer for a list.

### READ ONE: `GET /api/TravelRequest/5`

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var travelRequest = await _travelRequestService.GetByIdAsync(id);
    if (travelRequest == null)
    {
        return NotFound();
    }

    return Ok(_mapper.Map<TravelRequestResponseDto>(travelRequest));
}
```

- `[HttpGet("{id}")]`: adds `/{id}` to the URL. Whatever is in that spot is put into the parameter
  **with the same name**, `int id`. This is called **route binding**. `/api/TravelRequest/abc` isn't a
  number, so `[ApiController]` returns **400**.
- **Check for null first, then map.** The service returns `null` when the id doesn't exist → `NotFound()` = **404**.
- Found → map the entity to a Response DTO → **200**.

### CREATE: `POST /api/TravelRequest`

```csharp
[HttpPost]
public async Task<IActionResult> Create(TravelRequestInputDto input)
{
    try
    {
        var entity = _mapper.Map<TravelRequest>(input);
        var created = await _travelRequestService.CreateAsync(entity);
        var response = _mapper.Map<TravelRequestResponseDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = response.TravelRequestId }, response);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ex.Message);
    }
}
```

What happens, in order:
1. ASP.NET Core reads the **JSON body** and fills a `TravelRequestInputDto` (property names are matched
   ignoring upper/lower case: `employeeName` → `EmployeeName`).
2. `[ApiController]` checks `[Required]`, `[MaxLength]`, `[Range]`. **Any failure → 400 with a list of
   errors, and our method never runs.**
3. **Map in:** `_mapper.Map<TravelRequest>(input)` creates a new entity with the 6 user fields copied.
   `TravelRequestId` = 0, `Status` = `"Draft"` (the entity's default), `CreatedOn` = empty. These are ignored by the profile.
4. The service applies the rules (`Status = "Draft"`, `CreatedOn = now`, date check) and saves. After saving,
   `created.TravelRequestId` holds the new id from the database.
5. **Map out:** `_mapper.Map<TravelRequestResponseDto>(created)` builds the response, including `TripDays`.
6. `CreatedAtAction(...)` returns **201 Created**, with:
   - a **`Location` header** pointing at the new record, e.g. `/api/TravelRequest/7`.
     It builds that URL by asking "what's the URL of the **`GetById`** endpoint with **`id = 7`**?";
   - the Response DTO as the JSON body.

> **C# concept — `try` / `catch`**
> Code inside `try { }` runs normally. If it **throws** an exception of the type named in
> `catch (ArgumentException ex)`, execution **jumps into the `catch` block** instead of crashing.
> `ex.Message` is the text we wrote in the service: *"EndDate cannot be before StartDate."*
> Here we turn the business error into a polite **400 Bad Request**.

> **C# concept — `nameof(GetById)`**
> Gives the text `"GetById"`, but checked by the compiler. If someone renames the method, this updates too
> (or fails to build), instead of silently breaking like a typed string would.

> **C# concept — anonymous object `new { id = ... }`**
> A quick throw-away object with one property called `id`. Here it supplies the `{id}` value for the
> `GetById` URL.

### UPDATE: `PUT /api/TravelRequest/5`

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, TravelRequestInputDto input)
{
    try
    {
        var entity = _mapper.Map<TravelRequest>(input);
        var updated = await _travelRequestService.UpdateAsync(id, entity);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<TravelRequestResponseDto>(updated));
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ex.Message);
    }
}
```

- **Two inputs:** `id` comes from the **URL**, `input` comes from the **JSON body**. `[ApiController]`
  works out which is which by their types.
- **Map in** → the service loads the existing row and copies only the editable fields onto it (Step 12), so
  `Status` and `CreatedOn` keep their database values.
- Service returned `null` → **404**. Otherwise **map out** → **200** + the updated record.
- Same `try/catch` as Create for the date rule.

### DELETE: `DELETE /api/TravelRequest/5`

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    var deleted = await _travelRequestService.DeleteAsync(id);
    return deleted ? NoContent() : NotFound();
}
```

- No DTOs and no mapping: nothing comes in except the `id`, and nothing goes back.
- `NoContent()`: **204**. It worked, and there's nothing to send back. This is the standard answer to a
  successful DELETE.

> **C# concept — the ternary operator `condition ? a : b`**
> A one-line `if/else`: *"if `deleted` is true then `NoContent()`, otherwise `NotFound()`"*.

### Where mapping happens, at a glance

| Endpoint | Map **in** (Input DTO → Entity) | Map **out** (Entity → Response DTO) |
|---|---|---|
| `GET` all | — | ✅ list |
| `GET` one | — | ✅ |
| `POST` | ✅ | ✅ |
| `PUT` | ✅ | ✅ |
| `DELETE` | — | — |

Mapping happens **only in the controller**. The Service and Repository only ever see Entities.

## Step 18: The final `Program.cs`

After Steps 6, 10, 13 and 16, `Program.cs` looks like this. Compare it with Step 2: only the `using`
lines and **four registrations** were added.

```csharp
using Microsoft.EntityFrameworkCore;
using TMS.Api.Mapping;
using TMS.Business.Services;
using TMS.Data;
using TMS.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Register services in the DI container ----------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Step 6: DbContext — use SQL Server with the connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Step 10: "When someone asks for IRepository<X>, give them Repository<X>"
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Step 13: "When someone asks for ITravelRequestService, give them TravelRequestService"
builder.Services.AddScoped<ITravelRequestService, TravelRequestService>();

// Step 16: AutoMapper — find MapperProfile and make IMapper available for injection
builder.Services.AddAutoMapper(cfg => { }, typeof(MapperProfile));

var app = builder.Build();

// ---------- 2. Configure the HTTP request pipeline ----------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

# Part 7 — Run and test

## Step 19: Test every endpoint in Swagger

Press **F5**. The Swagger page now lists 5 endpoints under **TravelRequest**. Scroll to the bottom and
open **Schemas**: you'll see `TravelRequestInputDto` and `TravelRequestResponseDto`. Swagger reads our DTOs
to show the JSON shapes.

For each endpoint: click it → **Try it out** → fill in → **Execute**, and look at the **response code**
and **body**.

| # | Do this | Expect |
|---|---|---|
| 1 | `POST` with the JSON below | **201**, body has `travelRequestId: 1`, `status: "Draft"`, `tripDays: 4`, a `createdOn` time |
| 2 | `GET /api/TravelRequest` | **200**, an array with your request |
| 3 | `GET /api/TravelRequest/1` | **200**, just that request |
| 4 | `GET /api/TravelRequest/999` | **404** |
| 5 | `PUT /api/TravelRequest/1` with `"destination": "Pune"` | **200**, destination changed, `status` and `createdOn` **unchanged** |
| 6 | `POST` with an extra `"status": "Approved"` in the JSON | **201**, but `status` is still **`"Draft"`**. The Input DTO has no `Status`, so it was ignored |
| 7 | `PUT` with `endDate` earlier than `startDate` | **400** `"EndDate cannot be before StartDate."` (from the **service**) |
| 8 | `POST` with `"estimatedBudget": 0` | **400** from `[Range]` (from **`[ApiController]`**, the method never ran) |
| 9 | `POST` without `"destination"` | **400** `"The Destination field is required."` |
| 10 | `DELETE /api/TravelRequest/1` | **204** |
| 11 | `GET /api/TravelRequest/1` again | **404** |

Sample body for `POST` / `PUT`:

```json
{
  "employeeName": "Asha Patel",
  "destination": "Mumbai",
  "startDate": "2026-11-02",
  "endDate": "2026-11-05",
  "purpose": "Client onboarding workshop",
  "estimatedBudget": 45000
}
```

Sample response from `POST` (201):

```json
{
  "travelRequestId": 1,
  "employeeName": "Asha Patel",
  "destination": "Mumbai",
  "startDate": "2026-11-02T00:00:00",
  "endDate": "2026-11-05T00:00:00",
  "tripDays": 4,
  "purpose": "Client onboarding workshop",
  "estimatedBudget": 45000,
  "status": "Draft",
  "createdOn": "2026-10-19T10:15:42.123Z"
}
```

After test 1, run `SELECT * FROM TravelRequests` in SSMS. The row is there with `Status = Draft`, and there
is **no** `TripDays` column. It only exists in the response.

---

# Part 8 — The big picture again

## One request, all the way down and back up

Here is **`POST /api/TravelRequest`**, now that you know every piece:

```
  Client (Swagger)
     │  POST /api/TravelRequest   { "employeeName": "Asha Patel", ... }
     ▼
  ┌──────────────────────────────────────────────────────┐
  │ ASP.NET Core pipeline (Program.cs, Half 2)            │
  │  UseHttpsRedirection → UseAuthorization → MapControllers
  │  routing: POST + /api/TravelRequest → Controller.Create
  │  JSON body → TravelRequestInputDto                    │
  │  [ApiController] validation ──── fails? ──► 400       │
  └──────────────────────────────────────────────────────┘
     │ TravelRequestInputDto
     ▼
  TravelRequestController.Create          (TMS.Api)
     │  _mapper.Map<TravelRequest>(input)              ← MAP IN  (MapperProfile)
     │ TravelRequest (entity)
     ▼
  TravelRequestService.CreateAsync        (TMS.Business)
     │  date rule ──── fails? ──► throws → controller catch → 400
     │  Status = "Draft", CreatedOn = now
     ▼
  Repository<TravelRequest>.AddAsync      (TMS.Data)     state: Added
  Repository<TravelRequest>.SaveAsync
     ▼
  AppDbContext.SaveChangesAsync           (EF Core)      INSERT INTO [TravelRequests] ...
     ▼
  SQL Server: TravelDB.dbo.TravelRequests                new id = 7
     │
     ▲  the same TravelRequest entity (now with id 7) is returned up to the controller
     │
  TravelRequestController.Create
     │  _mapper.Map<TravelRequestResponseDto>(created) ← MAP OUT (adds TripDays)
     ▼
  201 Created, Location: /api/TravelRequest/7, body = TravelRequestResponseDto as JSON
```

## Which class travels where

```
   Client  ◄──── JSON ────►  Controller  ◄─── Entity ───►  Service  ◄─── Entity ───►  Repository  ◄──►  DB
               Input DTO in               TravelRequest              TravelRequest
            Response DTO out
                         (mapping happens here, and only here)
```

DTOs never go below the controller. Entities never go above it.

## Who creates whom (Dependency Injection)

When that request arrives, ASP.NET Core needs a `TravelRequestController`, and builds the whole chain
from the registrations in `Program.cs`:

```
TravelRequestController        needs ITravelRequestService  and  IMapper
  ├─ IMapper                   (Step 16)    built from MapperProfile
  └─ TravelRequestService      (Step 13)    needs IRepository<TravelRequest>
       └─ Repository<TravelRequest>   (Step 10)    needs AppDbContext
            └─ AppDbContext           (Step 6)     needs DbContextOptions (SQL Server + connection string)
```

We never wrote `new TravelRequestService(...)` or `new Mapper(...)` anywhere. When the request finishes,
the scoped objects are thrown away, and the next request gets fresh ones.

## Each piece's job, in one table

| Piece | Project | Does | Never does |
|---|---|---|---|
| **Controller** | TMS.Api | HTTP in → map → call service → map → HTTP status out | business rules, database access |
| **Input DTO** | TMS.Api | defines and validates the incoming JSON | contain system-owned fields (Id, Status, CreatedOn) |
| **Response DTO** | TMS.Api | defines the outgoing JSON | contain internal-only fields |
| **MapperProfile** | TMS.Api | rules for DTO ↔ Entity conversion | business rules |
| **Service** | TMS.Business | business rules, works with Entities | know about HTTP, DTOs or SQL |
| **Repository** | TMS.Data | generic get / add / update / delete / save | business rules |
| **DbContext** | TMS.Data | connection, change tracking, C# → SQL | — |
| **Entity** | TMS.Data | describes one table | — |
| **Program.cs** | TMS.Api | registers everything, builds the pipeline, starts the server | — |

## Why bother with all these layers?

- **Easy to find things:** a status-code problem is in the controller; a wrong JSON field is in a DTO or
  the profile; a wrong rule is in the service; a wrong query is in the repository.
- **Easy to change:** switch SQL Server to PostgreSQL → change `UseSqlServer` in one line in `Program.cs`.
  Add a database column → the API's JSON doesn't change unless *you* add it to the Response DTO.
- **Easy to grow:** an `Expense` feature needs a new entity, DTOs, a few `CreateMap` lines, a service and a
  controller, but **no new repository code**. `IRepository<Expense>` already works.
- **Easy to test:** the service depends on `IRepository`, an interface, so a test can give it a fake one.

---

# Part 9 — Common errors and how to fix them

| Error message (shortened) | Cause | Fix |
|---|---|---|
| *Unable to resolve service for type 'TMS.Business.Services.ITravelRequestService' while attempting to activate 'TravelRequestController'* | You forgot to **register** it in `Program.cs` | Add the `AddScoped<...>` line (Step 13). Same idea for `IRepository<>` (Step 10) and `AppDbContext` (Step 6) |
| *A network-related or instance-specific error occurred while establishing a connection to SQL Server* | Wrong `Server=` in the connection string, or SQL Server isn't running | Copy the exact server name from the SSMS login screen; check the SQL Server service is running |
| *Cannot open database "TravelDB" requested by the login* | Database not created yet | Run `Update-Database` (Step 7) |
| *Invalid object name 'TravelRequests'* | Table missing: migration not applied | Run `Update-Database` |
| `Add-Migration` says it can't create the DbContext, or can't find the connection string | Wrong **startup project** or **default project** | Startup project = `TMS.Api`; Package Manager Console default project = `TMS.Data` |
| *The term 'Add-Migration' is not recognized* | `Microsoft.EntityFrameworkCore.Tools` not installed | Install it (Step 3), then restart Visual Studio |
| Warning: *No store type was specified for the decimal property 'EstimatedBudget'* | Missing `[Column(TypeName = "decimal(18,2)")]` | Add the attribute (Step 4) |
| *Unable to resolve service for type 'AutoMapper.IMapper' while attempting to activate 'TravelRequestController'* | AutoMapper not registered | Add `AddAutoMapper(...)` (Step 16) |
| *AutoMapperMappingException: Missing type map configuration or unsupported mapping* | You called `_mapper.Map<X>(y)` for a pair that has no `CreateMap` | Add `CreateMap<SourceType, X>()` to `MapperProfile` (Step 15). Check the direction: `<From, To>` |
| *Unmapped members were found* (from `AssertConfigurationIsValid()`, a check you can run in a unit test) | A destination property has no matching source property and no rule | Either add `.ForMember(..., opt => opt.MapFrom(...))` or `.ForMember(..., opt => opt.Ignore())` (Step 15) |
| A response field is always `null` / `0` | Property names differ between Entity and Response DTO (e.g. `Dest` vs `Destination`) | Rename to match, or add a `.ForMember(... MapFrom ...)` rule |
| Output window shows an AutoMapper **licence** warning at startup | AutoMapper 15+ is commercial; no licence key configured | Harmless for learning. For real projects, follow your team's licensing decision (Step 15a) |
| Swagger page is empty | Controller missing `[ApiController]` / `[Route]`, class not `public`, or `MapControllers()` missing | Check Step 17 and Step 2 |
| `POST` returns 400 with *"The model field is required"* or a JSON error | The body isn't valid JSON, or the date format is wrong | Use `"2026-11-02"` style dates, and double quotes around names and text |
| 500 Internal Server Error | An exception that nobody caught | Look at the Visual Studio **Output** window for the full error |

---

# Appendix A — Cheat sheets

## ASP.NET Core

| Item | What it does |
|---|---|
| `[ApiController]` | Automatic 400 on invalid Input DTO; reads the DTO from the body, `id` from the URL |
| `[Route("api/[controller]")]` | Base URL of the controller |
| `[HttpGet]` `[HttpPost]` `[HttpPut("{id}")]` `[HttpDelete("{id}")]` | Connect an HTTP verb (+ optional URL part) to a method |
| `ControllerBase` | Base class for API controllers |
| `Ok(obj)` | 200 + data |
| `CreatedAtAction(action, routeValues, obj)` | 201 + `Location` header + data |
| `NoContent()` | 204, no data |
| `BadRequest(msg)` | 400 |
| `NotFound()` | 404 |

## EF Core

| Item | Hits the DB? | What it does |
|---|---|---|
| `DbContext` | — | Connection + change tracker + C# → SQL translator |
| `DbSet<T>` / `Set<T>()` | — | One table |
| `ToListAsync()` | ✅ | Run the query, return all rows |
| `FindAsync(id)` | ✅ (unless already tracked) | Find by primary key, `null` if missing |
| `AddAsync(e)` | ❌ | Mark `Added` |
| `Update(e)` | ❌ | Mark `Modified` |
| `Remove(e)` | ❌ | Mark `Deleted` |
| `SaveChangesAsync()` | ✅ | Run all pending INSERT/UPDATE/DELETE in one transaction |

## AutoMapper

| Item | What it does |
|---|---|
| `class MapperProfile : Profile` | A class that holds mapping rules (in its constructor) |
| `CreateMap<From, To>()` | Allow converting `From` into `To`; same-named properties are copied automatically |
| `.ForMember(dest => dest.X, opt => opt.Ignore())` | Never fill `X` |
| `.ForMember(dest => dest.X, opt => opt.MapFrom(src => ...))` | Fill `X` with a calculated value |
| `.ReverseMap()` | Also create the opposite direction (only if you really need it) |
| `AddAutoMapper(cfg => { }, typeof(MapperProfile))` | Register AutoMapper + load all profiles in that project |
| `IMapper` | What you inject into a controller |
| `_mapper.Map<To>(source)` | Convert one object (or a whole list: `Map<IEnumerable<To>>(list)`) |

## Data annotation attributes

| Attribute | On an Input DTO | On an Entity |
|---|---|---|
| `[Key]` | — | Primary key (IDENTITY for `int`) |
| `[Required]` | 400 if missing | `NOT NULL` |
| `[MaxLength(n)]` | 400 if too long | `nvarchar(n)` |
| `[Range(a, b)]` | 400 if outside range | — |
| `[Column(TypeName = "...")]` | — | exact SQL type |

## C# glossary (in the order you met them)

| Term | One-line meaning |
|---|---|
| top-level statements | `Program.cs` code with no class/`Main` around it |
| `var` | let the compiler work out the type |
| `namespace` / `using` | "folder" for code / "I'll use code from that folder" |
| `class` / object | blueprint / one thing made from the blueprint |
| property `{ get; set; }` | a value stored on an object |
| attribute `[...]` | a label read by other code |
| `= string.Empty` | default value so a string is never null |
| inheritance `: Base` | "is a Base, plus more" |
| constructor | method that runs when the object is created |
| `base(...)` | pass values to the parent's constructor |
| generics `<T>` | "works for any type T" |
| lambda `x => ...` | a small nameless function |
| `interface` | a contract: method list with no code |
| `Task`, `async`, `await` | promise of a result later / method that waits / wait without blocking |
| `T?` | may be null |
| `IEnumerable<T>` | a sequence you can loop over |
| `private readonly` field | internal value set once in the constructor |
| constructor injection | DI container passes dependencies into the constructor |
| `if` / early `return` | stop early when something is wrong |
| `throw` / `try` / `catch` | raise an error / run code that may fail / handle the error |
| `static` method | works only with its inputs, not the object's fields |
| ternary `a ? b : c` | one-line if/else |
| `nameof(x)` | the name of `x` as text, checked by the compiler |
| anonymous object `new { a = 1 }` | quick throw-away object |
| object initializer `new X { A = 1 }` | create and set properties in one go |
| DTO (Data Transfer Object) | a plain class describing the JSON coming in or going out |
| Input DTO / Response DTO | what the client may send / what the client gets back |
| mapping / mapper | copying values from one class to another / the tool that does it (AutoMapper) |
| `Profile` | AutoMapper class where the mapping rules are written |

---

# Appendix B — Trainer notes

## Small differences from the EMS reference project

| EMS | TMS | Why |
|---|---|---|
| `void DeleteAsync` / `void UpdateAsync` | `void Delete` / `void Update` | The `Async` suffix is for methods that return a `Task` |
| `Task<TEntity> GetByIdAsync` | `Task<TEntity?> GetByIdAsync` | Makes "may be null" explicit |
| `AddTransient` | `AddScoped` | Matches the `DbContext` lifetime |
| Controller takes and returns the Entity | Input DTO in, Response DTO out | Client can't set Id / Status / CreatedOn; table changes don't leak into the JSON |
| `EmployeeModel` (one Model class) | `TravelRequestInputDto` + `TravelRequestResponseDto` | Different shapes for in and out |
| Only GetAll + Insert | Full CRUD | — |
| AutoMapper **15.1.0** | AutoMapper **15.1.3** | 15.1.0 has a high-severity advisory (GHSA-rvv3-g6hj-g44x); 15.1.3 is patched |
| Mapper registration commented out: `new MapperConfiguration(...).CreateMapper()` | `AddAutoMapper(cfg => { }, typeof(MapperProfile))` | In v15 `MapperConfiguration` also needs a logger; `AddAutoMapper` handles it |
| `CreateMap<EmployeeModel, Employee>().ReverseMap()` | One `CreateMap` per direction actually used, with explicit `Ignore()` | Only map what you need; protected fields are visibly protected |
| Folder + namespace `MapperProfile` with class `MapperProfile` | Folder `Mapping`, class `MapperProfile` | A class named the same as its namespace causes confusing compiler errors |

## Exercises for the freshers (same pattern, next flows from the spec)

1. **Submit a draft:** `POST /api/TravelRequest/{id}/submit`. The service checks `Status == "Draft"` and sets
   `"Submitted"`. Then block Update and Delete when the status is not Draft.
2. **Add a column:** add `TravelClass` (Economy / Business) to the entity → `Add-Migration` → `Update-Database`.
   Then add it to the Input DTO and the Response DTO, and notice that AutoMapper needs **no change**
   (same name → copied automatically).
3. **Policy check:** reject a request whose `EstimatedBudget` is above a fixed limit (a rule in the service → 400).
4. **New feature, Expenses:** entity `Expense` (`TravelRequestId`, `Category`, `Amount`) + `DbSet` +
   migration + `ExpenseInputDto` / `ExpenseResponseDto` + two `CreateMap` lines + service + controller.
   No new repository code is needed: `IRepository<Expense>` already works.
