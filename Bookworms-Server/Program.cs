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

// Establish database context ----------------------------------------------------------------------------------

string? connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
	? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
	: builder.Configuration.GetConnectionString("DefaultConnection");
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
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(opt =>
{
	// Show padlock icon only on authenticated methods
	opt.OperationFilter<AuthenticationFilter>();
	
	// Setup authorize box
	opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
	opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
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
		opt.TokenValidationParameters = new TokenValidationParameters
		{
			IssuerSigningKey = new SymmetricSecurityKey(AuthService.SecretBytes),
			ValidateIssuer = false,
			ValidateAudience = false
		};
		// Make authorization failure (401 & 403) responses consistent with other bad requests
		opt.Events = AuthService.BearerEvents();
	});
builder.Services.AddAuthorization();


// Register services -------------------------------------------------------------------------------------------
// (Dependency Injections)

builder.Services.AddSingleton<IBookApiService, OpenLibraryApiService>();
// builder.Services.AddSingleton<IBookApiService, GoogleBooksApiService>();
// builder.Services.AddSingleton<IBookApiService, TestDataApiService>();

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

app.UseSwagger();
app.UseSwaggerUI(opt =>
{
	// Use Dark theme
	opt.InjectStylesheet("/Swagger/Themes/_base.css");
	opt.InjectStylesheet("/Swagger/Themes/one-dark.css");
	opt.InjectStylesheet("/Swagger/Themes/one-light.css");
	
	// Other style tweaks
	opt.InjectStylesheet("/Swagger/Themes/_custom.css");
	opt.InjectJavascript("/Swagger/AuthorizationTweaks.js");
	opt.InjectJavascript("/Swagger/ResponseTweaks.js");
    
	// Use Root URL
	opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	opt.RoutePrefix = string.Empty;
    
	// Minimize Schemas at bottom of page by default
	opt.DefaultModelsExpandDepth(0);
    
	// Enable Try it out mode by default
	opt.EnableTryItOutByDefault();
});

// Don't bother with static files and other web stuff in Staging, which is only used for running tests
if (!app.Environment.IsStaging())
{
	app.UseHttpsRedirection();
	app.UseDefaultFiles();
	app.UseStaticFiles();
	app.UseAuthorization();
}

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