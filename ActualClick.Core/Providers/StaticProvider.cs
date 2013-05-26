using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace Extentions.DataService.Providers
{
    public class StaticProvider
    {
        public dynamic GetJs(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Extension != ".js")
                throw new Exception("current file is not js file");

            string fileText = null;

            try
            {
                fileText = File.ReadAllText(Path.Combine(HttpRuntime.AppDomainAppPath, fileInfo.Name));
            }
            catch
            {
                fileText = "current file not found";
            }

            return fileText;
        }
        public dynamic GetCss(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Extension != ".css")
                throw new Exception("current file is not css file");

            string fileText = null;

            try
            {
                fileText = File.ReadAllText(Path.Combine(HttpRuntime.AppDomainAppPath, fileInfo.Name));
            }
            catch
            {
                fileText = "current file not found";
            }

            return fileText;
        }
        public dynamic GetImage(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Extension != ".png" && fileInfo.Extension != ".jpg" && fileInfo.Extension != ".gif")
                throw new Exception("current file is not image file");

            string imagesFolder = Path.Combine(HttpRuntime.AppDomainAppPath, "images");

            byte[] fileBytes = null;

            try
            {
                fileBytes = File.ReadAllBytes(Path.Combine(imagesFolder, fileInfo.Name));
            }
            catch
            {
                throw new Exception("current file not found");
            }

            return new
            {
                content = fileBytes,
                content_type = "image/" + fileInfo.Extension.Replace(".", "")
            };
        }
    }
}
