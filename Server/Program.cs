using Microsoft.Extensions.Options;
using Server.Config;
using Server.Network.Routing;
using Server.Middleware;
using Server.Models.Database;
using Server.Network;
using Server.Repositories;
using Server.Security;
using Server.Services;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Controllers + enum dạng chữ (khớp JsonDefaults bên Client)
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Database, JWT
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// DI
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddSingleton<EmailService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TokenBlacklist>();

// Chuẩn bị cho real-time
builder.Services.AddSignalR();

// Socket server TCP
builder.Services.Configure<SocketServerOptions>(builder.Configuration.GetSection(SocketServerOptions.SectionName));
builder.Services.AddSingleton<ClientRegistry>();

// Định tuyến order / trạng thái bàn
builder.Services.AddSingleton<BackendRouting, RestaurantBackend>();
builder.Services.AddSingleton(sp => new ProcessedOrderLog(Path.Combine(
    builder.Environment.ContentRootPath,
    sp.GetRequiredService<IOptions<SocketServerOptions>>().Value.ProcessedOrderLogPath)));
builder.Services.AddSingleton<MessageRouter>();
builder.Services.AddSingleton<TcpSocketServer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TcpSocketServer>());

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();