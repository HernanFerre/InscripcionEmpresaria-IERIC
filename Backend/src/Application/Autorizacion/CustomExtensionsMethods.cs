using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Auth
{
    static class CustomExtensionsMethods
    {
        public const string PublicAuthenticationScheme =
            "PublicAuthentication";

        public static IServiceCollection AddAutorizacion(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            IConfigurationSection authSection =
                configuration.GetSection("AuthSettings");

            if (!authSection.Exists())
            {
                throw new Exception(
                    "Debe configurar la sección AuthSettings " +
                    "en su archivo de configuración."
                );
            }

            AuthSettings authSettings =
                authSection.Get<AuthSettings>()
                ?? throw new Exception(
                    "No fue posible cargar la configuración " +
                    "de AuthSettings."
                );

            if (
                string.IsNullOrWhiteSpace(
                    authSettings.AuthorizationSecret
                )
            )
            {
                throw new Exception(
                    "Debe configurar " +
                    "AuthSettings:AuthorizationSecret."
                );
            }

            if (
                string.IsNullOrWhiteSpace(
                    authSettings.PublicKey
                )
            )
            {
                throw new Exception(
                    "Debe configurar AuthSettings:PublicKey."
                );
            }

            services.AddScoped<AuthSettings>(
                _ => authSettings
            );

            services.AddScoped<IAutorizacionRepository>(
                _ => new AutorizacionRepository(authSettings)
            );

            services.AddScoped<IUsuarioRepository>(
                _ => new UsuarioRepository(authSettings)
            );

            byte[] authorizationKey =
                Encoding.ASCII.GetBytes(
                    authSettings.AuthorizationSecret
                );

            RSA publicRsa = RSA.Create();

            publicRsa.FromXmlString(
                authSettings.PublicKey
            );

            RsaSecurityKey publicSigningKey =
                new RsaSecurityKey(publicRsa);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;

                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,

                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        authorizationKey
                                    ),

                                ValidateIssuer = false,
                                ValidateAudience = false,
                                ValidateLifetime = true
                            };
                    }
                )
                .AddJwtBearer(
                    PublicAuthenticationScheme,
                    options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;

                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,

                                IssuerSigningKey =
                                    publicSigningKey,

                                ValidateIssuer = false,
                                ValidateAudience = false,
                                ValidateLifetime = true,

                                ClockSkew =
                                    TimeSpan.FromMinutes(1),

                                ValidAlgorithms =
                                    new[]
                                    {
                                        SecurityAlgorithms
                                            .RsaSha256
                                    }
                            };
                    }
                );

            return services;
        }
    }
}