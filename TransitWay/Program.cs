using Microsoft.EntityFrameworkCore;
using TransitWay.Data;
using TransitWay.Hubs;
using TransitWay.Services;
using TransitWay.Services.AttachmentService;

var builder = WebApplication.CreateBuilder(args);

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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        })
);

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

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GoogleAuthService>();
//builder.Services.AddHostedService<LocationSimulationService>();
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