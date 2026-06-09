# APBD Task 10 - Blazor

This is my Blazor app for managing students and their courses. It loads data from a REST API using HttpClient. You can see the student list, open student details, add a new student, assign a course to a student and mark students as observed.

## How to run the Blazor app

You need the .NET 10 SDK.

Go to the APBD_TASK10_BLAZOR folder and run:

dotnet run --project APBD_TASK10_BLAZOR/APBD_TASK10_BLAZOR.csproj

Then open http://localhost:5208 in the browser. There is also an https profile on https://localhost:7266. You can also open the solution in Rider and press run.

## How to run the API

I did not make a separate API project. The API is in the same project as the Blazor app, so when you run the app the API runs too. The endpoints are in Api/StudentsApi.cs and they are added in Program.cs with app.MapStudentsApi(). The data is kept in memory in Data/InMemoryDataStore.cs with some example students and courses.

The endpoints are:
- GET /api/students - list of students
- GET /api/students/{id} - one student with the assigned courses
- POST /api/students - add a student
- GET /api/courses - list of courses
- POST /api/students/{id}/courses - assign a course to a student

Even though the API is in the same project, the UI only talks to it through HttpClient, it does not use the data store directly.

## Which Blazor variant I chose

I used Blazor Web App with Interactive Server render mode. I picked it because it is simple and good for a panel with forms and tables, and the code runs on the server so I can use dependency injection easily. I turned off prerendering in Components/App.razor so the lifecycle methods only run once.

## Where things are

Typed client / API communication:
Services/StudentsApiClient.cs has all the methods for calling the API (GetStudentsAsync, GetStudentAsync, GetCoursesAsync, CreateStudentAsync, AssignCourseAsync). It is registered in Program.cs. The HttpClient base address comes from NavigationManager.BaseUri so it works on any port. I do not create a new HttpClient in the components.

OnInitializedAsync:
Used in Students.razor to load the student list, and in StudentDetails.razor to load the course list for the assign form.

OnParametersSetAsync:
Used in StudentDetails.razor to load the student by the id from the url. It runs again when you go from /students/1 to /students/2.

OnAfterRenderAsync:
Used in StudentDetails.razor, CreateStudent.razor and ObservedStudents.razor to import the JavaScript module after the first render (with the firstRender check). CreateStudent.razor also reads the last semester from localStorage there.

EditForm and validation:
CreateStudent.razor has the create form with EditForm, OnValidSubmit, DataAnnotationsValidator, ValidationSummary and ValidationMessage. StudentDetails.razor has the assign course form with EditForm and InputSelect. The validation rules are on the DTOs in Models/Dtos.cs (CreateStudentDto and AssignCourseDto).

StateContainer:
Services/ObservedStudentsState.cs keeps the observed students. It is registered as Scoped in Program.cs, not Singleton. The count is shown in MainLayout.razor and NavMenu.razor and it updates when you observe or unobserve someone.

JS Interop:
wwwroot/js/interop.js has the functions (copyToClipboard, confirmDialog, saveLastSemester, getLastSemester). Copy email is in StudentDetails.razor, the confirm dialog before removing is in ObservedStudents.razor, and saving the last semester in localStorage is in CreateStudent.razor. The module reference is released in DisposeAsync (IAsyncDisposable).

RenderFragment / RenderFragment<T>:
Components/Shared/DataTable.razor is a generic table that uses RenderFragment (HeaderTemplate, EmptyTemplate) and RenderFragment<TItem> (RowTemplate). It is used in Students.razor and ObservedStudents.razor. LoadingPanel.razor and ApiError.razor are also reusable components.

ErrorBoundary:
MainLayout.razor wraps the page body in an ErrorBoundary and resets it when you navigate. Students.razor also has an ErrorBoundary around the list. Normal API errors like the API being down or a 404 are handled with try/catch and the ApiError component, not with the ErrorBoundary.

Routing and 404:
The pages use @page and the menu uses NavLink. After creating a student I use NavigationManager to go to the details page. A wrong url shows the 404 page in NotFound.razor.

## Questions

How is OnInitializedAsync different from OnParametersSetAsync?
OnInitializedAsync runs one time when the component is created. OnParametersSetAsync runs the first time and also every time a parameter changes, so it is good for loading data that depends on a route parameter like the id.

Why do we usually run DOM-dependent code in OnAfterRenderAsync?
Because the HTML and the JavaScript are only ready after the component has rendered. Before that there is nothing to work with, so DOM and JS code goes in OnAfterRenderAsync, usually with the firstRender check so it only happens once.

Why should you be careful with state registered as Singleton in Blazor Server?
A Singleton is shared by all users. In Blazor Server every user has their own circuit, so per user state like the observed students should be Scoped. If it was Singleton everyone would share the same list.

What does a typed client give you compared to calling HttpClient directly in every component?
You have one place with all the urls, the json handling and the error handling. The components just call methods like GetStudentsAsync. It is less repeating and you do not make a new HttpClient in every component.

How is NavLink different from a regular <a> link?
NavLink adds an active css class when the current url matches its href, so the current page can be highlighted in the menu. A normal <a> does not do that.

What is RenderFragment<T> used for?
It is a template that gets an item. The component calls it for each item and passes the item in. This lets a generic component like DataTable let the caller decide how each row looks.

When does JS Interop make sense, and when is it better to stay with Blazor?
JS Interop makes sense for browser only things like clipboard, localStorage or a confirm dialog. For normal things like rendering, events and data binding it is better to stay with Blazor because it already does that.

What problem does ErrorBoundary solve, and what should it not replace?
ErrorBoundary catches unexpected exceptions while rendering and shows a fallback instead of breaking the whole page. It should not replace normal error handling like validation or showing a message when the API is down, those should be done with try/catch.
