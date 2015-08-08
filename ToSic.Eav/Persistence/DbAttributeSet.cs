using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.Persistence
{
    public class DbAttributeSetCommands : DbExtensionCommandsBase
    {
        public DbAttributeSetCommands(EavContext cntx) : base(cntx)
        {
        }

        /// <summary>
        /// Add a new AttributeSet
        /// </summary>
        public AttributeSet AddAttributeSet(string name, string description, string staticName, string scope, bool autoSave = true)
        {
            return AddAttributeSet(name, description, staticName, scope, autoSave, null);
        }

        internal AttributeSet AddAttributeSet(string name, string description, string staticName, string scope, bool autoSave, int? appId)
        {
            if (string.IsNullOrEmpty(staticName))
                staticName = Guid.NewGuid().ToString();

            var targetAppId = appId.HasValue ? appId.Value : Context.AppId /* _appId*/;

            // ensure AttributeSet with StaticName doesn't exist on App
            if (Context.DbS.AttributeSetExists(staticName, targetAppId))
                throw new Exception("An AttributeSet with StaticName \"" + staticName + "\" already exists.");

            var newSet = new AttributeSet
            {
                Name = name,
                StaticName = staticName,
                Description = description,
                Scope = scope,
                ChangeLogIDCreated = Context.Versioning.GetChangeLogId(),
                AppID = targetAppId
            };

            Context.AddToAttributeSets(newSet);

            if (Context._contentTypes.ContainsKey(Context.AppId /* _appId*/))
                Context._contentTypes.Remove(Context.AppId /* _appId*/);

            if (autoSave)
                Context.SaveChanges();

            return newSet;
        }


        /// <summary>
        /// Remove an Attribute from an AttributeSet and delete values
        /// </summary>
        public void RemoveAttributeInSet(int attributeId, int attributeSetId)
        {
            // Delete the AttributeInSet
            Context.DeleteObject(Context.AttributesInSets.Single(a => a.AttributeID == attributeId && a.AttributeSetID == attributeSetId));

            // Delete all Values an their ValueDimensions
            var valuesToDelete = Context.Values.Where(v => v.AttributeID == attributeId && v.Entity.AttributeSetID == attributeSetId).ToList();
            foreach (var valueToDelete in valuesToDelete)
            {
                valueToDelete.ValuesDimensions.ToList().ForEach(Context.DeleteObject);
                Context.DeleteObject(valueToDelete);
            }

            // Delete all Entity-Relationships
            var relationshipsToDelete = Context.EntityRelationships.Where(r => r.AttributeID == attributeId).ToList(); // No Filter by AttributeSetID is needed here at the moment because attribute can't be in multiple sets currently
            relationshipsToDelete.ForEach(Context.DeleteObject);

            Context.SaveChanges();
        }
    }
}
