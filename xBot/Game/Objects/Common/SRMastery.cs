using System.Collections.Specialized;

namespace xBot.Game.Objects.Common
{
	public class SRMastery
	{
		public uint ID { get; }
		public string Name { get; }
		public string Description { get; }
		public byte Level { get; internal set; }

		public SRMastery(uint ID)
		{
			NameValueCollection data = DataManager.GetMastery(ID);

			this.ID = ID;
			if (data != null)
			{
				Name = data["name"] ?? "";
				Description = data["description"] ?? "";
			}
			else
			{
				Name = "Mastery_" + ID;
				Description = "";
			}
		}
	}
}
