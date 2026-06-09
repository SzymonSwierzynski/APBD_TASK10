# Student Panel (Blazor)

A small **Blazor** panel for managing students and their courses. It loads data
from a REST API over **HttpClient**, uses validated forms (`EditForm` + Data
Annotations), routing, shared application state, a small JavaScript Interop
example, reusable/templated components, a layout, and reasonable error handling.

The whole UI is built as `.razor` components — this is **not** a classic MVC
application.

---

## How to run

Requirements: **.NET 10 SDK** (the project targets `net10.0`).

### Run the Blazor application

From the solution folder (`APBD_TASK10_BLAZOR/`):

```bash
dotnet run --project APBD_TASK10_BLAZOR/APBD_TASK10_BLAZOR.csproj
```

Then open the URL printed in the console, e.g. <http://localhost:5208>.

There are two launch profiles in
[`Properties/launchSettings.json`](APBD_TASK10_BLAZOR/Properties/launchSettings.json):

* `http`  → <http://localhost:5208> (used by `dotnet run` by default)
* `https` → <https://localhost:7266>

You can also open `APBD_TASK10_BLAZOR.sln` in Rider / Visual Studio and press
Run.

### How to run the API

The REST API is hosted **inside the same ASP.NET Core process** as the Blazor
app (this is the "simplified variant with in-memory data" allowed by the task).
So when you run the Blazor application above, the API is already running too.

The endpoints live in
[`Api/StudentsApi.cs`](APBD_TASK10_BLAZOR/Api/StudentsApi.cs) and are mapped in
[`Program.cs`](APBD_TASK10_BLAZOR/Program.cs) via `app.MapStudentsApi();`. The
data is kept in a singleton in-memory store
([`Data/InMemoryDataStore.cs`](APBD_TASK10_BLAZOR/Data/InMemoryDataStore.cs)),
seeded with a few students and courses.

You can hit the API directly to confirm it works:

```bash
curl http://localhost:5208/api/students
curl http://localhost:5208/api/students/1
curl http://localhost:5208/api/courses
curl -X POST http://localhost:5208/api/students \
  -H "Content-Type: application/json" \
  -d '{"indexNumber":"s30030","firstName":"Pawel","lastName":"Zielinski","email":"pawel@example.com","semester":5}'
curl -i -X POST http://localhost:5208/api/students/2/courses \
  -H "Content-Type: application/json" -d '{"courseId":3}'
```

| Method & route                     | Purpose                          |
| ---------------------------------- | -------------------------------- |
| `GET /api/students`                | student list                     |
| `GET /api/students/{id}`           | student details + courses        |
| `POST /api/students`               | create a student                 |
| `GET /api/courses`                 | course list                      |
| `POST /api/students/{id}/courses`  | assign a course to a student     |

> **Important:** even though the API is in the same process, the UI talks to it
> **only over HTTP** through a typed `HttpClient`. The UI never touches
> `InMemoryDataStore` directly.

---

## Which Blazor variant and why

**Blazor Web App, global render mode `InteractiveServer` (prerendering off).**

* It is an internal admin-style panel (forms, lists, tables) — exactly the
  scenario where Interactive Server shines: fast startup, small payload, direct
  access to server-side DI.
* It keeps the project to a single, easy-to-run project.
* Prerendering is disabled in [`Components/App.razor`](APBD_TASK10_BLAZOR/Components/App.razor)
  so each component's lifecycle runs **once** (the data pages show their own
  loading state while the SignalR circuit connects, and JS interop only runs
  once the client is actually interactive).

The render mode is applied globally to `<Routes>` and `<HeadOutlet>` in
`App.razor`.

---

## Where to find each required mechanism

### Typed client / API communication
* [`Services/StudentsApiClient.cs`](APBD_TASK10_BLAZOR/Services/StudentsApiClient.cs)
  — the single place that knows the HTTP routes (`GetStudentsAsync`,
  `GetStudentAsync`, `GetCoursesAsync`, `CreateStudentAsync`,
  `AssignCourseAsync`). Components never `new` an `HttpClient`.
* Registered in [`Program.cs`](APBD_TASK10_BLAZOR/Program.cs). The `HttpClient`
  base address is taken from the running app itself
  (`NavigationManager.BaseUri`), so it works on whatever http/https port you
  launch with.

