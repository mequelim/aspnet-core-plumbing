using AspNet.Plumbing.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AspNet.Plumbing.Shared
{
    /// <summary>
    /// Provides extension methods for configuring dependency injection in ASP.NET Core applications.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Provides extension methods for configuring application services in the ASP.NET Core pipeline, including authentication, authorization, and CORS policy setup.
        /// </summary>
        extension(WebApplicationBuilder builder)
        {
            /// <summary>
            /// Configures authentication for the application using the specified options.
            /// </summary>
            /// <param name="configureAction">An action to configure <see cref="PlumbingOptions"/> with the required authentication settings. </param>
            /// <returns>The <see cref="WebApplicationBuilder"/> instance with authentication configured.</returns>
            /// <exception cref="InvalidOperationException">Thrown when the <see cref="PlumbingOptions.AuthorityUrl"/> is not provided or is invalid.</exception>
            /// <remarks>
            /// This method sets up JWT Bearer authentication with custom token validation parameter and event handlers.
            /// The <see cref="PlumbingOptions.AuthorityUrl"/> is used as the authority for token validation.
            /// </remarks>
            public WebApplicationBuilder ConfigureAuthentication(Action<PlumbingOptions> configureAction)
            {
                PlumbingOptions options = new();
                configureAction(options);

                if(string.IsNullOrWhiteSpace(options.AuthorityUrl))
                {
                    throw new InvalidOperationException("##### [AspNetCore.Plumbing.DependencyInjection.cs] [ConfigureAuthentication()] AuthorityUrl is required. #####");
                }

                builder.Services
                    .AddAuthentication("Bearer")
                    .AddJwtBearer((opt) =>
                    {
                        opt.Authority = options.AuthorityUrl;
                        opt.MapInboundClaims = false;
                        opt.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false, RoleClaimType = "role" };
                        opt.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = (context) =>
                            {
                                Console.Error.WriteLine(
                                    $"##### [AspNetCore.Plumbing.DependencyInjection.cs] [ConfigureAuthentication()] JWT failed: {context.Exception.Message}. #####"
                                );

                                return Task.CompletedTask;
                            },
                            OnTokenValidated = (context) =>
                            {
                                Console.WriteLine(
                                    $"##### [AspNetCore.Plumbing.DependencyInjection.cs] [ConfigureAuthentication()] JWT valid: {
                                        string.Join(
                                            ", ",
                                            context.Principal!.Claims.Select((ctx) => $"{ctx.Type}={ctx.Value}")
                                        )
                                    }. #####"
                                );

                                return Task.CompletedTask;
                            }
                        };
                    });

                return builder;
            }

            /// <summary>
            /// Configures authorization for the application using the specified options.
            /// </summary>
            /// <param name="configureAction">A delegate to configure <see cref="PlumbingOptions"/> used for setting up authorization policies.</param>
            /// <returns>The <see cref="WebApplicationBuilder"/> instance with authorization configured.</returns>
            /// <exception cref="InvalidOperationException">Thrown when the <see cref="PlumbingOptions.ApiScope"/> is not provided or is empty.</exception>
            /// <remarks>
            /// This method sets up default and custom authorization policies.
            /// The default policy requires authenticated users.
            /// A custom policy is added based on the <see cref="PlumbingOptions.ApiScope"/> and <see cref="PlumbingOptions.ApiScopePolicyName"/>.
            /// </remarks>
            public WebApplicationBuilder ConfigureAuthorization(Action<PlumbingOptions> configureAction)
            {
                PlumbingOptions options = new();
                configureAction(options);

                if(string.IsNullOrWhiteSpace(options.ApiScope))
                {
                    throw new InvalidOperationException("##### [AspNetCore.Plumbing.DependencyInjection.cs] [ConfigureAuthorization()] ApiScope is required. #####");
                }

                builder.Services.AddAuthorization((opt) =>
                {
                    opt.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    opt.AddPolicy(
                        name: options.ApiScopePolicyName,
                        configurePolicy: (policy) =>
                        {
                            policy.RequireAuthenticatedUser();
                            policy.RequireClaim("scope", options.ApiScope);
                        }
                    );
                });

                return builder;
            }

            /// <summary>
            /// Adds a CORS policy to the application using the specified configuration options.
            /// </summary>
            /// <param name="configureAction">A delegate to configure <see cref="PlumbingOptions"/> used for setting up the CORS policy.</param>
            /// <returns>The <see cref="WebApplicationBuilder"/> instance with the CORS policy configured.</returns>
            /// <remarks>
            /// This method allows you to define a CORS policy with specific allowed origins, headers, and methods.
            /// If no allowed origins are specified in <see cref="PlumbingOptions.CorsAllowedOrigins"/>, the policy defaults to a permissive mode, allowing any origin.
            /// </remarks>
            public WebApplicationBuilder AddCorsPolicy(Action<PlumbingOptions> configureAction)
            {
                PlumbingOptions options = new();
                configureAction(options);

                builder.Services.AddCors((opt) =>
                {
                    opt.AddPolicy(options.CorsPolicyName, (policy) =>
                    {
                        // If no allowed origins are specified, fall back to allowing any origin (permissive mode).
                        if(options.CorsAllowedOrigins.Any((origin) => !string.IsNullOrEmpty(origin)))
                        {
                            policy
                                .WithOrigins(options.CorsAllowedOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        }
                        else
                        {
                            policy
                                .AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        }
                    });
                });

                return builder;
            }
        }
    }
}