using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HookappServer
{
    public class PhotoManager
    {
        private static PhotoManager _instance;
        public static PhotoManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PhotoManager();

                return _instance;
            }
        }

        public PhotoManager()
        {
            if (!Directory.Exists("UserPics"))
                Directory.CreateDirectory("UserPics");
        }

        public void SaveUserImage(byte[] buffer, string imageID)
        {
            MemoryStream ms = new MemoryStream(buffer);
            FileStream fStream = new FileStream("UserPics/" + imageID + ".jpeg", FileMode.Create);
            ms.WriteTo(fStream);
            ms.Close();
            fStream.Close();
            fStream.Dispose();
        }

        public Stream GetImage(string imageName)
        {
            if (File.Exists("UserPics/" + imageName + ".jpeg"))
            {
                Bitmap fromFile = Bitmap.FromFile("UserPics/" + imageName + ".jpeg") as Bitmap;
                var stream = new MemoryStream();

                fromFile.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);

                return stream;
            }
            else
                return null;
        }
    }
}
