using SortOutMyMusicLib.Lib;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using SystemWrapper.IO;

namespace SortOutMyMusicLib
{
    public class DependencyResolverInitializer
    {
        public static IContainer Initialize(Registry pluginRegistry=null)
        {
            var container = new Container(x =>
            {
                x.AddRegistry(new CoreRegistry());
                if (pluginRegistry != null)
                    x.AddRegistry(pluginRegistry);
            });
            return container;
        }

        public class CoreRegistry : Registry
        {
            public CoreRegistry()
            {
                Scan(scan =>
                {
                    scan.TheCallingAssembly();
                    scan.AssemblyContainingType<IConfigReader>();
                    scan.AssemblyContainingType<IFileWrap>();
                    scan.LookForRegistries();
                    scan.WithDefaultConventions();
                });

                For<IITunesLibraryHelper>().Use<ITunesLibraryHelper>();
                For<IAppConstants>().Singleton().Use<AppConstants>();
                //For<IDirToDoList>().Singleton();
                //For<IITunesLibraryHelper>().Singleton();
            }
        }
    }
}