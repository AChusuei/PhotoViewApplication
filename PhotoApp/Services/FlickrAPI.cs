using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlickrNet;
using PhotoApp.OAuthServices;

namespace PhotoApp.PhotoServices
{
    public class FlickrAPI : Flickr, IOAuthService, IPhotoService
    {
        private const string _apiKey = "ede024e9afd0c16b296568a532687dfa";
        private const string _secretKey = "a5b0ec729fafef41";

        private static IDictionary<string, OAuthRequestToken> _requestTokens = new Dictionary<string, OAuthRequestToken>();
        private static IDictionary<string, IOAuthUser> _users = new Dictionary<string, IOAuthUser>();

        public FlickrAPI() : base(_apiKey, _secretKey) { }

        string IOAuthService.GetOAuthAuthenticationUrl(string callbackUrl)
        {
            // If we are getting a new OAuthRequestToken, the old access tokens need to be removed
            OAuthAccessToken = null;
            OAuthAccessTokenSecret = null;
            var requestToken = OAuthGetRequestToken(callbackUrl);
            _requestTokens[requestToken.Token] = requestToken;
            return OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Read);
        }

        IOAuthUser IOAuthService.GetOAuthUser(string requestToken, string verifier)
        {
            var user = (_users.ContainsKey(requestToken) ? _users[requestToken] : null);
            if (user != null) return user;
            var accessToken = OAuthGetAccessToken(_requestTokens[requestToken], verifier);
            _users[requestToken] = new OAuthUser { Id = accessToken.UserId, FullName = accessToken.FullName, UserName = accessToken.Username };
            return _users[requestToken];
        }

        IEnumerable<IPhoto> IPhotoService.GetPhotos(IOAuthUser owner, string tag)
        {
            var photos = PeopleGetPhotos(owner.Id, PhotoSearchExtras.Tags).ToList();
            if (!String.IsNullOrWhiteSpace(tag))
            {
                photos = photos.Where(p => p.Tags.Any(t => t == tag)).ToList();
            }

            return photos.Select(p => new Photo { Id = p.PhotoId, Name = p.Title, ThumbNailUrl = p.ThumbnailUrl, SmallUrl = p.SmallUrl, LargeUrl = p.LargeUrl, Owner = owner, Tags = p.Tags });
        }

        IEnumerable<string> IPhotoService.GetAllTags(IOAuthUser owner)
        {
            return PeopleGetPhotos(owner.Id, PhotoSearchExtras.Tags).SelectMany(p => p.Tags).Where(t => !String.IsNullOrWhiteSpace(t)).Distinct();
        }
    }
}