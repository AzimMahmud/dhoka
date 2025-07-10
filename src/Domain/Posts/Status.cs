namespace Domain.Posts;

public enum Status
{
    Init,
    Pending,
    Approved,
    Rejected,
    Settled,
}

public enum AnonymityPreference
{
    Public,
    Anonymous
}
