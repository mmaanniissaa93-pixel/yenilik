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
				Name = data["name"] ?? ServerName;
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
				Name = data["name"] ?? ServerName;
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
			return Name;
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
