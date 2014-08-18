using System;
using log4net;

namespace SortOutMyMusicLib.Lib
{
    public interface IProcessRunner
    {
        int Exec(string cmdFile, string cmdArgs, string workingDir);
    }

    public class ProcessRunner : IProcessRunner
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ProcessRunner));

        public int Exec(string cmdFile, string cmdArgs, string workingDir)
        {
            // http://stackoverflow.com/questions/1469764/run-command-prompt-commands
            int returnCode;
            using (var process = new System.Diagnostics.Process())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = cmdFile,
                    Arguments = cmdArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = workingDir
                };
                process.StartInfo = startInfo;
                process.Start();

                var stdOut = process.StandardOutput.ReadToEnd();
                Log.Info(string.Concat("EXECUTING: '", cmdFile, " ", cmdArgs.TrimEnd(), "'"));
                Log.Info(stdOut);
                returnCode = process.ExitCode;
            }
            if (returnCode != 0)
                throw new Exception("Command Execution Failed!");
            return returnCode;
        }
    }

    public interface IAnnouncer
    {
    }
}