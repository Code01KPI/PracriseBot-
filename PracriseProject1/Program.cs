using PracriseProject1;
using Npgsql;

Bot bot = new Bot();
await bot.StartAsync();

#region PostgreSQL session store
class PostgreStore : Stream
{
	private readonly NpgsqlConnection _sql;
	private readonly string _sessionName;
	private byte[] _data;
	private int _dataLen;
	private DateTime _lastWrite;
	private Task _delayedWrite;

	/// <param name="databaseUrl">Heroku DB URL of the form "postgres://user:password@host:port/database"</param>
	/// <param name="sessionName">Entry name for the session data in the WTelegram_sessions table (default: "Heroku")</param>
	public PostgreStore(string databaseUrl, string sessionName = null)
	{
		_sessionName = sessionName ?? "Heroku";
		var parts = databaseUrl.Split(':', '/', '@');
		_sql = new NpgsqlConnection($"User ID={parts[3]};Password={parts[4]};Host={parts[5]};Port={parts[6]};Database={parts[7]};Pooling=true;SSL Mode=Require;Trust Server Certificate=True;");
		_sql.Open();
		using (var create = new NpgsqlCommand($"CREATE TABLE IF NOT EXISTS Telegram_sessions (name text NOT NULL PRIMARY KEY, data bytea)", _sql))
			create.ExecuteNonQuery();
		using var cmd = new NpgsqlCommand($"SELECT data FROM Telegram_sessions WHERE name = '{_sessionName}'", _sql);
		using var rdr = cmd.ExecuteReader();
		if (rdr.Read())
			_dataLen = (_data = rdr[0] as byte[]).Length;
	}

	protected override void Dispose(bool disposing)
	{
		_delayedWrite?.Wait();
		_sql.Dispose();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		Array.Copy(_data, 0, buffer, offset, count);
		return count;
	}

	public override void Write(byte[] buffer, int offset, int count) // Write call and buffer modifications are done within a lock()
	{
		_data = buffer; _dataLen = count;
		if (_delayedWrite != null) return;
		var left = 1000 - (int)(DateTime.UtcNow - _lastWrite).TotalMilliseconds;
		if (left < 0)
		{
			using var cmd = new NpgsqlCommand($"INSERT INTO Telegram_sessions (name, data) VALUES ('{_sessionName}', @data) ON CONFLICT (name) DO UPDATE SET data = EXCLUDED.data", _sql);
			cmd.Parameters.AddWithValue("data", count == buffer.Length ? buffer : buffer[offset..(offset + count)]);
			cmd.ExecuteNonQuery();
			_lastWrite = DateTime.UtcNow;
		}
		else // delay writings for a full second
			_delayedWrite = Task.Delay(left).ContinueWith(t => { lock (this) { _delayedWrite = null; Write(_data, 0, _dataLen); } });
	}

	public override long Length => _dataLen;
	public override long Position { get => 0; set { } }
	public override bool CanSeek => false;
	public override bool CanRead => true;
	public override bool CanWrite => true;
	public override long Seek(long offset, SeekOrigin origin) => 0;
	public override void SetLength(long value) { }
	public override void Flush() { }
}
#endregion
