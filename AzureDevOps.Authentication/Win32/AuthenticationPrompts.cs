using Microsoft.Alm.Authentication;

namespace AzureDevOps.Authentication
{
    public class AuthenticationPrompts : Base
    {
        public AuthenticationPrompts(RuntimeContext context) : base(context)
        {
            var adal = GetService<IAdal>();

            if (adal is null)
            {
                // Since there's no pre-existing Gui service registered with the current
                // context, we'll need to allocate and add one to it.
                adal = new Adal(Context);

                SetService(adal);
            }
        }
    }
}