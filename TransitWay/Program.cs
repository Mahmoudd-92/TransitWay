using Microsoft.EntityFrameworkCore;
using System;
using TransitWay.Data;
using TransitWay.Hubs;
using TransitWay.Services;
using TransitWay.Services.AttachmentService;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ? AddControllers ??? ????? ?? ?? ??? JsonOptions
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
// ? AddCors ??? ????? ??
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GoogleAuthService>();
builder.Services.AddHostedService<LocationSimulationService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// ? UseStaticFiles ??? ????? ??
app.UseStaticFiles();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TrackingHub>("/trackingHub");

app.Run();