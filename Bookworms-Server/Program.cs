using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BookwormsServer.Filters;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using BookwormsServer.Services.Interfaces;

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
		});
        
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
		        // Make authorization failure (401) responses consistent with other bad requests
		        opt.Events = AuthService.BearerEvents();
		    });
		builder.Services.AddAuthorization();
        
        
		// Register services -------------------------------------------------------------------------------------------
		// (Dependency Injections)
		
		builder.Services.AddSingleton<IBookApiService, GoogleBooksApiService>();
		//builder.Services.AddSingleton<IBookApiService, TestDataApiService>();

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
	        if (dbContext.Database.EnsureCreated())
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
	            
	            // Use Root URL
				opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	            opt.RoutePrefix = string.Empty;
	            
	            // Minimize Schemas at bottom of page by default
	            opt.DefaultModelsExpandDepth(0);
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

	    string userData = File.ReadAllText("TestData/UserEntities.json");
		List<User> users = JsonSerializer.Deserialize<List<User>>(userData, jso)!;
	    dbContext.Users.ExecuteDelete();
		dbContext.Users.AddRange(users);
		dbContext.SaveChanges();
		
	    string bookData = File.ReadAllText("TestData/BookEntities.json");
		List<Book> books = JsonSerializer.Deserialize<List<Book>>(bookData, jso)!;
	    dbContext.Books.ExecuteDelete();
		dbContext.Books.AddRange(books);
		dbContext.SaveChanges();
		
	    string reviewData = File.ReadAllText("TestData/ReviewEntities.json");
		List<Review> reviews = JsonSerializer.Deserialize<List<Review>>(reviewData, jso)!;
	    dbContext.Reviews.ExecuteDelete();
		dbContext.Reviews.AddRange(reviews);
		dbContext.SaveChanges();
    }
}