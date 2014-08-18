using System;
using log4net;
using SortOutMyMusicLib.Lib;

namespace SortOutMyMusicLib
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            Environment.ExitCode = 0;
            log4net.Config.XmlConfigurator.Configure();
            try
            {
                var container = DependencyResolverInitializer.Initialize();
                container.GetInstance<ConsoleClient>().Execute();
            }
            catch (Exception e)
            {
                LogException(e);
                Environment.ExitCode = 1;
            }
        }

        private static void LogException(Exception e, int depth = 0)
        {
            if (e == null)
                return;
            if (depth > 0)
                LogError(string.Format("[Inner Exception: {0}]", depth));
            LogError(string.Format("Type: {0}", e.GetType()));
            LogError(string.Format("Message: {0}", e.Message));
            if (depth == 0)
            {
                LogError(string.Format("StackTrace: {0}", e.StackTrace));
            }
            LogException(e.InnerException, depth + 1);
        }

        public static void LogError(string message)
        {
            try
            {
                Log.Error(message);
            }
            catch (Exception)
            {
                Console.Error.Write(message);
            }
        }
    }
}
