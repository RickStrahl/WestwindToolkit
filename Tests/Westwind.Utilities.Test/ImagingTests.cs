using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;

namespace Westwind.Utilities.Test
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
        public void ResizeSquareBitMap()
        {
            string orig = SquareImageFile;
            string copied = ImageFileWork;

            using (var bmp = new Bitmap(orig))
            {
                var bmp2 = ImageUtils.ResizeImage(bmp, 100, 150);

                Assert.IsTrue(bmp2.Width == 100, "Image was not resized correctly.");

                bmp2.Save(copied);
                bmp2.Dispose();
            }
        }
    }
}
