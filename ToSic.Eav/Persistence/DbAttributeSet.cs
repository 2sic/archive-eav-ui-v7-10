using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.Persistence
{
    public class DbAttributeSetCommands
    {
        private EavContext Context;

        public DbAttributeSetCommands(EavContext eavDb)
        {
            Context = eavDb;
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

    }
}
