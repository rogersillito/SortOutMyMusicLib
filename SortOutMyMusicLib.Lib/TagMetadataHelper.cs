using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using log4net;
using TagLib;
using TagLibFile = TagLib.File;

namespace SortOutMyMusicLib.Lib
{
    public interface ITagMetadataHelper
    {
        bool ValidateMetadataIn(ContainerDir directory);
    }

    public class TagMetadataHelper : ITagMetadataHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (TagMetadataHelper));
        private IDictionary<string, HashSet<dynamic>> _consistencyDictionary;
        private bool _isValid;

        public bool ValidateMetadataIn(ContainerDir directory)
        {
            _isValid = false;
            _consistencyDictionary = new Dictionary<string, HashSet<dynamic>>();
            foreach (var path in directory.FilePaths)
            {
                var tLFile = TagLibFile.Create(path);
                var tag = tLFile.Tag;
                var fileName = Path.GetFileName(path);

                SaveConsistencyOf(tag, t => t.Album);
                SaveConsistencyOf(tag, t => t.FirstAlbumArtist);
                SaveConsistencyOf(tag, t => t.Grouping);
                SaveConsistencyOf(tag, t => t.TrackCount);
                SaveConsistencyOf(tag, t => t.DiscCount);

                CheckIsPopulated(tag, fileName, t => t.Track);
                CheckIsPopulated(tag, fileName, t => t.FirstPerformer);
                CheckIsPopulated(tag, fileName, t => t.Title);
                CheckIsPopulated(tag, fileName, t => t.BeatsPerMinute);
                CheckIsPopulated(tag, fileName, t => t.FirstGenre);
                CheckIsPopulated(tag, fileName, t => t.Year);
            }
            CheckConsistency();
            return _isValid;
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
            if (_consistencyDictionary.ContainsKey(tagPropertyName))
                _consistencyDictionary[tagPropertyName].Add(value);
            else
                _consistencyDictionary[tagPropertyName] = new HashSet<dynamic>(new dynamic[] {value});
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