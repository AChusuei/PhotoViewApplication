using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlickrNet;

namespace PhotoApp.OAuthServices
{
    public interface IOAuthUser
    {
        string Id { get; set; }
        string FullName { get; set; }
        string UserName { get; set; }
        string AccessToken { get; set; }
        string AccessTokenSecret { get; set; }
    }

    public class OAuthUser : IOAuthUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }

    public interface IOAuthService
    {
        string GetOAuthAuthenticationUrl(string callbackUrl);
        IOAuthUser GetOAuthUser(string requestToken, string verifier);
    }
}
