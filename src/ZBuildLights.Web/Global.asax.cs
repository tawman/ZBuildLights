﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NLog;
using StructureMap.Web.Pipeline;
using ZBuildLights.Core.Services;
using ZWaveControl;

namespace ZBuildLights.Web
{
    public class MvcApplication : HttpApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly Guid ApplicationId = Guid.NewGuid();

        protected void Application_Start()
        {
            Log.Debug("Application ({0}) starting", ApplicationId);

            AreaRegistration.RegisterAllAreas();
            InitializeStaticFactories();
            AutoMapperConfig.Initialize();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void InitializeStaticFactories()
        {
            SystemClock.UseCurrentTime();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            HttpContextLifecycle.DisposeAndClearAll();
        }

        protected void Application_End()
        {
            ZWaveManagerFactory.Destroy();
            Log.Debug("Application ({0}) ending", ApplicationId);
        }
    }
}