### Component lifecycle
* `OnInitializedAsync` — load data needed **once** when the component starts:
  * student list in [`Components/Pages/Students.razor`](APBD_TASK10_BLAZOR/Components/Pages/Students.razor)
  * course list (for the assign form) in [`Components/Pages/StudentDetails.razor`](APBD_TASK10_BLAZOR/Components/Pages/StudentDetails.razor)
* `OnParametersSetAsync` — load data that depends on the **route parameter**
  `{id}`: student details in
  [`Components/Pages/StudentDetails.razor`](APBD_TASK10_BLAZOR/Components/Pages/StudentDetails.razor)
  (re-runs when navigating `/students/1` → `/students/2`).
* `OnAfterRenderAsync(firstRender)` — DOM/JS-module work after rendering:
  importing the JS module in
  [`StudentDetails.razor`](APBD_TASK10_BLAZOR/Components/Pages/StudentDetails.razor),
  [`CreateStudent.razor`](APBD_TASK10_BLAZOR/Components/Pages/CreateStudent.razor)
  (also restores the last semester from `localStorage`), and
  [`ObservedStudents.razor`](APBD_TASK10_BLAZOR/Components/Pages/ObservedStudents.razor).

### EditForm + validation
* Create form: [`Components/Pages/CreateStudent.razor`](APBD_TASK10_BLAZOR/Components/Pages/CreateStudent.razor)
  — `EditForm` with `OnValidSubmit`, `DataAnnotationsValidator`,
  `ValidationSummary`, `InputText` / `InputNumber`, and per-field
  `ValidationMessage`. After a valid submit it redirects to the new student's
  details page.
* Assign-course form: [`Components/Pages/StudentDetails.razor`](APBD_TASK10_BLAZOR/Components/Pages/StudentDetails.razor)
  — `EditForm` with `InputSelect`; the validation rule guarantees an existing
  course is selected.
* The validation rules themselves live on the request DTOs in
  [`Models/Dtos.cs`](APBD_TASK10_BLAZOR/Models/Dtos.cs) (`CreateStudentDto`,
  `AssignCourseDto`).

### StateContainer (shared state)
* [`Services/ObservedStudentsState.cs`](APBD_TASK10_BLAZOR/Services/ObservedStudentsState.cs)
  — holds the observed students, exposes `Count` and an `OnChange` event.
  Registered as **Scoped** in [`Program.cs`](APBD_TASK10_BLAZOR/Program.cs)
  (one per user circuit). The counter is shown in the layout
  ([`Components/Layout/MainLayout.razor`](APBD_TASK10_BLAZOR/Components/Layout/MainLayout.razor))
  and in the menu
  ([`Components/Layout/NavMenu.razor`](APBD_TASK10_BLAZOR/Components/Layout/NavMenu.razor)),
  and updates live whenever you observe/unobserve a student.

### JS Interop
* Module: [`wwwroot/js/interop.js`](APBD_TASK10_BLAZOR/wwwroot/js/interop.js)
  (`copyToClipboard`, `confirmDialog`, `saveLastSemester` / `getLastSemester`).
* Used in:
  * `StudentDetails.razor` — **Copy email** to the clipboard.
  * `CreateStudent.razor` — save/restore the last chosen semester in
    `localStorage`.
  * `ObservedStudents.razor` — a native **confirm** dialog before removing.
* Every component that imports the module releases the reference via
  **`IAsyncDisposable.DisposeAsync`** (and swallows `JSDisconnectedException`).

### Reusable component with RenderFragment / RenderFragment&lt;T&gt;
* [`Components/Shared/DataTable.razor`](APBD_TASK10_BLAZOR/Components/Shared/DataTable.razor)
  — generic (`@typeparam TItem`) table using `RenderFragment` (`HeaderTemplate`,
  `EmptyTemplate`) and `RenderFragment<TItem>` (`RowTemplate`). It is used in two
  places: the student list (`Students.razor`) and the observed list
  (`ObservedStudents.razor`).
* Two more shared components:
  [`LoadingPanel.razor`](APBD_TASK10_BLAZOR/Components/Shared/LoadingPanel.razor)
  and [`ApiError.razor`](APBD_TASK10_BLAZOR/Components/Shared/ApiError.razor),
  reused across pages.

### ErrorBoundary
* [`Components/Layout/MainLayout.razor`](APBD_TASK10_BLAZOR/Components/Layout/MainLayout.razor)
  wraps `@Body` in an `ErrorBoundary` (reset on each navigation) as a backstop
  for **unexpected** exceptions.
