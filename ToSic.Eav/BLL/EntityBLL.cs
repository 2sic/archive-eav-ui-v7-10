using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.BLL
{
    public class EntityBLL
    {

        // todo: refactor - this uses the complex model, maybe shouldn't be here?
        // I'm guessing this is an old code - and probably not needed any more??? 

        /// <summary>
        /// Test whehter Entity can be deleted safe if it has no relationships
        /// </summary>
        /// <returns>Item1: Indicates whether Entity can be deleted. Item2: Messages why Entity can't be deleted safe.</returns>
        public Tuple<bool, string> CanDeleteEntity(EavContext Context, int entityId)
        {
            var messages = new List<string>();
            var entityModel = new DbLoadIntoEavDataStructure(Context).GetEavEntity(entityId);

            if (!entityModel.IsPublished && entityModel.GetPublished() == null)	// allow Deleting Draft-Only Entity always
                return new Tuple<bool, string>(true, null);

            var entityChild = Context.EntityRelationships.Where(r => r.ChildEntityID == entityId).Select(r => r.ParentEntityID).ToList();
            if (entityChild.Any())
                messages.Add(string.Format("Entity has {0} Child-Relationships to Entities: {1}.", entityChild.Count, string.Join(", ", entityChild)));

            var assignedEntitiesFieldProperties = Context.DbS.GetEntitiesInternal(Constants.AssignmentObjectTypeIdFieldProperties, entityId).Select(e => e.EntityID).ToList();
            if (assignedEntitiesFieldProperties.Any())
                messages.Add(string.Format("Entity has {0} assigned Field-Property-Entities: {1}.", assignedEntitiesFieldProperties.Count, string.Join(", ", assignedEntitiesFieldProperties)));

            var assignedEntitiesDataPipeline = Context.DbS.GetEntitiesInternal(Constants.AssignmentObjectTypeEntity, entityId).Select(e => e.EntityID).ToList();
            if (assignedEntitiesDataPipeline.Any())
                messages.Add(string.Format("Entity has {0} assigned Data-Pipeline Entities: {1}.", assignedEntitiesDataPipeline.Count, string.Join(", ", assignedEntitiesDataPipeline)));

            return Tuple.Create(!messages.Any(), string.Join(" ", messages));
        }
    }
}
