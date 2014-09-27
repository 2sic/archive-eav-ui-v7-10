using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Helpers for DataSources
    /// </summary>
    public class Helpers
    {
        #region Helpers for JSON

        private static Dictionary<string, object> GetDictionaryFromEntity(IEntity entity, int dimensionId = 0, bool addEntityId = true, bool addEntityGuid = true)
        {
            var attributes = entity.Attributes.ToDictionary(k => k.Value.Name, v => v.Value[dimensionId]);
            if (addEntityId)
                attributes.Add("EntityId", entity.EntityId);
            if (addEntityGuid)
                attributes.Add("EntityGuid", entity.EntityGuid);
            return attributes;
        }

        /// <summary>
        /// Get Entities in a flat Dictionary that can be used for JSON Serialization
        /// </summary>
        /// <param name="streams">Dictionary with Key = Name of the Stream in Output, Value = an IDataStream or <see cref="IEnumerable{IEntity}"/> or a single IEntity</param>
        /// <param name="addEntityId">Indicates whether Result should contan EntityId</param>
        /// <param name="addEntityGuid">Indicates whether Result should contan EntityGuid</param>
        /// <param name="dimensionId">DimensionId of the values to use</param>
        public static Dictionary<string, object> GetEntitiesForJson(IDictionary<string, object> streams, bool addEntityId = true, bool addEntityGuid = true, int dimensionId = 0)
        {
            var result = new Dictionary<string, object>();
            foreach (var list in streams)
            {
                object entities;

                // Add Entities to Result as a Dictionary
                var stream = list.Value as IDataStream;
                var enumerableEntities = list.Value as IEnumerable<IEntity>;
                var singleEntity = list.Value as IEntity;

                if (stream != null)
                    entities = stream.List.Select(e => GetDictionaryFromEntity(e.Value, addEntityId: addEntityId, addEntityGuid: addEntityGuid, dimensionId: dimensionId));
                else if (enumerableEntities != null)
                    entities = enumerableEntities.Select(e => GetDictionaryFromEntity(e, addEntityId: addEntityId, addEntityGuid: addEntityGuid, dimensionId: dimensionId));
                else if (singleEntity != null)
                    entities = GetDictionaryFromEntity(singleEntity, addEntityId: addEntityId, addEntityGuid: addEntityGuid, dimensionId: dimensionId);
                else
                    entities = list.Value;

                result.Add(list.Key, entities);
            }

            return result;
        }

        #endregion
    }
}