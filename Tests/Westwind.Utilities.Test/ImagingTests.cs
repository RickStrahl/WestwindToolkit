using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class ImagingTests
    {
        [TestMethod]
        public void RotateFileToFileTest()
        {
            string orig = @"c:\sailbig.jpg";
            string work = @"c:\sailbig2.jpg";
            string rotated = @"c:\sailbig.jpg.copy";

            File.Copy(orig,work,true);
            ImageUtils.RoateImage(work,rotated,RotateFlipType.Rotate270FlipNone);            
            File.Copy(rotated,work,true);

        }

        [TestMethod]
        public void ResizeBitMap()
        {
            string orig = @"c:\sailbig.jpg";
            string copied = @"c:\sailbig2.jpg";

            using (var bmp = new Bitmap(orig))
            {
                var bmp2 = ImageUtils.ResizeImage(bmp, 150, 150);
                bmp2.Save(copied);
                bmp2.Dispose();
            }
        }
    }
}
