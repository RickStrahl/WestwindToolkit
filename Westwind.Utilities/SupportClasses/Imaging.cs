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
using System.IO;

namespace Westwind.Utilities
{
	/// <summary>
	/// Summary description for wwImaging.
	/// </summary>
	public class Imaging
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
		public static Bitmap ResizeImage(string filename,int width, int height)
		{
			Bitmap bmpOut = null;

			try 
			{
				Bitmap bmp = new Bitmap(filename);
				ImageFormat format = bmp.RawFormat;

				decimal ratio;
				int newWidth = 0;
				int newHeight = 0;

				//*** If the image is smaller than a thumbnail just return it
				if (bmp.Width < width && bmp.Height < height) 
					return bmp;
			
				if (bmp.Width > bmp.Height)
				{
					ratio = (decimal) width / bmp.Width;
					newWidth = width;
					decimal lnTemp = bmp.Height * ratio;
					newHeight = (int)lnTemp;
				}
				else 
				{
					ratio = (decimal) height / bmp.Height;
					newHeight = height;
					decimal lnTemp = bmp.Width * ratio;
					newWidth = (int) lnTemp;
				}

				bmpOut = new Bitmap(newWidth, newHeight);
				Graphics g = Graphics.FromImage(bmpOut);
				g.InterpolationMode =InterpolationMode.HighQualityBicubic;
				g.FillRectangle( Brushes.White,0,0,newWidth,newHeight);
				g.DrawImage(bmp,0,0,newWidth,newHeight);

				//System.Drawing.Image imgOut = loBMP.GetThumbnailImage(lnNewWidth,lnNewHeight,null,IntPtr.Zero);
				bmp.Dispose();
                bmpOut.Dispose();
			}
			catch 
			{
				return null;
			}
		
			return bmpOut;
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
        public static Bitmap ResizeImage(byte[] data, int width, int height)
        {
            Bitmap bmpOut = null;

            try
            {
                Bitmap bmp = new Bitmap(new MemoryStream(data));
                ImageFormat format = bmp.RawFormat;

                decimal ratio;
                int newWidth = 0;
                int newHeight = 0;

                //*** If the image is smaller than a thumbnail just return it
                if (bmp.Width < width && bmp.Height < height)
                    return bmp;

                if (bmp.Width > bmp.Height)
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

                bmpOut = new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage(bmpOut);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                g.DrawImage(bmp, 0, 0, newWidth, newHeight);

                //System.Drawing.Image imgOut = loBMP.GetThumbnailImage(lnNewWidth,lnNewHeight,null,IntPtr.Zero);
                bmp.Dispose();
                //bmpOut.Dispose();
            }
            catch
            {
                return null;
            }

            return bmpOut;
        }

		public static bool ResizeImage(string filename, string outputFilename, 
			                           int width, int height)
		{
			Bitmap bmpOut = null;

			try 
			{
				Bitmap bmp = new Bitmap(filename);
				ImageFormat format = bmp.RawFormat;

				decimal ratio;
				int newWidth = 0;
				int newHeight = 0;

				//*** If the image is smaller than a thumbnail just return it
				if (bmp.Width < width && bmp.Height < height) 
				{ 
                    if (outputFilename != filename)
					    bmp.Save(outputFilename);
                    bmp.Dispose();
					return true;
				}

				if (bmp.Width > bmp.Height)
				{
					ratio = (decimal) width / bmp.Width;
					newWidth = width;
					decimal temp = bmp.Height * ratio;
					newHeight = (int)temp;
				}
				else 
				{
					ratio = (decimal) height / bmp.Height;
					newHeight = height;
					decimal lnTemp = bmp.Width * ratio;
					newWidth = (int) lnTemp;
				}

				bmpOut = new Bitmap(newWidth, newHeight);
				Graphics g = Graphics.FromImage(bmpOut);
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.FillRectangle( Brushes.White,0,0,newWidth,newHeight);
				g.DrawImage(bmp,0,0,newWidth,newHeight);

				//System.Drawing.Image imgOut = loBMP.GetThumbnailImage(lnNewWidth,lnNewHeight,null,IntPtr.Zero);
				bmp.Dispose();

				bmpOut.Save(outputFilename,format);
				bmpOut.Dispose();
			}
			catch(Exception ex) 
			{
                var msg = ex.GetBaseException();
				return false;
			}
		
			return true;
		}	
	}
}
