using Amazon.DynamoDBv2.DataModel;

namespace Domain.Contacts;

[DynamoDBTable("Contacts")]
public class Contact
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
