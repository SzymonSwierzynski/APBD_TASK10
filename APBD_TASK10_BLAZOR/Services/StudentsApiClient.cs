using System.Net;
using System.Net.Http.Json;
using APBD_TASK10_BLAZOR.Models;

namespace APBD_TASK10_BLAZOR.Services;

public class ApiException : Exception
{
    public ApiException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}

public class StudentsApiClient
{
    private readonly HttpClient _http;

    public StudentsApiClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<StudentDto>> GetStudentsAsync(CancellationToken ct = default)
    {
        try
        {
            var students = await _http.GetFromJsonAsync<List<StudentDto>>("api/students", ct);
            return students ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("Could not load the student list from the API.", ex);
        }
    }

    public async Task<StudentDetailsDto?> GetStudentAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/students/{id}", ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StudentDetailsDto>(cancellationToken: ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException($"Could not load student #{id} from the API.", ex);
        }
    }

    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(CancellationToken ct = default)
    {
        try
        {
            var courses = await _http.GetFromJsonAsync<List<CourseDto>>("api/courses", ct);
            return courses ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("Could not load the course list from the API.", ex);
        }
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/students", request, ct);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<StudentDto>(cancellationToken: ct);
            return created ?? throw new ApiException("The API did not return the created student.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("Could not create the student. The API did not respond correctly.", ex);
        }
    }

    public async Task<AssignCourseClientResult> AssignCourseAsync(int studentId, AssignCourseDto request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/students/{studentId}/courses", request, ct);
            return response.StatusCode switch
            {
                HttpStatusCode.NoContent => AssignCourseClientResult.Success,
                HttpStatusCode.Conflict => AssignCourseClientResult.AlreadyAssigned,
                HttpStatusCode.NotFound => AssignCourseClientResult.NotFound,
                _ => throw new ApiException($"Unexpected API response: {(int)response.StatusCode}."),
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("Could not assign the course. The API did not respond correctly.", ex);
        }
    }
}

public enum AssignCourseClientResult
{
    Success,
    AlreadyAssigned,
    NotFound,
}
