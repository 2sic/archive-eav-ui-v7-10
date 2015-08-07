using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.Persistence
{
    public class DbAttributeSet
    {
        private EavContext cntx;

        public DbAttributeSet(EavContext eavDb)
        {
            cntx = eavDb;
        }

        //private AttributeSet AddAttributeSet(string name, string description, string staticName, string scope, bool autoSave, int? appId)
        //{
        //    if (string.IsNullOrEmpty(staticName))
        //        staticName = Guid.NewGuid().ToString();

        //    var targetAppId = appId.HasValue ? appId.Value : _appId;

        //    // ensure AttributeSet with StaticName doesn't exist on App
        //    if (AttributeSetExists(staticName, targetAppId))
        //        throw new Exception("An AttributeSet with StaticName \"" + staticName + "\" already exists.");

        //    var newSet = new AttributeSet
        //    {
        //        Name = name,
        //        StaticName = staticName,
        //        Description = description,
        //        Scope = scope,
        //        ChangeLogIDCreated = GetChangeLogId(),
        //        AppID = targetAppId
        //    };

        //    AddToAttributeSets(newSet);

        //    if (_contentTypes.ContainsKey(_appId))
        //        _contentTypes.Remove(_appId);

        //    if (autoSave)
        //        cntx.SaveChanges();

        //    return newSet;
        //}

    }
}
