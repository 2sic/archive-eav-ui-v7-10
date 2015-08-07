using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.Persistence
{
    public class DbVersioning
    {
        public EavContext Context { get; internal set; }

        public DbVersioning(EavContext cntx)
        {
            Context = cntx;
        }

        /// <summary>
        /// Creates a ChangeLog immediately
        /// </summary>
        /// <remarks>Also opens the SQL Connection to ensure this ChangeLog is used for Auditing on this SQL Connection</remarks>
        public int GetChangeLogId(string userName)
        {
            if (Context.MainChangeLogId == 0)
            {
                if (Context.Connection.State != ConnectionState.Open)
                    Context.Connection.Open();	// make sure same connection is used later
                Context.MainChangeLogId = Context.AddChangeLog(userName).Single().ChangeID;
            }

            return Context.MainChangeLogId;
        }

        /// <summary>
        /// Creates a ChangeLog immediately
        /// </summary>
        internal int GetChangeLogId()
        {
            return GetChangeLogId(Context.UserName);
        }

        /// <summary>
        /// Set ChangeLog ID on current Context and connection
        /// </summary>
        /// <param name="changeLogId"></param>
        public void SetChangeLogId(int changeLogId)
        {
            if (Context.MainChangeLogId != 0)
                throw new Exception("ChangeLogID was already set");


            Context.Connection.Open();	// make sure same connection is used later
            Context.SetChangeLogIdInternal(changeLogId);
            Context.MainChangeLogId = changeLogId;
        }




        /// <summary>
        /// Persist modified Entity to DataTimeline
        /// </summary>
        internal void SaveEntityToDataTimeline(Entity currentEntity)
        {
            var export = new XmlExport(Context);
            var entityModelSerialized = export.GetEntityXElement(currentEntity.EntityID);
            var timelineItem = new DataTimelineItem
            {
                SourceTable = "ToSIC_EAV_Entities",
                Operation = Constants.DataTimelineEntityStateOperation,
                NewData = entityModelSerialized.ToString(),
                SourceGuid = currentEntity.EntityGUID,
                SourceID = currentEntity.EntityID,
                SysLogID = GetChangeLogId(),
                SysCreatedDate = DateTime.Now
            };
            Context.AddToDataTimeline(timelineItem);

            Context.SaveChanges();
        }

        /// <summary>
        /// Get all Versions of specified EntityId
        /// </summary>
        public DataTable GetEntityVersions(int entityId)
        {
            // get Versions from DataTimeline
            var entityVersions = (from d in Context.DataTimeline
                                  join c in Context.ChangeLogs on d.SysLogID equals c.ChangeID
                                  where d.Operation == Constants.DataTimelineEntityStateOperation && d.SourceID == entityId
                                  orderby c.Timestamp descending
                                  select new { d.SysCreatedDate, c.User, c.ChangeID }).ToList();

            // Generate DataTable with Version-Numbers
            var versionNumber = entityVersions.Count;	// add version number decrement to prevent additional sorting
            var result = new DataTable();
            result.Columns.Add("Timestamp", typeof(DateTime));
            result.Columns.Add("User", typeof(string));
            result.Columns.Add("ChangeId", typeof(int));
            result.Columns.Add("VersionNumber", typeof(int));
            foreach (var version in entityVersions)
                result.Rows.Add(version.SysCreatedDate, version.User, version.ChangeID, versionNumber--);	// decrement versionnumber

            return result;
        }
    }
}
