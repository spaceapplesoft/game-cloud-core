using Microsoft.Extensions.Logging;

namespace GameCloud.Core
{
    public static class Logs
    {
        public static ILogger General { get; private set; }

        public static LoggerFactory Factory { get; }

        static Logs()
        {
            Factory = new LoggerFactory();
            General = Factory.CreateLogger("General");
        }
    }
}