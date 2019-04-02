using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Git;

namespace Prototype
{
    public class PrototypeUtilities : Utilities
    {
        public PrototypeUtilities(RuntimeContext context) : base(context)
        {
        }

        public override bool TryReadGitRemoteHttpDetails(out string commandLine, out string imagePath)
        {
            // TODO MMINNS
            commandLine = null;
            imagePath = null;
            return false;
        }
    }
}