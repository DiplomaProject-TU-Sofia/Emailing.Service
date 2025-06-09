using Azure.Messaging.ServiceBus;
using Emailing.Service.Data;
using Emailing.Service.Data.Repositories;
using Emailing.Service.Services;
using Microsoft.EntityFrameworkCore;

namespace Emailing.Service
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		public Startup(IConfiguration configuration) => Configuration = configuration;

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<EmailingDbContext>(options =>
					options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")),
					ServiceLifetime.Singleton);

			services.AddControllers();
			services.AddSingleton(_ =>
				new ServiceBusClient(Configuration.GetConnectionString("ServiceBus")));

			services.AddHostedService<EmailingBackgroundService>();
			services.AddTransient<UserRepository>();
			services.AddLogging();


			services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", builder =>
				{
					builder
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader();
				});
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseCors("AllowAll");

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
