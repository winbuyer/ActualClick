using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Bootstrapper;
using log4net.Config;
using System.IO;
using System.Web;
using Nancy.Conventions;
using Nancy;
using Nancy.Session;

namespace WinBuyer.B2B.Widget.DataService.Core
{
    public class ErrorHandler : IApplicationStartup
    {
        public IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                return null;
            }
        }
        public IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                return null;
            }
        }
        public IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get
            {
                return null;
            }
        }

        public void Initialize(IPipelines pipelines)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(HttpRuntime.AppDomainAppPath, "log4net.config")));
            CookieBasedSessions.Enable(pipelines);
        }
    }

    public class CustomBoostrapper : DefaultNancyBootstrapper
    {

    }
}