* [`Components/Pages/Students.razor`](APBD_TASK10_BLAZOR/Components/Pages/Students.razor)
  wraps the list in a second `ErrorBoundary` with custom `ErrorContent`.
* These do **not** replace normal error handling: expected API failures (network
  down, 404) are caught with `try/catch` and shown via the `ApiError` component.

### Routing / 404
* Routes are defined with `@page` directives; navigation uses `NavLink`
  ([`NavMenu.razor`](APBD_TASK10_BLAZOR/Components/Layout/NavMenu.razor)) and
  `NavigationManager` (redirect after create).
* A non-existent route shows a real 404 page
  ([`Components/Pages/NotFound.razor`](APBD_TASK10_BLAZOR/Components/Pages/NotFound.razor)),
  wired through `app.UseStatusCodePagesWithReExecute("/not-found", ...)` and the
  router's `NotFoundPage`.

---

## Pages / routes

| Route                  | Component                | What it does                                  |
| ---------------------- | ------------------------ | --------------------------------------------- |
| `/`                    | `Home.razor`             | landing page with links                       |
| `/students`            | `Students.razor`         | student list (HTTP, loading + error states)   |
| `/students/{id:int}`   | `StudentDetails.razor`   | details, assigned courses, assign-course form |
| `/students/create`     | `CreateStudent.razor`    | validated create form, redirects on success   |
| `/observed`            | `ObservedStudents.razor` | observed students (shared state)              |
| anything else          | `NotFound.razor`         | 404                                           |

---

## Questions for README

**How is `OnInitializedAsync` different from `OnParametersSetAsync`?**
`OnInitializedAsync` runs once, when the component instance is created. It is for
one-time setup / data that does not depend on parameters (e.g. the course list).
`OnParametersSetAsync` runs after the component receives parameters — on the
first render **and** every time a parameter (such as a route value) changes, so
it is the right place to (re)load data that depends on `{id}`.

**Why do we usually run DOM-dependent code in `OnAfterRenderAsync`?**
The DOM and the JavaScript runtime exist only **after** the component has
rendered on the client. Before that (and during prerendering) there is no
element to touch and no JS to call. `OnAfterRenderAsync` with the `firstRender`
flag lets us safely run JS interop / DOM work exactly once, after the markup is
in place.

**Why should you be careful with state registered as Singleton in Blazor
Server?**
A Singleton is shared by **every** user of the server. In Blazor Server each
user has their own circuit, so per-user UI state (like "my observed students")
must be **Scoped** — one instance per circuit. A Singleton would leak one user's
state to everyone and cause concurrency problems. That is why
`ObservedStudentsState` is registered as Scoped.

**What does a typed client give you compared to calling `HttpClient` directly in
every component?**
One place that owns the routes, serialization, and error translation. Components
call meaningful methods (`GetStudentsAsync()`) instead of repeating URLs and
JSON handling; the base address and error handling are configured once; it is
easier to test and to change. It also stops people from `new`-ing an
`HttpClient` per component.

**How is `NavLink` different from a regular `<a>` link?**
`NavLink` renders an anchor but also adds an `active` CSS class when the current
URL matches its `href` (with `NavLinkMatch.All` / `Prefix`). A plain `<a>` has no
notion of the active route, so you would have to compute the highlight yourself.

**What is `RenderFragment<T>` used for?**
It is a template parameterized by an item: the caller supplies markup and the
component invokes it once per item, passing that item in. It lets a generic
component (like `DataTable<TItem>`) stay reusable while each caller decides how a
single row/item is rendered.

**When does JS Interop make sense, and when is it better to stay with Blazor?**
Use JS interop for things the browser only exposes to JavaScript — clipboard,
`localStorage`, native dialogs, focus, a specific JS chart library, etc. Stay
with Blazor for anything it already does well: rendering, events, data binding,
conditional UI. Reaching for JS to do what Blazor does anyway just adds
complexity and breaks the component model.

**What problem does `ErrorBoundary` solve, and what should it not replace?**
It catches **unexpected** exceptions thrown while rendering a part of the UI and
shows fallback content instead of taking down the whole circuit/page. It should
**not** replace normal business error handling — expected outcomes (validation
errors, "not found", an API that is down) should be handled explicitly with
`try/catch` and user-friendly messages (here: the `ApiError` component and the
"Student not found" panel), not by letting them bubble into an `ErrorBoundary`.
# APBD_TASK10
