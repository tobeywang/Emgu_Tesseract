using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Google.Apis.Vision.v1.Data;
using Newtonsoft.Json;
using Tesseract;
using System.Drawing.Drawing2D;

namespace Emgu_Tesseract_ap
{
    public partial class Form1 : Form
    {
        //System.IO.Path.GetDirectoryName(Application.ExecutablePath)--D:\Cathay_T\My\MyProject\Emgu_Tesseract\Emgu_Tesseract\Emgu_Tesseract_ap\bin\Debug
        private string ap_path = Directory.GetParent(Directory.GetParent(System.IO.Path.GetDirectoryName(Application.ExecutablePath)).FullName).FullName;

        private string image_filename = "";

        public Form1()
        {
            InitializeComponent();
        }

        //Emgu + Contour Match
        private void button1_Click(object sender, EventArgs e)
        {
            DateTime dt_start = DateTime.Now;
            string Target_Img = txt_file.Text;
            if (string.IsNullOrEmpty(Target_Img))
                Target_Img = @"C:\Users\tobeywang\Desktop\Idscan\image2.jpg";
            var img = CvInvoke.Imread(Target_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);
            if (img.IsEmpty)
            {
                Console.WriteLine("can not load the image \n");
                return;
            }
            image_filename = "image2";
            ////1.原圖
            ////CvInvoke.Imshow("Image", img);
            //Mat grayImg = new Mat();
            //CvInvoke.CvtColor(img, grayImg, ColorConversion.Rgb2Gray);
            //CvInvoke.Imshow("CvtColor gray", grayImg);
            ////2.show to picturebox
            //Image<Gray, Byte> inputImage = new Image<Gray, Byte>(@"C:\Users\tobeywang\Desktop\Idscan\image2.jpg");
            pictureBox1.Image = img.Bitmap;
            //Mat gausImg = new Mat();
            //CvInvoke.GaussianBlur(grayImg, gausImg, new Size(3, 3), 0);

            #region 尋找證件的邊緣

            //實驗結果：如要找身份證的邊框可能比較困難，因拍攝的角度不一定，照成的灰階化結果會影響邊緣針測的參數
            ////3.使用canny算子查找邊緣 -------------------------------
            //高斯平滑>Pixel梯度計算>閥值判斷是否為邊緣(梯度高於高閥值為邊界、低於低閥值非邊界、其他則是以周圍是否被判斷為邊界)
            //Mat cannyImg = new Mat();
            ////兩個門檻參數threshold1與threshold2
            ////圖形的任一點像素，若其值大於threshold2，則認定它屬於邊緣像素，若小於threshold1則不為邊緣像素
            //CvInvoke.Canny(gausImg, cannyImg, 50, 30);
            //if (cannyImg.IsEmpty)
            //    Console.WriteLine("can not cover the image \n");
            //else
            //    CvInvoke.Imshow("Canny Image", cannyImg);

            //VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            //try
            //{
            //    CvInvoke.FindContours(cannyImg, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            //    Console.WriteLine("canny contours count:"+contours.Size);
            //    List<result> all = new List<result>();
            //    RotatedRect maxRect = new RotatedRect();
            //    var big_area = new VectorOfPoint();
            //    double maxArea=0.0;
            //    for (int i = 0; i < contours.Size; i++)
            //    {
            //        result r = new result();
            //        using (VectorOfPoint contour = contours[i])
            //        {
            //            //最大區域
            //            RotatedRect min_ar = CvInvoke.MinAreaRect(contour);
            //            var area=CvInvoke.ContourArea(contour);
            //            if (area>maxArea)
            //            {
            //                maxArea = area;
            //                maxRect = min_ar;
            //                big_area = contour;
            //            }
            //        }
            //    }
            //    Console.WriteLine("canny contours big :" + maxRect.Size);
            //    var BoundingBox = CvInvoke.BoundingRectangle(big_area);
            //    CvInvoke.Rectangle(img, BoundingBox, new MCvScalar(255, 0, 255, 255), 2);
            //    CvInvoke.Imshow("Canny_list Image", img);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("error: \n" + ex.Message);
            //}

            ////---------------------------------

            #endregion 尋找證件的邊緣

            //Match
            //var model_bigimg = CvInvoke.Imread(@"C:\Users\tobeywang\Desktop\Idscan\data\idImg_13.png", Emgu.CV.CvEnum.ImreadModes.Unchanged);
            string Model_Img = txt_model_file.Text;
            if (string.IsNullOrEmpty(Model_Img))
                Model_Img = @"D:\Cathay_T\My\MyProject\Emgu_Tesseract\Emgu_Tesseract\Emgu_Tesseract_ap\Info_Pic\_contour_binnary_re8.png";
            var model_bigimg = CvInvoke.Imread(Model_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);

            Mat changeImg = new Mat();
            List<pass_result> ori_matlist = Get_contour_binnary(img, out changeImg);
            if (ori_matlist.Count > 0)
            {
                Console.WriteLine("like word contour count: \n" + ori_matlist.Count);
                foreach (var item in ori_matlist)
                {
                    long matchTime;
                    long score;
                    int matches_size;
                    var result_mat = Draw(model_bigimg, item.mat, out matchTime, out score, out matches_size);
                    TimeSpan toNow = new TimeSpan(matchTime);
                    //DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                    //DateTime dtResult = dtStart.Add(toNow);
                    //Console.WriteLine(" matchSec:" + toNow.TotalSeconds + " score:" + (score / 10000.0).ToString("P2"));((int)score / (matches_size * 2)).ToString("P2")
                    Console.WriteLine("Match Image info_path:" + item.path);

                    Console.WriteLine(" matchSec:" + toNow.TotalMilliseconds.ToString("G17") + " score:" + score.ToString());
                    Console.WriteLine(" match size:" + matches_size.ToString());
                    Console.WriteLine(" match %:" + ((int)score / (matches_size * 2)).ToString("P2"));
                    //Show 框出截到的部分
                    //CvInvoke.Rectangle(changeImg, BoundingBox, new MCvScalar(255, 0, 255, 255), 3);
                    CvInvoke.Imshow("match Image " + item.path.Split('/').Last(), result_mat);
                }
            }
            else
                Console.WriteLine("contours are not found");
            var dt_durision = dt_start.Subtract(DateTime.Now).Duration();
            Console.WriteLine("執行時間：" + dt_durision.TotalSeconds.ToString() + " 秒");
            CvInvoke.Imshow("final Image", changeImg);
        }

