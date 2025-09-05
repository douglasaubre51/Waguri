namespace Api.Services
{
    public class SessionService
    {
        public bool IsExpired(HttpContext context)
        {
            if (context.Session.GetString("projectId") is null) return true;

            return false;
        }

        public string? GetProjectId(HttpContext context)
            => context.Session.GetString("projectId");

        public string? GetProjectUrl(HttpContext context)
            => context.Session.GetString("projectUrl");

        public string? GetApiUrl(HttpContext context)
            => context.Session.GetString("apiUrl");
    }
}
