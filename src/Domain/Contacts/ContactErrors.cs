using SharedKernel;

namespace Domain.Contacts;

public static class ContactErrors
{
    public static Error NotFound(Guid contactId) => Error.NotFound(
        "Contact.NotFound",
        $"The contact info with the Id = '{contactId}' was not found");
    
   
}
