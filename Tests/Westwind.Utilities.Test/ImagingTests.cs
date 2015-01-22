using System;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;

namespace Westwind.Utilities.Tests
{
    [TestClass]
    public class ImagingTests
    {
        private string ImageFile = @"supportfiles\sailbig.jpg";
        private string ImageFileWork = @"supportfiles\sailbigWork.jpg";
        private string ImageFileRotated = @"supportfiles\sailbigRotated.jpg";
        private string SquareImageFile = @"supportfiles\SquareImage.jpg";

        [TestMethod]
        public void RotateFileToFileTest()
        {
            string orig = ImageFile;
            string work = ImageFileWork;
            string rotated = ImageFileRotated;

            File.Copy(orig,work,true);
            ImageUtils.RoateImage(work,rotated,RotateFlipType.Rotate270FlipNone);            
            File.Copy(rotated,work,true);

        }

        [TestMethod]
        public void RotateFileToSelfFileTest()
        {
            string orig = ImageFile;

            // work on sailbig3 and write output to sailbig3
            string work = ImageFileWork;
            string rotated = ImageFileRotated;

            File.Copy(orig, work, true);
            ImageUtils.RoateImage(work, rotated, RotateFlipType.Rotate270FlipNone);            
        }

        [TestMethod]
        public void RotateFileInMemory()
        {
            string orig = ImageFile;

            // work on sailbig3 and write output to sailbig3
            string work = ImageFileWork;
            string rotated = ImageFileRotated;

            var imgData = File.ReadAllBytes(orig); 
            var rotatedData = ImageUtils.RoateImage(imgData, RotateFlipType.Rotate270FlipNone);

            Assert.IsNotNull(rotatedData);

            File.WriteAllBytes(rotated, rotatedData);
        }



        [TestMethod]
        public void ResizeBitMap()
        {
            string orig = ImageFile;
            string copied = ImageFileWork;

            using (var bmp = new Bitmap(orig))
            {
                var bmp2 = ImageUtils.ResizeImage(bmp, 150, 150);
                bmp2.Save(copied);
                bmp2.Dispose();
            }
        }


        [TestMethod] 
        public void ResizeBitMapFile()
        {
            string orig = ImageFile;
            string copied = ImageFileWork;

            bool res = ImageUtils.ResizeImage(orig,copied, 150, 150);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void ResizeSquareBitMap()
        {
            string orig = SquareImageFile;
            string copied = ImageFileWork;

            using (var bmp = new Bitmap(orig))
            {
                var bmp2 = ImageUtils.ResizeImage(bmp, 100, 150);

                Assert.IsTrue(bmp2.Width == 150, "Image was not resized correctly.");

                Console.WriteLine(bmp2.RawFormat.Guid + " (New)");
                Console.WriteLine(bmp.RawFormat.Guid + " (File)");
                Console.WriteLine(ImageFormat.Jpeg.Guid + " (Jpeg)");
                               
                bmp2.Save(copied,ImageFormat.Jpeg);
                bmp2.Dispose();
            }
        }


    }
}
