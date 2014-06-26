using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav
{
	public partial class EavContext
	{
		private static List<Dimension> _cachedDimensions;

		//ToDo: Move to IEavConfiguration (not at the moment 28.3.2013)
		#region Cached Dimensions

		private void EnsureDimensionsCache()
		{
			if (_cachedDimensions == null)
				_cachedDimensions = Dimensions.ToList();
		}

		/// <summary>
		/// Clear DimensionsCache in current Application Cache
		/// </summary>
		public void ClearDimensionsCache()
		{
			_cachedDimensions = null;
		}

		#endregion
	}
}