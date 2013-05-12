using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using PhotoApp.PhotoServices;

namespace PhotoApp.Tests
{
    [TestFixture]
    public class FlickrAPITests
    {
        FlickrAPI _flickrApi;

        [SetUp]
        public void SetUp()
        {
            _flickrApi = FlickrAPI.Instance; 
        }

        [Test]
        public void SingletonInstanceMethodShouldReturnNonNullObject()
        {
            Assert.NotNull(_flickrApi);
        }

        [Test]
        public void InstanceShouldHaveAPIandSecretKeySet()
        {
            Assert.IsNotNullOrEmpty(_flickrApi.ApiKey);
            Assert.IsNotNullOrEmpty(_flickrApi.ApiSecret);
        }
    }
}