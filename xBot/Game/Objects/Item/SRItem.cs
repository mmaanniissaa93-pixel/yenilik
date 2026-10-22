using System;
using System.Collections.Specialized;
using System.Text;
using xBot.Game.Objects.Common;

namespace xBot.Game.Objects.Item
{
	public class SRItem
	{
		#region (Properties)
		public uint ID { get; protected set; }
		public string ServerName { get; protected set; }
		public string Name { get; protected set; }
		public byte ID1 { get; protected set; }
		public byte ID2 { get; protected set; }
		public byte ID3 { get; protected set; }
		public byte ID4 { get; protected set; }
		public string Icon { get; set; }
		public ushort QuantityMax { get; set; }
		public ushort Quantity { get; set; }
		public byte LevelRequired { get; set; }
		public SRRentable Rentable { get; internal set; }
		#endregion

		#region (Constructor)
		private SRItem(uint ID)
		{
			NameValueCollection data = DataManager.GetItemData(ID);

			this.ID = ID;
			if (data != null)
			{
				ServerName = data["servername"] ?? ("ITEM_UNKNOWN_" + ID);
				string rawName = data["name"];
				Name = !string.IsNullOrWhiteSpace(rawName) ? rawName : ServerName;
				ID1 = 3;
				byte b;
				ID2 = byte.TryParse(data["tid2"], out b) ? b : (byte)0;
				ID3 = byte.TryParse(data["tid3"], out b) ? b : (byte)0;
				ID4 = byte.TryParse(data["tid4"], out b) ? b : (byte)0;

				Icon = data["icon"] ?? "";
				Quantity = 1;
				ushort s;
				QuantityMax = ushort.TryParse(data["stack"], out s) ? (s > 0 ? s : (ushort)1) : (ushort)1;
				LevelRequired = byte.TryParse(data["level"], out b) ? b : (byte)0;
			}
			else
			{
				ServerName = "ITEM_UNKNOWN_" + ID;
				Name = "Unknown Item (" + ID + ")";
				ID1 = 3;
				ID2 = 0;
				ID3 = 0;
				ID4 = 0;
				Icon = "";
				Quantity = 1;
				QuantityMax = 1;
				LevelRequired = 0;
			}
		}
		private SRItem(string ServerName)
		{
			NameValueCollection data = DataManager.GetItemData(ServerName);

			this.ServerName = ServerName;
			if (data != null)
			{
				uint idVal;
				this.ID = uint.TryParse(data["id"], out idVal) ? idVal : 0;
				string rawName2 = data["name"];
				Name = !string.IsNullOrWhiteSpace(rawName2) ? rawName2 : ServerName;
				ID1 = 3;
				byte b;
				ID2 = byte.TryParse(data["tid2"], out b) ? b : (byte)0;
				ID3 = byte.TryParse(data["tid3"], out b) ? b : (byte)0;
				ID4 = byte.TryParse(data["tid4"], out b) ? b : (byte)0;

				Icon = data["icon"] ?? "";
				Quantity = 1;
				ushort s;
				QuantityMax = ushort.TryParse(data["stack"], out s) ? (s > 0 ? s : (ushort)1) : (ushort)1;
				LevelRequired = byte.TryParse(data["level"], out b) ? b : (byte)0;
			}
			else
			{
				this.ID = 0;
				Name = ServerName;
				ID1 = 3;
				ID2 = 0;
				ID3 = 0;
				ID4 = 0;
				Icon = "";
				Quantity = 1;
				QuantityMax = 1;
				LevelRequired = 0;
			}
		}
		protected SRItem(SRItem value)
		{
			ID = value.ID;
			ServerName = value.ServerName;
			Name = value.Name;
			ID1 = value.ID1;
			ID2 = value.ID2;
			ID3 = value.ID3;
			ID4 = value.ID4;

			Icon = value.Icon;
			Quantity = value.Quantity;
			QuantityMax = value.QuantityMax;
			LevelRequired = value.LevelRequired;
			Rentable = value.Rentable;
		}
		#endregion

