using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for Content in the EAV
	/// </summary>
	public class EavContentController : WebApi
    {
        /// <summary>
        /// Get an entity with all languages - for editing the content in a UI or similar
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public Formats.EntityWithLanguages GetOne(int appId, string contentType, int id, string format = "multi-language")
        {
            switch (format)
            {
                case "multi-language":
                    var found = GetEntityOrThrowError(contentType, id, appId);
                    Formats.EntityWithLanguages ce = new Formats.EntityWithLanguages()
                    {
                        Id = found.EntityId,
                        Guid = found.EntityGuid,
                        Type = new Formats.Type() { Name = found.Type.Name, StaticName = found.Type.StaticName },
                        TitleAttributeName = found.Title.Name,
                        Attributes = found.Attributes.Select(a => new Formats.Attribute()
                        {
                            Key = a.Key,
                            Values = a.Value.Values.Select(v => new Formats.ValueSet()
                            {
                                Value = v.Serialized,
                                Dimensions = v.Languages.ToDictionary(l => l.Key, y => y.ReadOnly)
                            }).ToArray()
                        }).ToArray()
                    };
                    return ce;
                default:
                    throw new Exception("format: " + format + " unknown");
            }
        }



    }
}