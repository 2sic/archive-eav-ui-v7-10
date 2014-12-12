using System;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Custom Attribute for DataSources and usage in Pipeline Designer
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class PipelineDesignerAttribute : System.Attribute
	{

	}
}