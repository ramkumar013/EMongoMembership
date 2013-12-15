using Microsoft.Web.WebPages.OAuth;

namespace ExtendedMongoMembership.Sample
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "122703441237134",
                appSecret: "b92363d66dcda8db558828d3e7874189");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
