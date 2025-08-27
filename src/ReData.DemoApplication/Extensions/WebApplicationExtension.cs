using Microsoft.EntityFrameworkCore;

namespace ReData.DemoApplication.Extensions;

public static class WebApplicationExtension
{
    public static void Migrate<T>(this WebApplication webApplication)
        where T : DbContext
    {
        using (var scope = webApplication.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<T>();
            db.Database.Migrate();
        }
    }
}
