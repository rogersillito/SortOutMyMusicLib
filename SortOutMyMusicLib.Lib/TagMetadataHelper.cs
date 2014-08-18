using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private IDictionary<string, HashSet<dynamic>> _consistencyDictionary;

        public bool ValidateMetadataIn(ContainerDir directory)
        {
            _consistencyDictionary = new Dictionary<string, HashSet<dynamic>>();
            foreach (var path in directory.FilePaths)
            {
                var tLFile = TagLibFile.Create(path);
                var tag = tLFile.Tag;
                TrackConsistencyFor(tag, t => t.Album);
                //TODO: handle lists here....!  maybe check first entry only?
                TrackConsistencyFor(tag, t => t.AlbumArtists);
            }
            return false;
        }

        private void TrackConsistencyFor<TTagProp>(Tag tag, Expression<Func<Tag, TTagProp>> tagProperty)
        {
            var value = tag.GetPropertyValue(tagProperty);
            var tagPropertyName = tagProperty.GetPropertyName();
            if (_consistencyDictionary.ContainsKey(tagPropertyName))
                _consistencyDictionary[tagPropertyName].Add(value);
            else
                _consistencyDictionary[tagPropertyName] = new HashSet<dynamic>(new dynamic[] {value});
        }
    }
}