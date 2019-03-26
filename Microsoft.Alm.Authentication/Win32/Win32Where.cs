using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Microsoft.Alm.Authentication.Git;
using Microsoft.Win32;

namespace Microsoft.Alm.Authentication.Win32
{
    internal class Win32Where : Git.Where
    {
        public Win32Where(RuntimeContext context) : base(context)
        {
        }

        public IRegistryStorage RegistryStorage
        {
            get { return Storage as IRegistryStorage; }
        }

        public override bool FindGitInstallations(out List<Installation> installations)
        {
            const string GitAppName = @"Git";
            const string GitSubkeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Git_is1";
            const string GitValueName = "InstallLocation";

            void ScanApplicationData(IList<Installation> output)
            {
                var appDataRoamingPath = string.Empty;

                if ((appDataRoamingPath = Settings.GetFolderPath(Environment.SpecialFolder.ApplicationData)) != null)
                {
                    appDataRoamingPath = Path.Combine(appDataRoamingPath, GitAppName);

                    output.Add(new Installation(Context, appDataRoamingPath, KnownDistribution.GitForWindows64v2));
                    output.Add(new Installation(Context, appDataRoamingPath, KnownDistribution.GitForWindows32v2));
                    output.Add(new Installation(Context, appDataRoamingPath, KnownDistribution.GitForWindows32v1));
                }

                var appDataLocalPath = string.Empty;

                if ((appDataLocalPath = Settings.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) != null)
                {
                    appDataLocalPath = Path.Combine(appDataLocalPath, GitAppName);

                    output.Add(new Installation(Context, appDataLocalPath, KnownDistribution.GitForWindows64v2));
                    output.Add(new Installation(Context, appDataLocalPath, KnownDistribution.GitForWindows32v2));
                    output.Add(new Installation(Context, appDataLocalPath, KnownDistribution.GitForWindows32v1));
                }

                var programDataPath = string.Empty;

                if ((programDataPath = Settings.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) != null)
                {
                    programDataPath = Path.Combine(programDataPath, GitAppName);

                    output.Add(new Installation(Context, programDataPath, KnownDistribution.GitForWindows64v2));
                    output.Add(new Installation(Context, programDataPath, KnownDistribution.GitForWindows32v2));
                    output.Add(new Installation(Context, programDataPath, KnownDistribution.GitForWindows32v1));
                }
            }

            void ScanProgramFiles(IList<Installation> output)
            {
                var programFiles32Path = string.Empty;

                if ((programFiles32Path = Settings.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)) != null)
                {
                    programFiles32Path = Path.Combine(programFiles32Path, GitAppName);

                    output.Add(new Installation(Context, programFiles32Path, KnownDistribution.GitForWindows32v2));
                    output.Add(new Installation(Context, programFiles32Path, KnownDistribution.GitForWindows32v1));
                }

                if (Settings.Is64BitOperatingSystem)
                {
                    var programFiles64Path = string.Empty;

                    if ((programFiles64Path = Settings.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) != null)
                    {
                        programFiles64Path = Path.Combine(programFiles64Path, GitAppName);

                        output.Add(new Installation(Context, programFiles64Path, KnownDistribution.GitForWindows64v2));
                    }
                }
            }

            void ScanRegistry(IList<Installation> output)
            {
                var reg32HklmPath = RegistryStorage.RegistryReadString(RegistryHive.LocalMachine, RegistryView.Registry32, GitSubkeyName, GitValueName);
                var reg32HkcuPath = RegistryStorage.RegistryReadString(RegistryHive.CurrentUser, RegistryView.Registry32, GitSubkeyName, GitValueName);

                if (!string.IsNullOrEmpty(reg32HklmPath))
                {
                    output.Add(new Installation(Context, reg32HklmPath, KnownDistribution.GitForWindows32v2));
                    output.Add(new Installation(Context, reg32HklmPath, KnownDistribution.GitForWindows32v1));
                }

                if (!string.IsNullOrEmpty(reg32HkcuPath))
                {
                    output.Add(new Installation(Context, reg32HkcuPath, KnownDistribution.GitForWindows32v2));
                    output.Add(new Installation(Context, reg32HkcuPath, KnownDistribution.GitForWindows32v1));
                }

                if (Settings.Is64BitOperatingSystem)
                {
                    var reg64HklmPath = RegistryStorage.RegistryReadString(RegistryHive.LocalMachine, RegistryView.Registry64, GitSubkeyName, GitValueName);
                    var reg64HkcuPath = RegistryStorage.RegistryReadString(RegistryHive.CurrentUser, RegistryView.Registry64, GitSubkeyName, GitValueName);

                    if (!string.IsNullOrEmpty(reg64HklmPath))
                    {
                        output.Add(new Installation(Context, reg64HklmPath, KnownDistribution.GitForWindows64v2));
                    }

                    if (!string.IsNullOrEmpty(reg64HkcuPath))
                    {
                        output.Add(new Installation(Context, reg64HkcuPath, KnownDistribution.GitForWindows64v2));
                    }
                }
            }

            void ScanShellPath(IList<Installation> output)
            {
                var shellPathValue = string.Empty;

                if (FindApp(GitAppName, out shellPathValue))
                {
                    // `Where.App` returns the path to the executable, truncate to the installation root
                    if (shellPathValue.EndsWith(Installation.AllVersionCmdPath, StringComparison.OrdinalIgnoreCase))
                    {
                        shellPathValue = shellPathValue.Substring(0, shellPathValue.Length - Installation.AllVersionCmdPath.Length);
                    }

                    output.Add(new Installation(Context, shellPathValue, KnownDistribution.GitForWindows64v2));
                    output.Add(new Installation(Context, shellPathValue, KnownDistribution.GitForWindows32v2));
                    output.Add(new Installation(Context, shellPathValue, KnownDistribution.GitForWindows32v1));
                }
            }

            var candidates = new List<Installation>();

            ScanShellPath(candidates);
            ScanRegistry(candidates);
            ScanProgramFiles(candidates);
            ScanApplicationData(candidates);

            var pathSet = new HashSet<Installation>();
            foreach (var candidate in candidates)
            {
                if (candidate.IsValid())
                {
                    pathSet.Add(candidate);
                }
            }

            installations = pathSet.ToList();

            Trace.WriteLine($"found {installations.Count} Git installation(s).");

            return installations.Count > 0;
        }
    }
}
