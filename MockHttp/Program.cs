using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
