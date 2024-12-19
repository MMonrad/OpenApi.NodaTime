using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public interface IRestore : IHaveSolution
{
    Target Restore =>
        d => d
            .TryDependentFor<ICompile>()
            .Executes(() =>
            {
                DotNetRestore(s => s
                    .SetProjectFile(Solution)
                    .EnableLockedMode());
            });
}
