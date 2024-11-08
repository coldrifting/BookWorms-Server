using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SwaggerThemes;
using WebApplication1.Models;
using WebApplication1.Swagger;

namespace WebApplication1;

// The following URL is very helpful for understanding the basics of how ASP.Net works
// https://medium.com/net-core/build-a-restful-web-api-with-asp-net-core-6-30747197e229
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        // Add our DB Context(s) here
        builder.Services.AddDbContext<MovieContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MovieContext")));
        
        builder.Services.AddControllers();
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
				Scheme = "Bearer"
			});
		});
        
        // Authorization & Authentication
        builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseInMemoryDatabase("AppDb"));
        builder.Services.AddAuthorization();

        builder.Services.AddIdentityApiEndpoints<IdentityUser>()
	        .AddEntityFrameworkStores<ApplicationDbContext>();

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
				opt.InjectJavascript("/Swagger/MoveAuthorizeBtn.js");
	            
	            // Use Root URL
				opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	            opt.RoutePrefix = string.Empty;
	            
	            // Minimize Schemas at bottom of page by default
	            opt.DefaultModelsExpandDepth(0);
            });
        }

        app.UseHttpsRedirection();

        // Actions can be split into separate files in the Controllers folder
        app.MapControllers();
        
        // Map authorization API endpoints
        app.UseAuthorization();
        
        // TODO - Can't customize these routes, replace?
        // https://darko-subic.medium.com/how-to-disable-asp-net-core-identity-auto-generated-routes-6dbd09b5e815
        // Copy and/or edit default code, see above link
        app.MapGroup("/account").WithTags("User Account").MapIdentityApi<IdentityUser>();

        app.Run();
    }
}