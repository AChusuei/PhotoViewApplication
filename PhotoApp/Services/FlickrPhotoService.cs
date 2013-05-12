using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlickrNet;

namespace PhotoApp.PhotoServices
{
    public interface IPhoto
    {
        string Id { get; set; }
        string Name { get; set; }
        string LargeUrl { get; set; }
        string ThumbNailUrl { get; set; }
        IPhotoUser Owner { get; set; }
        IEnumerable<string> Tags { get; set; }
    }

    public class Photo : IPhoto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LargeUrl { get; set; }
        public string ThumbNailUrl { get; set; }
        public IPhotoUser Owner { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }

    public interface IPhotoUser
    {
        string Id { get; set; }
        string FullName { get; set; }
        string UserName { get; set; }
        string AccessToken { get; set; }
        string AccessTokenSecret { get; set; }
    }

    public class PhotoUser : IPhotoUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }

    public interface IOAuthPhotoService
    {
        string GetOAuthAuthenticationUrl(string callbackUrl);
        IPhotoUser CreateOAuthAccessTokens(string requestToken, string verifier);
        bool IsOAuthAuthenticated { get; }
        IPhotoUser GetUser(string userKey);
        IEnumerable<IPhoto> GetPhotos(IPhotoUser owner, string tags);
        IEnumerable<string> GetAllTags(IPhotoUser owner);
    }

    public class FlickrPhotoService : IOAuthPhotoService
    {
        private static IDictionary<string, IPhotoUser> _users = new Dictionary<string, IPhotoUser>();

        string IOAuthPhotoService.GetOAuthAuthenticationUrl(string callbackUrl)
        {
            return FlickrAPI.Instance.GetOAuthAuthenticationUrl(callbackUrl);
        }

        IPhotoUser IOAuthPhotoService.CreateOAuthAccessTokens(string requestToken, string verifier)
        {
            var accessToken = FlickrAPI.Instance.SetOAuthAccessToken(requestToken, verifier);
            _users[requestToken] = new PhotoUser { Id = accessToken.UserId, FullName = accessToken.FullName, UserName = accessToken.Username };
            return _users[requestToken];
        }

        bool IOAuthPhotoService.IsOAuthAuthenticated
        {
            get
            {
                return FlickrAPI.Instance.IsOAuthAuthenticated;
            }
        }

        IPhotoUser IOAuthPhotoService.GetUser(string requestToken)
        {
            return (_users.ContainsKey(requestToken) ? _users[requestToken] : null);
        }

        IEnumerable<IPhoto> IOAuthPhotoService.GetPhotos(IPhotoUser owner, string tag)
        {
            var photos = FlickrAPI.Instance.PeopleGetPhotos(owner.Id, PhotoSearchExtras.Tags).ToList();
            if (!String.IsNullOrWhiteSpace(tag))
            {
                photos = photos.Where(p => p.Tags.Any(t => t == tag)).ToList();
            }
            
            return photos.Select(p => new Photo { Id = p.PhotoId, Name = p.Title, ThumbNailUrl = p.ThumbnailUrl, LargeUrl = p.LargeUrl, Owner = owner, Tags = p.Tags });
        }

        IEnumerable<string> IOAuthPhotoService.GetAllTags(IPhotoUser owner)
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
            OAuthAccessToken = null;
            OAuthAccessTokenSecret = null;
            var requestToken = OAuthGetRequestToken(callbackUrl);
            _requestTokens[requestToken.Token] = requestToken;
            return OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Read);
        }

        public OAuthAccessToken SetOAuthAccessToken(string requestToken, string verifier)
        {
            var accessToken = OAuthGetAccessToken(_requestTokens[requestToken], verifier);
            return accessToken;
        }

        public bool IsOAuthAuthenticated
        {
            get
            {
                return (OAuthAccessToken != null && OAuthAccessTokenSecret != null);
            }
        }
    }
}