		#region (Methods)
		public static SRItem Create(uint ID,SRRentable Rentable)
		{
			SRItem item = new SRItem(ID);
			item.Rentable = Rentable;
			if (item.isEquipable())
			{
				SREquipable equipable = new SREquipable(item);
				item = equipable;
			}
			else if (item.isCoS())
			{
				SRCoS cos = new SRCoS(item);
				item = cos;
			}
			else if (item.isEtc())
			{
				SREtc etc = new SREtc(item);
				item = etc;
			}
			return item;
		}
		public static SRItem Create(string ServerName, SRRentable Rentable = null)
		{
			SRItem item = new SRItem(ServerName);
			if (Rentable == null)
				Rentable = new SRRentable(0);
			item.Rentable = Rentable;
			if (item.isEquipable())
			{
				SREquipable equipable = new SREquipable(item);
				item = equipable;
			}
			else if (item.isCoS())
			{
				SRCoS cos = new SRCoS(item);
				item = cos;
			}
			else if (item.isEtc())
			{
				SREtc etc = new SREtc(item);
				item = etc;
			}
			return item;
		}
		public bool isEquipable()
		{
			return ID2 == 1;
		}
		public bool isCoS()
		{
			return ID2 == 2;
		}
		public bool isEtc()
		{
			return ID2 == 3;
		}
		public ushort GetUsageType()
		{
			uint rentableId = Rentable != null ? Rentable.ID : 0;
			return (ushort)((ushort)rentableId | ID1 << 2 | ID2 << 5 | ID3 << 7 | ID4 << 11);
		}
		/// <summary>
		/// Silkroad-R (v164 / Elysian) ve modern Joymax sunucularda 32-bit m_sTID / m_dwTypeID kullanım tipi.
		/// Bits 0..3: RentableId, Bits 4..9: TID2, Bits 10..15: TID1, Bits 16..23: TID3, Bits 24..31: TID4.
		/// Gerçek client'ın (sro_client.exe) 0x704C paketlerinde gönderdiği bit dizilimi ile birebir eşleşir.
		/// </summary>
		public uint GetUsageType32()
		{
			uint rentableId = Rentable != null ? Rentable.ID : 0;
			return (uint)(
				(rentableId & 0x03) |
				((uint)(ID2 & 0x3F) << 4) |
				((uint)(ID1 & 0x3F) << 10) |
				((uint)(ID3 & 0xFF) << 16) |
				((uint)(ID4 & 0xFF) << 24)
			);
		}
		private byte GetDegreeOrSubtype()
		{
			if (string.IsNullOrEmpty(ServerName))
				return 1;

			// Reverse Return Scroll
			if (ID2 == 3 && ID3 == 3 && ID4 == 3)
				return 3;

			// Check for patterns like _SUPERSET_5_BAG -> 5
			if (ServerName.IndexOf("_SUPERSET_", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				int idx = ServerName.IndexOf("_SUPERSET_", StringComparison.OrdinalIgnoreCase);
				int after = idx + "_SUPERSET_".Length;
				int nextUnderscore = ServerName.IndexOf('_', after);
				if (nextUnderscore > after)
				{
					string numStr = ServerName.Substring(after, nextUnderscore - after);
					byte deg;
					if (byte.TryParse(numStr, out deg) && deg > 0)
						return deg;
				}
			}

			// Check for degree suffix like _01, _02, ... _16
			int lastUnderscore = ServerName.LastIndexOf('_');
			if (lastUnderscore >= 0 && lastUnderscore < ServerName.Length - 1)
			{
				string suffix = ServerName.Substring(lastUnderscore + 1);
				byte deg;
				if (byte.TryParse(suffix, out deg) && deg > 0 && deg <= 16)
					return deg;
			}

			return 1;
		}
		public bool isType(byte ID2, byte ID3, byte ID4)
		{
			return this.ID2 == ID2 && this.ID3 == ID3 && this.ID4 == ID4;
		}

		public string GetQuantityText()
		{
			return (QuantityMax > 1) ? " (" + Quantity + "/" + QuantityMax + ")" : "";
		}
		public virtual string GetFullName()
		{
			return !string.IsNullOrWhiteSpace(Name) ? Name : (!string.IsNullOrWhiteSpace(ServerName) ? ServerName : ("Item #" + ID));
		}
		public virtual string GetTooltip()
		{
			return GetFullName();
		}
		public virtual SRItem Clone()
		{
			return new SRItem(this);
		}
		#endregion
	}
}
