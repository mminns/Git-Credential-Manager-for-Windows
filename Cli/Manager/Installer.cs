using System;

namespace Microsoft.Alm.Cli
{
    public class Installer : IInstaller
    {
        private Program _program;

        public Installer(Program program)
        {
            _program = program;
        }

        public ResultValue Result => throw new NotImplementedException();

        public int ExitCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DeployConsole()
        {
            throw new NotImplementedException();
        }

        public void RemoveConsole()
        {
            throw new NotImplementedException();
        }
    }
}