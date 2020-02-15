using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Emgu.CV;

namespace Emgu_Tesseract
{
    public partial class Index : System.Web.UI.Page
    {
        protected void Button1_Click(object sender, EventArgs e)
        {
            //開啟檔案
            Console.WriteLine("start \n");
            string name = FileUpload1.FileName;
            var mat = CvInvoke.Imread(FileUpload1.PostedFile.FileName, Emgu.CV.CvEnum.ImreadModes.Unchanged);
            if (mat.IsEmpty)
            {
                Console.WriteLine("can not load the image \n");
            }
            //原圖
            CvInvoke.Imshow(name, mat);
            Console.WriteLine("get image \n");
            Mat grayImg = new Mat();
            CvInvoke.CvtColor(mat, grayImg, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
            CvInvoke.Imshow("Gray Image", grayImg);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        //private OpenCVResult CaptureFace(Mat objMat)
        //{
        //    long detectionTime;
        //    List<Rectangle> faces = new List<Rectangle>();
        //    List<Rectangle> eyes = new List<Rectangle>();

        //    DetectFace.Detect(
        //        objMat, "haarcascade_frontalface_default.xml", "haarcascade_eye.xml",
        //        faces, eyes,
        //        out detectionTime);

        //    // 重新計算比例
        //    decimal diWidth = decimal.Parse(picRender.Width.ToString()) / decimal.Parse(objMat.Bitmap.Width.ToString());
        //    decimal diHeight = decimal.Parse(picRender.Height.ToString()) / decimal.Parse(objMat.Bitmap.Height.ToString());

        //    List<Rectangle> objDraw = new List<Rectangle>();

        //    for (int i = 0; i < faces.Count; i++)
        //    {
        //        objDraw.Add(new Rectangle(
        //            (int)(faces[i].X * diWidth),
        //            (int)(faces[i].Y * diHeight),
        //            (int)(faces[i].Width * diWidth),
        //            (int)(faces[i].Height * diHeight)
        //            ));
        //    }

        //    OpenCVResult result = new OpenCVResult()
        //    {
        //        eyes = eyes,
        //        faces = faces,
        //    };

        //    return result;
        //}
    }

    public class OpenCVResult
    {
        public List<Rectangle> eyes { get; set; }
        public List<Rectangle> faces { get; set; }
    }
}