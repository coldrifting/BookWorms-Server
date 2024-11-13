using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication1.Filters;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1;

// The following URL is very helpful for understanding the basics of how ASP.Net works
// https://medium.com/net-core/build-a-restful-web-api-with-asp-net-core-6-30747197e229
public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
	    bool useDocker = builder.Configuration.GetValue<bool>("UseDocker");

	    if (useDocker)
	    {
		    var mysqlContainer = new ContainerBuilder()
			    .WithImage("mysql:8.4.2")
			    .WithName("mysql-container")
			    .WithPortBinding(32770, 3306)
			    .WithEnvironment("MYSQL_DATABASE", "mysql-db")
			    .WithEnvironment("MYSQL_USER", "postmaster")
			    .WithEnvironment("MYSQL_PASSWORD", "password")
			    .WithEnvironment("MYSQL_ROOT_PASSWORD", "root")
			    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
			    .Build();

		    Task.Run(async () => await mysqlContainer.StartAsync()).Wait();
	    }
	    
        
        var serverVersion = new MySqlServerVersion(new Version(8, 4, 2));
        
        // Add services to the container
        // Add our DB Context here
	    string? connString = builder.Configuration.GetConnectionString(useDocker ? "Docker" : "Local");
        builder.Services.AddDbContext<BookwormsDbContext>(o =>
	        o.UseMySql(connString, serverVersion));

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
			opt.SwaggerDoc("v1", new() { Title = "MyAPI", Version = "v1" });
			opt.AddSecurityDefinition("Bearer", new()
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
		        opt.TokenValidationParameters = new()
		        {
			        IssuerSigningKey = new SymmetricSecurityKey(AuthService.SecretBytes),
			        ValidateIssuer = false,
			        ValidateAudience = false
		        };
		        // Make authorization failure (401) responses consistent with other bad requests
		        opt.Events = AuthService.BearerEvents();
		    });
		builder.Services.AddAuthorization();
		
        WebApplication app = builder.Build();
        
        // Ensure the database is migrated
        using (var serviceScope = app.Services.CreateScope()) {
	        var dbContext = serviceScope.ServiceProvider.GetRequiredService<BookwormsDbContext>();
	        dbContext.Database.Migrate();
        }
        
        // Configure the HTTP request pipeline.
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
        
        app.Run();
    }
}