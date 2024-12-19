using Nuke.Common;
using Nuke.Common.ProjectModel;

public interface IHaveSolution : INukeBuild
{
    [Solution]
    [Required]
    Solution Solution => TryGetValue(() => Solution);
}