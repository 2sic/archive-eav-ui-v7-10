using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Serializers
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    public class Serializer
    {
        #region Language

        private string _Language = "";

        private string Language
        {
            get
            {
                if(_Language == "")
                    _Language = Thread.CurrentThread.CurrentCulture.Name;
                return _Language;
            }
        }

        #endregion
        #region Many variations of the Prepare-Statement expecting various kinds of input
        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Prepare(IDataSource source, IEnumerable<string> streamsToPublish = null)
        {
            if (streamsToPublish == null)
                streamsToPublish = source.Out.Select(p => p.Key);

            var y = streamsToPublish.Where(k => source.Out.ContainsKey(k))
                .ToDictionary(k => k, s => source.Out[s].List.Select(c => GetDictionaryFromEntity(c.Value, Language))
            );

            return y;
        }

        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Prepare(IDataSource source, string streamsToPublish)
        {
            return Prepare(source, streamsToPublish.Split(','));
        }

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public IEnumerable<Dictionary<string, object>> Prepare(IDataStream stream)
        {
            return Prepare(stream.List);  
        }

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public IEnumerable<Dictionary<string, object>> Prepare(IDictionary<int, IEntity> list)
        {
            return list.Select(c => GetDictionaryFromEntity(c.Value, Language));
        }


        public IEnumerable<Dictionary<string, object>> Prepare(IEnumerable<IEntity> entities)
        {
            return entities.Select(c => GetDictionaryFromEntity(c, Language));
        }

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public Dictionary<string, object> Prepare(IEntity entity)
        {
            return GetDictionaryFromEntity(entity, Language);
        }

        #endregion


        public virtual Dictionary<string, object> GetDictionaryFromEntity(IEntity entity, string language)
        {
            var dynamicEntity = entity;//new DynamicEntity(entity, new[] { language }, Sxc);

            // Convert DynamicEntity to dictionary
            var dictionary = (from d in entity.Attributes select d.Value).ToDictionary(k => k.Name, v =>
            {
                // bool propertyNotFound;
                var value = dynamicEntity.GetBestValue(v.Name);// .GetEntityValue(v.Name, out propertyNotFound);
                if (v.Type == "Entity" && value is ToSic.Eav.Data.EntityRelationship)
                    return ((Data.EntityRelationship)value).Select(p => new { Id = p.EntityId, p.Title });
                return value;
            }, StringComparer.OrdinalIgnoreCase);

            // If ID is not used by the entity itself as an internal value, give the object a Id property as well since it's nicer to use in JS
            // Note that for editing purposes or similar, there is always the extended info-object, so this is purely for "normal" working with the data
            dictionary.Add((dictionary.ContainsKey("Id") ? "EntityId" : "Id"), entity.EntityId);
            if (!dictionary.ContainsKey("Title"))
                dictionary.Add("Title", entity.GetBestValue("EntityTitle"));

            return dictionary;
        }
    }
}