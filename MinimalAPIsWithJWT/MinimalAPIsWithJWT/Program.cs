using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPIsWithJWT.Models;
using  Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<JobDB>(options =>{
    options.UseSqlServer("DummyConnection"); 
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepositoryService>(new UserRepositoryService());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });
});
var app = builder.Build();
app.Urls.Add("http://localhost:3000");
//app.MapGet("/", () => "Hello World!");
app.MapGet("/jobs", async (int pageNumber, int pageSize, JobDB db) =>
    // await db.Job.Skip((pageNumber - 1) * pageSize)
    // .Take(pageSize).ToListAsync()
{
    var userDto =new UserRepositoryService().GetUser(new UserModel(){Password = "abc123", UserName = "admin"}); 
    if (userDto == null)
    {
        //response.StatusCode = 401;
        return;
    }

    if (builder != null)
    {
        if (builder.Configuration != null)
        {
            var token =new TokenService().BuildToken(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"],builder.Configuration["Jwt:Audience"], userDto);
           // response.WriteAsJsonAsync(new { token = token });
        // return await token.ToCharArray();

        }
    }
}
   
    
    
);


// app.MapPost("/login", () =>
// {
//     return postUser([FromBodyAttribute]
//     UserModel userModel, TokenService tokenService,
//     IUserRepositoryService userRepositoryService, HttpResponse response);
// });
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.Run();

 void postUser([FromBodyAttribute] UserModel userModel, TokenService tokenService,
    IUserRepositoryService userRepositoryService, HttpResponse response)
{
    var userDto = userRepositoryService.GetUser(userModel);
    if (userDto == null)
    {
        response.StatusCode = 401;
        return;
    }

    if (builder != null)
    {
        if (builder.Configuration != null)
        {
            var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"],builder.Configuration["Jwt:Audience"], userDto);
            response.WriteAsJsonAsync(new { token = token });
        }
    }

    return;
    
}