namespace Microsoft.Alm.Cli
{
    internal interface ILogger
    {
        void LogEvent(Program program, string message, string eventTypeName);
    }
}