﻿using Carter;
using FluentValidation;
using STemplate.Application.Assemblies;
using STemplate.Application.Extension;
using STemplate.Insfrastructure.Utilities.ApiDoc.Swagger;
using STemplate.Insfrastructure.Utilities.Caching.Redis;
using STemplate.Insfrastructure.Utilities.Cors;
using STemplate.Insfrastructure.Utilities.HangFire;
using STemplate.Insfrastructure.Utilities.Identity;
using STemplate.Insfrastructure.Utilities.Logging;
using STemplate.Insfrastructure.Utilities.MediatR;
using STemplate.Insfrastructure.Utilities.ServiceBus;
using STemplate.Insfrastructure.Utilities.Telemetry;
using STemplate.Persistence.Context;
using STemplate.Persistence.Extensions;
using STemplate.Persistence.GenericRepository;
using STemplate.Persistence.SearchEngine;
using STemplate.Persistence.UnitOfWork;

namespace STemplate.Extensions
{
    public static class StartUpInstallExtension
    {
        public static async Task AddStartupServicesAsync(this WebApplicationBuilder builder)
        {
            builder.Services.AddCarter();
            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddEndpointsApiExplorer();
            builder.AddCustomSwaggerGen();
            builder.AddCors();
            builder.Services.AddMvc();
            //Log-Cache-Mediatr-FluentValidation-Mass transit
            builder.AddSerilog();
            builder.AddTelemeter();
            builder.AddRedis();
            var assembly = ApiAssemblyExtensions.GetLibrariesAssemblies();
            builder.AddHangFire(assembly);
            await builder.Services.ConfigureDbContextAsync(builder.Configuration);
            builder.AddMediatR(assembly);
            builder.Services.AddValidatorsFromAssembly(ApplicationAssemblyExtension.GetApplicationAssembly(), includeInternalTypes: true);
            builder.AddCustomMassTransit(assembly, (busRegistrationContext, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.AddConsumers(busRegistrationContext);
                busFactoryConfigurator.AddPublishers();
            });
            builder.AddIdentitySettings();
            //Service Registered
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICoreDbContext, CoreDbContext>();
            builder.Services.AddScoped(typeof(IWriteDbRepository<>), typeof(WriteDbRepository<>));
            builder.Services.AddScoped(typeof(IReadDbRepository<>), typeof(ReadDbRepository<>));
            await SearchEngineRegistration.MigrateElasticDbAsync(assembly, builder.Configuration);
            builder.Services.AddScoped<ICoreSearchEngineContext, CoreSearchEngineContext>();
        }
    }
}
