using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public interface IPackNuget : IHaveSolution, IHaveGitVersion, IHaveConfiguration, IHaveArtifacts
{
    [UsedImplicitly]
    Target PackNuGet =>
        d => d
            .TryDependsOn<ICompile>()
            .Executes(() =>
            {
                DotNetPack(s =>
                    s.SetProject(Solution)
                        .EnableNoBuild()
                        .SetVersion(Versioning.SemVer)
                        .SetConfiguration(Configuration)
                        .EnableNoRestore()
                        .SetInformationalVersion(Versioning.InformationalVersion)
                        .SetProperty("PackageOutputPath", ArtifactsDirectory)
                );
            });
}
