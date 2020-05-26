using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            IdentityBuilder idBuilder = services.AddIdentityCore<User> (opt => {
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 8;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            });

            idBuilder = new IdentityBuilder (idBuilder.UserType, typeof (Role), idBuilder.Services);
            
            idBuilder.AddEntityFrameworkStores<DataContext>();
            idBuilder.AddRoleValidator<RoleValidator<Role>>();
            idBuilder.AddRoleManager<RoleManager<Role>>();
            idBuilder.AddSignInManager<SignInManager<User>>();

            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer (c => c.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey (Encoding.ASCII
                            .GetBytes (Configuration.GetSection ("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                });
            services.AddDbContext<DataContext> (x => x.UseSqlServer (Configuration.GetConnectionString ("DefaultConnection")));
            services.AddControllers (opt=>{
                var policy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                opt.Filters.Add(new AuthorizeFilter(policy));
            })
            .AddNewtonsoftJson (opt => {
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddCors ();
            services.AddAutoMapper ((typeof (DatingRepository)).Assembly);
            services.AddScoped<IDatingRepository, DatingRepository> ();
            services.Configure<CloudinarySettings> (Configuration.GetSection ("CloudinarySettings"));

            
            services.AddScoped<LogUserActivity> ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseExceptionHandler (builder => builder
                    .Run (async context => {
                        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature> ();
                        if (error != null) {
                            context.Response.AddApplicationError (error.Error.Message);
                            await context.Response.WriteAsync (error.Error.Message);
                        }
                    }));
            }

            // app.UseHttpsRedirection();
            app.UseCors (x => x.AllowAnyOrigin ().AllowAnyMethod ().AllowAnyHeader ());
            app.UseAuthentication ();
            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}