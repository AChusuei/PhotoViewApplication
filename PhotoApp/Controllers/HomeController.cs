using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using PhotoApp.PhotoServices;
using PhotoApp.Models;
using FlickrNet;

namespace PhotoApp.Controllers
{
    public class HomeController : Controller
    {
        private string BaseUrl
        {
            get
            {
                return Request.Url.GetLeftPart(UriPartial.Authority);
            }
        }

        private IOAuthPhotoService PhotoService
        {
            get 
            {
                return ObjectFactory.GetInstance<IOAuthPhotoService>();
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["RequestToken"] != null)
            {
                return Redirect(BaseUrl + String.Format(@"/Photos?oauth_token={0}", Session["RequestToken"]));
            }
            else
            {
                ViewBag.Message = "So you'd like to see your photos, right? You'll need to login first at the url below.";
                return View();
            }
        }

        [HttpGet]
        public ActionResult Photos(string oauth_token, string oauth_verifier, string q)
        {
            var token = oauth_token;
            if (String.IsNullOrWhiteSpace(token))
            {
                token = Session["RequestToken"] as string;
            }
            var user = PhotoService.GetUser(token);
            if (user == null)
            {
                user = PhotoService.CreateOAuthAccessTokens(oauth_token, oauth_verifier);
                Session["RequestToken"] = oauth_token;
            }
            var photos = PhotoService.GetPhotos(user, (q ?? String.Empty ));
            var alltags = PhotoService.GetAllTags(user);
            var photosmodel = new PhotosModel { Owner = user, Photos = photos, Tags = alltags };
            return View(photosmodel);
        }

        [HttpGet]
        public ActionResult LogOn()
        {
            return Redirect(PhotoService.GetOAuthAuthenticationUrl(BaseUrl + "/Photos"));
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            Session["RequestToken"] = null;
            return Redirect(BaseUrl);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
