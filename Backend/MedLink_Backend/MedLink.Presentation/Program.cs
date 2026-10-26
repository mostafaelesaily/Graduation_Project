using MedLink.Application.Dependency_Injection;
using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.DependencyInjection;
using MedLink.Presentation.Extensions;
using MedLink.Presentation.Middlewares;
using MedLink.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerJwtAuthentication();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddMailService();
builder.Services.AddFileService();
builder.Services.AddCacheService(builder.Configuration);
builder.Services.AddGoogleAuthService();
builder.Services.AddAppDI();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationNotifier, SignalRNotificationNotifier>();

var app = builder.Build();

await app.SeedDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

app.UseCors("ReactPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MedLink.Presentation.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
