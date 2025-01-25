using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using BookwormsServer.Services.Interfaces;
using BookwormsServer.Swagger;
using Swashbuckle.AspNetCore.Filters;

namespace BookwormsServer;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        // Establish database context ----------------------------------------------------------------------------------

	    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	    builder.Services.AddDbContext<BookwormsDbContext>(o =>
		    o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
	    
	    // Configure Swagger -------------------------------------------------------------------------------------------
	    
        // Use lowercase api endpoints
        builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true); 
        
        builder.Services.AddControllers(opt => 
	        opt.Filters.Add(new ProducesAttribute("application/json")));
        
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
		
		// Return enums in response bodies as strings, not numbers
		builder.Services.AddControllers().AddJsonOptions(options =>
		{
		   options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

		   options.JsonSerializerOptions.DefaultIgnoreCondition =
		       JsonIgnoreCondition.WhenWritingNull;
		});
		
		// -------------------------------------------------------------------------------------------------------------
		
		
        WebApplication app = builder.Build();
        
        
        // Ensure the database is migrated -----------------------------------------------------------------------------
        
        // (This is best done here, after the WebApplication has been created)
        using (var serviceScope = app.Services.CreateScope()) {
	        var dbContext = serviceScope.ServiceProvider.GetRequiredService<BookwormsDbContext>();
	        
	        // If running the application or the tests fails here, you need to drop your database and then try again
	        if (dbContext.Database.GetPendingMigrations().Any())
	        {
				dbContext.Database.Migrate();
	        }
        }
        
        
        // Configure the HTTP request pipeline -------------------------------------------------------------------------
        
        if (app.Environment.IsDevelopment())
        {
	        app.UseStaticFiles();
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
        }

        app.UseHttpsRedirection();

        // Enable endpoint caching
		app.UseResponseCaching();

        // Endpoints can be split into separate files in the Controllers folder
        app.MapControllers();
        
        // Persist test data -------------------------------------------------------------------------------------------

        using (var serviceScope = app.Services.CreateScope()) {
	        var dbContext = serviceScope.ServiceProvider.GetRequiredService<BookwormsDbContext>();
	        PersistTestData(dbContext);
        }
        
        // -------------------------------------------------------------------------------------------------------------
        
        app.Run();
    }

    public static void PersistTestData(BookwormsDbContext dbContext)
    {
	    JsonSerializerOptions jso = new()
	    {
		    PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
	    };

	    dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
	    
	    string userData = File.ReadAllText("TestData/UserEntities.json");
		var users = JsonSerializer.Deserialize<List<User>>(userData, jso)!;
	    dbContext.Users.ExecuteDelete();
		dbContext.Users.AddRange(users);
		dbContext.SaveChanges();
		
	    string parentData = File.ReadAllText("TestData/ParentEntities.json");
		var parents = JsonSerializer.Deserialize<List<Parent>>(parentData, jso)!;
	    dbContext.Parents.ExecuteDelete();
		dbContext.Parents.AddRange(parents);
		dbContext.SaveChanges();

	    string teacherData = File.ReadAllText("TestData/TeacherEntities.json");
		var teachers = JsonSerializer.Deserialize<List<Teacher>>(teacherData, jso)!;
	    dbContext.Teachers.ExecuteDelete();
		dbContext.Teachers.AddRange(teachers);
		dbContext.SaveChanges();
		
	    string bookData = File.ReadAllText("TestData/BookEntities.json");
		var books = JsonSerializer.Deserialize<List<Book>>(bookData, jso)!;
	    dbContext.Books.ExecuteDelete();
		dbContext.Books.AddRange(books);
		dbContext.SaveChanges();
		
	    string reviewData = File.ReadAllText("TestData/ReviewEntities.json");
		var reviews = JsonSerializer.Deserialize<List<Review>>(reviewData, jso)!;
	    dbContext.Reviews.ExecuteDelete();
		dbContext.Reviews.AddRange(reviews);
		dbContext.SaveChanges();
		
	    string childData = File.ReadAllText("TestData/ChildEntities.json");
		var children = JsonSerializer.Deserialize<List<Child>>(childData, jso)!;
	    dbContext.Children.ExecuteDelete();
		dbContext.Children.AddRange(children);
		
		dbContext.SaveChanges();
		
		// Ensure a child is selected if at least one exists under a parent
		foreach (var child in dbContext.Children.Include(child => child.Parent))
		{
			if (child.Parent is not null && child.Parent.SelectedChild is null)
			{
				child.Parent.SelectedChild = child;
			}
		}
		
		dbContext.SaveChanges();
    }
}