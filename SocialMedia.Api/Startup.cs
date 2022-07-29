using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocialMedia.Core.Interfaces;
using SocialMedia.Core.Services;
using SocialMedia.Infrastructure.Data;
using SocialMedia.Infrastructure.Filters;
using SocialMedia.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
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
                })
                .ConfigureApiBehaviorOptions(options => // To disable the validation ModelState implicity in Controller with 'ApiController' decorator to custom the error response 
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

            //ConnectionStrings for a Dbcontext
            services.AddDbContext<SocialMediaContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("SocialMedia"))
            );

            //dependecy injection with interfaces
            services.AddTransient<IPostService, PostService>();

            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Configure the actionFilter as a Middleware globally
            services.AddMvc(options =>
            {
                options.Filters.Add<ValidationFilter>();
            }).AddFluentValidation(options => //Add Validators with FluentValidations package and execute as a Middleware globally
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
