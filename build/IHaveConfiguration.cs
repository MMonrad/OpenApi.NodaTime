using Nuke.Common;

public interface IHaveConfiguration : INukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration =>
        TryGetValue(() => Configuration)
        ?? (IsLocalBuild ? Configuration.Debug : Configuration.Release);
}
