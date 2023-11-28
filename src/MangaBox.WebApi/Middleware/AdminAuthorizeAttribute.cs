namespace MangaBox.WebApi;

public class AdminAuthorizeAttribute : AuthorizeAttribute
{
    public AdminAuthorizeAttribute()
    {
        Roles = "Admin";
    }
}
