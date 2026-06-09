using System.ComponentModel.DataAnnotations;

namespace APBD_TASK10_BLAZOR.Models;

public record StudentDto
{
    public int Id { get; init; }
    public string IndexNumber { get; init; } = "";
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Email { get; init; } = "";
    public int Semester { get; init; }

    public string FullName => $"{FirstName} {LastName}";
}

public record CourseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public int Ects { get; init; }
}

public record StudentCourseDto
{
    public int StudentId { get; init; }
    public int CourseId { get; init; }
    public DateTime AssignedAt { get; init; }
}

public record StudentDetailsDto
{
    public required StudentDto Student { get; init; }
    public IReadOnlyList<CourseDto> Courses { get; init; } = [];
}

public class CreateStudentDto
{
    [Required(ErrorMessage = "Index number is required.")]
    public string IndexNumber { get; set; } = "";

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string Email { get; set; } = "";

    [Range(1, 8, ErrorMessage = "Semester must be a number from 1 to 8.")]
    public int Semester { get; set; } = 1;
}

public class AssignCourseDto
{
    [Required(ErrorMessage = "Please select a course.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an existing course.")]
    public int CourseId { get; set; }
}
