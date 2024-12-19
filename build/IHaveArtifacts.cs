using Nuke.Common;
using Nuke.Common.IO;

public interface IHaveArtifacts : INukeBuild
{
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
}
