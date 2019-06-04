using System;
using System.IO;
using AutoMapper;
using System.Reflection;
using System.Text;
using DBRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using mohome_api.Infrastructure;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using mohome_api.API_Errors;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using mohome_api.Filters;

namespace mohome_api
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
            services.AddRouting(options => options.LowercaseUrls = true);

            Mapper.Initialize(cfg => cfg.AddProfile<MappingProfile>());

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            services.AddAutoMapper();
            services.AddCors(o => o.AddPolicy("CORSPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            }));


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {

                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = Configuration["Jwt:Issuer"],
                       ValidAudience = Configuration["Jwt:Issuer"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                       // ClockSkew = TimeSpan.Zero
                   };
               });


            services.AddAutoMapper();
            services.AddMvc(options =>
            {
                options.Filters.Add(new ModelActionFilter());
                options.Filters.Add(new MohomeAuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1); ;
            services.AddMvc().AddControllersAsServices();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Mohome API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.DocumentFilter<LowercaseDocumentFilter>();
            });
           

            services.AddTransient<IIdentityRepository, IdentityRepository>();
            services.AddTransient<IPhotoRepository, PhotoRepository>();
            services.AddTransient<IMusicRepository, MusicRepository>();
            services.AddTransient<MohomeContext>();
            services.AddSingleton<PhotoHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("CORSPolicy");


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.ConfigureExceptionHandler();
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mohome API V1");
                c.RoutePrefix = string.Empty;
            });

           

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
