using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using log4net;
using TagLib;
using TagLibFile = TagLib.File;

namespace SortOutMyMusicLib.Lib
{
    public interface ITagMetadataHelper
    {
        void ValidateMetadataIn(ContainerDir directory, IssueLog issues);
    }

    public class TagMetadataHelper : ITagMetadataHelper
    {
        //TODO: log creator... http://stackoverflow.com/questions/16796690/c-sharp-static-readonly-log4net-logger-any-way-to-change-logger-in-unit-test
        private static readonly ILog Log = LogManager.GetLogger(typeof (TagMetadataHelper));
        private IDictionary<string, HashSet<dynamic>> _consistencyDictionary;
        private bool _isValid;

        public void ValidateMetadataIn(ContainerDir directory, IssueLog issues)
        {
            _isValid = false;
            _consistencyDictionary = new Dictionary<string, HashSet<dynamic>>();
            foreach (var file in directory.Files)
            {
                var tLFile = TagLibFile.Create(file.Path);
                var tag = tLFile.Tag;
                SaveConsistencyOfPropertiesIn(tag);
                CheckRequiredFieldsArePopulatedIn(tag, file.Name);
                CheckCoverImagesIn(file);
                tLFile.Dispose();
            }
            CheckConsistency();
            issues.MetadataNeedsFixing = !_isValid;
        }

        private void CheckCoverImagesIn(MediaFile file)
        {
            //TODO: check first image is a FrontCover of acceptable size
            //TODO: and then check we don't have multiple FrontCovers
            //TODO: if NO FrontCover, try to insert 1 from FolderImagePath at position 0
            //TODO: when STILL NO front cover - log FrontCover missing
            //var frontCovers = file.Images.Where(im => im.Type == PictureType.FrontCover).ToList();
            //if (frontCovers.Count > 1)
            //{
            //    _isValid = false;
            //    Log.WarnFormat("Multiple Front cover count in tag ({0})", frontCovers.Count);
            //}
            //foreach (var coverImg in frontCovers)
            //{
            //    if (coverImg.IsAcceptableSize)
            //    {
            //        AddToConsistencyDictionary("FrontCover", coverImg.CheckSum);            
            //    }
            //    else
            //    {
            //        _isValid = false;
            //        Log.WarnFormat("Front cover in tag of unacceptable dimensions ({0}x{1})", coverImg.ImageData.Width, coverImg.ImageData.Height);
            //    }
            //}
        }

        private void CheckRequiredFieldsArePopulatedIn(Tag tag, string fileName)
        {
            CheckIsPopulated(tag, fileName, t => t.Track);
            CheckIsPopulated(tag, fileName, t => t.FirstPerformer);
            CheckIsPopulated(tag, fileName, t => t.Title);
            CheckIsPopulated(tag, fileName, t => t.BeatsPerMinute);
            CheckIsPopulated(tag, fileName, t => t.FirstGenre);
            CheckIsPopulated(tag, fileName, t => t.Year);
        }

        private void SaveConsistencyOfPropertiesIn(Tag tag)
        {
            SaveConsistencyOf(tag, t => t.Album);
            SaveConsistencyOf(tag, t => t.FirstAlbumArtist);
            SaveConsistencyOf(tag, t => t.Grouping);
            SaveConsistencyOf(tag, t => t.TrackCount);
            SaveConsistencyOf(tag, t => t.DiscCount);
        }

        private void CheckConsistency()
        {
            foreach (var inconsistentProperty in _consistencyDictionary.Select(cd => new { Name = cd.Key, DistinctCount = cd.Value.Count }).Where(p => p.DistinctCount > 1))
            {
                Log.WarnFormat("Values of '{0}' tag property are inconsistent", inconsistentProperty.Name);
                _isValid = false;
            }
        }

        private void SaveConsistencyOf<TTagProp>(Tag tag, Expression<Func<Tag, TTagProp>> tagProperty)
        {
            var value = tag.GetPropertyValue(tagProperty);
            var tagPropertyName = tagProperty.GetPropertyName();
            AddToConsistencyDictionary(tagPropertyName, value);
        }

        private void AddToConsistencyDictionary(string propertyName, object value)
        {
            if (_consistencyDictionary.ContainsKey(propertyName))
                _consistencyDictionary[propertyName].Add(value);
            else
                _consistencyDictionary[propertyName] = new HashSet<dynamic>(new dynamic[] {value});
        }

        private void CheckIsPopulated(Tag tag, string fileName, Expression<Func<Tag, string>> tagProperty)
        {
            var value = tag.GetPropertyValue(tagProperty);
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                return;
            Log.WarnFormat("{1,16} tag is empty \"{0}\"", fileName, tagProperty.GetPropertyName());
            _isValid = false;
        }

        private void CheckIsPopulated(Tag tag, string fileName, Expression<Func<Tag, uint>> tagProperty)
        {
            var value = tag.GetPropertyValue(tagProperty);
            if (value != null && (uint) value != 0)
                return;
            Log.WarnFormat("{1,16} tag is 0 \"{0}\"", fileName, tagProperty.GetPropertyName());
            _isValid = false;
        }
    }
}