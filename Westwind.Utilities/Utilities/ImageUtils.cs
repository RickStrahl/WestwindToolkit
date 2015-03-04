#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System;
using System.Collections.Generic;
using System.IO;

namespace Westwind.Utilities
{
    /// <summary>
    /// Summary description for wwImaging.
    /// </summary>
    public static class ImageUtils
    {
        /// <summary>
        /// Creates a resized bitmap from an existing image on disk. Resizes the image by 
        /// creating an aspect ratio safe image. Image is sized to the larger size of width
        /// height and then smaller size is adjusted by aspect ratio.
        /// 
        /// Image is returned as Bitmap - call Dispose() on the returned Bitmap object
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>Bitmap or null</returns>
        public static Bitmap ResizeImage(string filename, int width, int height, InterpolationMode mode = InterpolationMode.HighQualityBicubic)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(filename))
                {
                    return ResizeImage(bmp, width, height, mode);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resizes an image from byte array and returns a Bitmap.
        /// Make sure you Dispose() the bitmap after you're done 
        /// with it!
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap ResizeImage(byte[] data, int width, int height, InterpolationMode mode = InterpolationMode.HighQualityBicubic)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(new MemoryStream(data)))
                {
                    return ResizeImage(bmp, width, height, mode);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resizes an image and saves the image to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="outputFilename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="jpegCompressionMode">
        /// If using a jpeg image 
        /// </param>
        /// <returns></returns>
        public static bool ResizeImage(string filename, string outputFilename,
                                        int width, int height,
                                        InterpolationMode mode = InterpolationMode.HighQualityBicubic,
                                        int jpegCompressionMode = 85)
        {

            using (var bmpOut = ResizeImage(filename, width, height, mode))
            {
                var imageFormat = GetImageFormatFromFilename(filename);
                if (imageFormat == ImageFormat.Emf)
                    imageFormat = bmpOut.RawFormat;

                if(imageFormat == ImageFormat.Jpeg)
                    SaveJpeg(bmpOut, outputFilename, jpegCompressionMode);
                else
                    bmpOut.Save(outputFilename, imageFormat);
            }

            return true;
        }


        /// <summary>
        /// Resizes an image from a bitmap.
        /// 
        /// Note it will size to the larger of the sides 
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap ResizeImage(Bitmap bmp, int width, int height,
                                         InterpolationMode mode = InterpolationMode.HighQualityBicubic)
        {
            Bitmap bmpOut = null;

            try
            {
                decimal ratio;
                int newWidth = 0;
                int newHeight = 0;

                // If the image is smaller than a thumbnail just return original size
                if (bmp.Width < width && bmp.Height < height)
                {
                    newWidth = bmp.Width;
                    newHeight = bmp.Height;
                }
                else
                {
                    if (bmp.Width == bmp.Height)
                    {
                        if (height > width)
                        {
                            newHeight = height;
                            newWidth = height;
                        }
                        else
                        {
                            newHeight = width;
                            newWidth = width;
                        }
                    }
                    else if (bmp.Width >= bmp.Height)
                    {
                        ratio = (decimal)width / bmp.Width;
                        newWidth = width;
                        decimal lnTemp = bmp.Height * ratio;
                        newHeight = (int)lnTemp;
                    }
                    else
                    {
                        ratio = (decimal)height / bmp.Height;
                        newHeight = height;
                        decimal lnTemp = bmp.Width * ratio;
                        newWidth = (int)lnTemp;
                    }
                }

                //bmpOut = new Bitmap(bmp, new Size( newWidth, newHeight));                     
                bmpOut = new Bitmap(newWidth, newHeight);
                bmpOut.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

                Graphics g = Graphics.FromImage(bmpOut);
                g.InterpolationMode = mode;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                g.DrawImage(bmp, 0, 0, newWidth, newHeight);
            }
            catch
            {
                return null;
            }

            return bmpOut;
        }


        /// <summary>
        /// Saves a jpeg BitMap  to disk with a jpeg quality setting.
        /// Does not dispose the bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to save</param>
        /// <param name="outputFileName">file to write it to</param>
        /// <param name="jpegQuality"></param>
        /// <returns></returns>
        public static bool SaveJpeg(Bitmap bmp, string outputFileName, long jpegQuality = 90)
        {
            try
            {
                //get the jpeg codec
                ImageCodecInfo jpegCodec = null;
                if (Encoders.ContainsKey("image/jpeg"))
                    jpegCodec = Encoders["image/jpeg"];

                EncoderParameters encoderParams = null;
                if (jpegCodec != null)
                {
                    //create an encoder parameter for the image quality
                    EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, jpegQuality);

                    //create a collection of all parameters that we will pass to the encoder
                    encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;
                }
                bmp.Save(outputFileName, jpegCodec, encoderParams);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rotates an image and writes out the rotated image to a file.
        /// </summary>
        /// <param name="filename">The original image to roatate</param>
        /// <param name="outputFilename">The output file of the rotated image file. If not passed the original file is overwritten</param>
        /// <param name="type">Type of rotation to perform</param>
        /// <returns></returns>
        public static bool RoateImage(string filename, string outputFilename = null,
                                      RotateFlipType type = RotateFlipType.Rotate90FlipNone,
                                      int jpegCompressionMode = 85)
        {
            Bitmap bmpOut = null;

            if (string.IsNullOrEmpty(outputFilename))
                outputFilename = filename;

            try
            {
                ImageFormat imageFormat;
                using (Bitmap bmp = new Bitmap(filename))
                {
                    imageFormat = GetImageFormatFromFilename(filename);
                    if (imageFormat == ImageFormat.Emf)
                        imageFormat = bmp.RawFormat;

                    bmp.RotateFlip(type);

                    using (bmpOut = new Bitmap(bmp.Width, bmp.Height))
                    {
                        bmpOut.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

                        Graphics g = Graphics.FromImage(bmpOut);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bmp, 0, 0, bmpOut.Width, bmpOut.Height);
                        
                        if (imageFormat == ImageFormat.Jpeg)
                            SaveJpeg(bmpOut, outputFilename, jpegCompressionMode);
                        else
                            bmpOut.Save(outputFilename, imageFormat);                                
                    }                    
                }

            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException();
                return false;
            }

            return true;
        }

        public static byte[] RoateImage(byte[] data, RotateFlipType type = RotateFlipType.Rotate90FlipNone)
        {
            Bitmap bmpOut = null;

            try
            {
                Bitmap bmp = new Bitmap(new MemoryStream(data));

                ImageFormat imageFormat;
                imageFormat = bmp.RawFormat;
                bmp.RotateFlip(type);

                bmpOut = new Bitmap(bmp.Width, bmp.Height);
                bmpOut.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

                Graphics g = Graphics.FromImage(bmpOut);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, 0, 0, bmpOut.Width, bmpOut.Height);

                bmp.Dispose();

                using (var ms = new MemoryStream())
                {
                    bmpOut.Save(ms, imageFormat);
                    bmpOut.Dispose();

                    ms.Flush();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException();
                return null;
            }

        }


        /// <summary>
        /// A quick lookup for getting image encoders
        /// </summary>
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            //get accessor that creates the dictionary on demand
            get
            {
                //if the quick lookup isn't initialised, initialise it
                if (_encoders != null)
                    return _encoders;

                _encoders = new Dictionary<string, ImageCodecInfo>();

                //get all the codecs
                foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                {
                    //add each codec to the quick lookup
                    _encoders.Add(codec.MimeType.ToLower(), codec);
                }

                //return the lookup
                return _encoders;
            }
        }
        private static Dictionary<string, ImageCodecInfo> _encoders = null;


        /// <summary>
        /// Tries to return an image format 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Image format or ImageFormat.Emf if no match was found</returns>
        public static ImageFormat GetImageFormatFromFilename(string filename)
        {
            Bitmap bmpOut;
            string ext = Path.GetExtension(filename).ToLower();

            ImageFormat imageFormat;

            if (ext == ".jpg" || ext == ".jpeg")
                imageFormat = ImageFormat.Jpeg;
            else if (ext == ".png")
                imageFormat = ImageFormat.Png;
            else if (ext == ".gif")
                imageFormat = ImageFormat.Gif;
            else if (ext == ".bmp")
                imageFormat = ImageFormat.Bmp;
            else
                imageFormat = ImageFormat.Emf;

            return imageFormat;
        }
    }
}
