using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav
{
    public class Constants
    {
        /// <summary>
        /// Name of the Default App in all Zones
        /// </summary>
        public const string DefaultAppName = "Default";
        /// <summary>
        /// Default Entity AssignmentObjectTypeId
        /// </summary>
        public const int DefaultAssignmentObjectTypeId = 1;
        public const string CultureSystemKey = "Culture";
        /// <summary>
        /// DataTimeline Operation-Key for Entity-States (Entity-Versioning)
        /// </summary>
        public const string DataTimelineEntityStateOperation = "s";


        #region DataSource Constants

        /// <summary>
        /// Default ZoneId. Used if none is specified on the Context.
        /// </summary>
        public readonly static int DefaultZoneId = 1;
        /// <summary>
        /// AppId where MetaData (Entities) are stored.
        /// </summary>
        public readonly static int MetaDataAppId = 1;
        /// <summary>
        /// AssignmentObjectTypeId for FieldProperties (Field MetaData)
        /// </summary>
        public readonly static int AssignmentObjectTypeIdFieldProperties = 2;

        /// <summary>
        /// AssignmentObjectTypeId for DataPipelines
        /// </summary>
        public readonly static int AssignmentObjectTypeEntity = 4;


        /// <summary>
        /// StaticName of the DataPipeline AttributeSet
        /// </summary>
        public readonly static string DataPipelineStaticName = "DataPipeline";
        /// <summary>
        /// StaticName of the DataPipelinePart AttributeSet
        /// </summary>
        public readonly static string DataPipelinePartStaticName = "DataPipelinePart";

        /// <summary>
        /// Attribute Name on the Pipeline-Entity describing the Stream-Wiring
        /// </summary>
        public const string DataPipelineStreamWiringStaticName = "StreamWiring";

        /// <summary>
        /// Default In-/Out-Stream Name
        /// </summary>
        public const string DefaultStreamName = "Default";


        #endregion

    }
}
