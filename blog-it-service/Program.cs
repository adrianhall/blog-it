// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Datasync;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Samples.BlogIt.Database;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

/*
 * Entity Framework - set up database context
 */
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString == null)
{
    throw new ApplicationException("DefaultConnection is not set");
}
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

/*
 * Authentication and authorization
 * Use JWT Authentication with the UserId claim being the name.
 */
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        NameClaimType = "UserId",
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]))
    };
});
builder.Services.AddAuthorization();

/*
 * Datasync Service
 */
builder.Services.AddHttpContextAccessor();
builder.Services.AddDatasyncControllers();

/**************************************************************************************************
 * 
 * Build the application context and start the application.
 */
var app = builder.Build();

/*
 * Initialize the database.
 */
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.InitializeDatabaseAsync().ConfigureAwait(false);
}

/*
 * Configure service pipeline.
 */
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

/*
 * Run the application.
 */
app.Run();
