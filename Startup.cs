// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using idsrv4testaspid.Data;
using idsrv4testaspid.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Serilog;

namespace idsrv4testaspid
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
			string connectionString = Configuration.GetConnectionString("IdentityServerDB");


			services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

	        var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

	        var identityServer = services.AddIdentityServer(options =>
		        {
			        options.Events.RaiseErrorEvents = true;
			        options.Events.RaiseInformationEvents = true;
			        options.Events.RaiseFailureEvents = true;
			        options.Events.RaiseSuccessEvents = true;
		        })
		        .AddConfigurationStore(options =>
		        {
			        options.ConfigureDbContext = builder =>
				        builder.UseSqlServer(connectionString,
					        sql => sql.MigrationsAssembly(migrationsAssembly));
		        })
		        // this adds the operational data from DB (codes, tokens, consents)
		        .AddOperationalStore(options =>
		        {
			        options.ConfigureDbContext = builder =>
				        builder.UseSqlServer(connectionString,
					        sql => sql.MigrationsAssembly(migrationsAssembly));

			        // this enables automatic token cleanup. this is optional.
			        options.EnableTokenCleanup = true;
			        options.TokenCleanupInterval = 30;
		        }).AddAspNetIdentity<ApplicationUser>();

            if (Environment.IsDevelopment())
            {
	            identityServer.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
                    options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
                });
        }
	    private void InitializeDatabase(IApplicationBuilder app)
	    {
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				using (var context = serviceScope.ServiceProvider.GetService<PersistedGrantDbContext>())
				{
					context.Database.Migrate();
				}
				using (var context = serviceScope.ServiceProvider.GetService<ConfigurationDbContext>())
				{
					context.Database.Migrate();
				}
				using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
				{
					context.Database.Migrate();
				}

				//using (var scope = serviceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				//{
				//	scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

				//	var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
				//	context.Database.Migrate();
				//	EnsureSeedIdSrv3Data(context);
				//}
				//serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

				//using (var scope = serviceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				//{
				//	scope.ServiceProvider.GetService<ConfigurationDbContext>().Database.Migrate();

				//	var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
				//	context.Database.Migrate();
				//	EnsureSeedIdSrv3Data(context);
				//}
				//var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				//context.Database.Migrate();

				//SeedData.EnsureSeedAspIdData(serviceScope.ServiceProvider);

			 //   if (!context.Clients.Any())
			 //   {
				//    foreach (var client in Config.GetClients())
				//    {
				//	    context.Clients.Add(client.ToEntity());
				//    }
				//    context.SaveChanges();
			 //   }

			 //   if (!context.IdentityResources.Any())
			 //   {
				//    foreach (var resource in Config.GetIdentityResources())
				//    {
				//	    context.IdentityResources.Add(resource.ToEntity());
				//    }
				//    context.SaveChanges();
			 //   }

			 //   if (!context.ApiResources.Any())
			 //   {
				//    foreach (var resource in Config.GetApis())
				//    {
				//	    context.ApiResources.Add(resource.ToEntity());
				//    }
				//    context.SaveChanges();
			 //   }
		    }
	    }

		public void Configure(IApplicationBuilder app)
        {
	        InitializeDatabase(app);

			if (Configuration.GetValue<bool>("EnableDebugLog"))
	        {
		        Log.Logger = new LoggerConfiguration()
			        .MinimumLevel.Debug()
			        .Enrich.FromLogContext()
			        .WriteTo.RollingFile(Path.Combine(Environment.ContentRootPath, "log-{Date}.txt"))
			        .CreateLogger();
	        }
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

			app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
