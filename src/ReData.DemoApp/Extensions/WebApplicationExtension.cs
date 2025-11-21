using Microsoft.EntityFrameworkCore;

namespace ReData.DemoApp.Extensions;

public static class WebApplicationExtension
{
    public static void Migrate<T>(this IServiceProvider serviceProvider)
        where T : DbContext
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<T>();
            db.Database.Migrate();
        }
        
    }
    
}
