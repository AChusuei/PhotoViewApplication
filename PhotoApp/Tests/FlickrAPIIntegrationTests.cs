using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using PhotoApp.PhotoServices;
using PhotoApp.OAuthServices;

namespace PhotoApp.Tests
{
    /// <summary>
    /// These tests are definitely more along the integration side,
    /// since the FlickrAPI requires connection to the actual Flickr service.
    /// </summary>
    [TestFixture]
    public class FlickrAPIIntegrationTests
    {
        FlickrAPI _flickrApi;

        [SetUp]
        public void SetUp()
        {
            _flickrApi = new FlickrAPI(); 
        }

        [Test]
        public void ShouldHaveAPIandSecretKeySet()
        {
            Assert.IsNotNullOrEmpty(_flickrApi.ApiKey);
            Assert.IsNotNullOrEmpty(_flickrApi.ApiSecret);
        }

        [Test]
        public void GetOAuthAuthenticationUrlShouldGiveARealURL()
        {
            var url = ((IOAuthService)_flickrApi).GetOAuthAuthenticationUrl(@"http://www.example.com");
            Assert.IsNotNullOrEmpty(url);
            Assert.True(url.StartsWith(@"http://www.flickr.com/services/oauth/authorize?oauth_token="));
        }
    }
}