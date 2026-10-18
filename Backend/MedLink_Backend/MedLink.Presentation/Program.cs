using MedLink.Application.Dependency_Injection;
using MedLink.infrastructure.DependencyInjection;
using MedLink.Presentation.Middlewares;
using Microsoft.OpenApi;
using MedLink.Presentation.Hubs;
using MedLink.Application.Interfaces.Common;
using MedLink.Presentation.Realtime;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the accessToken only (without the word Bearer)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddMailService();
builder.Services.AddFileService();
builder.Services.AddCacheService(builder.Configuration);
builder.Services.AddAppDI();
builder.Services.AddNotificationService();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnablePersistAuthorization());
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
