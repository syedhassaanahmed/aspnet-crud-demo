using AspNet.Crud.Demo.Services;
using AspNet.Crud.Demo.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection(nameof(CosmosDbSettings)));

builder.Services.AddDbContext<ProductDbContext>
(
    (IServiceProvider sp, DbContextOptionsBuilder options) =>
    {
        var cosmosDbSettings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;

        options.UseCosmos(
            cosmosDbSettings.AccountEndpoint,
            cosmosDbSettings.AccountKey,
            cosmosDbSettings.DatabaseName);
    }
);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope();
    var dbcontext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbcontext.Database.EnsureCreated();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

// Needed for Integration Tests project to discover the Program class
public partial class Program { }
