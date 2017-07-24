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
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    var container = DependencyResolverInitializer.Initialize();
                    switch (options.Mode)
                    {
                        case Options.ProcessingMode.Rebuild:
                            container.GetInstance<RebuildTask>().RebuildFolderStructure(options.SourceDir, options.RebuildOverwrite, options.OutDir);
                            return;
                        case Options.ProcessingMode.DirScan:
                            container.GetInstance<MusicLibTask>().InitialiseAndStartDirScan();
                            return;
                    }
                }
                // Display the default usage information
                Console.WriteLine(options.GetUsage());
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
