using DbMgr;
using ShopifyAppDemo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SecurityTokenValidators.Clear();
    options.SecurityTokenValidators.Add(new ShopifySessionTokenValidator());

    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.IncludeErrorDetails = builder.Environment.IsDevelopment();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAudience = builder.Configuration["ClientId"] ?? throw new ArgumentNullException("ClientId cannot be null"),
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["ClientSecret"] ?? throw new ArgumentNullException("Jwt:Key cannot be null"))),
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization(); // TODO: do I need to add this, or it's added by default?

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
    options.InstanceName = "AppDemo";
});

builder.Services.AddDbMgr(builder.Configuration.GetConnectionString("postgres"));
builder.Services.AddSecretMgr(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    //app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseFileServer();

app.UseAuthentication();
app.UseAuthorization();

// Routing stuffs
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
