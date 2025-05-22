namespace Domain.Posts;

public interface IPostCounterRepository
{
    Task<int> GetCountAsync(string counterType);
    Task SetCountAsync(string counterType, int count);
    Task IncrementAsync(string counterType, int delta);
}
