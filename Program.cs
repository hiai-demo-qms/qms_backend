using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer; // Add this directive to resolve 'UseSqlServer'
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using WebApplication1.Application.Services;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ensure the Microsoft.EntityFrameworkCore.SqlServer package is installed in your project
// You can install it via NuGet Package Manager or by running:
// dotnet add package Microsoft.EntityFrameworkCore.SqlServer
var configuration = builder.Configuration!;
builder.Services.AddDbContext<QmsDbContext>(opts =>
    {
        opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
    }
);

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
}).AddEntityFrameworkStores<QmsDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
    o.TokenLifespan = TimeSpan.FromHours(5));


builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    opts.SaveToken = true;
    opts.RequireHttpsMetadata = false;
    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080") // URL frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

//builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserManagement, UserManagement>();
builder.Services.AddScoped<IDocumentManagement, DocumentManagement>();
builder.Services.AddHttpClient<IChatbotService, ChatbotService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QmsDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

