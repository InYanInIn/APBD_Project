using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RCP_Project.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

// Register the services
builder.Services.AddScoped<ExchangeService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddAuthentication(options =>  
{  
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;  
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  
    }).AddJwtBearer(opt =>  
    {  
        opt.TokenValidationParameters = new TokenValidationParameters  
        {  
            ValidateIssuer = true,   //by who  
            ValidateAudience = true, //for whom  
            ValidateLifetime = true,  
            ClockSkew = TimeSpan.FromMinutes(2),  
            ValidIssuer = "https://localhost:5001", //should come from configuration  
            ValidAudience = "https://localhost:5001", //should come from configuration  
            IssuerSigningKey = new 
                SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecretKey"]))  
        };  
        opt.Events = new JwtBearerEvents  
        {  
            OnAuthenticationFailed = context =>  
            {  
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))  
                {                
                    context.Response.Headers.Add("Token-expired", "true");  
                }            
                
                return Task.CompletedTask;  
            }    
        };
    }).AddJwtBearer("IgnoreTokenExpirationScheme",opt =>  
    {  
        opt.TokenValidationParameters = new TokenValidationParameters  
        {  
            ValidateIssuer = true,   //by who  
            ValidateAudience = true, //for whom  
            ValidateLifetime = false,  
            ClockSkew = TimeSpan.FromMinutes(2),  
            ValidIssuer = builder.Configuration["Issuer"], //should come from configuration  
            ValidAudience = builder.Configuration["Audience"], //should come from configuration  
            IssuerSigningKey = new 
                SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecretKey"]))  
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Standard", policy => policy.RequireRole("Standard"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();