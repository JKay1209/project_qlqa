using Server.Config;
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

// Chuẩn bị cho real-time (chưa dùng)
builder.Services.AddSignalR();

// Socket server TCP (Task 05)
builder.Services.AddHostedService<TcpSocketServer>();

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();