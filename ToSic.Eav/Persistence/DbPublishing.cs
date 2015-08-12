using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.BLL;

namespace ToSic.Eav.Persistence
{
    public class DbPublishing: BllCommandBase
    {
        //public DbPublishing(EavDataController dc) : base(dc) { }
        public DbPublishing(EavDataController c) : base(c) { }


        /// <summary>
        /// Publish a Draft Entity
        /// </summary>
        /// <param name="entityId">ID of the Draft-Entity</param>
        public Entity PublishEntity(int entityId)
        {
            return PublishEntity(entityId, true);
        }

        /// <summary>
        /// Publish a Draft-Entity
        /// </summary>
        /// <param name="entityId">ID of the Draft-Entity</param>
        /// <param name="autoSave">Call SaveChanges() automatically?</param>
        /// <returns>The published Entity</returns>
        internal Entity PublishEntity(int entityId, bool autoSave = true)
        {
            var unpublishedEntity = Context.DbS.GetEntity(entityId);
            if (unpublishedEntity.IsPublished)
                throw new InvalidOperationException(string.Format("EntityId {0} is already published", entityId));

            Entity publishedEntity;

            // Publish Draft-Entity
            if (!unpublishedEntity.PublishedEntityId.HasValue)
            {
                unpublishedEntity.IsPublished = true;
                publishedEntity = unpublishedEntity;
            }
            // Replace currently published Entity with draft Entity and delete the draft
            else
            {
                publishedEntity = Context.DbS.GetEntity(unpublishedEntity.PublishedEntityId.Value);
                Context.ValCommands.CloneEntityValues(unpublishedEntity, publishedEntity);

                // delete the Draft Entity
                Context.EntCommands.DeleteEntity(unpublishedEntity, false);
            }

            if (autoSave)
                Context.SqlDb.SaveChanges();

            return publishedEntity;
        }
    }
}
