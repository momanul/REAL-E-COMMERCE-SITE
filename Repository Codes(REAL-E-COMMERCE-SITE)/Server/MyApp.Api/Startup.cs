using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyApp.DAL;
using MyApp.DAL.Models;
using MyApp.DTO.Identity;
using MyApp.DTO.ViewModels;
using MyApp.Identity;
using MyApp.Identity.Models;
using MyApp.Library.SentEmail;
using MyApp.Library.UniqueValueProviders;
using MyApp.Repo.DAL;
using MyApp.Repo.DAL.Identity;
using MyApp.Repo.DAL.Repository;
using MyApp.Repo.Interface;

namespace MyApp.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }//m        

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRepository<ProductVM>, ProductRepository>();
            services.AddScoped<IRepository<CategoryVM>, CategoryRepository>();
            services.AddScoped<IRepository<ProductSizeVM>, ProductSizeRepository>();
            services.AddScoped<IRepository<ColorVM>, ColorRepository>();
            services.AddScoped<IRepository<BrandVM>, BrandRepository>();
            services.AddScoped<IRepository<ProductImageVM>, ProductMGRepository>();
            services.AddScoped<ProductMGRepository>();
            services.AddScoped<ProductImages>();

            //For Identity
            services.AddScoped<IAsyncRepository<RoleVM>, RoleRepository>();
            services.AddScoped<IUserRepository<UserVM>, UserRepository>();
            services.AddScoped<ISignInRepository<LoginVM>, SignInRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //add library service
            services.AddScoped<ISentEmail, SentEmail>();
            services.AddScoped<IRandomGenerator, RandomGenerator>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //For Session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });

            //___add cors
            services.AddCors();//
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //___DataDbContext
            services.AddDbContext<DataDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("MyAppData"), b => b.MigrationsAssembly("MyApp.Api")));
            //___MyAppIdentityDbContext
            services.AddDbContext<MyAppIdentityDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("MyAppIdentity"), b => b.MigrationsAssembly("MyApp.Api")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<MyAppIdentityDbContext>()
               .AddDefaultTokenProviders();

            //__Add and Valid Token for Authentication
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(x =>
             {
                 x.SaveToken = true;
                 x.RequireHttpsMetadata = false;

                 x.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = "api/Account/login",
                     ValidIssuer = "api/Account/login",
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecureKey"))
                 };
             });


        }//m

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            //___use cors
            app.UseCors(options => options
                           .AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials());

            //___Authorize checking 
            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }//m

    }//c
}//ns
