using System.ComponentModel;

namespace ToSic.Eav
{
	public class EntityDeletingEventArgs : CancelEventArgs
	{
		public int EntityId { get; set; }
		public string CancelMessage { get; set; }

		public override string ToString()
		{
			return "Cancel: " + Cancel + " EntityID: " + EntityId + " CancelMessage: " + CancelMessage;
		}
	}
}
