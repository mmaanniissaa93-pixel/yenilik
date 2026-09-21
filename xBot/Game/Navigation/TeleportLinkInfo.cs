using xBot.Game.Objects.Common;
namespace xBot.Game.Navigation
{
	public enum TransitionMode { Interaction, WalkTrigger }

	public class TeleportLinkInfo
	{
		public uint SourceId { get; set; }
		public uint DestinationId { get; set; }
		public uint NpcId { get; set; }
		public string SourceName { get; set; }
		public string DestinationName { get; set; }
		public SRCoord BoardCoord { get; set; }
		// Optional point just beyond a walk-trigger's database spawn.  It is kept
		// separate from BoardCoord so route planning can still use the server's
		// reachable spawn while the final trigger walk can continue through the
		// visible doorway.
		public SRCoord TriggerCoord { get; set; }
		public SRCoord ArriveCoord { get; set; }
		public bool IsFerry { get; set; }
        public TransitionMode TransitionMode { get; set; }
        public string ServerName { get; set; }
        public int TypeId1 { get; set; }
        public int Gold { get; set; }
        public int MinimumLevel { get; set; }
        public bool HasEntityModel { get; set; }

		public override string ToString()
		{
			return $"[{SourceName} -> {DestinationName}] Board:({(int)BoardCoord.PosX},{(int)BoardCoord.PosY}) Arrive:({(int)ArriveCoord.PosX},{(int)ArriveCoord.PosY})";
		}
	}

}
