using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoApp.PhotoServices;

namespace PhotoApp.Models
{
    public class PhotosModel
    {
        public IPhotoUser Owner { get; set; }
        public IEnumerable<IPhoto> Photos { get; set; }
        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<IEnumerable<IPhoto>> PaginatePhotosbySize(int size)
        {
            return SplitIntoBuckets<IPhoto>(Photos, size);
        }

        private static IEnumerable<IEnumerable<T>> SplitIntoBuckets<T>(IEnumerable<T> listToSplit, int maxBucketSize)
        {
            for (int i = 0; i < (int)Math.Ceiling((decimal)listToSplit.Count() / maxBucketSize); i++)
                yield return listToSplit.Skip(maxBucketSize * i).Take(maxBucketSize);
        }
    }

    
}