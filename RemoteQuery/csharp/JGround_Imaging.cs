//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Org.JGround.Imaging {


    public static class ImagingUtils {

        private static List<String> supportedExtensions = new List<string>();
        static ImagingUtils() {
            supportedExtensions.Add(".jpg");
            supportedExtensions.Add(".gif");
            supportedExtensions.Add(".png");
            supportedExtensions.Add(".jpeg");
        }

        public static bool IsExtensionSupported(String fileName) {
            String ext = Path.GetExtension(fileName.ToLower());
            return supportedExtensions.Contains(ext);
        }


        public static void SaveImageWithBestEncoder(String fileName, Image image) {
            ImageCodecInfo jpeg = null;
            ImageCodecInfo png = null;
            ImageCodecInfo gif = null;
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            foreach(ImageCodecInfo encoder in encoders) {
                jpeg = encoder.FormatDescription.Equals("JPEG") ? encoder : jpeg;
                png = encoder.FormatDescription.Equals("PNG") ? encoder : png;
                gif = encoder.FormatDescription.Equals("GIF") ? encoder : gif;
            }

            EncoderParameters encoderParameters = new EncoderParameters();
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

            if(fileName.ToUpper().EndsWith("JPG")) {
                image.Save(fileName, jpeg, encoderParameters);
            } else if(fileName.ToUpper().EndsWith("PNG")) {
                image.Save(fileName, png, encoderParameters);
            } else if(fileName.ToUpper().EndsWith("GIF")) {
                image.Save(fileName, gif, encoderParameters);
            } else {
                image.Save(fileName);
            }
        }


        public static Image FixedSize(Image imgPhoto, int Width, int Height) {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if(nPercentH < nPercentW) {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            } else {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                //PixelFormat.Format24bppRgb
                              PixelFormat.Format32bppRgb
                              );
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Red);
            grPhoto.InterpolationMode =
                //InterpolationMode.HighQualityBicubic;
            InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static Image ResizeFixedWidth(Image imgPhoto, int Width, int MaxHeight) {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int Height = Width * sourceHeight / sourceWidth;

            if(Height > MaxHeight) {
                Width = MaxHeight * sourceWidth / sourceHeight;
                Height = Width * sourceHeight / sourceWidth;
            }

            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if(nPercentH < nPercentW) {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            } else {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format32bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Transparent);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }


        public static void ResizeToHeight(String input, int newHeight, String output) {
            Image inputImage = Image.FromFile(input);
            Image resizedImage = ResizeToMaxHeight(inputImage, newHeight);
            SaveImageWithBestEncoder(output, resizedImage);
            resizedImage.Dispose();
            inputImage.Dispose();
        }

        public static Image ResizeToMaxHeight(Image imgPhoto, int maxHeight) {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            if(sourceHeight <= maxHeight) {
                return imgPhoto;
            }
            int width = maxHeight * sourceWidth / sourceHeight;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)width / (float)sourceWidth);
            nPercentH = ((float)maxHeight / (float)sourceHeight);
            if(nPercentH < nPercentW) {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((width -
                              (sourceWidth * nPercent)) / 2);
            } else {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((maxHeight -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(width, maxHeight,
                              PixelFormat.Format32bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Transparent);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }


    }

}
