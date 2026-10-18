using MedLink.Domain.Entities.Common.Identity;
using MedLink.infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class Identity_Dependency_Injection
    {
      public static IServiceCollection AddIdentityServices
          (
           this IServiceCollection services ,
           ConfigurationManager configuration
          )
        {
            services.AddIdentity<AppUser, IdentityRole>
                (options => options.User.RequireUniqueEmail = true)
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                 .AddJwtBearer(options =>
                 {
                     options.RequireHttpsMetadata = false;
                     options.SaveToken = true;

                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidIssuer = configuration["JWT:Issuer"],

                         ValidateAudience = false,

                         ValidateIssuerSigningKey = true,
                         IssuerSigningKey = new SymmetricSecurityKey(
                             Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                     };

                     options.Events = new JwtBearerEvents
                     {
                         OnMessageReceived = context =>
                         {
                             var accessToken = context.Request.Query["access_token"];
                             var path = context.HttpContext.Request.Path;

                             // Accept the token from the query string ONLY for SignalR hub requests
                             if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                             {
                                 context.Token = accessToken;
                             }

                             return Task.CompletedTask;
                         }
                     };


                 });

            return services;
        }

    }
}
