using Core_DataAccessPlugIns.ChatServices;
using Core_DataAccessPlugIns.Models;
using Core_DataAccessPlugIns.RequestResponse;
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

 

app.MapPost("/info", async (ChatInfoGenerator info, PromptRequest request) =>
{
   var resp = await info.GetCustomersByCityAndCountry(request.Prompt);
    return Results.Ok(resp);
});

app.MapPost("/orderdetails", async (ChatInfoGenerator info, PromptRequest prompt) =>
{
    var resp = await info.GetFreightDetailsAsync(prompt.Prompt);
    return Results.Ok(resp);
});

//app.MapPost("/prompt", async (ChatInfoGenerator info, PromptRequest request) =>
//{
//    if (request.Prompt is null)
//    {
//        return Results.BadRequest("Prompt cannot be null");
//    }
//    var response = await info.ParsePromptToDictionaryValues(request.Prompt);
//    return Results.Ok(response);
//});

app.Run();

 