        //Emgu
        private void button2_Click(object sender, EventArgs e)
        {
            //1.讀入圖片
            string Target_Img = txt_file.Text;
            if (string.IsNullOrEmpty(Target_Img))
                Target_Img = @"C:\Users\tobeywang\Desktop\Idscan\image2.jpg";
            var img = CvInvoke.Imread(Target_Img, Emgu.CV.CvEnum.ImreadModes.Color);


            pictureBox1.Image = img.Bitmap;
            //2.改變尺吋:interpolation內插方式
            Mat changeImg = new Mat();
            Size newsize = new System.Drawing.Size(428, 270);
            CvInvoke.Resize(img, changeImg, newsize, 0, 0, Inter.Cubic);
            CvInvoke.Imshow("Resize Image", changeImg);
            //3.找輪廓
            Mat denoImg = new Mat();
            int[,] array_1 = new int[,] { { 0, 1, 1 } };
            //去噪
            CvInvoke.FastNlMeansDenoisingColored(changeImg, denoImg, 10, 3, 3, 3);
            Mat garyImg = new Mat();
            Point p_struc = new Point(-1, -1);
            //Mat temp = new Mat(3, 3, DepthType.Cv8U, 0);
            Mat temp = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), p_struc);
            CvInvoke.Transform(denoImg, garyImg, temp);
            //Gray
            CvInvoke.Imshow("Gary Image", garyImg);
            //二值化
            Mat binnaryImg = new Mat();
            //閾值 180  maxval:255
            CvInvoke.Threshold(garyImg, binnaryImg, 180, 255, ThresholdType.BinaryInv);
            CvInvoke.Imshow("Threshold Image", binnaryImg);
            var ele = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(15, 10), p_struc);
            Mat dilateImg = new Mat();
            //new MCvScalar(255, 0, 255, 255) 外框的顏色
            CvInvoke.Dilate(binnaryImg, dilateImg, ele, p_struc, 1, BorderType.Default, new MCvScalar(0, 0, 0, 0));
            CvInvoke.Imshow("Dilate Image", dilateImg);
            Mat temp2 = new Mat();
            CvInvoke.CvtColor(dilateImg, temp2, ColorConversion.Bgr2Gray);
            //32sC1 to 8uC1
            CvInvoke.Imshow("32sC1 to 8uC1 Image", temp2);
            //4.找文字區域
            Mat hierImg = new Mat();
            try
            {
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(temp2, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
                    if (contours.Size > 0)
                    {
                        Console.WriteLine("contours count: \n" + contours.Size);
                        for (int i = 0; i < contours.Size; i++)
                        {
                            using (VectorOfPoint contour = contours[i])
                            {
                                // 使用 BoundingRectangle 取得框選矩形
                                Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                                CvInvoke.Rectangle(changeImg, BoundingBox, new MCvScalar(255, 0, 255, 255), 3);
                            }
                        }
                        CvInvoke.Imshow("final Image", changeImg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: \n" + ex.Message);
            }
        }

        //Emgu + Vision API
        private void button3_Click(object sender, EventArgs e)
        {
            DateTime dt_start = DateTime.Now;
            string Target_Img = txt_file.Text;
            if (string.IsNullOrEmpty(Target_Img))
                Target_Img = @"C:\Users\tobeywang\Desktop\Idscan\image2.jpg";
            var img = CvInvoke.Imread(Target_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);

            if (img.IsEmpty)
            {
                Console.WriteLine("can not load the image \n");
                return;
            }
            pictureBox1.Image = img.Bitmap;
            image_filename = "image2";
            Mat changeImg = new Mat();
            List<pass_result> ori_matlist = Get_contour_binnary(img, out changeImg);
            if (ori_matlist.Count > 0)
            {
                Console.WriteLine("like word contour count: \n" + ori_matlist.Count);
                foreach (var item in ori_matlist)
                {
                    //讀出來的字樣
                    var s_result = Run_vision_api(item.path);
                    Console.WriteLine("contours:" + item.path);
                    Console.WriteLine("Data:" + s_result);
                }
            }
            else
                Console.WriteLine("contours are not found");
            var dt_durision = dt_start.Subtract(DateTime.Now).Duration();
            Console.WriteLine("執行時間：" + dt_durision.TotalSeconds.ToString() + " 秒");
            CvInvoke.Imshow("final Image", changeImg);

            //Vision API Testing
            //string path = @"D:\Cathay_T\My\MyProject\Emgu_Tesseract\Emgu_Tesseract\Emgu_Tesseract_ap\Info_Pic\_contour_binnary_re8.png";
            ////讀出來的字樣
            //var s_result = Run_vision_api(path);
            //Console.WriteLine("contours:" + path);
            //Console.WriteLine("Data:" + s_result);
            //var dt_durision = dt_start.Subtract(DateTime.Now).Duration();
            //Console.WriteLine("執行時間：" + dt_durision.TotalSeconds.ToString() + " 秒");
        }

        //Emgu + Full Match
        private void button4_Click(object sender, EventArgs e)
        {
            DateTime dt_start = DateTime.Now;
            string Target_Img = txt_file.Text;
            if (string.IsNullOrEmpty(Target_Img))
                Target_Img = @"C:\Users\tobeywang\Desktop\Idscan\image5.jpg";
            var img = CvInvoke.Imread(Target_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);
            if (img.IsEmpty)
            {
                Console.WriteLine("can not load the image \n");
                return;
            }
            image_filename = "image2";
            pictureBox1.Image = img.Bitmap;
            //標準圖像
            //var model_bigimg = CvInvoke.Imread(@"C:\Users\tobeywang\Desktop\Idscan\data\idImg_13.png", Emgu.CV.CvEnum.ImreadModes.Unchanged);
            string Model_Img = txt_model_file.Text;
            if (string.IsNullOrEmpty(Model_Img))
                Model_Img = @"D:\Cathay_T\My\MyProject\Emgu_Tesseract\Emgu_Tesseract\Emgu_Tesseract_ap\Info_Pic\_contour_binnary_re8.png";
            var model_bigimg = CvInvoke.Imread(Model_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);

            Mat changeImg = new Mat();
            //拿到一張大圖
            List<pass_result> ori_matlist = Full_Match(img, out changeImg);
            if (ori_matlist.Count > 0)
            {
                Console.WriteLine("like word contour count: \n" + ori_matlist.Count);
                foreach (var item in ori_matlist)
                {
                    long matchTime;
                    long score;
                    int matches_size;
                    var result_mat = Draw(model_bigimg, item.mat, out matchTime, out score, out matches_size);
                    TimeSpan toNow = new TimeSpan(matchTime);
                    //DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                    //DateTime dtResult = dtStart.Add(toNow);
                    //Console.WriteLine(" matchSec:" + toNow.TotalSeconds + " score:" + (score / 10000.0).ToString("P2"));
                    CvInvoke.Imshow("match Image", item.mat);
                    Console.WriteLine("Match Image info_path:" + item.path);
                    Console.WriteLine(" matchSec:" + toNow.TotalMilliseconds.ToString("G17") + " score:" + score.ToString());
                    Console.WriteLine(" match size:" + matches_size.ToString());
                    if (matches_size > 0)
                        Console.WriteLine(" match %:" + ((int)score / (matches_size * 2)).ToString("P2"));
                    //Show 框出截到的部分
                    //CvInvoke.Rectangle(changeImg, BoundingBox, new MCvScalar(255, 0, 255, 255), 3);
                    CvInvoke.Imshow("match result Image", result_mat);
                }
            }
            else
                Console.WriteLine("contours are not found");
            var dt_durision = dt_start.Subtract(DateTime.Now).Duration();
            Console.WriteLine("執行時間：" + dt_durision.TotalSeconds.ToString() + " 秒");
            CvInvoke.Imshow("final Image", changeImg);
        }

        //Emgu + Tesseract
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                //語言包：https://github.com/tesseract-ocr/tesseract/wiki/Data-Files#data-files-for-version-302
                #region Tesseract Dll
                TesseractEngine ocr;
                //多個語言 +
                ocr = new TesseractEngine(@"D:\Cathay_T\My\MyProject\Emgu_Tesseract\Emgu_Tesseract\Emgu_Tesseract_ap\tessdata", "chi_tra+eng", EngineMode.Default);
                DateTime dt_start = DateTime.Now;
                string Target_Img = txt_file.Text;
                if (string.IsNullOrEmpty(Target_Img))
                    Target_Img = @"C:\Users\tobeywang\Desktop\Idscan\image2.jpg";
                var img = CvInvoke.Imread(Target_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);

                //import pic 
                Bitmap bit = img.Bitmap;
                //bit = PreprocesImage(bit);//進行影象處理,如果識別率低可試試
                Tesseract.Page page = ocr.Process(bit);
                string str = page.GetText();//識別後的內容
                Console.WriteLine("結果內容：" + str);
                page.Dispose();
                #endregion

                #region Emgu.CV.OCR.Tesseract 
                //Emgu.CV.OCR.Tesseract ocr = new Emgu.CV.OCR.Tesseract(@"D:\Cathay_T\My\MyProject\Emgu_Tesseract\Emgu_Tesseract\Emgu_Tesseract_ap\tessdata", "chi_tra+eng", Emgu.CV.OCR.OcrEngineMode.Default);
                //DateTime dt_start = DateTime.Now;
                //string Target_Img = txt_file.Text;
                //if (string.IsNullOrEmpty(Target_Img))
                //    Target_Img = @"C:\Users\tobeywang\Desktop\Idscan\image2.jpg";
                //var img = CvInvoke.Imread(Target_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);

                ////import pic 
                //Bitmap bit = img.Bitmap;
                //bit = PreprocesImage(bit);//進行影象處理,如果識別率低可試試
                //Mat temp_img = new Mat();
                //var pix = new Emgu.CV.OCR.Pix(ConvertBitmapToMat(bit));
                //ocr.SetImage(pix);
                //ocr.Recognize();
                //string str = ocr.GetUNLVText();//識別後的內容
                //Console.WriteLine("結果內容：" + str);
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:" + ex.Message);
            }
        }

        //取得外框四個角點
        private void button6_Click(object sender, EventArgs e)
        {
            DateTime dt_start = DateTime.Now;
            string Target_Img = txt_file.Text;
            if (string.IsNullOrEmpty(Target_Img))
                Target_Img = @"C:\Users\TobeyWang\Desktop\Idscan\id\id1.png";
            var img = CvInvoke.Imread(Target_Img, Emgu.CV.CvEnum.ImreadModes.Unchanged);
            if (img.IsEmpty)
            {
                Console.WriteLine("can not load the image \n");
                return;
            }
            image_filename = "image2";
            pictureBox1.Image = img.Bitmap;
            Mat changeImg = new Mat();
            Get_contour_binnary_canny(img, out changeImg);
        }

        //矯轉(直轉橫)
        private void button7_Click(object sender, EventArgs e)
        {

        }

        private List<pass_result> Get_contour_binnary(Mat model_img, out Mat changeImg)
        {
            //2.改變尺吋:interpolation內插方式
            #region 2.改變尺吋，將圖縮小減少跑的時間

            changeImg = new Mat();
            Size newsize = new System.Drawing.Size(428, 270);
            CvInvoke.Resize(model_img, changeImg, newsize, 0, 0, Inter.Cubic);
            //CvInvoke.Imshow("Resize Image", changeImg);

            #endregion 2.改變尺吋，將圖縮小減少跑的時間

            Mat oriImg = changeImg;
            Point p_struc = new Point(-1, -1);

            #region 解出小圖的前置作業

            //去噪
            Mat denoImg = new Mat();
            int[,] array_1 = new int[,] { { 0, 1, 1 } };
            //有重截大邊框
            //CvInvoke.FastNlMeansDenoisingColored(main_Sub, denoImg, 10, 3, 3, 3);
            CvInvoke.FastNlMeansDenoisingColored(oriImg, denoImg, 10, 3, 3, 3);
            Mat garyImg = new Mat();
            //取得內核的小大
            //Mat temp = new Mat(3, 3, DepthType.Cv8U, 0);
            Mat temp = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), p_struc);
            CvInvoke.Transform(denoImg, garyImg, temp);
            //Gray
            //CvInvoke.Imshow("Gary Image", garyImg);
            //二值化
            Mat binnaryImg = new Mat();
            //閾值 180  maxval:255
            CvInvoke.Threshold(garyImg, binnaryImg, 180, 255, ThresholdType.BinaryInv);
            //CvInvoke.Imshow("Threshold Image(180,255)", binnaryImg);

            #endregion 解出小圖的前置作業

            #region 圖像接近Sample

            ////2.轉灰
            ////Image<Gray, byte> gray_Image = new Image<Gray, byte>(Sub.Bitmap);
            //Mat garyImg = new Mat();
            //CvInvoke.CvtColor(main_Sub, garyImg, ColorConversion.Bgr2Gray);
            ////CvInvoke.Imshow("gary Image ", garyImg);
            ////3.二值化
            //Mat binnaryImg = new Mat();
            ////Otsu:黑字白底 BINARY:白字黑底
            //CvInvoke.Threshold(garyImg, binnaryImg, 120, 255, ThresholdType.Otsu);
            ////CvInvoke.Imshow("Threshold Image(120,255) ", binnaryImg);

            #endregion 圖像接近Sample

            var ele = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(15, 10), p_struc);
            //膨脹圖(灰度圖)
            Mat dilateImg = new Mat();
            //new MCvScalar(255, 0, 255, 255) 外框的顏色
            CvInvoke.Dilate(binnaryImg, dilateImg, ele, p_struc, 1, BorderType.Default, new MCvScalar(0, 0, 0, 0));
            //CvInvoke.Imshow("Dilate Image", dilateImg);
            //腐蝕圖(灰度圖)
            Mat erodeImg = new Mat();
            CvInvoke.Erode(binnaryImg, erodeImg, ele, p_struc, 1, BorderType.Default, new MCvScalar(0, 0, 0, 0));
            //CvInvoke.Imshow("Erode Image", erodeImg);
            //膨脹圖 與 腐蝕圖 差異
            Mat temp2 = new Mat();
            CvInvoke.AbsDiff(dilateImg, erodeImg, temp2);
            //CvInvoke.Imshow("AbsDiff Image", temp2);//temp2應該是一個灰度圖
            //差異的灰度圖要做二值化(或是轉為灰階圖)才能清楚觀察結果
            Mat sideImg = new Mat();
            CvInvoke.Threshold(temp2, sideImg, 40, 255, ThresholdType.Binary);
            //CvInvoke.Imshow("Threshold Image(40,255)", sideImg);

            #region 32sC1 to 8uC1

            Mat temp3 = new Mat();
            //轉灰階圖
            CvInvoke.CvtColor(temp2, temp3, ColorConversion.Bgr2Gray);
            //CvInvoke.Imshow("32sC1 to 8uC1 Image", temp3);

            #endregion 32sC1 to 8uC1

            //4.找文字區域
            Mat hierImg = new Mat();
            var contours = new VectorOfVectorOfPoint();
            List<pass_result> result = new List<pass_result>();
            try
            {
                CvInvoke.FindContours(temp3, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                if (contours.Size > 0)
                {
                    Console.WriteLine("contours count: \n" + contours.Size);
                    //存下截圖
                    string directory_path = ap_path + @"\Info_Pic\" + DateTime.Now.ToShortDateString().Replace("/", "") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();

                    #region 建立資料夾

                    int i_dir_count = 1;
                    while (Directory.Exists(directory_path))
                    {
                        directory_path = directory_path + "_" + i_dir_count.ToString();
                        i_dir_count++;
                    }
                    //新增資料夾
                    Directory.CreateDirectory(directory_path);

                    #endregion 建立資料夾

                    for (int i = 0; i < contours.Size; i++)
                    {
                        long matchTime;
                        long score;
                        using (VectorOfPoint contour = contours[i])
                        {
                            // 使用 BoundingRectangle 取得框選矩形
                            Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                            //截下框-------------------------------------------------
                            //只找中華民國身份證字樣 x:73 y:22 width:188 Height:43
                            Console.WriteLine("contour" + i.ToString() + " x:" + BoundingBox.X + " y:" + BoundingBox.Y);
                            Console.WriteLine("contour" + i.ToString() + " size:" + BoundingBox.Width + " x " + BoundingBox.Height);
                            if ((BoundingBox.Width > 100 && BoundingBox.Height > 20))
                            {
                                //1.切圖
                                Image<Bgr, byte> Sub = oriImg.ToImage<Bgr, byte>().GetSubRect(BoundingBox);
                                Sub.ToBitmap().Save(ap_path + @"\Info_Pic\contour" + i.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                                //CvInvoke.Imshow("CropImage ", Sub);
                                //2.轉灰
                                //Image<Gray, byte> gray_Image = new Image<Gray, byte>(Sub.Bitmap);
                                Mat min_gray = new Mat();
                                CvInvoke.CvtColor(Sub, min_gray, ColorConversion.Bgr2Gray);
                                //CvInvoke.Imshow("gary Image ", min_gray);
                                //3.二值化
                                Mat binnaryImg_detail = new Mat();
                                //Otsu:黑字白底 BINARY:白字黑底
                                CvInvoke.Threshold(min_gray, binnaryImg_detail, 120, 255, ThresholdType.Otsu);
                                //CvInvoke.Imshow("binnaryImg contour " + i.ToString(), binnaryImg_detail);
                                //-------------------------------------------------------

                                Mat binnary_resize = new Mat();
                                var fixHeight = Convert.ToInt32((Convert.ToDouble(model_img.Size.Width) / Convert.ToDouble(binnaryImg_detail.Width)) * Convert.ToDouble(binnaryImg_detail.Height));
                                CvInvoke.Resize(binnaryImg_detail, binnary_resize, new Size(model_img.Width, fixHeight), 0, 0, Inter.Cubic);
                                string path = directory_path + @"\contour_binnary" + i.ToString() + ".png";
                                string re_path = directory_path + @"\contour_binnary_re" + i.ToString() + ".png";
                                binnaryImg_detail.Bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                                binnary_resize.Bitmap.Save(re_path, System.Drawing.Imaging.ImageFormat.Png);
                                Console.WriteLine("contour" + i.ToString() + " size " + BoundingBox.Size);
                                CvInvoke.Rectangle(oriImg, BoundingBox, new MCvScalar(255, 0, 255, 255), 3);
                                result.Add(new pass_result
                                {
                                    mat = binnary_resize,
                                    path = path,
                                    path_resize = re_path
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: \n" + ex.Message);
            }
            return result;
        }

        //截出卡片的最大邊框
        private void Get_contour_binnary_canny(Mat model_img, out Mat changeImg)
        {
            //2.改變尺吋:interpolation內插方式
            #region 2.改變尺吋，將圖縮小減少跑的時間

            changeImg = new Mat();
            Size newsize = new System.Drawing.Size(428, 270);
            CvInvoke.Resize(model_img, changeImg, newsize, 0, 0, Inter.Cubic);
            //CvInvoke.Imshow("Resize Image", changeImg);

            #endregion 2.改變尺吋，將圖縮小減少跑的時間

            Mat oriImg = changeImg;
            //3.使用canny算子查找邊緣 -------------------------------
            #region 3.尋找證件的邊緣
            //實驗結果：如要找身份證的邊框可能比較困難，因拍攝的角度不一定，照成的灰階化結果會影響邊緣針測的參數
            //高斯平滑 > Pixel梯度計算 > 閥值判斷是否為邊緣(梯度高於高閥值為邊界、低於低閥值非邊界、其他則是以周圍是否被判斷為邊界)
            Mat cannyImg = new Mat();
            //兩個門檻參數threshold1與threshold2
            //圖形的任一點像素，若其值大於threshold2，則認定它屬於邊緣像素，若小於threshold1則不為邊緣像素
            CvInvoke.Canny(changeImg, cannyImg, 30, 10);//threshold1越大，取的結果越銳利，越少框角
            if (cannyImg.IsEmpty)
                Console.WriteLine("can not cover the image \n");
            else
                CvInvoke.Imshow("Canny Image", cannyImg);

            Image<Bgr, byte> main_Sub = changeImg.ToImage<Bgr, byte>();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            try
            {
                CvInvoke.FindContours(cannyImg, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                Console.WriteLine("canny contours count:" + contours.Size);
                List<result> all = new List<result>();
                RotatedRect maxRect = new RotatedRect();
                PointF[] point4 = new PointF[4];

                Mat changeImg_temp = new Mat();

                var big_area = new VectorOfPoint();
                double maxArea = 0.0;
                for (int i = 0; i < contours.Size; i++)
                {
                    result r = new result();
                    using (VectorOfPoint contour = contours[i])
                    {
                        //最大區域
                        RotatedRect min_ar = CvInvoke.MinAreaRect(contour);
                        var area = CvInvoke.ContourArea(contour);
                        if (area > maxArea)
                        {
                            maxArea = area;
                            maxRect = min_ar;
                            big_area = contour;
                            //get 4 point
                            point4=CvInvoke.BoxPoints(min_ar);

                            //var BoundingBox_temp = CvInvoke.BoundingRectangle(big_area);
                            //CvInvoke.Rectangle(changeImg_temp, BoundingBox_temp, new MCvScalar(255, 0, 255, 255), 2);

                        }
                    }
                }
                //CvInvoke.Imshow("Canny_list_area Image", changeImg_temp);
                Console.WriteLine("canny contours big :" + maxRect.Size);
                //邊框四個角點
                Console.WriteLine(string.Format("canny contours 4 :({0},{1}) ({2},{3}) ({4},{5}) ({6},{7})",
                    point4[0].X, point4[0].Y, point4[1].X, point4[1].Y, point4[2].X, point4[2].Y, point4[3].X, point4[3].Y)) ;
                var BoundingBox = CvInvoke.BoundingRectangle(big_area);

                #region 框出邊界

                CvInvoke.Rectangle(changeImg, BoundingBox, new MCvScalar(255, 0, 255, 255), 2);
                CvInvoke.Imshow("Canny_list Image", changeImg);

                #endregion 框出邊界

                #region 截下邊界

                main_Sub = changeImg.ToImage<Bgr, byte>().GetSubRect(BoundingBox);
                CvInvoke.Imshow("Canny_cut Image", main_Sub);

                #endregion 截下邊界
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: \n" + ex.Message);
            }

            //---------------------------------

            #endregion 3.尋找證件的邊緣
            #region 
            //Point p_struc = new Point(-1, -1);

            //#region 解出小圖的前置作業

            ////去噪
            //Mat denoImg = new Mat();
            //int[,] array_1 = new int[,] { { 0, 1, 1 } };
            ////有重截大邊框
            //CvInvoke.FastNlMeansDenoisingColored(main_Sub, denoImg, 10, 3, 3, 3);
            //Mat garyImg = new Mat();
            ////取得內核的小大
            ////Mat temp = new Mat(3, 3, DepthType.Cv8U, 0);
            //Mat temp = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), p_struc);
            //CvInvoke.Transform(denoImg, garyImg, temp);
            ////Gray
            ////CvInvoke.Imshow("Gary Image", garyImg);
            ////二值化
            //Mat binnaryImg = new Mat();
            ////閾值 180  maxval:255
            //CvInvoke.Threshold(garyImg, binnaryImg, 180, 255, ThresholdType.BinaryInv);
            ////CvInvoke.Imshow("Threshold Image(180,255)", binnaryImg);

            //#endregion 解出小圖的前置作業

            //#region 圖像接近Sample

            //////2.轉灰
            //////Image<Gray, byte> gray_Image = new Image<Gray, byte>(Sub.Bitmap);
            ////Mat garyImg = new Mat();
            ////CvInvoke.CvtColor(main_Sub, garyImg, ColorConversion.Bgr2Gray);
            //////CvInvoke.Imshow("gary Image ", garyImg);
            //////3.二值化
            ////Mat binnaryImg = new Mat();
            //////Otsu:黑字白底 BINARY:白字黑底
            ////CvInvoke.Threshold(garyImg, binnaryImg, 120, 255, ThresholdType.Otsu);
            //////CvInvoke.Imshow("Threshold Image(120,255) ", binnaryImg);

            //#endregion 圖像接近Sample

            //var ele = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(15, 10), p_struc);
            ////膨脹圖(灰度圖)
            //Mat dilateImg = new Mat();
            ////new MCvScalar(255, 0, 255, 255) 外框的顏色
            //CvInvoke.Dilate(binnaryImg, dilateImg, ele, p_struc, 1, BorderType.Default, new MCvScalar(0, 0, 0, 0));
            ////CvInvoke.Imshow("Dilate Image", dilateImg);
            ////腐蝕圖(灰度圖)
            //Mat erodeImg = new Mat();
            //CvInvoke.Erode(binnaryImg, erodeImg, ele, p_struc, 1, BorderType.Default, new MCvScalar(0, 0, 0, 0));
            ////CvInvoke.Imshow("Erode Image", erodeImg);
            ////膨脹圖 與 腐蝕圖 差異
            //Mat temp2 = new Mat();
            //CvInvoke.AbsDiff(dilateImg, erodeImg, temp2);
            ////CvInvoke.Imshow("AbsDiff Image", temp2);//temp2應該是一個灰度圖
            ////差異的灰度圖要做二值化(或是轉為灰階圖)才能清楚觀察結果
            //Mat sideImg = new Mat();
            //CvInvoke.Threshold(temp2, sideImg, 40, 255, ThresholdType.Binary);
            ////CvInvoke.Imshow("Threshold Image(40,255)", sideImg);

            //#region 32sC1 to 8uC1

            //Mat temp3 = new Mat();
            ////轉灰階圖
            //CvInvoke.CvtColor(temp2, temp3, ColorConversion.Bgr2Gray);
            ////CvInvoke.Imshow("32sC1 to 8uC1 Image", temp3);

            //#endregion 32sC1 to 8uC1

            ////4.找文字區域
            //Mat hierImg = new Mat();
            //contours = new VectorOfVectorOfPoint();
            //List<pass_result> result = new List<pass_result>();
            //try
            //{
            //    CvInvoke.FindContours(temp3, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            //    if (contours.Size > 0)
            //    {
            //        Console.WriteLine("contours count: \n" + contours.Size);
            //        //存下截圖
            //        string directory_path = ap_path + @"\Info_Pic\" + DateTime.Now.ToShortDateString().Replace("/", "") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();

            //        #region 建立資料夾

            //        int i_dir_count = 1;
            //        while (Directory.Exists(directory_path))
            //        {
            //            directory_path = directory_path + "_" + i_dir_count.ToString();
            //            i_dir_count++;
            //        }
            //        //新增資料夾
            //        Directory.CreateDirectory(directory_path);

            //        #endregion 建立資料夾

            //        for (int i = 0; i < contours.Size; i++)
            //        {
            //            long matchTime;
            //            long score;
            //            using (VectorOfPoint contour = contours[i])
            //            {
            //                // 使用 BoundingRectangle 取得框選矩形
            //                Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
            //                //截下框-------------------------------------------------
            //                //只找中華民國身份證字樣 x:73 y:22 width:188 Height:43
            //                Console.WriteLine("contour" + i.ToString() + " x:" + BoundingBox.X + " y:" + BoundingBox.Y);
            //                Console.WriteLine("contour" + i.ToString() + " size:" + BoundingBox.Width + " x " + BoundingBox.Height);
            //                if ((BoundingBox.Width > 100 && BoundingBox.Height > 20))
            //                {
            //                    //1.切圖
            //                    Image<Bgr, byte> Sub = main_Sub.GetSubRect(BoundingBox);
            //                    Sub.ToBitmap().Save(ap_path + @"\Info_Pic\contour" + i.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
            //                    //CvInvoke.Imshow("CropImage ", Sub);
            //                    //2.轉灰
            //                    //Image<Gray, byte> gray_Image = new Image<Gray, byte>(Sub.Bitmap);
            //                    Mat min_gray = new Mat();
            //                    CvInvoke.CvtColor(Sub, min_gray, ColorConversion.Bgr2Gray);
            //                    //CvInvoke.Imshow("gary Image ", min_gray);
            //                    //3.二值化
            //                    Mat binnaryImg_detail = new Mat();
            //                    //Otsu:黑字白底 BINARY:白字黑底
            //                    CvInvoke.Threshold(min_gray, binnaryImg_detail, 120, 255, ThresholdType.Otsu);
            //                    //CvInvoke.Imshow("binnaryImg contour " + i.ToString(), binnaryImg_detail);
            //                    //-------------------------------------------------------

            //                    Mat binnary_resize = new Mat();
            //                    var fixHeight = Convert.ToInt32((Convert.ToDouble(model_img.Size.Width) / Convert.ToDouble(binnaryImg_detail.Width)) * Convert.ToDouble(binnaryImg_detail.Height));
            //                    CvInvoke.Resize(binnaryImg_detail, binnary_resize, new Size(model_img.Width, fixHeight), 0, 0, Inter.Cubic);
            //                    string path = directory_path + @"\contour_binnary" + i.ToString() + ".png";
            //                    string re_path = directory_path + @"\contour_binnary_re" + i.ToString() + ".png";
            //                    binnaryImg_detail.Bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            //                    binnary_resize.Bitmap.Save(re_path, System.Drawing.Imaging.ImageFormat.Png);
            //                    Console.WriteLine("contour" + i.ToString() + " size " + BoundingBox.Size);
            //                    CvInvoke.Rectangle(oriImg, BoundingBox, new MCvScalar(255, 0, 255, 255), 3);
            //                    result.Add(new pass_result
            //                    {
            //                        mat = binnary_resize,
            //                        path = path,
            //                        path_resize = re_path
            //                    });
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("error: \n" + ex.Message);
            //}
            //changeImg = main_Sub.Mat;
            //return result;
            #endregion
        }

        private List<pass_result> Full_Match(Mat model_img, out Mat changeImg)
        {
            //2.改變尺吋:interpolation內插方式

            #region 2.改變尺吋，將圖縮小減少跑的時間

            changeImg = new Mat();
            Size newsize = new System.Drawing.Size(428, 270);
            CvInvoke.Resize(model_img, changeImg, newsize, 0, 0, Inter.Cubic);
            //CvInvoke.Imshow("Resize Image", changeImg);

            #endregion 2.改變尺吋，將圖縮小減少跑的時間

            Mat oriImg = changeImg;
            //3.使用canny算子查找邊緣 -------------------------------

            #region 3.尋找證件的邊緣
            //灰階化
            Mat canny_gray = new Mat();
            CvInvoke.CvtColor(changeImg, canny_gray, ColorConversion.Rgb2Gray);
            CvInvoke.Imshow("Rgb2Gray Image", canny_gray);
            //Sobel(這段在canny function 就會執行(Pixel梯度計算)
            Mat canny_sobel = new Mat();
            CvInvoke.Sobel(canny_gray, canny_sobel, canny_gray.Depth, 1, 0);
            CvInvoke.Imshow("Sobel Image", canny_sobel);

            //實驗結果：如要找身份證的邊框可能比較困難，因拍攝的角度不一定，照成的灰階化結果會影響邊緣針測的參數
            //高斯平滑 > Pixel梯度計算 > 閥值判斷是否為邊緣(梯度高於高閥值為邊界、低於低閥值非邊界、其他則是以周圍是否被判斷為邊界)
            Mat cannyImg = new Mat();
            //兩個門檻參數threshold1與threshold2
            //圖形的任一點像素，若其值大於threshold2，則認定它屬於邊緣像素，若小於threshold1則不為邊緣像素
            CvInvoke.Canny(canny_gray, cannyImg, 30, 10);
            if (cannyImg.IsEmpty)
                Console.WriteLine("can not cover the image \n");
            else
                CvInvoke.Imshow("Canny Image", cannyImg);

            Image<Bgr, byte> main_Sub = changeImg.ToImage<Bgr, byte>();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            try
            {
                CvInvoke.FindContours(cannyImg, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
                Console.WriteLine("canny contours count:" + contours.Size);
                List<result> all = new List<result>();
                RotatedRect maxRect = new RotatedRect();
                var big_area = new VectorOfPoint();
                double maxArea = 0.0;
                for (int i = 0; i < contours.Size; i++)
                {
                    result r = new result();
                    using (VectorOfPoint contour = contours[i])
                    {
                        //最大區域
                        RotatedRect min_ar = CvInvoke.MinAreaRect(contour);
                        var area = CvInvoke.ContourArea(contour);
                        if (area > maxArea)
                        {
                            maxArea = area;
                            maxRect = min_ar;
                            big_area = contour;
                        }
                    }
                }
                Console.WriteLine("canny contours big :" + maxRect.Size);
                var BoundingBox = CvInvoke.BoundingRectangle(big_area);

                #region 框出邊界

                //CvInvoke.Rectangle(changeImg, BoundingBox, new MCvScalar(255, 0, 255, 255), 2);
                ////CvInvoke.Imshow("Canny_list Image", changeImg);

                #endregion 框出邊界

                #region 截下邊界

                main_Sub = changeImg.ToImage<Bgr, byte>().GetSubRect(BoundingBox);
                //CvInvoke.Imshow("Canny_cut Image", main_Sub);

                #endregion 截下邊界
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: \n" + ex.Message);
            }

            //---------------------------------

            #endregion 3.尋找證件的邊緣

            //2.轉灰
            //Image<Gray, byte> gray_Image = new Image<Gray, byte>(Sub.Bitmap);
            Mat min_gray = new Mat();
            CvInvoke.CvtColor(main_Sub, min_gray, ColorConversion.Bgr2Gray);
            //CvInvoke.Imshow("gary Image ", min_gray);
            //3.二值化
            Mat binnaryImg_detail = new Mat();
            //Otsu:黑字白底 BINARY:白字黑底
            CvInvoke.Threshold(min_gray, binnaryImg_detail, 120, 255, ThresholdType.Otsu);
            //CvInvoke.Imshow("binnaryImg contour ", binnaryImg_detail);

            //被比較的圖片是小圖，而要比較的圖片是大圖
            string directory_path = ap_path + @"\Info_Pic\" + DateTime.Now.ToShortDateString().Replace("/", "") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();

            #region 建立資料夾

            int i_dir_count = 1;
            while (Directory.Exists(directory_path))
            {
                directory_path = directory_path + "_" + i_dir_count.ToString();
                i_dir_count++;
            }
            //新增資料夾
            Directory.CreateDirectory(directory_path);

            #endregion 建立資料夾

            List<pass_result> result = new List<pass_result>();
            binnaryImg_detail.Bitmap.Save(directory_path + @"\contour_binnary_full_" + DateTime.Now.Millisecond + ".png", System.Drawing.Imaging.ImageFormat.Png);
            result.Add(new pass_result
            {
                mat = binnaryImg_detail,
                path = directory_path + @"\contour_binnary_full_" + DateTime.Now.Millisecond + ".png"
            });
            //截掉邊框後的圖片
            changeImg = main_Sub.Mat;
            return result;
        }

        /// <summary>
        /// 二張圖片的相似程度
        /// </summary>
        /// <param name="modelImage">比對圖</param>
        /// <param name="observedImage">原圖</param>
        /// <param name="matchTime">比對時間</param>
        /// <param name="score">相似程度</param>
        /// <param name="matches_size"></param>
        /// <returns></returns>
        public static Mat Draw(Mat modelImage, Mat observedImage, out long matchTime, out long score, out int matches_size)
        {
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography, out score);

                //Draw the matched keypoints
                Mat result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                   matches, result, new MCvScalar(200, 237, 204), new MCvScalar(255, 255, 255), mask);
                matches_size = matches.Size;

                #region draw the projected region on the image

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                  new PointF(rect.Left, rect.Bottom),
                  new PointF(rect.Right, rect.Bottom),
                  new PointF(rect.Right, rect.Top),
                  new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);

#if NETFX_CORE
               Point[] points = Extensions.ConvertAll<PointF, Point>(pts, Point.Round);
#else
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
#endif
                    using (VectorOfPoint vp = new VectorOfPoint(points))
                    {
                        CvInvoke.Polylines(result, vp, true, new MCvScalar(200, 237, 204), 5);
                    }
                }

                #endregion draw the projected region on the image

                return result;
            }
        }

        private static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography, out long score)
        {
            int k = 2;
            double uniquenessThreshold = 0.80;

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();

            using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
            {
                KAZE featureDetector = new KAZE();
                //對被比對的圖片-尋找特徵點
                Mat modelDescriptors = new Mat();
                featureDetector.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                watch = Stopwatch.StartNew();
                //對要比對的圖片-尋找特徵點
                Mat observedDescriptors = new Mat();
                featureDetector.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);

                // KdTree for faster results / less accuracy
                using (var ip = new Emgu.CV.Flann.KdTreeIndexParams())
                using (var sp = new SearchParams())
                using (DescriptorMatcher matcher = new FlannBasedMatcher(ip, sp))
                {
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    // Calculate score based on matches size
                    // ---------------------------------------------->
                    score = 0;
                    var data = mask.GetData();
                    for (int i = 0; i < matches.Size; i++)
                    {
                        if (int.Parse(data.GetValue(i, 0).ToString()) == 0) continue;
                        //if (mask.GetData(i)[0] == 0) continue;
                        foreach (var e in matches[i].ToArray())
                            ++score;
                    }
                    // <----------------------------------------------

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, matches, mask, 2);
                    }
                }
                watch.Stop();
            }
            matchTime = watch.ElapsedMilliseconds;
        }

        #region Vision API
        private string Run_vision_api(string image_path)
        {
            var TextResult = "";
            var JsonResult = "";

            try
            {
                Bitmap image = new Bitmap(image_path);
                byte[] data = null;
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, image.RawFormat);
                    data = ms.ToArray();
                }
                //取得憑証
                var c = new Class1();
                var cred = c.CreateCredential();
                var service = c.CreateService(cred);

                Google.Apis.Vision.v1.Data.BatchAnnotateImagesRequest batchRequest = new BatchAnnotateImagesRequest();
                batchRequest.Requests = new List<AnnotateImageRequest>();
                batchRequest.Requests.Add(new Google.Apis.Vision.v1.Data.AnnotateImageRequest()
                {
                    //DOCUMENT_TEXT_DETECTION TEXT_DETECTION FACE_DETECTION
                    //https://cloud.google.com/vision/docs/features
                    Features = new List<Feature>() { new Feature() { Type = "TEXT_DETECTION", MaxResults = 1 }, },
                    ImageContext = new ImageContext() { LanguageHints = new List<string>() { "zh" } },
                    Image = new Google.Apis.Vision.v1.Data.Image() { Content = Convert.ToBase64String(data) }
                });

                var annotate = service.Images.Annotate(batchRequest);
                BatchAnnotateImagesResponse batchAnnotateImagesResponse = annotate.Execute();
                if (batchAnnotateImagesResponse.Responses.Any())
                {
                    AnnotateImageResponse annotateImageResponse = batchAnnotateImagesResponse.Responses[0];
                    if (annotateImageResponse.Error != null)
                    {
                        if (annotateImageResponse.Error.Message != null)
                            Console.WriteLine(annotateImageResponse.Error.Message);
                    }
                    else
                    {
                        if (annotateImageResponse.TextAnnotations != null && annotateImageResponse.TextAnnotations.Any())
                        {
                            TextResult = annotateImageResponse.TextAnnotations[0].Description.Replace("\n", "\r\n");
                            JsonResult = JsonConvert.SerializeObject(annotateImageResponse.TextAnnotations[0]);
                            Console.WriteLine("json:" + annotateImageResponse.TextAnnotations[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error:" + ex.Message;
            }
            return TextResult;
        }
        #endregion

        private void btn_select_Click(object sender, EventArgs e)
        {
            get_file();
        }

        private void get_file()
        {
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = @"JPeg Image|*.jpg|PNG Image|*.png";
            //saveFileDialog1.FilterIndex = 2;
            //saveFileDialog1.RestoreDirectory = true;
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //    txt_file.Text = saveFileDialog1.FileName;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = @"JPeg Image|*.jpg|PNG Image|*.png";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                txt_file.Text = openFileDialog1.FileName;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btn_model_select_Click(object sender, EventArgs e)
        {
            get_file();
        }

        #region Tesseract
        private Bitmap PreprocesImage(Bitmap image)
        {
            //You can change your new color here. Red,Green,LawnGreen any..
            System.Drawing.Color actualColor;
            //make an empty bitmap the same size as scrBitmap
            //調整大小與對比度回傳結果
            image = ResizeImage(image, image.Width * 5, image.Height * 5);
            string directory_path = ap_path + @"\Info_Pic\" + DateTime.Now.ToShortDateString().Replace("/", "") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();

            #region 建立資料夾

            int i_dir_count = 1;
            while (Directory.Exists(directory_path))
            {
                directory_path = directory_path + "_" + i_dir_count.ToString();
                i_dir_count++;
            }
            //新增資料夾
            Directory.CreateDirectory(directory_path);

            #endregion 建立資料夾

            image.Save(directory_path + @"\contour_Preprocess_Resize.png", System.Drawing.Imaging.ImageFormat.Png);

            Bitmap newBitmap = new Bitmap(image.Width, image.Height);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    //get the pixel from the scrBitmap image
                    actualColor = image.GetPixel(i, j);
                    // > 150 because.. Images edges can be of low pixel colr. if we set all pixel color to new then there will be no smoothness left.
                    if (actualColor.R > 23 || actualColor.G > 23 || actualColor.B > 23)//在這裡設定RGB
                        newBitmap.SetPixel(i, j, System.Drawing.Color.White);
                    else
                        newBitmap.SetPixel(i, j, System.Drawing.Color.Black);
                }
            }
            return newBitmap;
        }
        /// <summary>
        /// 調整圖片大小和對比度
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution * 2);//2,3
            //image.Save(@"D:\UpWork\OCR_WinForm\Preprocess_HighRes.jpg");

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.Clamp);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        #endregion

        public Mat ConvertBitmapToMat(Bitmap bmp)
        {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

            // data = scan0 is a pointer to our memory block.
            IntPtr data = bmpData.Scan0;

            // step = stride = amount of bytes for a single line of the image
            int step = bmpData.Stride;

            // So you can try to get you Mat instance like this:
            Mat mat = new Mat(bmp.Height, bmp.Width, Emgu.CV.CvEnum.DepthType.Cv32F, 4, data, step);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            return mat;
        }

    }

    public class result
    {
        public int width { set; get; }
        public int hight { set; get; }
    }

    public class pass_result
    {
        public Mat mat { set; get; }
        public string path { set; get; }
        public string path_resize { set; get; }
    }
}