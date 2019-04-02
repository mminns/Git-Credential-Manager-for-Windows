using System;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Git;

namespace Prototype
{
    public class GenericRuntimeContext : RuntimeContext
    {
        public GenericRuntimeContext(Func<RuntimeContext, INetwork> getNetwork, Func<RuntimeContext, ISettings> getSettings, Func<RuntimeContext, IStorage> getStorage, Func<RuntimeContext, ITrace> getTrace, Func<RuntimeContext, IUtilities> getUtilities, Func<RuntimeContext, IWhere> getWhere) : base(getNetwork, getSettings, getStorage, getTrace, getUtilities, getWhere)
        {
        }

        public GenericRuntimeContext(INetwork network, ISettings settings, IStorage storage, ITrace trace, IUtilities utilities, IWhere @where) : base(network, settings, storage, trace, utilities, @where)
        {
        }
    }
}
