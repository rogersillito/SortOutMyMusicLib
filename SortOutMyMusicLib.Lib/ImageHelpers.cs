using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using log4net;
using TagLibFile = TagLib.File;

namespace SortOutMyMusicLib.Lib
{
    public interface IImageHelpers
    {
        bool IsSizeAcceptable(int width, int height);
        string GetFileExtensionFor(Image image);
        IEnumerable<CoverImage> GetCoverImagesFrom(string mediaFilePath);
        IList<string> GetCoverImagePathsOfAcceptableSizeFrom(string dirPath);
        bool IsAcceptableCoverImage(int width, int height, string fileExtension);
        bool SaveCoverImagesForFirstTagIn(ContainerDir dir);
        IEnumerable<string> GetImageFilePathsFrom(string dirPath);
    }

    public class ImageHelpers : IImageHelpers
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ImageHelpers));
        private readonly IAppConstants _appConstants;
        private readonly IFileSystemHelpers _fileSystemHelpers;

        public ImageHelpers(IAppConstants appConstants, IFileSystemHelpers fileSystemHelpers)
        {
            _appConstants = appConstants;
            _fileSystemHelpers = fileSystemHelpers;
        }

        public bool IsSizeAcceptable(int width, int height)
        {
            throw new NotImplementedException();
        }

        public string GetFileExtensionFor(Image image)
        {
            if (ImageFormat.Jpeg.Equals(image.RawFormat))
                return ".jpg";
            if (ImageFormat.Png.Equals(image.RawFormat))
                return ".png";
            if (ImageFormat.Gif.Equals(image.RawFormat))
                return ".gif";
            if (ImageFormat.Bmp.Equals(image.RawFormat))
                return ".bmp";
            throw new NotImplementedException(String.Format("Image type not handled: {0}", image.RawFormat));
        }

        public IEnumerable<CoverImage> GetCoverImagesFrom(string mediaFilePath)
        {
            // to save into tag, see: http://forums.codeguru.com/showthread.php?506806-TagLib-Writing-Images-to-File
            var tagLibFile = TagLibFile.Create(mediaFilePath);
            var ic = new ImageConverter();
            var iNumberOfPictures = tagLibFile.Tag.Pictures.Length;
            for (var i = 0; i < iNumberOfPictures; i++)
            {
                var image = (Image)ic.ConvertFrom(tagLibFile.Tag.Pictures[i].Data.Data);
                var type = tagLibFile.Tag.Pictures[i].Type.ToString();
                yield return new CoverImage {ImageData = image, Type = type};
            }
        }

        public IList<string> GetCoverImagePathsOfAcceptableSizeFrom(string dirPath)
        {
            return GetImageFilePathsFrom(dirPath).Where(ImageIsOfAcceptableSize).ToList();
        }

        public bool IsAcceptableCoverImage(int width, int height, string fileExtension)
        {
            return ImageIsOfAcceptableSize(width, height) && fileExtension == ".jpg";
        }

        private bool ImageIsOfAcceptableSize(int width, int height)
        {
            return width >= _appConstants.MinAcceptableImageDimension && height >= _appConstants.MinAcceptableImageDimension;
        }

        private bool ImageIsOfAcceptableSize(string imagePath)
        {
            using (var img = Image.FromFile(imagePath))
                return ImageIsOfAcceptableSize(img.Width, img.Height);
        }

        public bool SaveCoverImagesForFirstTagIn(ContainerDir dir)
        {
            var mediaFile = dir.FilePaths.FirstOrDefault();
            var saved = false;
            if (mediaFile == null) return saved;
            var counter = 0;
            foreach (var img in GetCoverImagesFrom(mediaFile))
            {
                var fileExtension = GetFileExtensionFor(img.ImageData);
                if (!IsAcceptableCoverImage(img.ImageData.Width, img.ImageData.Height, fileExtension)) 
                    continue;
                var saveAsName = String.Concat(dir.Path, "\\", counter > 0 ? img.Type : _appConstants.CoverImageFilename, fileExtension);
                _fileSystemHelpers.RenameIfExistingFile(saveAsName);
                img.ImageData.Save(saveAsName);
                Log.Info(String.Concat("Extracted a cover image: '", saveAsName, "' from '", Path.GetFileName(mediaFile), "'"));
                saved = true;
                counter++;
            }
            return saved;
        }

        public IEnumerable<string> GetImageFilePathsFrom(string dirPath)
        {
            return Directory.GetFiles(dirPath).Where(fp => fp.IsImageFile());
        }
    }
}