using Core_DataAccessPlugIns.ChatServices;
using Core_DataAccessPlugIns.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<NwContext>(options => { 
    options.UseSqlServer(builder.Configuration.GetConnectionString("NwConnection"));
});
 
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<ChatInfoGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

 

app.MapGet("/info", async (ChatInfoGenerator info, string? city, string? country) =>
{
   var resp = await info.GetCustomersByCityAndCountry(city: city, country: country);
    return Results.Ok(resp);
});

app.MapGet("/orderdetails", async (ChatInfoGenerator info, string? propertyname, string? propertyvalue, string? operation) =>
{
    var resp = await info.GetFreightDetailsAsync(propertyname: propertyname, propertyvalue: propertyvalue, operation: operation);
    return Results.Ok(resp);
});


app.Run();

 
