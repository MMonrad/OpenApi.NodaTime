using Nuke.Common;
using Nuke.Common.Tools.GitVersion;

public interface IHaveGitVersion : INukeBuild
{
    [GitVersion(NoFetch = true, UpdateBuildNumber = false)]
    [Required]
    GitVersion Versioning => TryGetValue(() => Versioning);
}
