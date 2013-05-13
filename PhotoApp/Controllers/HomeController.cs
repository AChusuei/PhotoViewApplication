using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using PhotoApp.PhotoServices;
using PhotoApp.OAuthServices;
using PhotoApp.Models;
using FlickrNet;

namespace PhotoApp.Controllers
{
    public class HomeController : Controller
    {
        public const string OAuthSessionKey = @"OAuthUser";

        private string BaseUrl
        {
            get
            {
                // this is not great, but for unit testing purposes need the null check.
                return (Request.Url == null ? @"http://localhost:8000" : Request.Url.GetLeftPart(UriPartial.Authority));
            }
        }

        private IPhotoService PhotoService
        {
            get 
            {
                return ObjectFactory.GetInstance<IPhotoService>();
            }
        }

        private IOAuthService OAuthService
        {
            get
            {
                return ObjectFactory.GetInstance<IOAuthService>();
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session[OAuthSessionKey] != null)
            {
                return Redirect(BaseUrl + @"/Photos");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Photos(string oauth_token, string oauth_verifier, string q)
        {
            if (Session[OAuthSessionKey] == null)
            {
                Session[OAuthSessionKey] = OAuthService.GetOAuthUser(oauth_token, oauth_verifier);
            }
            var user = Session[OAuthSessionKey] as IOAuthUser;
            var photos = PhotoService.GetPhotos(user, (q ?? String.Empty));
            var alltags = PhotoService.GetAllTags(user);
            var photosmodel = new PhotosModel { Owner = user, Photos = photos, Tags = alltags };
            return View(photosmodel);
        }

        [HttpGet]
        public ActionResult LogOn()
        {
            return Redirect(OAuthService.GetOAuthAuthenticationUrl(BaseUrl + "/Photos"));
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            Session[OAuthSessionKey] = null;
            return Redirect(BaseUrl);
        }
    }
}
