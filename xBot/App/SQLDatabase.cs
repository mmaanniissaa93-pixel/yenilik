using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace xBot.App
{
	public class SQLDatabase : IDisposable
	{
		private string Path { get; }
		private SQLiteConnection db;
		private SQLiteCommand q;
		private readonly object m_sync = new object();
		private bool m_disposed;
		private int m_commandOwnerThreadId;
		public SQLDatabase(string Path)
		{
			this.Path = Path;
		}
		public string LastError { get; private set; }
		/// <summary>
		/// Creates a zero-byte database file to be used correctly by SQLite. Return success.
		/// </summary>
		public bool Create()
		{
			try
			{
				SQLiteConnection.CreateFile(Path);
				return true;
			}
			catch (Exception ex)
			{
				LastError = ex.Message;
				return false;
			}
		}
		/// <summary>
		/// Connect database. Returns sucess.
		/// </summary>
		public bool Connect()
		{
			lock (m_sync)
			{
				try
				{
					CloseLocked();
					db = new SQLiteConnection("Data Source=" + Path + ";Version=3;");
					q = new SQLiteCommand(db);
					q.CommandTimeout = 30; // 30sn: kilitlenmede 16dk bekleme yerine hızlı fail
					db.Open();
					return true;
				}
				catch (Exception ex)
				{
					LastError = ex.Message;
					return false;
				}
			}
		}
		/// <summary>
		/// Execute SQL query and return the number of columns affected. Returns (-1) if the database is not connected.
		/// </summary>
		/// <param name="sql">SQLite query</param>
		public int ExecuteQuery(string sql)
		{
			lock (m_sync)
			{
				if (db == null) return -1;
				if (IsResultQuery(sql))
				{
					AcquireCommandOwnershipLocked();
					q.Parameters.Clear();
					q.CommandText = sql;
					return 0;
				}
				using (SQLiteCommand command = new SQLiteCommand(sql, db))
				{
					command.CommandTimeout = 30;
					return command.ExecuteNonQuery();
				}
			}
		}
		/// <summary>
		/// Execute query previously prepared and return the number of columns affected. Returns (-1) if the database is not connected.
		/// </summary>
		public int ExecuteQuery()
		{
			lock (m_sync)
			{
				if (db == null) return -1;
				EnsureCommandOwnershipLocked();
				try { return q.ExecuteNonQuery(); }
				finally { ReleaseCommandOwnershipLocked(); }
			}
		}
		/// <summary>
		/// Prepares SQL query.
		/// </summary>
		/// <param name="sql">SQLite query</param>
		/// <returns>Sucess</returns>
		public bool Prepare(string sql)
		{
			lock (m_sync)
			{
				if (db == null) return false;
				AcquireCommandOwnershipLocked();
				q.Parameters.Clear();
				q.CommandText = sql;
				return true;
			}
		}
		/// <summary>
		/// Bind column value to the query previously prepared.
		/// </summary>
		/// <param name="column">Column name</param>
		/// <param name="value">Value to bind</param>
		public void Bind(string column,object value)
		{
			lock (m_sync)
			{
				EnsureCommandOwnershipLocked();
				q.Parameters.Add(new SQLiteParameter(column, value));
			}
		}
		public List<NameValueCollection> GetResult()
		{
			List<NameValueCollection> result = new List<NameValueCollection>();
			lock (m_sync)
			{
				EnsureCommandOwnershipLocked();
				try
				{
					using (SQLiteDataReader reader = q.ExecuteReader())
					{
						while (reader.Read())
							result.Add(reader.GetValues());
					}
				}
				finally { ReleaseCommandOwnershipLocked(); }
			}
			return result;
		}
		public List<NameValueCollection> GetResultFromQuery(string sql)
		{
			List<NameValueCollection> result = new List<NameValueCollection>();
			if (db != null)
			{
				using (SQLiteCommand cmd = new SQLiteCommand(sql, db))
				{
					cmd.CommandTimeout = 30;
					using (SQLiteDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
							result.Add(reader.GetValues());
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Parametreli sorgu: SQL injection ve quote patlamasını önler.
		/// Örn: GetResultFromQuery("SELECT * FROM items WHERE servername=@p0", servername)
		/// </summary>
		public List<NameValueCollection> GetResultFromQuery(string sql, params object[] args)
		{
			List<NameValueCollection> result = new List<NameValueCollection>();
			if (db != null)
			{
				using (SQLiteCommand cmd = new SQLiteCommand(sql, db))
				{
					cmd.CommandTimeout = 30;
					if (args != null)
					{
						for (int i = 0; i < args.Length; i++)
							cmd.Parameters.AddWithValue("@p" + i, args[i] ?? DBNull.Value);
					}
					using (SQLiteDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
							result.Add(reader.GetValues());
					}
				}
			}
			return result;
		}
		public void Begin()
		{
			ExecuteQuery("BEGIN");
		}
		public void End()
		{
			ExecuteQuery("END");
		}
		public void Close()
		{
			lock (m_sync) { CloseLocked(); }
		}
		private void CloseLocked()
		{
			m_commandOwnerThreadId = 0;
			try { q?.Dispose(); } catch { }
			q = null;
			try
			{
				if (db != null)
				{
					if (db.State != System.Data.ConnectionState.Closed)
						db.Close();
					db.Dispose();
				}
			}
			catch { }
			db = null;
		}
		private static bool IsResultQuery(string sql)
		{
			return !string.IsNullOrWhiteSpace(sql)
				&& sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
		}
		private void AcquireCommandOwnershipLocked()
		{
			int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
			if (m_commandOwnerThreadId != 0 && m_commandOwnerThreadId != currentThreadId)
				throw new InvalidOperationException("Another thread owns the prepared SQLite command.");
			m_commandOwnerThreadId = currentThreadId;
		}
		private void EnsureCommandOwnershipLocked()
		{
			if (m_commandOwnerThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
				throw new InvalidOperationException("Prepare or execute a result query on this thread before using the command.");
		}
		private void ReleaseCommandOwnershipLocked()
		{
			m_commandOwnerThreadId = 0;
			q?.Parameters.Clear();
		}
		public void Dispose()
		{
			if (m_disposed) return;
			m_disposed = true;
			Close();
			System.GC.SuppressFinalize(this);
		}
	}
}
