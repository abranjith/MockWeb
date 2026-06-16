using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "AllowAll";

// Add services to the container.
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddServer(new OpenApiServer
    {
        Url = "https://localhost:7192",
        Description = "Development HTTPS"
    });
    options.AddServer(new OpenApiServer
    {
        Url = "http://localhost:5218",
        Description = "Development HTTP"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Enable annotations for better file upload support
    options.EnableAnnotations();
});

// Register mock services
builder.Services.AddSingleton<MockHttp.Services.IResponseGeneratorService, MockHttp.Services.ResponseGeneratorService>();
builder.Services.AddSingleton<MockHttp.Services.IImageService, MockHttp.Services.ImageService>();
builder.Services.AddSingleton<MockHttp.Services.IMockDataStore, MockHttp.Services.MockDataStore>();
builder.Services.AddSingleton<MockHttp.Services.IWebSocketChatService, MockHttp.Services.WebSocketChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection must skip WebSocket upgrade requests. A WebSocket client cannot follow
// the 307 that UseHttpsRedirection issues for GETs on the HTTP port, and would otherwise see
// the redirect as an abnormal close instead of the 101 Switching Protocols handshake.
app.UseWhen(
    ctx => !ctx.WebSockets.IsWebSocketRequest,
    branch => branch.UseHttpsRedirection());

app.UseCors(CorsPolicyName);

app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });

app.MapControllers();

app.Run();

// Expose Program for WebApplicationFactory in tests
public partial class Program { }
