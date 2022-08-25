using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialMedia.Core.CustomEntities;
using SocialMedia.Core.Interfaces;
using SocialMedia.Core.Services;
using SocialMedia.Infrastructure.Data;
using SocialMedia.Infrastructure.Extensions;
using SocialMedia.Infrastructure.Filters;
using SocialMedia.Infrastructure.Interfaces;
using SocialMedia.Infrastructure.Options;
using SocialMedia.Infrastructure.Repositories;
using SocialMedia.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddControllers(options => //Add Global Exceptions Filter 'GlobalExceptionFilter' in Infrastructure/Filters (Essential to manage the responses when the service returns a BarRequest)
            {
                options.Filters.Add<GlobalExceptionFilter>();
            })
            .AddNewtonsoftJson(options => // Ignore the circular reference error
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; 
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // To ignore propierties 'null'
            })
            .ConfigureApiBehaviorOptions(options => // To disable the validation ModelState implicity in Controller with 'ApiController' decorator to custom the error response 
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddOptions(Configuration) // Create a Singleton for enviroment variables for some data in appsettings.json
                .AddDbContexts(Configuration); //ConnectionStrings for a Dbcontext
            services.AddServices(); //dependecy injection with interfaces
            services.AddSwagger($"{Assembly.GetExecutingAssembly().GetName().Name}.xml"); // Generate documentation with Swagger

            //JWT Authentication Configuration
            services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Scheme for Authenticating
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Authentication:Issuer"],
                    ValidAudience = Configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]))
                };
            });

            //Configure the actionFilter as a Middleware globally
            services
            .AddMvc(options =>
            {
                options.Filters.Add<ValidationFilter>();
            })
            .AddFluentValidation(options => //Add Validators with FluentValidations package and execute as a Middleware globally
            {
                options.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSwagger(); // Add Swagger to the Configuration

            app.UseSwaggerUI(options =>
            {
                // RoutePrefix set
                //options.SwaggerEndpoint("../swagger/v1/swagger.json", "Social Media API v1");

                // RoutePrefix empty
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Media API v1");
                options.RoutePrefix = string.Empty;
            });

            app.UseAuthentication(); // First   
            app.UseAuthorization();  // and Then          

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
