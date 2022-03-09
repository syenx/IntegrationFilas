using Amazon.SecretsManager;
using Amazon.SQS;
using AutoMapper;
using config.rabbitMQ;
using config.rabbitMQ.impl;
using EDM.Infohub.BPO;
using EDM.Infohub.BPO.Models.SQS;
using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.BPO.Services.impl;
using EDM.Infohub.Integration.DataAccess;
using EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces;
using EDM.Infohub.Integration.Models;
using EDM.Infohub.Integration.RabbitMQ;
using EDM.Infohub.Integration.RabbitMQ.impl;
using EDM.Infohub.Integration.Security;
using EDM.Infohub.Integration.Services;
using EDM.Infohub.Integration.Services.Impl;
using EDM.Infohub.Integration.SQS;
using EDM.Infohub.Integration.SQS.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace EDM.Infohub.Integration
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
            services.AddControllers();

            //AWS
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSecretsManager>();
            services.AddMemoryCache();
            services.AddAWSService<IAmazonSQS>();

            //SQS
            var sqsPapelQueue = new SQSQueueConfig();
            var sqsEventoQueue = new SQSQueueConfig();
            Configuration.GetSection("SQSQueuesConfig").GetSection("papel").Bind(sqsPapelQueue);
            Configuration.GetSection("SQSQueuesConfig").GetSection("evento").Bind(sqsEventoQueue);
            var sqsConfig = new SQSConfig();
            sqsConfig.QueuesConfig = new Dictionary<string, ISQSQueueConfiguration>();
            sqsConfig.QueuesConfig.Add("papel", sqsPapelQueue);
            sqsConfig.QueuesConfig.Add("evento", sqsEventoQueue);
            services.AddSingleton<ISQSConfiguration>(sqsConfig);
            services.AddTransient<ISendSQSPapel, SendSQSPapel>();
            services.AddTransient<ISendSQSEvento, SendSQSEvento>();
            services.AddTransient<RastreamentoProcessor>();

            //Start Rabbit Service
            services.AddScoped<IRabbitMQConnection, RabbitMQConnection>();
            services.AddScoped<IReceiver, Receiver>();
            services.AddScoped<ISender, Sender>();
            services.AddScoped<RabbitReceiver>();
            services.AddScoped<MessageProcessor>();
            services.AddScoped<AuxiliaryMessageProcessor>();

            services.AddAutoMapper(typeof(Startup));

            services.AddScoped<IEDMCommonService, EDMCommonService>();
            services.AddScoped<IFixedIncomeOn, EDMFixedIncomeOn>();
            services.AddScoped<ISecureGateway, SecureGatewayClient>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<DecodeLuzMapper>();

            services.AddScoped<IEDMLuzService, EDMLuzService>();
            services.AddScoped<PuDeEventosRepository>();
            services.AddScoped<DatabaseService>();
            services.AddSingleton<INifiClient, NifiClient>();
            services.AddSingleton<INifiService, NifiService>();
            services.AddSingleton<IPriceTokenClient, PriceTokenClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RabbitReceiver receiver, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();

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

            receiver.StartRabbitService();
        }
    }
}
