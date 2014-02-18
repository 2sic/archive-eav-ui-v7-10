namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that passes through all In Connections. Can be used con consollidate/merge multiple Sources into one.
	/// </summary>
	public class PassThrough : BaseDataSource
	{
		public override string Name { get { return "PassThrough"; } }

		/// <summary>
		/// Constructs a new PassThrough DataSources
		/// </summary>
		public PassThrough()
		{
			Out = In;
		}
	}
}