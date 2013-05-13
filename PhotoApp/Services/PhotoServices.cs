using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        IEnumerable<IPhoto> GetPhotos(IOAuthUser owner, string tags);
        IEnumerable<string> GetAllTags(IOAuthUser owner);
    }
}
