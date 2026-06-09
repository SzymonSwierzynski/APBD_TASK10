using APBD_TASK10_BLAZOR.Models;

namespace APBD_TASK10_BLAZOR.Data;

public class InMemoryDataStore
{
    private readonly object _gate = new();

    private readonly List<StudentDto> _students;
    private readonly List<CourseDto> _courses;
    private readonly List<StudentCourseDto> _assignments;

    private int _nextStudentId;

    public InMemoryDataStore()
    {
        _courses =
        [
            new CourseDto { Id = 1, Name = "Programming in C#", Ects = 6 },
            new CourseDto { Id = 2, Name = "Databases", Ects = 5 },
            new CourseDto { Id = 3, Name = "Web Applications", Ects = 4 },
            new CourseDto { Id = 4, Name = "Algorithms and Data Structures", Ects = 6 },
            new CourseDto { Id = 5, Name = "Operating Systems", Ects = 5 },
        ];

        _students =
        [
            new StudentDto
            {
                Id = 1, IndexNumber = "s20001", FirstName = "Anna", LastName = "Kowalska",
                Email = "anna.kowalska@example.com", Semester = 4
            },
            new StudentDto
            {
                Id = 2, IndexNumber = "s20002", FirstName = "Marek", LastName = "Nowak",
                Email = "marek.nowak@example.com", Semester = 2
            },
            new StudentDto
            {
                Id = 3, IndexNumber = "s20003", FirstName = "Julia", LastName = "Wisniewska",
                Email = "julia.wisniewska@example.com", Semester = 6
            },
        ];

        _assignments =
        [
            new StudentCourseDto { StudentId = 1, CourseId = 1, AssignedAt = DateTime.UtcNow.AddDays(-10) },
            new StudentCourseDto { StudentId = 1, CourseId = 2, AssignedAt = DateTime.UtcNow.AddDays(-5) },
            new StudentCourseDto { StudentId = 2, CourseId = 1, AssignedAt = DateTime.UtcNow.AddDays(-3) },
        ];

        _nextStudentId = _students.Max(s => s.Id) + 1;
    }

    public IReadOnlyList<StudentDto> GetStudents()
    {
        lock (_gate)
        {
            return _students.OrderBy(s => s.LastName).ToList();
        }
    }

    public StudentDetailsDto? GetStudentDetails(int id)
    {
        lock (_gate)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student is null)
            {
                return null;
            }

            var courseIds = _assignments
                .Where(a => a.StudentId == id)
                .Select(a => a.CourseId)
                .ToHashSet();

            var courses = _courses
                .Where(c => courseIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToList();

            return new StudentDetailsDto { Student = student, Courses = courses };
        }
    }

    public IReadOnlyList<CourseDto> GetCourses()
    {
        lock (_gate)
        {
            return _courses.OrderBy(c => c.Name).ToList();
        }
    }

    public StudentDto CreateStudent(CreateStudentDto request)
    {
        lock (_gate)
        {
            var student = new StudentDto
            {
                Id = _nextStudentId++,
                IndexNumber = request.IndexNumber.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                Semester = request.Semester,
            };

            _students.Add(student);
            return student;
        }
    }

    public AssignCourseResult AssignCourse(int studentId, int courseId)
    {
        lock (_gate)
        {
            var studentExists = _students.Any(s => s.Id == studentId);
            var courseExists = _courses.Any(c => c.Id == courseId);
            if (!studentExists || !courseExists)
            {
                return AssignCourseResult.NotFound;
            }

            var alreadyAssigned = _assignments
                .Any(a => a.StudentId == studentId && a.CourseId == courseId);
            if (alreadyAssigned)
            {
                return AssignCourseResult.AlreadyAssigned;
            }

            _assignments.Add(new StudentCourseDto
            {
                StudentId = studentId,
                CourseId = courseId,
                AssignedAt = DateTime.UtcNow,
            });

            return AssignCourseResult.Assigned;
        }
    }
}

public enum AssignCourseResult
{
    Assigned,
    AlreadyAssigned,
    NotFound,
}
