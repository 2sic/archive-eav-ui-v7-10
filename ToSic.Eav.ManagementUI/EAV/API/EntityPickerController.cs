using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ToSic.Eav.ManagementUI.API
{
    public class EntityPickerController : ApiController
    {
        /// <summary>
        /// Returns a list of entities, optionally filtered by AttributeSetId.
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="attributeSetId"></param>
        /// <param name="dimensionId"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<dynamic> GetAvailableEntities(int zoneId, int appId, int? attributeSetId, int? dimensionId)
        {
            var dimensionIds = (dimensionId.HasValue ? dimensionId : 0).Value;

            IContentType contentType = null;
            if (attributeSetId.HasValue)
                contentType = DataSource.GetCache(zoneId, appId).GetContentType(attributeSetId.Value);

            var dsrc = DataSource.GetInitialDataSource(zoneId, appId);
            var entities = (from l in dsrc["Default"].List
                           where contentType == null || l.Value.Type == contentType
                           select new { Value = l.Key, Text = l.Value.Title == null || l.Value.Title[dimensionIds] == null || string.IsNullOrWhiteSpace(l.Value.Title[dimensionIds].ToString()) ? "(no Title, " + l.Key + ")" : l.Value.Title[dimensionIds] }).OrderBy(l => l.Text.ToString()).ToList();

            return entities;
        }

    }
}