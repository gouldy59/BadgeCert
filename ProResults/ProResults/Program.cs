using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProResults.Data;
using ProResults.Services;
// Ensure the required package is installed: Microsoft.AspNetCore.Mvc.NewtonsoftJson
// Add the following using directive at the top of the file:
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("BadgeManagementDb"));

// Add custom services
builder.Services.AddScoped<BadgeValidationService>();
builder.Services.AddScoped<PdfService>();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();


// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    // Add sample user if not exists
    if (!context.Users.Any())
    {
        context.Users.Add(new ProResults.Models.User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Demo User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        context.Users.Add(new ProResults.Models.User
        {
            Id = Guid.NewGuid(),
            Email = "shane.gould@prometric.com",
            Name = "shane User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        context.Users.Add(new ProResults.Models.User
        {
            Id = Guid.NewGuid(),
            Email = "kerrie.Callaghan@prometric.com",
            Name = "shane User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        context.SaveChanges();
    }
}

app.Run("http://0.0.0.0:5001");
