using System;
using System.Linq;
using ToSic.Eav.DataSources.Tokens;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class AssignedEntityAttributePropertyAccess : IPropertyAccess
	{
		private readonly string _name;
		private readonly IMetaDataSource _metaDataSource;
		private readonly Guid _objectToProvideSettingsTo;
		private IEntity _assignedEntity;
		private bool _entityLoaded;

		public string Name { get { return _name; } }

		/// <summary>
		/// Constructs a new AssignedEntity AttributePropertyAccess
		/// </summary>
		/// <param name="name">Name of the PropertyAccess, e.g. pipelinesettings</param>
		/// <param name="objectId">EntityGuid of the Entity to get assigned Entities of</param>
		/// <param name="metaDataSource">DataSource that provides MetaData</param>
		public AssignedEntityAttributePropertyAccess(string name, Guid objectId, IMetaDataSource metaDataSource)
		{
			_name = name;
			_objectToProvideSettingsTo = objectId;
			_metaDataSource = metaDataSource;
		}

		private void LoadEntity()
		{
			var assignedEntities = _metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, _objectToProvideSettingsTo);
			_assignedEntity = assignedEntities.FirstOrDefault(e => e.Type.StaticName != DataSource.DataPipelinePartStaticName);
			_entityLoaded = true;
		}

		/// <summary>
		/// Get Property of AssignedEntity
		/// </summary>
		/// <param name="propertyName">Name of the Property</param>
		/// <param name="propertyNotFound">referenced Bool to set if Property was not found on AssignedEntity</param>
		public string GetProperty(string propertyName, ref bool propertyNotFound)
		{
			if (!_entityLoaded)
				LoadEntity();

			try
			{
				return _assignedEntity[propertyName][0].ToString();
			}
			catch
			{
				propertyNotFound = true;
				return string.Empty;
			}
		}
	}
}