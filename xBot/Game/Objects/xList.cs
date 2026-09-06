using System;
using System.Collections.Generic;
namespace xBot.Game.Objects
{
	/// <summary>
	/// Generic list that can be expandable and handled as static.
	/// </summary>
	public class xList<T>
	{
		private int m_objectCount;
		private List<T> m_list;
		private readonly object m_sync = new object();
		public int Capacity { get { lock (m_sync) { return m_list.Count; } } }
		public int Count { get { lock (m_sync) { return m_objectCount; } } }
		public xList()
		{
			m_list = new List<T>();
			m_objectCount = 0;
		}
		public xList(int Capacity)
		{
			m_list = new List<T>(Capacity);
			for (int i = 0; i < m_list.Capacity; i++)
				m_list.Add(default(T));
			m_objectCount = 0;
		}
		public xList(xList<T> value)
		{
			m_sync = new object();
			lock (value.m_sync)
			{
				m_list = new List<T>(value.m_list);
				m_objectCount = value.m_objectCount;
			}
		}
		public T this[int index]
		{
			get {
				lock (m_sync)
				{
					if (index < 0 || index >= m_list.Count)
						return default(T);
					return m_list[index];
				}
			}
			set {
				lock (m_sync)
				{
					if (index >= m_list.Count)
					{
						// Expand the list
						for (int i = m_list.Count; i <= index; i++)
							m_list.Add(default(T));
					}
					// Keep control about real objects at list
					if (EqualityComparer<T>.Default.Equals(value, default(T)))
					{
						if (!EqualityComparer<T>.Default.Equals(m_list[index], default(T)))
							m_objectCount--;
					}
					else
					{
						if (EqualityComparer<T>.Default.Equals(m_list[index], default(T)))
							m_objectCount++;
					}
					// Set new value
					m_list[index] = value;
				}
			}
		}
		public void Add(T value)
		{
			this[Capacity] = value;
		}
		public void RemoveAt(int index)
		{
			lock (m_sync)
			{
				if (index < 0 || index >= m_list.Count)
					return;
				if (!EqualityComparer<T>.Default.Equals(m_list[index], default(T)) && m_objectCount > 0)
					m_objectCount--;
				m_list.RemoveAt(index);
			}
		}
		public void Clear()
		{
			lock (m_sync)
			{
				m_list.Clear();
				m_objectCount = 0;
			}
		}
		public void Resize(int newCapacity)
		{
			lock (m_sync)
			{
				if(newCapacity < m_list.Count)
				{
					for (int i = m_list.Count-1; i >= newCapacity; i--)
					{
						if (!EqualityComparer<T>.Default.Equals(m_list[i], default(T)) && m_objectCount > 0)
							m_objectCount--;
						m_list.RemoveAt(i);
					}
				}
				else if(newCapacity > m_list.Count)
				{
					for (int i = m_list.Count; i < newCapacity; i++)
						m_list.Add(default(T));
				}
			}
		}
		public bool Exists(Predicate<T> match)
		{
			lock (m_sync) { return m_list.Exists(match); }
		}
		public T Find(Predicate<T> match)
		{
			lock (m_sync) { return m_list.Find(match); }
		}
		/// <summary>
		/// Find the first item match from the starting index.
		/// </summary>
		public int FindIndex(Predicate<T> match, int startIndex = 0)
		{
			lock (m_sync) { return FindIndexLocked(match, startIndex, m_list.Count - 1); }
		}
		/// <summary>
		/// Find the first item match limited by the indices specified.
		/// </summary>
		public int FindIndex(Predicate<T> match, int startIndex, int endIndex)
		{
			lock (m_sync) { return FindIndexLocked(match, startIndex, endIndex); }
		}
		private int FindIndexLocked(Predicate<T> match, int startIndex, int endIndex)
		{
			if (startIndex < 0) startIndex = 0;
			if (endIndex >= m_list.Count) endIndex = m_list.Count - 1;
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (match(m_list[i]))
					return i;
			}
			return -1;
		}
	}
}
