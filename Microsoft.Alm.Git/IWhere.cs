using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Alm.Git
{
    public interface IWhere
    {
        bool GitPortableConfig(out string portableConfig);
        bool GitSystemConfig(GitInstallation? gitInstallation, out string systemConfig);
        bool GitXdgConfig(out string xdgConfig);
        bool GitGlobalConfig(out string globalConfig);
        bool GitLocalConfig(string directory, out string localConfig);
        string Home();
        bool FindGitInstallations(out List<GitInstallation> installations);
        bool FindGitInstallation(string customPath, KnownGitDistribution gitForWindows32v1, out GitInstallation installation);
        bool GitLocalConfig(out string gitConfigPath);
    }
}
