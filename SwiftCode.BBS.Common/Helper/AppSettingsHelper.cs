using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SwiftCode.BBS.Common.Helper
{
    public class AppSettingsHelper
    {
        private static IConfiguration Configuration { get; set; }

        private static string ContentPath { get;set; }

        public AppSettingsHelper(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public AppSettingsHelper(string contentPath)
        {
            var appsettingsPath = "appsettings.json";

            //如果配置文件是根据环境变量来区分的，可以这样配置
            //appsettingsPath = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";

            Configuration = new ConfigurationBuilder()
                .SetBasePath(contentPath)
                .Add(new JsonConfigurationSource { Path= appsettingsPath,Optional=false,ReloadOnChange=true })
                .Build();

            ContentPath = contentPath;
        }

        /// <summary>
        /// 封装要操作的字符
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static string app(params string[] sections)
        {
            try
            {
                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception)
            {

            }
            return "";
        }

        /// <summary>
        /// 递归获取配置信息数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static List<T> app<T>(params string[] sections)
        {
            List<T> list = new List<T>();
            Configuration.Bind(string.Join(":", sections), list);
            return list;
        }
    }
}