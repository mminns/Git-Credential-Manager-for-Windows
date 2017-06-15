using Microsoft.Alm.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Bitbucket.Authentication.Mercurial
{
    public static class MercurialHelper
    {
        public const string EnvironmentVariableKey = "GCM_MCM";

        public static bool IsMercurial
        {
            get
            {
                if (Environment.GetCommandLineArgs().Contains("--mercurial")
                    || Environment.GetCommandLineArgs().Contains("-mercurial")
                    || Environment.GetCommandLineArgs().Contains("--hg")
                    || Environment.GetCommandLineArgs().Contains("-hg")
                    || Environment.GetCommandLineArgs().Contains("-m")
                    || "true".Equals(Environment.GetEnvironmentVariable(EnvironmentVariableKey), StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                return false;
            }
        }

        public static string UriToTargetName(TargetUri targetUri, string @namespace)
        {
            BaseSecureStore.ValidateTargetUri(targetUri);

            if (IsMercurial)
            {
                // ignore the namespace
                var username = targetUri.ActualUri.UserInfo;

                string baseUrl = targetUri.ToString();
                string targetName = null;
                if (string.IsNullOrWhiteSpace(username))
                {
                    targetName = $"@{baseUrl}";
                }
                else
                {
                    targetName = $"{username}@@{baseUrl}";
                }

                targetName = targetName.TrimEnd('/', '\\');

                return $"{targetName}@Mercurial";
            }
            else
            {
                if (String.IsNullOrWhiteSpace(@namespace))
                    throw new ArgumentNullException(@namespace);

                string targetName = $"{@namespace}:{targetUri.ActualUri.AbsoluteUri}";
                targetName = targetName.TrimEnd('/', '\\');

                return targetName;
            }
        }
    }
}
