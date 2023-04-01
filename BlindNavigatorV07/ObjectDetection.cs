using Android.Graphics;
using Emgu.CV; //Emgu.CV треба докачати через NuGet
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;
using Plugin.Media;
using System;

namespace BlindNavigatorV07
{
    internal class ObjectDetection
    {
        //public static int Detect(Bitmap image)
        //{
        //    return 1;
        //}
        private CascadeClassifier _humanClassifier;
        private CascadeClassifier _carClassifier;
        private CascadeClassifier _columnClassifier;
        private CascadeClassifier _wallClassifier;

        public ObjectDetection()
        {
            // Завантажити класифікатори
            //_humanClassifier = new CascadeClassifier("Human.xml");
            //_carClassifier = new CascadeClassifier("haarcascade_car.xml");
            //_columnClassifier = new CascadeClassifier("haarcascade_column.xml");
            //_wallClassifier = new CascadeClassifier("haarcascade_wall.xml");
        }
        public Image<Bgr, byte> ConvertBitmapDataToImage(byte[] imageData)
        {
            Mat mat=new Mat();
            CvInvoke.Imdecode(imageData, ImreadModes.AnyColor, mat);
            var image = mat.ToImage<Bgr, byte>();

            return image;
        }
        public int Detect(byte[] imageData)
        {
            // Створити Emgu.CV Image з Bitmap
            var image = ConvertBitmapDataToImage(imageData);

            // Конвертувати зображення в градації сірого
            var grayImage = image.Convert<Gray, byte>();
        }
    }
}
