using System.Text;
using CommandLine;
using CommandLine.Text;

namespace SortOutMyMusicLib
{
    class Options
    {
        [Option('m', "mode", Required = true, HelpText = "Sets the processing mode.")]
        public ProcessingMode Mode { get; set; }

        [Option('s', "sourceDir", Required = true, HelpText = "Source root directory to find media files in.")]
        public string SourceDir { get; set; }

        [Option('o', "outDir", HelpText = "directory where output files will be copied to (default to sourceDir).")]
        public string OutDir { get; set; }

        [Option('r', "rebuildOverwrite", DefaultValue = false, HelpText = "Allows existing target files to be overwritten when doing a rebuild.")]
        public bool RebuildOverwrite { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var text = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            var usage = new StringBuilder();
            usage.AppendLine(text.ToString());
            usage.AppendLine();
            usage.AppendLine("Possible processing mode values:");
            usage.AppendLine("rebuild: Reorganises media files into Artist / Album folder structure, using ID3 tag.");
            usage.AppendLine("scan:    Performs a range of tidy functions by walking the media library.");
            return usage.ToString();
        }

        public enum ProcessingMode
        {
            Rebuild,
            DirScan
        }
    }
}