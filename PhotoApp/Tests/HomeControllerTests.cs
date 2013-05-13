using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using PhotoApp.PhotoServices;
using PhotoApp.OAuthServices;
using PhotoApp.Controllers;
using StructureMap;
using Rhino.Mocks;
using RMM = Rhino.Mocks.MockRepository;
using MvcContrib.TestHelper;
using PhotoApp.Models;

namespace PhotoApp.Tests
{
    public class HomeControllerTests
    {
        private const string OAuthURL = @"http://www.flickr.com/services/oauth/authorize?oauth_token=72157626737672178-022bbd2f4c2f3432";

        TestControllerBuilder _builder;
        HomeController _controller;
        IPhotoService _photoService;
        IOAuthService _oAuthService;
        IOAuthUser _oAuthUserOne;
        IEnumerable<IPhoto> _photosUserOne;
        IPhoto _photoOne;
        IPhoto _photoTwo;
        IPhoto _photoThree;
        IList<string> _userOneTags;

        [SetUp]
        public void SetUp()
        {
            _oAuthUserOne = RMM.GenerateStub<IOAuthUser>();
            _oAuthUserOne.Id = "Id1";
            _oAuthUserOne.UserName = "User1";
            _oAuthUserOne.FullName = "User One Of America";
            _oAuthUserOne.AccessToken = "72157626737672178-oneoneoneoneone1";
            _oAuthUserOne.AccessTokenSecret = "111111111111111";

            _oAuthService = RMM.GenerateStub<IOAuthService>();
            _oAuthService.Stub(s => s.GetOAuthAuthenticationUrl(null)).IgnoreArguments().Return(OAuthURL);
            _oAuthService.Stub(s => s.GetOAuthUser(@"72157626737672178-oneoneoneoneone1", @"5d1b96a26b494074")).Return(_oAuthUserOne);

            _userOneTags = new List<string> { "children", "school", "parents" };
            _photoOne = RMM.GenerateStub<IPhoto>();
            _photoOne.Id = @"PhotoIdOne";
            _photoOne.Name = @"PhotoIdOne";
            _photoOne.LargeUrl = @"http://www.flickr.com/large/one.jpg";
            _photoOne.ThumbNailUrl = @"http://www.flickr.com/tn/one.jpg";
            _photoOne.Owner = _oAuthUserOne;
            _photoOne.Tags = new List<string> { "children", "school" };
            _photoTwo = RMM.GenerateStub<IPhoto>();
            _photoTwo.Id = @"PhotoIdTwo";
            _photoTwo.Name = @"PhotoIdTwo";
            _photoTwo.LargeUrl = @"http://www.flickr.com/large/two.jpg";
            _photoTwo.ThumbNailUrl = @"http://www.flickr.com/tn/two.jpg";
            _photoTwo.Owner = _oAuthUserOne;
            _photoTwo.Tags = new List<string> { "parents" };
            _photoThree = RMM.GenerateStub<IPhoto>();
            _photoThree.Id = @"PhotoIdThree";
            _photoThree.Name = @"PhotoIdThree";
            _photoThree.LargeUrl = @"http://www.flickr.com/large/three.jpg";
            _photoThree.ThumbNailUrl = @"http://www.flickr.com/tn/three.jpg";
            _photoThree.Owner = _oAuthUserOne;
            _photoThree.Tags = new List<string> { "parents", "school" };

            _photosUserOne = new List<IPhoto> { _photoOne, _photoTwo, _photoThree };

            _photoService = RMM.GenerateMock<IPhotoService>();
            _photoService.Stub(s => s.GetPhotos(_oAuthUserOne, String.Empty)).Return(_photosUserOne);
            _photoService.Stub(s => s.GetPhotos(_oAuthUserOne, "school")).Return(_photosUserOne.Where(p => p.Tags.Contains("school")));
            _photoService.Stub(s => s.GetAllTags(_oAuthUserOne)).Return(_userOneTags);

            _builder = new TestControllerBuilder();
            _controller = new HomeController();
            _builder.InitializeController(_controller);

            ObjectFactory.Inject<IOAuthService>(_oAuthService);
            ObjectFactory.Inject<IPhotoService>(_photoService);
        }

        [Test]
        public void IndexShouldReturnViewWhenSessionVariableisNull()
        {
            var result = _controller.Index();
            Assert.IsAssignableFrom(typeof(ViewResult), result);
        }

        [Test]
        public void IndexShouldRedirectWhenSessionVariableisSet()
        {
            _controller.Session[HomeController.OAuthSessionKey] = _oAuthUserOne;
            var result = _controller.Index();
            Assert.IsAssignableFrom(typeof(RedirectResult), result);
        }

        [Test]
        public void PhotosShouldGetAllPhotosWhenSessionVariableisNull()
        {
            var result = _controller.Photos(@"72157626737672178-oneoneoneoneone1", @"5d1b96a26b494074", null);
            Assert.IsAssignableFrom(typeof(ViewResult), result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom(typeof(PhotosModel), viewResult.Model);
            var model = viewResult.Model as PhotosModel;
            Assert.AreEqual(_oAuthUserOne, model.Owner);
            Assert.AreEqual(_photosUserOne, model.Photos);
            Assert.AreEqual(3, model.Photos.Count());
            Assert.AreEqual(_userOneTags, model.Tags);
        }

        [Test]
        public void PhotosShouldGetAllPhotosWhenSessionVariableHasOAuthUser()
        {
            _controller.Session[HomeController.OAuthSessionKey] = _oAuthUserOne;
            var result = _controller.Photos(@"garbage-token", @"check", null);
            Assert.IsAssignableFrom(typeof(ViewResult), result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom(typeof(PhotosModel), viewResult.Model);
            var model = viewResult.Model as PhotosModel;
            Assert.AreEqual(_oAuthUserOne, model.Owner);
            Assert.AreEqual(_photosUserOne, model.Photos);
            Assert.AreEqual(3, model.Photos.Count());
            Assert.AreEqual(_userOneTags, model.Tags);
        }

        [Test]
        public void PhotosShouldGetOnlyPhotosMatchingTagParameter()
        {
            var result = _controller.Photos(@"72157626737672178-oneoneoneoneone1", @"5d1b96a26b494074", "school");
            Assert.IsAssignableFrom(typeof(ViewResult), result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom(typeof(PhotosModel), viewResult.Model);
            var model = viewResult.Model as PhotosModel;
            Assert.AreEqual(2, model.Photos.Count(p => p.Tags.Contains("school")));
        }

        [Test]
        public void LogOnShouldRedirectToOAuthUrl()
        {
            var result = _controller.LogOn();
            Assert.IsAssignableFrom(typeof(RedirectResult), result);
            var redirectResult = result as RedirectResult;
            Assert.AreEqual(OAuthURL, redirectResult.Url);
        }

        [Test]
        public void LogOutShouldRedirectAndSetSessiontoNull()
        {
            _controller.Session[HomeController.OAuthSessionKey] = _oAuthUserOne;
            var result = _controller.LogOut();
            Assert.IsAssignableFrom(typeof(RedirectResult), result);
            var redirectResult = result as RedirectResult;
            Assert.AreEqual(@"http://localhost:8000", redirectResult.Url);
            Assert.Null(_controller.Session[HomeController.OAuthSessionKey]);
        }

        [TearDown]
        public void TearDown()
        {
            // Kill all instances in the objectfactory
            ObjectFactory.EjectAllInstancesOf<IOAuthService>();
            ObjectFactory.EjectAllInstancesOf<IPhotoService>();
        }
    }
}