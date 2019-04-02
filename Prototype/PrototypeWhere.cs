using System.Collections.Generic;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Git;

namespace Prototype
{
    public class PrototypeWhere : Microsoft.Alm.Authentication.Git.Where
    {
        public PrototypeWhere(RuntimeContext context) : base(context)
        {
        }

        public override bool FindGitInstallations(out List<Installation> installations)
        {
            // TODO mminns
            installations = new List<Installation>();
            return false;
        }
    }
}