using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xBot.Game.Objects.Entity
{
	public class SRMob:SRNpc
	{
		public Mob MobType { get; set; }
		public byte Appearence { get; internal set; }
		public byte Level { get; set; }

		public SRMob(uint ID) : base(ID)
		{
			byte lvl;
			if (m_data != null && byte.TryParse(m_data["level"], out lvl))
				Level = lvl;
		}
		public SRMob(string ServerName) : base(ServerName)
		{
			byte lvl;
			if (m_data != null && byte.TryParse(m_data["level"], out lvl))
				Level = lvl;
		}
		public SRMob(SRNpc value) : base(value)
		{
			byte lvl;
			if (m_data != null && byte.TryParse(m_data["level"], out lvl))
				Level = lvl;
		}


		public enum Mob : byte
		{
			General = 0,
			Champion = 1,
			Giant = 4,
			Titan = 5,
			Strong = 6,
			Elite = 7,
			Unique = 8,
			PartyGeneral = 0x10,
			PartyChampion = 0x11,
			PartyGiant = 0x14,
			Event = 0xFF
		}
	}
}
