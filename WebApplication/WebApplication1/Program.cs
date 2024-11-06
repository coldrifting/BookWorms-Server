using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

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

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Actions can be split into separate files in the Controllers folder
        app.MapControllers();

        app.Run();
    }
}