using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using BookwormsServer.Filters;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using BookwormsServer.Services.Interfaces;
using Newtonsoft.Json;

namespace BookwormsServer;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        
        // Establish database context ----------------------------------------------------------------------------------
        
	    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	    string? databaseVersion = builder.Configuration.GetValue<string>("Database:Version");

	    if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(databaseVersion))
	    {
		    Console.Error.WriteLine("Please specify a valid connection string and MySQL database version in " +
		                            $"appsettings.{builder.Environment.EnvironmentName}.json.");
		    return;
	    }
	    
	    var serverVersion = new MySqlServerVersion(new Version(databaseVersion));

	    if (builder.Configuration.GetValue("Database:TestContainer:Use", false))
	    {
		    MySqlConnectionStringBuilder connString = new MySqlConnectionStringBuilder(connectionString);
		    int hostPort = builder.Configuration.GetValue("Database:TestContainer:HostPort", 3306);
		    var mysqlContainer = new ContainerBuilder()
			    .WithImage($"mysql:{databaseVersion}")
			    .WithName("mysql-container")
			    .WithPortBinding(hostPort, (int)connString.Port)
			    .WithEnvironment("MYSQL_DATABASE", connString.Database)
			    .WithEnvironment("MYSQL_USER", connString.UserID)
			    .WithEnvironment("MYSQL_ROOT_PASSWORD", connString.Password)
			    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(hostPort))
			    .Build();

		    Task.Run(async () => await mysqlContainer.StartAsync()).Wait();
	    }

	    builder.Services.AddDbContext<AllBookwormsDbContext>(o =>
		    o.UseMySql(connectionString, serverVersion));

	    
	    // Configure Swagger -------------------------------------------------------------------------------------------
	    
        // Use lowercase api endpoints
        builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true); 
        
        builder.Services.AddControllers(opt => 
	        opt.Filters.Add(new ProducesAttribute("application/json")));
        
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
		
		// -------------------------------------------------------------------------------------------------------------
		
		
        WebApplication app = builder.Build();
        
        
        // Ensure the database is migrated -----------------------------------------------------------------------------
        
        // (This is best done here, after the WebApplication has been created)
        using (var serviceScope = app.Services.CreateScope()) {
	        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AllBookwormsDbContext>();
	        dbContext.Database.Migrate();
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

        // Endpoints can be split into separate files in the Controllers folder
        app.MapControllers();
        
        // Persist test data -------------------------------------------------------------------------------------------

        using (var serviceScope = app.Services.CreateScope()) {
	        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AllBookwormsDbContext>();
	        try
	        {
		        PersistTestData(dbContext);
	        }
	        catch (Exception)
	        {
		        Console.WriteLine("Test data already in DB");
	        }
        }
        
        // -------------------------------------------------------------------------------------------------------------
        
        app.Run();
    }

    private static void PersistTestData(AllBookwormsDbContext dbContext)
    {
		List<User> users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("TestData/UserEntities.json"))!;
		List<Book> books = JsonConvert.DeserializeObject<List<Book>>(File.ReadAllText("TestData/BookEntities.json"))!;
		List<Review> reviews = JsonConvert.DeserializeObject<List<Review>>(File.ReadAllText("TestData/ReviewEntities.json"))!;
		
		dbContext.Users.AddRange(users);
		dbContext.Books.AddRange(books);
		dbContext.Reviews.AddRange(reviews);
		dbContext.SaveChanges();
    }
}