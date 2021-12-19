using Api.Extensions;
using Api.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Runtime.ExceptionServices;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//[Authorize] would usually handle this
//app.Use(async (context, next) =>
//{
//    // Use this if there are multiple authentication schemes
//    var authResult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
//    if (authResult.Succeeded && authResult.Principal.Identity.IsAuthenticated)
//    {
//        await next();
//    }
//    else if (authResult.Failure != null)
//    {
//        // Rethrow, let the exception page handle it.
//        ExceptionDispatchInfo.Capture(authResult.Failure).Throw();
//    }
//    else
//    {
//        await context.ChallengeAsync();
//    }
//});


app.UseHttpsRedirection();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
