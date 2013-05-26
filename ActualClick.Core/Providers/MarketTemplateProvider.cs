using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;

namespace WinBuyer.B2B.Common.Implementation
{
    public class MarketTemplateProvider
    {
        public string GetTemplate()
        {
            string templateFile = string.Format(@"{0}templates\actualclick-template.html", HttpRuntime.AppDomainAppPath);

            if (!File.Exists(templateFile))
                throw new FileNotFoundException(templateFile);

            string response = File.ReadAllText(templateFile);

            return response;
        }
    }
}
