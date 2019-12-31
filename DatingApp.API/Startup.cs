using DatingApp.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DatingApp.API.Helpers;
using AutoMapper;

namespace DatingApp.API
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
      var cnnString = Configuration.GetConnectionString("DefaultConnection");
      services.AddDbContext<DataContext>(x => x.UseSqlite(cnnString));
      services.AddControllers()
        .AddNewtonsoftJson(opt => {
          opt.SerializerSettings.ReferenceLoopHandling
            = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });
      services.AddCors();
      var cloudinarySettings = Configuration.GetSection("CloudinarySettings");
      services.Configure<CloudinarySettings>(cloudinarySettings);
      services.AddAutoMapper(typeof(DatingRepository).Assembly);
      services.AddScoped<IAuthRepository, AuthRepository>();
      services.AddScoped<IDatingRepository, DatingRepository>();
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
            var tokenValue = Configuration.GetSection("AppSettings:Token").Value;
            var encodedTokenValue = Encoding.ASCII.GetBytes(tokenValue);

            options.TokenValidationParameters = new TokenValidationParameters
            {
              ValidateIssuerSigningKey = true,
              IssuerSigningKey = new SymmetricSecurityKey(encodedTokenValue),
              ValidateIssuer = false,
              ValidateAudience = false
            };
          });
      services.AddScoped<LogUserActivity>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else {
        app.UseExceptionHandler(builder => {
          builder.Run(async context => {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var error = context.Features.Get<IExceptionHandlerFeature>();
            if (error != null) {
              context.Response.AddApplicationError(error.Error.Message);
              await context.Response.WriteAsync(error.Error.Message);
            }
          });
        });
      }

      // app.UseHttpsRedirection();

      app.UseRouting();

      app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
