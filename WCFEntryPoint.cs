using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HookappServer
{
    [ServiceContract]
    public interface IWCFEntryPoint
    {
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "json/registeruser")]
        string RegisterUser(RegisterUserObject user);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "uploadphoto/{filename}")]
        void UploadPhoto(string filename, Stream fileContent);

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "getimage/{filename}/{width}/{height}")]
        Stream GetImage(string filename, string width, string height);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "json/confirmcode")]
        string ConfirmCode(ConfirmCodePayload confirmCodePayload);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "json/confirmuser")]
        string ConfirmUser(ConfirmUserPayload confirmUserPayload);

    }

    public class WCFEntryPoint : IWCFEntryPoint
    {
        public Stream GetImage(string filename, string width, string height)
        {
            Console.WriteLine("Fetching an image at UserPics/" + filename + ".jpeg");
            Image imgThumb = null;
            Image img = Image.FromFile("UserPics/" + filename + ".jpeg");
            imgThumb = CreateThumbnail(img, int.Parse(width), int.Parse(height));
            var stream = ToStream(imgThumb, ImageFormat.Jpeg);
            WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
            return stream;
        }

        private Image CreateThumbnail(Image image, int thumbWidth, int thumbHeight)
        {
            try
            {
                return image.GetThumbnailImage(
                thumbWidth,
                thumbHeight,
                delegate () {
                    return false;
                },
                IntPtr.Zero);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Stream ToStream(Image image, ImageFormat formaw)
        {
            try
            {
                var stream = new System.IO.MemoryStream();
                image.Save(stream, formaw);
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string RegisterUser(RegisterUserObject user)
        {
            Console.WriteLine("Registering user : " + user.Email);

            if (!WCFServer.Database.CanRegisterUser(user.JMBG))
                return "Ne mozete registrovati 2 korisnika sa istim JMBG";

            long userID = WCFServer.Database.RegisterUser(user);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            int randomCode = rand.Next(100000, 999999);
            SmsHandler.Instance.SendRegistryConfirmation(randomCode.ToString(), user.Mobile, userID);
            return "true. ID ["+userID+"]";
        }

        public void UploadPhoto(string filename, Stream fileContent)
        {
            byte[] buffer = new byte[100000];
            MemoryStream ms = new MemoryStream();
            int bytesRead, totalBytesRead = 0;
            do
            {
                bytesRead = fileContent.Read(buffer, 0, buffer.Length);
                totalBytesRead += bytesRead;

                ms.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);

            ms.Close();

            Console.WriteLine("Uploading image " + filename);
            PhotoManager.Instance.SaveUserImage(buffer, filename);
        }

        public string ConfirmCode(ConfirmCodePayload confirmCodePayload)
        {
            Console.WriteLine("Sent confirmation code " + confirmCodePayload.Code + ". Checking unconfirmed pool");
            return JsonConvert.SerializeObject(WCFServer.Database.GetConfirmedUser(confirmCodePayload.Code));
        }

        public string ConfirmUser(ConfirmUserPayload confirmUserPayload)
        {
            Console.WriteLine("Confirming user with google id : " + confirmUserPayload.GoogleToken);
            if (WCFServer.Database.ConfirmUser(confirmUserPayload))
                return "true";
            else
                return "false";
        }
    }

    public class ConfirmCodeUserObject
    {
        public string Name;
        public long UserID;
        public string Email;
    }

    public class ConfirmCodePayload
    {
        public string Code;
    }

    public class ConfirmUserPayload
    {
        public long UserID;
        public string GoogleToken;
    }

    public class RegisterUserObject
    {
        public string Name;
        public string Surname;
        public string Email;
        public string Address;
        public string Mobile;
        public string JMBG;
        public byte Gender;
        public string CountryCode;
        public string PhoneExtension;
    }
}
