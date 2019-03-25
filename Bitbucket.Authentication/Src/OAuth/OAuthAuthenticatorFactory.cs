using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication.OAuth
{
    public class OAuthAuthenticatorFactory
    {
        public static IOAuthAuthenticator GetAuthenticator(RuntimeContext context, string bbsConsumerKey, string bbsConsumerSecret)
        {
            if (string.IsNullOrWhiteSpace(bbsConsumerSecret) && string.IsNullOrWhiteSpace(bbsConsumerKey))
            {
                // bitbucket.org
                return new v2.OAuthAuthenticator(context);
            }

            return new v1.OAuthAuthenticator(context, bbsConsumerKey, bbsConsumerSecret);
        }
    }
}
