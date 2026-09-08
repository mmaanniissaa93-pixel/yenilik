using System;
using System.Collections.Generic;
namespace xBot.Game.Objects
{
	/// <summary>
	/// Generic dictionary that can be iterable and handled as array.
	/// </summary>
	public class xDictionary<TKey,TValue>
	{
		private Dictionary<TKey, TValue> m_dictionary;
		private List<TKey> m_enumerator;
		private readonly object m_sync = new object();
		public int Count { get { lock (m_sync) { return m_enumerator.Count; } } }

		#region (Constructor)
		public xDictionary()
		{
			m_dictionary = new Dictionary<TKey, TValue>();
			m_enumerator = new List<TKey>();
    }
		public xDictionary(xDictionary<TKey,TValue> value)
		{
			m_sync = new object();
			lock (value.m_sync)
			{
				m_dictionary = new Dictionary<TKey, TValue>(value.m_dictionary);
				m_enumerator = new List<TKey>(value.m_enumerator);
			}
		}
		#endregion

		#region (Dictionary Methods)
		public TValue this[TKey ID]
		{
			get
			{
				TValue value = default(TValue);
				lock (m_sync) { m_dictionary.TryGetValue(ID, out value); }
				return value;
			}
			set
			{
				lock (m_sync)
				{
					if (!m_dictionary.ContainsKey(ID))
						m_enumerator.Add(ID);
					m_dictionary[ID] = value;
				}
			}
		}
		public void RemoveKey(TKey ID)
		{
			lock (m_sync)
			{
				m_dictionary.Remove(ID);
				m_enumerator.Remove(ID);
			}
		}
		public void SetKey(TKey ID, TKey NewID)
		{
			TrySetKey(ID, NewID);
		}
		public bool TrySetKey(TKey ID, TKey NewID)
		{
			lock (m_sync)
			{
				if (EqualityComparer<TKey>.Default.Equals(ID, NewID))
					return m_dictionary.ContainsKey(ID);
				if (!m_dictionary.TryGetValue(ID, out TValue reference))
					return false;
				// Renaming onto an existing key would create duplicate entries in
				// m_enumerator and silently overwrite the target value.
				if (m_dictionary.ContainsKey(NewID))
					return false;
				m_dictionary.Remove(ID);
				int idx = m_enumerator.IndexOf(ID);
				if (idx >= 0)
					m_enumerator[idx] = NewID;
				else
					m_enumerator.Add(NewID);
				m_dictionary[NewID] = reference;
				return true;
			}
		}
		public void Clear()
		{
			lock (m_sync)
			{
				m_dictionary.Clear();
				m_enumerator.Clear();
			}
		}
		public bool ContainsKey(TKey ID)
		{
			lock (m_sync) { return m_dictionary.ContainsKey(ID); }
		}
		/// <summary>
		/// Thread-safe snapshot for enumeration without holding the lock.
		/// </summary>
		public List<TValue> Snapshot()
		{
			lock (m_sync)
			{
				List<TValue> copy = new List<TValue>(m_enumerator.Count);
				for (int i = 0; i < m_enumerator.Count; i++)
				{
					TKey key = m_enumerator[i];
					if (m_dictionary.TryGetValue(key, out TValue v))
						copy.Add(v);
				}
				return copy;
			}
		}
		#endregion (Dictionary Methods)

		#region (List Methods)
		public TValue GetAt(int index)
		{
			lock (m_sync) { return this[m_enumerator[index]]; }
		}
		public void RemoveAt(int index)
		{
			lock (m_sync)
			{
				if (index < 0 || index >= m_enumerator.Count)
					return;
				TKey key = m_enumerator[index];
				m_dictionary.Remove(key);
				m_enumerator.RemoveAt(index);
			}
		}
		public void InsertAt(int index,TKey ID,TValue value)
		{
			lock (m_sync)
			{
				if (!m_dictionary.ContainsKey(ID))
				{
					if (index < 0) index = 0;
					if (index > m_enumerator.Count) index = m_enumerator.Count;
					m_enumerator.Insert(index, ID);
				}
				m_dictionary[ID] = value;
			}
		}
		public TValue Find(Predicate<TValue> match)
		{
			foreach (TValue reference in Snapshot())
			{
				if(match(reference))
					return reference;
			}
			return default(TValue);
		}
		public List<TValue> FindAll(Predicate<TValue> match)
		{
			List<TValue> references = new List<TValue>();
			foreach (TValue reference in Snapshot())
			{
				if (match(reference))
					references.Add(reference);
			}
			return references;
		}
		#endregion (List Methods)
	}
}
