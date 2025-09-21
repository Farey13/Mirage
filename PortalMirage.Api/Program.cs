using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to use JWT Bearer authentication
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// =================================================================
//  Add Your Services to the Container (Dependency Injection)
// =================================================================
// 1. Add Configuration and JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

// 2. Add the Connection Factory
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddSingleton<PortalMirage.Data.Abstractions.IDbConnectionFactory>(_ =>
    new PortalMirage.Data.SqlConnectionFactory(connectionString));

// 3. Add the Repositories (DAL)
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IUserRepository, PortalMirage.Data.UserRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.ICalibrationLogRepository, PortalMirage.Data.CalibrationLogRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IKitValidationRepository, PortalMirage.Data.KitValidationRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.ISampleStorageRepository, PortalMirage.Data.SampleStorageRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IHandoverRepository, PortalMirage.Data.HandoverRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IMachineBreakdownRepository, PortalMirage.Data.MachineBreakdownRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IMediaSterilityCheckRepository, PortalMirage.Data.MediaSterilityCheckRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.ITaskRepository, PortalMirage.Data.TaskRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IDailyTaskLogRepository, PortalMirage.Data.DailyTaskLogRepository>();


// ... (all your other repositories)
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IRoleRepository, PortalMirage.Data.RoleRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IUserRoleRepository, PortalMirage.Data.UserRoleRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IPermissionRepository, PortalMirage.Data.PermissionRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IRolePermissionRepository, PortalMirage.Data.RolePermissionRepository>();
builder.Services.AddScoped<PortalMirage.Data.Abstractions.IAuditLogRepository, PortalMirage.Data.AuditLogRepository>();

// 4. Add the Services (BLL)
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IUserService, PortalMirage.Business.UserService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.ICalibrationLogService, PortalMirage.Business.CalibrationLogService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IKitValidationService, PortalMirage.Business.KitValidationService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.ISampleStorageService, PortalMirage.Business.SampleStorageService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IHandoverService, PortalMirage.Business.HandoverService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IMachineBreakdownService, PortalMirage.Business.MachineBreakdownService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IMediaSterilityCheckService, PortalMirage.Business.MediaSterilityCheckService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IDailyTaskLogService, PortalMirage.Business.DailyTaskLogService>();

// ... (all your other services)
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IRoleService, PortalMirage.Business.RoleService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IUserRoleService, PortalMirage.Business.UserRoleService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IRolePermissionService, PortalMirage.Business.RolePermissionService>();
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IAuditLogService, PortalMirage.Business.AuditLogService>();
// Add our new token generator service
builder.Services.AddScoped<PortalMirage.Business.Abstractions.IJwtTokenGenerator, PortalMirage.Business.JwtTokenGenerator>();
// =================================================================


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // This line has been added
app.UseAuthorization();

app.MapControllers();

app.Run();
