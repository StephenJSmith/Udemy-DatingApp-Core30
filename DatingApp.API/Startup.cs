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
      services.AddControllers();
      services.AddCors();
      services.AddScoped<IAuthRepository, AuthRepository>();
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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
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
