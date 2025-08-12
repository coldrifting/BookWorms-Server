using BookwormsServer.Services;
using BookwormsServer.Services.Interfaces;
using BookwormsServer.Swagger;
using Swashbuckle.AspNetCore.Filters;

namespace BookwormsServer;

public class Program
{
	public static void Main(string[] args)
	{
		// Configure Application Services
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

		builder.Services
			.AddSystemd()
			.AddSwaggerGen(Config.SwaggerGenOptions)
			.AddSwaggerExamplesFromAssemblyOf<SwaggerExamples.ImagesRequestBodyExample>()
		    
			.AddAuthorization()
		    .AddAuthentication(Config.AuthenticationOptions)
				.AddJwtBearer(Config.JwtBearerOptions).Services
				
			.AddDbContext<BookwormsDbContext>(opt => 
				Config.DbContextOptionsBuilder(opt, connectionString))
			
			.Configure<RouteOptions>(options => options.LowercaseUrls = true)
		    .AddControllers()
				.AddJsonOptions(Config.JsonOptions).Services
				
			.AddResponseCaching()
			.AddMemoryCache()
			
			.AddHttpClient()
			.AddSingleton<IBookApiService, OpenLibraryApiService>();

		// Configure Application HTTP Middleware - Order Matters!
		WebApplication app = builder.Build();

		// Ensure the database is migrated - Best done here, after the WebApplication has been created
		app.MigrateDatabase(seedDatabase: true);
		
		// Don't bother with static files and other web stuff in Staging, which is only used for running tests
		if (!app.Environment.IsStaging())
		{
			app.UseCors(Config.CorsPolicy);
			app.UseStaticFiles();
			app.UseAuthentication();
			app.UseAuthorization();
			
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.RoutePrefix = "";
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookworms API");
				
				c.InjectJavascript("/swagger/scripts/authorization-tweaks.js");
				c.InjectJavascript("/swagger/scripts/response-tweaks.js");

				c.InjectStylesheet("/swagger/stylesheets/themes/_base.css");
				c.InjectStylesheet("/swagger/stylesheets/themes/_custom.css");
				c.InjectStylesheet("/swagger/stylesheets/themes/one-dark.css");
				c.InjectStylesheet("/swagger/stylesheets/themes/one-light.css");

				c.ConfigObject.TryItOutEnabled = true;
				c.ConfigObject.DefaultModelsExpandDepth = -1;
			});
		}
		
		app.UseResponseCaching();
		app.MapControllers();

		// for quick health checks
		app.MapGet("/ping", () => "Healthy");
		app.Run();
	}
}