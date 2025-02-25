using System.Reflection;
using System.Text.Json.Serialization;
using BookwormsServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BookwormsServer.Services;
using BookwormsServer.Services.Interfaces;
using BookwormsServer.Swagger;
using Swashbuckle.AspNetCore.Filters;

// -------------------------------------------------------------------------------------------------------------

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Enable Systemd support
builder.Services.AddSystemd();

// Establish database context ----------------------------------------------------------------------------------

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BookwormsDbContext>(opt =>
{
	opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOpt =>
	{
		mySqlOpt.EnablePrimitiveCollectionsSupport();
	});
});

// Configure Swagger -------------------------------------------------------------------------------------------

// Use lowercase api endpoints
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Return enums in response bodies as strings, not numbers
builder.Services.AddControllers(opt => 
	opt.Filters.Add(new ProducesAttribute("application/json"))).AddJsonOptions(options =>
{
	options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

	options.JsonSerializerOptions.DefaultIgnoreCondition =
		JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddResponseCaching();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(opt =>
{
	// Show padlock icon only on authenticated methods
	opt.OperationFilter<AuthenticationFilter>();
	
	// Setup authorize box
	opt.SwaggerDoc("v1", new() { Title = "MyAPI", Version = "v1" });
	opt.AddSecurityDefinition("Bearer", new()
	{
		Name = "Authorization",
		Description = "Enter API Token Here",
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer"
	});
	
	opt.ExampleFilters();

	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerExamples.ImagesRequestBodyExample>();

// Authorization & Authentication
builder.Services
	.AddAuthentication(opt =>
	{
		opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(opt =>
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
	});
builder.Services.AddAuthorization();


// Register services -------------------------------------------------------------------------------------------
// (Dependency Injections)

builder.Services.AddSingleton<IBookApiService, OpenLibraryApiService>();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// -------------------------------------------------------------------------------------------------------------


WebApplication app = builder.Build();


// Ensure the database is migrated -----------------------------------------------------------------------------

// (This is best done here, after the WebApplication has been created)
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
}


// Configure the HTTP request pipeline -------------------------------------------------------------------------

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
	var url = context.Request.Path.Value;
	
	if (url is "/api" or "/api/" or "/api/index.html")
	{
		context.Request.Path = "/swagger/index.html";
	}

	if (url is "/app" or "/app/" or "/app/index.html")
	{
		context.Request.Path = "/app/index.html";
	}
	
	return next();
});

// Don't bother with static files and other web stuff in Staging, which is only used for running tests
if (!app.Environment.IsStaging())
{
	app.UseCors(o => o.WithOrigins(
		"https://www.bookworms.app",
		"https://bookworms.app")
		.WithExposedHeaders("*")
		.AllowAnyMethod()
		.AllowAnyHeader()
		.AllowCredentials());
	
	app.UseHttpsRedirection();
	app.UseStaticFiles();
	app.UseAuthentication();
	app.UseAuthorization();
}

// Generate Swagger json API file (But generate manually with static files)
app.UseSwagger();

// Enable endpoint caching
app.UseResponseCaching();

// Endpoints can be split into separate files in the Controllers folder
app.MapControllers();

// Persist test data -------------------------------------------------------------------------------------------

if (!app.Environment.IsProduction())
{
	using var serviceScope = app.Services.CreateScope();
	var dbContext = serviceScope.ServiceProvider.GetRequiredService<BookwormsDbContext>();
	dbContext.SeedTestData();
}

// -------------------------------------------------------------------------------------------------------------

app.Run();

// -------------------------------------------------------------------------------------------------------------

public abstract partial class Program;