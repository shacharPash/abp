﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Quartz;

namespace Volo.Abp.BackgroundJobs.Quartz
{
    public class QuartzJobExecutionAdapter<TArgs> : IJob
    {
        public ILogger<QuartzJobExecutionAdapter<TArgs>> Logger { get; set; }
        
        protected AbpBackgroundJobOptions Options { get; }
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        protected IBackgroundJobExecuter JobExecuter { get; }
        public QuartzJobExecutionAdapter(
            IOptions<AbpBackgroundJobOptions> options,
            IBackgroundJobExecuter jobExecuter,
            IServiceScopeFactory serviceScopeFactory)
        {
            JobExecuter = jobExecuter;
            ServiceScopeFactory = serviceScopeFactory;
            Options = options.Value;
            Logger = NullLogger<QuartzJobExecutionAdapter<TArgs>>.Instance;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (!Options.IsJobExecutionEnabled)
            {
                Logger.LogWarning("Background jobs system is disabled");
                return;
            }
            
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var args = (TArgs)context.JobDetail.JobDataMap.Get(nameof(TArgs));
                var jobType = Options.GetJob(typeof(TArgs)).JobType;
                var jobContext = new JobExecutionContext(scope.ServiceProvider, jobType, args);
                await JobExecuter.ExecuteAsync(jobContext);
            }
        }
    }
}
