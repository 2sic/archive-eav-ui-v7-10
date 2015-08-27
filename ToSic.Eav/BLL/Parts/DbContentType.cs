using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.BLL.Parts
{
    public class DbContentType: BllCommandBase
    {
        public DbContentType(EavDataController cntx) : base(cntx) {}



        public void AddOrUpdate(string staticName, string name, string description, int? usesConfigurationOfOtherSet = null, bool alwaysShareConfig = false)
        {
            var ct = Context.SqlDb.AttributeSets.FirstOrDefault(a => 
                a.AppID == Context.AppId && a.StaticName == staticName
                );

            if (ct == null)
            {
                Context.SqlDb.AddToAttributeSets(new AttributeSet() {
                    Name = name,
                    AppID = Context.AppId,
                    StaticName = staticName,
                    Description = description,
                    UsesConfigurationOfAttributeSet = usesConfigurationOfOtherSet,
                    AlwaysShareConfiguration = alwaysShareConfig
                });
            }
            else
            {
                ct.Name = name;
                ct.Description = description;
            }

            Context.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Delete an existing App with any Values and Attributes
        /// </summary>
        /// <param name="appId">AppId to delete</param>
        public void Delete(int appId)
        {
            // enure changelog exists and is set to SQL CONTEXT_INFO variable
            if (Context.Versioning.MainChangeLogId == 0)
                Context.Versioning.GetChangeLogId(Context.UserName);

            // Delete app using StoredProcedure
            Context.SqlDb.DeleteAppInternal(appId);

            // Remove App from Global Cache
            // PurgeGlobalCache(Context.ZoneId, Context.AppId);
        }


    }
}
