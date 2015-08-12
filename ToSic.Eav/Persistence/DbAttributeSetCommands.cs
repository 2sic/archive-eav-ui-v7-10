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
        /// Get a List of all AttributeSets
        /// </summary>
        public List<AttributeSet> GetAllAttributeSets()
        {
            return Context.AttributeSets.Where(a => a.AppID == Context.AppId).ToList();
        }

        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public AttributeSet GetAttributeSet(int attributeSetId)
        {
            return Context.AttributeSets.SingleOrDefault(a => a.AttributeSetID == attributeSetId && a.AppID == Context.AppId && !a.ChangeLogIDDeleted.HasValue);
        }
        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public AttributeSet GetAttributeSet(string staticName)
        {
            return Context.AttributeSets.SingleOrDefault(a => a.StaticName == staticName && a.AppID == Context.AppId && !a.ChangeLogIDDeleted.HasValue);
        }



        /// <summary>
        /// Get AttributeSetId by StaticName and Scope
        /// </summary>
        /// <param name="staticName">StaticName of the AttributeSet</param>
        /// <param name="scope">Optional Filter by Scope</param>
        /// <returns>AttributeSetId or Exception</returns>
        public int GetAttributeSetId(string staticName, AttributeScope? scope)
        {
            var scopeFilter = scope.HasValue ? scope.ToString() : null;

            try
            {
                return Context.AttributeSets.Single(s => s.AppID == Context.AppId /*_appId*/  && s.StaticName == staticName && (s.Scope == scopeFilter || scopeFilter == null)).AttributeSetID;
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Unable to get AttributeSet with StaticName \"" + staticName + "\" in Scope \"" + scopeFilter + "\".", ex);
            }
        }

        /// <summary>
        /// if AttributeSet refers another AttributeSet, get ID of the refered AttributeSet. Otherwise returns passed AttributeSetId.
        /// </summary>
        /// <param name="attributeSetId">AttributeSetId to resolve</param>
        internal int ResolveAttributeSetId(int attributeSetId)
        {
            var usesConfigurationOfAttributeSet = Context.AttributeSets.Where(a => a.AttributeSetID == attributeSetId).Select(a => a.UsesConfigurationOfAttributeSet).Single();
            return usesConfigurationOfAttributeSet.HasValue ? usesConfigurationOfAttributeSet.Value : attributeSetId;
        }

        /// <summary>
        /// Test whether AttributeSet exists on specified App and is not deleted
        /// </summary>
        public bool AttributeSetExists(string staticName, int appId)
        {
            return Context.AttributeSets.Any(a => !a.ChangeLogIDDeleted.HasValue && a.AppID == appId && a.StaticName == staticName);
        }



        /// <summary>
        /// Get AttributeSets
        /// </summary>
        /// <param name="appId">Filter by AppId</param>
        /// <param name="scope">optional Filter by Scope</param>
        internal IQueryable<AttributeSet> GetAttributeSets(int appId, AttributeScope? scope)
        {
            var result = Context.AttributeSets.Where(a => a.AppID == appId && !a.ChangeLogIDDeleted.HasValue);

            if (scope != null)
            {
                var scopeString = scope.ToString();
                result = result.Where(a => a.Scope == scopeString);
            }

            return result;
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on specified App. App must be saved and have an AppId
        /// </summary>
        internal void EnsureSharedAttributeSets(App app, bool autoSave = true)
        {
            if (app.AppID == 0)
                throw new Exception("App must have a valid AppID");

            // todo: bad - don't want data-sources here
            var sharedAttributeSets = GetAttributeSets(Constants.MetaDataAppId, null).Where(a => a.AlwaysShareConfiguration);
            foreach (var sharedSet in sharedAttributeSets)
            {
                // Skip if attributeSet with StaticName already exists
                if (app.AttributeSets.Any(a => a.StaticName == sharedSet.StaticName && !a.ChangeLogIDDeleted.HasValue))
                    continue;

                // create new AttributeSet
                var newAttributeSet = AddAttributeSet(sharedSet.Name, sharedSet.Description, sharedSet.StaticName, sharedSet.Scope, false, app.AppID);
                newAttributeSet.UsesConfigurationOfAttributeSet = sharedSet.AttributeSetID;
            }

            // Ensure new AttributeSets are created and cache is refreshed
            if (autoSave)
                Context.SaveChanges();
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on all Apps an Zones
        /// </summary>
        internal void EnsureSharedAttributeSets()
        {
            foreach (var app in Context.Apps)
                EnsureSharedAttributeSets(app, false);

            Context.SaveChanges();
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
            if (Context.AttSetCommands.AttributeSetExists(staticName, targetAppId))
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
