using System;
using System.Collections.Generic;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;

namespace xBot.Game.Navigation
{
	public enum TownServiceType
	{
		Blacksmith,
		Storage,
		PotionMerchant
	}

	public class TownServiceInfo
	{
		public string TownName { get; set; }
		public TownServiceType ServiceType { get; set; }
		public uint NpcId { get; set; }
		public string NpcName { get; set; }
		public SRCoord Coord { get; set; }

		public TownServiceInfo(string townName, TownServiceType serviceType, uint npcId, string npcName, SRCoord coord)
		{
			TownName = townName;
			ServiceType = serviceType;
			NpcId = npcId;
			NpcName = npcName;
			Coord = coord;
		}
	}

	public class TownManager
	{
		private static TownManager m_instance;
		public static TownManager Get
		{
			get
			{
				if (m_instance == null)
					m_instance = new TownManager();
				return m_instance;
			}
		}

		private readonly List<TownServiceInfo> m_services = new List<TownServiceInfo>();

		public TownManager()
		{
			InitializeServices();
		}

		private void InitializeServices()
		{
			m_services.Clear();

			// 1. Jangan (China)
			m_services.Add(new TownServiceInfo("Jangan", TownServiceType.Blacksmith, 2003, "Blacksmith Chulsan", new SRCoord(6425.0, 1070.0)));
			m_services.Add(new TownServiceInfo("Jangan", TownServiceType.Storage, 2013, "Storage-Keeper Wangu", new SRCoord(6455.0, 1105.0)));
			m_services.Add(new TownServiceInfo("Jangan", TownServiceType.PotionMerchant, 2005, "Herbalist Yangyun", new SRCoord(6368.0, 1078.0)));

			// 2. Donwhang (Western China)
			m_services.Add(new TownServiceInfo("Donwhang", TownServiceType.Blacksmith, 2051, "Blacksmith Agol", new SRCoord(3566.0, 2038.0)));
			m_services.Add(new TownServiceInfo("Donwhang", TownServiceType.Storage, 2057, "Storage-Keeper Paedo", new SRCoord(3532.0, 2028.0)));
			m_services.Add(new TownServiceInfo("Donwhang", TownServiceType.PotionMerchant, 2053, "Herbalist Bori", new SRCoord(3545.0, 2045.0)));

			// 3. Hotan (Oasis Kingdom)
			m_services.Add(new TownServiceInfo("Hotan", TownServiceType.Blacksmith, 2072, "Blacksmith Soboi", new SRCoord(142.0, 22.0)));
			m_services.Add(new TownServiceInfo("Hotan", TownServiceType.Storage, 2083, "Storage-Keeper Auisan", new SRCoord(115.0, 18.0)));
			m_services.Add(new TownServiceInfo("Hotan", TownServiceType.PotionMerchant, 2074, "Potion Merchant Manina", new SRCoord(132.0, 30.0)));

			// 4. Samarkand (Central Asia)
			m_services.Add(new TownServiceInfo("Samarkand", TownServiceType.Blacksmith, 22685, "Samarkand Blacksmith", new SRCoord(-4950.0, 1540.0)));
			m_services.Add(new TownServiceInfo("Samarkand", TownServiceType.Storage, 7537, "Storage-Keeper Saesa", new SRCoord(-4930.0, 1520.0)));
			m_services.Add(new TownServiceInfo("Samarkand", TownServiceType.PotionMerchant, 7532, "Nun Martel", new SRCoord(-4960.0, 1530.0)));

			// 5. Constantinople (Europe)
			m_services.Add(new TownServiceInfo("Constantinople", TownServiceType.Blacksmith, 22680, "Eastern Europe Blacksmith", new SRCoord(-10220.0, 2550.0)));
			m_services.Add(new TownServiceInfo("Constantinople", TownServiceType.Storage, 7497, "Constantinople Storage", new SRCoord(-10240.0, 2530.0)));
			m_services.Add(new TownServiceInfo("Constantinople", TownServiceType.PotionMerchant, 7497, "Nun Retaldi", new SRCoord(-10210.0, 2540.0)));
		}

		/// <summary>
		/// Finds the nearest town service to the character position.
		/// </summary>
		public TownServiceInfo FindNearestService(SRCoord myPosition, TownServiceType serviceType)
		{
			if (myPosition == null)
				return null;

			TownServiceInfo best = null;
			double minDistance = double.MaxValue;

			foreach (var service in m_services)
			{
				if (service.ServiceType != serviceType)
					continue;

				double dist = myPosition.DistanceTo(service.Coord);
				if (dist < minDistance)
				{
					minDistance = dist;
					best = service;
				}
			}

			// If NPC is already spawned nearby, update with precise live coordinates
			if (best != null)
			{
				SRNpc liveNpc = InfoManager.Npcs.Find(n => n.ID == best.NpcId ||
					(n.Name != null && n.Name.IndexOf(best.NpcName, StringComparison.OrdinalIgnoreCase) >= 0));

				if (liveNpc != null && liveNpc.Position != null)
				{
					best.Coord = liveNpc.Position;
				}
			}

			return best;
		}

		/// <summary>
		/// Checks whether a position is inside the known town service area.
		/// </summary>
		public bool IsNearTown(SRCoord position, double maxRange = 50.0)
		{
			if (position == null)
				return false;

			foreach (var service in m_services)
			{
				if (service.Coord != null && service.Coord.DistanceTo(position) <= maxRange)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Finds live NPC entity for the given service.
		/// </summary>
		public SREntity FindLiveNpc(TownServiceInfo service)
		{
			if (service == null)
				return null;

			// 1. Search in Npcs
			SREntity npc = InfoManager.Npcs.Find(n => n.ID == service.NpcId ||
				(n.Name != null && n.Name.IndexOf(service.NpcName, StringComparison.OrdinalIgnoreCase) >= 0) ||
				(n.Position != null && n.Position.DistanceTo(service.Coord) <= 35.0));

			if (npc != null)
				return npc;

			// 2. Search in Entities
			return InfoManager.Entities.Find(e => e.ID == service.NpcId ||
				(e.Name != null && e.Name.IndexOf(service.NpcName, StringComparison.OrdinalIgnoreCase) >= 0) ||
				(e.Position != null && e.Position.DistanceTo(service.Coord) <= 35.0));
		}
	}
}
