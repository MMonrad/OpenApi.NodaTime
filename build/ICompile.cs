using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public interface ICompile : IHaveSolution, IHaveGitVersion, IHaveConfiguration
{
    Target Compile =>
        _ => _
            .DependsOn<IRestore>()
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(Versioning.AssemblySemVer)
                    .SetFileVersion(Versioning.AssemblySemFileVer)
                    .SetInformationalVersion(Versioning.InformationalVersion)
                    .SetContinuousIntegrationBuild(IsServerBuild)
                    .EnableNoRestore());
            });
}
