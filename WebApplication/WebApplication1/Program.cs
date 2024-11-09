//using Microsoft.AspNetCore.Identity;

using System.Text;
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
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container
        // Add our DB Context here
        string? connString = builder.Configuration.GetConnectionString("DBContext");
        builder.Services.AddDbContext<RestDBContext>(opt =>
	        opt.UseSqlServer(connString));

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
				Description = "Insert bearer token here without 'Bearer' prefix",
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer",
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
			        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthService.Key)),
			        ValidateIssuer = false,
			        ValidateAudience = false
		        };
		        // Make authorization failure (401) responses consistent with other bad requests
		        opt.Events = new()
		        {
					OnChallenge = context =>
					{
				        context.Response.OnStarting(async () =>
				        {
					        context.Response.Headers.Append("content-type", "application/json; charset=utf-8 ");
				            ErrorDTO error = new("Unauthorized", "A valid token is required for this route");
				            await context.Response.WriteAsync(error.Json());
				        });

				        return Task.CompletedTask;
					}
		        };
		    });
		builder.Services.AddAuthorization();
		
        var app = builder.Build();
        
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