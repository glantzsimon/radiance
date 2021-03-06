﻿using K9.SharedLibrary.Models;
using K9.WebApplication.Config;
using K9.WebApplication.Services.Stripe;
using System;
using System.Threading;
using System.Web.Hosting;

namespace K9.WebApplication
{
    public class JobScheduler : IRegisteredObject
    {
        private readonly IStripeService _stripeService;
        private readonly TaskConfiguration _config;
        private Timer _timer;

        public JobScheduler(IOptions<TaskConfiguration> config, IStripeService stripeService)
        {
            _stripeService = stripeService;
            _config = config.Value;
            StartTimer();
        }

        public void Stop(bool immediate)
        {
            _timer.Dispose();
            HostingEnvironment.UnregisterObject(this);
        }

        private void StartTimer()
        {
            var delayStartby = TimeSpan.FromSeconds(_config.DelayStartBySeconds);
            var repeatEvery = TimeSpan.FromSeconds(_config.RepeatEverySeconds);

            _timer = new Timer(RunTasks, null, delayStartby, repeatEvery);
        }

        private void RunTasks(object state)
        {
        }
       
    }
}