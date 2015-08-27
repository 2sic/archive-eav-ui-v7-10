using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.BLL.Parts
{
    public class DbContentType: BllCommandBase
    {
        public DbContentType(EavDataController cntx) : base(cntx) {}



        public void AddOrUpdate(string staticName, string scope, string name, string description, int? usesConfigurationOfOtherSet = null, bool alwaysShareConfig = false)
        {
            var ct = GetAttributeSetByStaticName(staticName);

            if (ct == null)
            {
                ct = new AttributeSet()
                {
                    AppID = Context.AppId,
                    StaticName = Guid.NewGuid().ToString(),// staticName,
                    Scope = scope == "" ? null : scope,
                    UsesConfigurationOfAttributeSet = usesConfigurationOfOtherSet,
                    AlwaysShareConfiguration = alwaysShareConfig
                };
                Context.SqlDb.AddToAttributeSets(ct);
            }

            ct.Name = name;
            ct.Description = description;
            ct.ChangeLogIDCreated = Context.Versioning.GetChangeLogId(Context.UserName);

            Context.SqlDb.SaveChanges();
        }

        private AttributeSet GetAttributeSetByStaticName(string staticName)
        {
            return Context.SqlDb.AttributeSets.FirstOrDefault(a =>
                a.AppID == Context.AppId && a.StaticName == staticName
                );
        }

        public void Delete(string staticName)
        {
            var setToDelete = GetAttributeSetByStaticName(staticName);

            setToDelete.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId(Context.UserName);
            Context.SqlDb.SaveChanges();
        }


    }
}
