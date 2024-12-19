using BookingPost.API.Entities;
using System.Text.Json;
using BookingPost.API.RabbitMQ;
using BookingPost.API.RabbitMQ;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:7951");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RabbitMqService>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services
    .AddCors(options =>
        options.AddPolicy(
            "Support",
            policy =>
                policy
                    .WithOrigins("http://localhost:7777")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
        )
    );


var app = builder.Build();

app.UseCors("Support");
app.UseSwagger();
app.UseSwaggerUI();

app.MapMethods("/Bookings", new[] { "OPTIONS" }, (HttpContext context) =>
{
    context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:7777");
    context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
    return Results.Ok();
});

app.MapPost("/bookings", async (Booking booking, RabbitMqService rabbitMqService) =>
{
    try
    {
        var jsonBooking = JsonSerializer.Serialize(booking);
        rabbitMqService.PublishMessage(jsonBooking);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.Run();