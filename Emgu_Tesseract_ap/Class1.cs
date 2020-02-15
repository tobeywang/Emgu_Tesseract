using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Vision.v1;

namespace Emgu_Tesseract_ap
{
    internal class Class1
    {
        public string ApplicationName { get { return "Emgu_Tesseract_ap"; } }

        private string JsonKeypath
        {
            //@"D:\Cathay_T\My\MyProject\Emgu_Tesseract\Emgu_Tesseract\Emgu_Tesseract_ap\key.json"
            get
            {
                return
              Directory.GetParent(Directory.GetParent(System.IO.Path.GetDirectoryName(Application.ExecutablePath)).FullName).FullName +
              @"\key.json";
            }
        }

        //取得憑証
        public GoogleCredential CreateCredential()
        {
            using (var stream = new FileStream(JsonKeypath, FileMode.Open, FileAccess.Read))
            {
                string[] scopes = { VisionService.Scope.CloudPlatform };
                var credential = GoogleCredential.FromStream(stream);
                credential = credential.CreateScoped(scopes);
                return credential;
            }
        }

        public VisionService CreateService(GoogleCredential credential)
        {
            var service = new VisionService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
                GZipEnabled = true,
            });
            return service;
        }
    }
}