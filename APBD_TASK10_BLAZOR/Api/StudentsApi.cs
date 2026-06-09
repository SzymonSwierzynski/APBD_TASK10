using System.ComponentModel.DataAnnotations;
using APBD_TASK10_BLAZOR.Data;
using APBD_TASK10_BLAZOR.Models;

namespace APBD_TASK10_BLAZOR.Api;

public static class StudentsApi
{
    public static IEndpointRouteBuilder MapStudentsApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("Students");

        group.MapGet("/students", (InMemoryDataStore store) =>
            Results.Ok(store.GetStudents()));

        group.MapGet("/students/{id:int}", (int id, InMemoryDataStore store) =>
        {
            var details = store.GetStudentDetails(id);
            return details is null
                ? Results.NotFound(new { message = $"Student {id} was not found." })
                : Results.Ok(details);
        });

        group.MapPost("/students", (CreateStudentDto request, InMemoryDataStore store) =>
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true))
            {
                var errors = validationResults
                    .SelectMany(r => r.MemberNames.DefaultIfEmpty(""), (r, member) => new { member, r.ErrorMessage })
                    .GroupBy(x => x.member)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage ?? "").ToArray());
                return Results.ValidationProblem(errors);
            }

            var created = store.CreateStudent(request);
            return Results.Created($"/api/students/{created.Id}", created);
        });

        group.MapGet("/courses", (InMemoryDataStore store) =>
            Results.Ok(store.GetCourses()));

        group.MapPost("/students/{id:int}/courses", (int id, AssignCourseDto request, InMemoryDataStore store) =>
        {
            var result = store.AssignCourse(id, request.CourseId);
            return result switch
            {
                AssignCourseResult.Assigned => Results.NoContent(),
                AssignCourseResult.AlreadyAssigned => Results.Conflict(
                    new { message = "This course is already assigned to the student." }),
                _ => Results.NotFound(
                    new { message = "Student or course was not found." }),
            };
        });

        return app;
    }
}
