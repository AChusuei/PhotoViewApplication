using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlickrNet;
using PhotoApp.OAuthServices;

namespace PhotoApp.PhotoServices
{
    public interface IPhoto
    {
        string Id { get; set; }
        string Name { get; set; }
        string LargeUrl { get; set; }
        string ThumbNailUrl { get; set; }
        IOAuthUser Owner { get; set; }
        IEnumerable<string> Tags { get; set; }
    }

    public class Photo : IPhoto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LargeUrl { get; set; }
        public string ThumbNailUrl { get; set; }
        public IOAuthUser Owner { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }

    public interface IPhotoService
    {
        IOAuthUser GetUser(string userKey);
        IEnumerable<IPhoto> GetPhotos(IOAuthUser owner, string tags);
        IEnumerable<string> GetAllTags(IOAuthUser owner);
    }

    public class FlickrService : IPhotoService, IOAuthService
    {
        private static IDictionary<string, IOAuthUser> _users = new Dictionary<string, IOAuthUser>();

        string IOAuthService.GetOAuthAuthenticationUrl(string callbackUrl)
        {
            return FlickrAPI.Instance.GetOAuthAuthenticationUrl(callbackUrl);
        }

        IOAuthUser IOAuthService.GetOAuthUser(string requestToken, string verifier)
        {
            var accessToken = FlickrAPI.Instance.GetOAuthAccessToken(requestToken, verifier);
            _users[requestToken] = new OAuthUser { Id = accessToken.UserId, FullName = accessToken.FullName, UserName = accessToken.Username };
            return _users[requestToken];
        }

        IOAuthUser IPhotoService.GetUser(string requestToken)
        {
            return (_users.ContainsKey(requestToken) ? _users[requestToken] : null);
        }

        IEnumerable<IPhoto> IPhotoService.GetPhotos(IOAuthUser owner, string tag)
        {
            var photos = FlickrAPI.Instance.PeopleGetPhotos(owner.Id, PhotoSearchExtras.Tags).ToList();
            if (!String.IsNullOrWhiteSpace(tag))
            {
                photos = photos.Where(p => p.Tags.Any(t => t == tag)).ToList();
            }
            
            return photos.Select(p => new Photo { Id = p.PhotoId, Name = p.Title, ThumbNailUrl = p.ThumbnailUrl, LargeUrl = p.LargeUrl, Owner = owner, Tags = p.Tags });
        }

        IEnumerable<string> IPhotoService.GetAllTags(IOAuthUser owner)
        {
            return FlickrAPI.Instance.PeopleGetPhotos(owner.Id, PhotoSearchExtras.Tags).SelectMany(p => p.Tags).Where(t => !String.IsNullOrWhiteSpace(t)).Distinct();
        }
    }

    public class FlickrAPI : Flickr
    {
        private const string _apiKey = "ede024e9afd0c16b296568a532687dfa";
        private const string _secretKey = "a5b0ec729fafef41";

        private static volatile FlickrAPI _instance;
        private static Object lockobject = new Object();
        private static IDictionary<string, OAuthRequestToken> _requestTokens = new Dictionary<string, OAuthRequestToken>();

        private FlickrAPI() : base(_apiKey, _secretKey) { }

        public static FlickrAPI Instance
        {
            get 
            {
                if (_instance == null)
                {
                    lock (lockobject)
                    {
                        if (_instance == null)
                        { 
                            _instance = new FlickrAPI();
                        }
                    }
                }
                return _instance;
            }
        }

        public string GetOAuthAuthenticationUrl(string callbackUrl)
        {
            // If we are getting a new OAuthRequestToken, the old access tokens need to be removed
            OAuthAccessToken = null;
            OAuthAccessTokenSecret = null;
            var requestToken = OAuthGetRequestToken(callbackUrl);
            _requestTokens[requestToken.Token] = requestToken;
            return OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Read);
        }

        public OAuthAccessToken GetOAuthAccessToken(string requestToken, string verifier)
        {
            return OAuthGetAccessToken(_requestTokens[requestToken], verifier);
        }
    }
}
