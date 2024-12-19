using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;

public interface IClean : INukeBuild
{
    [UsedImplicitly]
    Target Clean =>
        d => d
            .Before<IRestore>()
            .Executes(() =>
            {
                SourceDirectory.GlobDirectories("**/bin", "**/obj")
                    .ForEach(path => path.DeleteDirectory());
                TemporaryDirectory.CreateOrCleanDirectory();
            });

    AbsolutePath SourceDirectory => RootDirectory / "src";
}
