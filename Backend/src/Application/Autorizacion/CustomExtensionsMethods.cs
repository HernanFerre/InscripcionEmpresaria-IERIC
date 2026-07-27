using System;
using System.Text;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
namespace Auth
{

    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddAutorizacion(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {


            IConfigurationSection authSection = configuration.GetSection("AuthSettings") ?? throw new Exception("Debe configurar la seccion AuthSettings en su archivo de configuración");

            AuthSettings authSettings = authSection.Get<AuthSettings>();

            services.AddScoped<AuthSettings>(s => authSettings);

            services.AddScoped<IAutorizacionRepository>(x => new AutorizacionRepository(authSettings));

            services.AddScoped<IUsuarioRepository>(x => new UsuarioRepository(authSettings));

            var key = Encoding.ASCII.GetBytes(authSettings.AuthorizationSecret);

            services.AddAuthentication(x =>
           {
               x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           })
           .AddJwtBearer(x =>
           {
               x.RequireHttpsMetadata = false;
               x.SaveToken = true;
               x.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(key),
                   ValidateIssuer = false,
                   ValidateAudience = false
               };
           });


            return services;
        }
    }
}