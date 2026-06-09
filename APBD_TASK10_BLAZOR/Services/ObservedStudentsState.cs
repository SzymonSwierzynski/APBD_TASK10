using APBD_TASK10_BLAZOR.Models;

namespace APBD_TASK10_BLAZOR.Services;

public class ObservedStudentsState
{
    private readonly Dictionary<int, StudentDto> _observed = new();

    public event Action? OnChange;

    public IReadOnlyCollection<StudentDto> Observed => _observed.Values;

    public int Count => _observed.Count;

    public bool IsObserved(int studentId) => _observed.ContainsKey(studentId);

    public void Add(StudentDto student)
    {
        if (_observed.TryAdd(student.Id, student))
        {
            NotifyStateChanged();
        }
    }

    public void Remove(int studentId)
    {
        if (_observed.Remove(studentId))
        {
            NotifyStateChanged();
        }
    }

    public void Toggle(StudentDto student)
    {
        if (IsObserved(student.Id))
        {
            Remove(student.Id);
        }
        else
        {
            Add(student);
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
