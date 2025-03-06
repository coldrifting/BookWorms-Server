using System.Reflection;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using BookwormsServer.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookwormsServer;

public static class Config
{
	public static void AuthenticationOptions(AuthenticationOptions opt)
	{
		opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	}

	public static void JwtBearerOptions(JwtBearerOptions opt)
	{
		opt.RequireHttpsMetadata = false;
		opt.SaveToken = true;
		opt.TokenValidationParameters = new()
		{
			IssuerSigningKey = new SymmetricSecurityKey(AuthService.SecretBytes), 
			ValidateIssuer = false,
			ValidateAudience = false
		};
		// 401 & 403 use custom error model, and check user is not deleted
		opt.Events = AuthService.BearerEvents();
	}
	
    public static void JsonOptions(JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
    
	public static void SwaggerGenOptions(SwaggerGenOptions opt)
	{
		// Show padlock icon only on authenticated methods
		opt.OperationFilter<AuthenticationFilter>();

		// Setup authorize box
		opt.SwaggerDoc("v1", new() { Title = "MyAPI", Version = "v1" });
		opt.AddSecurityDefinition("Bearer",
			new()
			{
				Name = "Authorization",
				Description = "Enter API Token Here",
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer"
			});

		opt.ExampleFilters();

		string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
		opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
	}
	
    public static void DbContextOptionsBuilder(DbContextOptionsBuilder opt, string? connectionString)
    {
	    opt.UseMySql(connectionString,
		    ServerVersion.AutoDetect(connectionString),
		    mySqlOpt => { mySqlOpt.EnablePrimitiveCollectionsSupport(); });
    }
    
    // Application Middleware
    
	public static void CorsPolicy(CorsPolicyBuilder o)
	{
		o.WithOrigins("https://www.bookworms.app", "https://bookworms.app")
			.WithExposedHeaders("*")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials();
	}
    
    public static void UseShortUrls(this WebApplication app)
    {
		// Redirect to API or App from base URL depending on if production or development
        app.Use((context, next) =>
		{
			var url = context.Request.Path.Value;
			if (url is "/" or "/index.html")
			{
				context.Request.Path = app.Environment.IsProduction()
					? "/app/index.html"
					: "/api/index.html";
			}

			return next();
		});

		// Use short urls
		app.Use((context, next) =>
		{
			context.Request.Path = context.Request.Path.Value switch
			{
				"/api" or "/api/" or "/api/index.html" => "/swagger/index.html",
				"/app" or "/app/" or "/app/index.html" => "/app/index.html",
				_ => context.Request.Path
			};

			return next();
		});
    }
    
    public static void MigrateDatabase(this WebApplication app, bool seedDatabase = false)
    {
		if (!app.Environment.IsProduction())
		{
			using var serviceScope = app.Services.CreateScope();
			var dbContext = serviceScope.ServiceProvider.GetRequiredService<BookwormsDbContext>();

			// If running the application or the tests fails here, you need to drop your database and then try again
			if (dbContext.Database.GetPendingMigrations().Any())
			{
				dbContext.Database.EnsureDeleted();
				dbContext.Database.Migrate();
			}

			if (seedDatabase)
			{
				dbContext.SeedTestData();
			}
		}
    }
}