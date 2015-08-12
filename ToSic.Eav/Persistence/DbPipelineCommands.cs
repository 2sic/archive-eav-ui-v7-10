using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Persistence
{
    public class DbPipelineCommands
    {
        /// <summary>
        /// Copy an existing DataPipeline by copying all Entities and uptdate their GUIDs
        /// </summary>
        public static Entity CopyDataPipeline(int appId, int pipelineEntityId, string userName)
        {
            var ctx = EavDataController.Instance(appId: appId);
            var dbs = new DbShortcuts(ctx);
            ctx.UserName = userName;

            // Clone Pipeline Entity with a new new Guid
            var sourcePipelineEntity = dbs.GetEntity(pipelineEntityId);
            if (sourcePipelineEntity.Set.StaticName != Constants.DataPipelineStaticName) //PipelineAttributeSetStaticName)
                throw new ArgumentException("Entity is not an DataPipeline Entity", "pipelineEntityId");
            var pipelineEntityClone = ctx.EntCommands.CloneEntity(sourcePipelineEntity, true);

            // Copy Pipeline Parts with configuration Entity, assign KeyGuid of the new Pipeline Entity
            var pipelineParts = dbs.GetEntities(Constants.AssignmentObjectTypeEntity, sourcePipelineEntity.EntityGUID);
            var pipelinePartClones = new Dictionary<string, Guid>();	// track Guids of originals and their clone
            foreach (var pipelinePart in pipelineParts)
            {
                var pipelinePartClone = ctx.EntCommands.CloneEntity(pipelinePart, true);
                pipelinePartClone.KeyGuid = pipelineEntityClone.EntityGUID;
                pipelinePartClones.Add(pipelinePart.EntityGUID.ToString(), pipelinePartClone.EntityGUID);

                // Copy Configuration Entity, assign KeyGuid of the Clone
                var configurationEntity = dbs.GetEntities(Constants.AssignmentObjectTypeEntity, pipelinePart.EntityGUID).SingleOrDefault();
                if (configurationEntity != null)
                {
                    var configurationClone = ctx.EntCommands.CloneEntity(configurationEntity, true);
                    configurationClone.KeyGuid = pipelinePartClone.EntityGUID;
                }
            }

            #region Update Stream-Wirings

            var streamWiring = pipelineEntityClone.Values.Single(v => v.Attribute.StaticName == Constants.DataPipelineStreamWiringStaticName);// StreamWiringAttributeName);
            var wiringsClone = new List<WireInfo>();
            var wiringsSource = DataPipelineWiring.Deserialize(streamWiring.Value);
            if (wiringsSource != null)
            {
                foreach (var wireInfo in wiringsSource)
                {
                    var wireInfoClone = wireInfo; // creates a clone of the Struct
                    if (pipelinePartClones.ContainsKey(wireInfo.From))
                        wireInfoClone.From = pipelinePartClones[wireInfo.From].ToString();
                    if (pipelinePartClones.ContainsKey(wireInfo.To))
                        wireInfoClone.To = pipelinePartClones[wireInfo.To].ToString();

                    wiringsClone.Add(wireInfoClone);
                }
            }

            streamWiring.Value = DataPipelineWiring.Serialize(wiringsClone);
            #endregion

            ctx.SqlDb.SaveChanges();

            return pipelineEntityClone;
        }
    }
}