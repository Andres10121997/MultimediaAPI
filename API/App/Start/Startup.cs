using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using System;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace API.App.Start
{
    public static class Startup
    {
        #region Init
        public static WebApplication InitApp(string[] args)
        {
            #region Objects
            WebApplicationBuilder builder;
            WebApplication app;
            #endregion

            builder = WebApplication.CreateBuilder(args: args);

            ConfigureServices(Builder: builder);

            app = builder.Build();

            Configure(App: app);

            return app;
        }

        public static async Task<WebApplication> InitAppAsync(string[] args) => await Task.Run<WebApplication>(function: () => InitApp(args: args));
        #endregion



        #region Configure
        private static void ConfigureServices(WebApplicationBuilder Builder)
        {
            Builder.Services.AddMvc()
                .AddJsonOptions(
                    configure: options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(item: new JsonStringEnumConverter());
                    }
                );
            Builder.Services.AddEndpointsApiExplorer();

            // Limitar peticiones por IP.
            // https://www.youtube.com/shorts/EoJl5wgE5UQ
            Builder.Services.AddRateLimiter(
                options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    options.AddFixedWindowLimiter(
                        policyName: "Limiter",
                        fixedWindowOptions =>
                        {
                            fixedWindowOptions.PermitLimit = 4; // Solicitudes máximas permitidas
                            fixedWindowOptions.Window = TimeSpan.FromSeconds(seconds: 15); // Ventana de tiempo
                            fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Cómo manejar solicitudes en cola
                            fixedWindowOptions.QueueLimit = 1; // Máximo de solicitudes para hacer cola
                        }
                    );
                }
            );

            // Cierta información que se visualiza en la interfaz de usuario.
            Builder.Services.AddSwaggerGen(options =>
            {
                // https://learn.microsoft.com/es-mx/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.1&tabs=visual-studio
                options.SwaggerDoc(
                    name: "v1",
                    info: new OpenApiInfo
                    {
                        Title = "Indicadores de Chile",
                        Version = "v1",
                        Description = "Una API para obtener, principalmente, la lista de distintas \"divisas\", como la UF y el dólar.",
                        Contact = new OpenApiContact
                        {
                            Name = "Andrés Sagredo",
                            Email = string.Empty,
                            Url = new Uri(uriString: "https://github.com/Andres10121997")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "Uso bajo Apache 2.0",
                            Url = new Uri(uriString: "https://www.apache.org/licenses/LICENSE-2.0")
                        }
                    }
                );
            });
        }

        private static void Configure(WebApplication App)
        {
            // Configure the HTTP request pipeline.
            if (App.Environment.IsDevelopment())
            {
                App.UseSwagger();
                App.UseSwaggerUI();
            }

            App.UseRateLimiter();

            // Forces redirection to HTTPS
            App.UseHttpsRedirection();

            App.UseAuthorization();

            App.MapControllers()
                .RequireRateLimiting(policyName: "Limiter");
        }
        #endregion
    }
}