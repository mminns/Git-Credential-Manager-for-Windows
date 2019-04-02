namespace Microsoft.Alm.Cli
{
    public enum ResultValue : int
    {
        UnknownFailure = -1,
        Success = 0,
        InvalidCustomPath,
        DeploymentFailed,
        NetFxNotFound,
        Unprivileged,
        GitConfigGlobalFailed,
        GitConfigSystemFailed,
        GitNotFound,
        RemovalFailed,
    }

    public interface IInstaller
    {
        void DeployConsole();
        ResultValue Result { get; }
        int ExitCode { get; set; }
        void RemoveConsole();
    }
}