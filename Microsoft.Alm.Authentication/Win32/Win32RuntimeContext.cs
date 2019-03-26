using System;
using Microsoft.Alm.Authentication.Git;

namespace Microsoft.Alm.Authentication.Win32
{
    public class Win32RuntimeContext : RuntimeContext
    {
        public Win32RuntimeContext(Func<RuntimeContext, INetwork> getNetwork, Func<RuntimeContext, ISettings> getSettings, Func<RuntimeContext, IStorage> getStorage, Func<RuntimeContext, ITrace> getTrace, Func<RuntimeContext, IUtilities> getUtilities, Func<RuntimeContext, IWhere> getWhere) : base(getNetwork, getSettings, getStorage, getTrace, getUtilities, getWhere)
        {
        }

        public Win32RuntimeContext(INetwork network, ISettings settings, IStorage storage, ITrace trace, IUtilities utilities, IWhere @where) : base(network, settings, storage, trace, utilities, @where)
        {
        }

        /// <summary>
        /// The default `<see cref="RuntimeContext"/>` instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly RuntimeContext Default = Create();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static RuntimeContext Create()
        {
            var context = new RuntimeContext();

            context.SetService<INetwork>(new Network(context));
            context.SetService<ISettings>(new Settings(context));
            context.SetService<IStorage>(new Win32Storage(context));
            context.SetService<Git.ITrace>(new Git.Trace(context));
            context.SetService<Git.IUtilities>(new Win32Utilities(context));
            context.SetService<Git.IWhere>(new Win32Where(context));

            return context;
        }
    }
}