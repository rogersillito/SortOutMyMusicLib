using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using log4net;
using TagLib;
using TagLibFile = TagLib.File;

namespace SortOutMyMusicLib.Lib
{
    public interface IImageHelpers
    {
        void SetImagesFor(ContainerDir dir);
        string GetFileExtensionFor(Image image);
        IEnumerable<CoverImage> GetCoverImagesFrom(string mediaFilePath);
        IList<string> GetFolderImagePathsOfAcceptableSizeFrom(string dirPath);
        void TrySaveFolderImageFromAMediaFileIn(ContainerDir dir);
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

        public void SetImagesFor(ContainerDir dir)
        {
            foreach (var file in dir.Files)
            {
                file.Images = GetCoverImagesFrom(file.Path);
            }
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
                var picture = tagLibFile.Tag.Pictures[i];
                var image = (Image)ic.ConvertFrom(picture.Data.Data);
                yield return new CoverImage 
                {
                    ImageData = image, 
                    Type = picture.Type,
                    IsAcceptableSize = ImageIsOfAcceptableSize(image.Width, image.Height), 
                    FileExtension = GetFileExtensionFor(image),
                    CheckSum = picture.Data.Checksum
                };
            }
            tagLibFile.Dispose();
        }

        public IList<string> GetFolderImagePathsOfAcceptableSizeFrom(string dirPath)
        {
            return GetImageFilePathsFrom(dirPath).Where(ImageIsOfAcceptableSize).ToList();
        }

        public bool ImageIsOfAcceptableSize(int width, int height)
        {
            return width >= _appConstants.MinAcceptableImageDimension && height >= _appConstants.MinAcceptableImageDimension;
        }

        private bool ImageIsOfAcceptableSize(string imagePath)
        {
            using (var img = Image.FromFile(imagePath))
                return ImageIsOfAcceptableSize(img.Width, img.Height);
        }

        public void TrySaveFolderImageFromAMediaFileIn(ContainerDir dir)
        {
            var acceptableFolderImage = ExtractFirstAcceptableFolderImageFrom(dir);
            if (acceptableFolderImage == null) return;
            var saveAsPath = String.Concat(dir.Path, "\\", _appConstants.FolderImageFilename);
            _fileSystemHelpers.RenameIfThereIsAnExistingFileAt(saveAsPath);
            acceptableFolderImage.ImageData.Save(saveAsPath);
            dir.FolderImagePath = saveAsPath;
            Log.Info(String.Concat("Extracted '", _appConstants.FolderImageFilename, "' from a media file."));
        }

        private CoverImage ExtractFirstAcceptableFolderImageFrom(ContainerDir dir)
        {
            var allImages = dir.Files
                .OrderBy(f => f.Name)
                .SelectMany(mf => mf.Images);
            return allImages.FirstOrDefault(im => im.IsAcceptableSize
                                               && im.Type == PictureType.FrontCover
                                               && GetFileExtensionFor(im.ImageData) == ".jpg");
        }

        public IEnumerable<string> GetImageFilePathsFrom(string dirPath)
        {
            return Directory.GetFiles(dirPath).Where(fp => fp.IsImageFile());
        }
    }
}