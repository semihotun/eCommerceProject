﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STemplate.Persistence.Context;
namespace STemplate.Persistence.Extensions
{
    public static class DbContextRegistiration
    {
        /// <summary>
        /// Add DbContext
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var regionName = configuration["RegionName"] + "SqlConnectionString";
            var connectionString = configuration[regionName];
            services.AddEntityFrameworkSqlServer().AddDbContext<CoreDbContext>(option =>
            {
                option.UseSqlServer(connectionString,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 15,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });
            var optionBuilder = new DbContextOptionsBuilder<CoreDbContext>()
                .UseSqlServer(connectionString);
            using (var ctx = new CoreDbContext(optionBuilder.Options, null))
            {
                if (!ctx.Database.EnsureCreated())
                {
                    ctx.Database.EnsureCreated();
                    ctx.Database.Migrate();
                }
                else
                {
                    ctx.Database.Migrate();
                }
            }
            return services;
        }
    }
}