namespace Application.Shared.Helpers;
using System;
public class SnowflakeId
{
  private static readonly DateTime Epoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
  private readonly object _lock = new object();

  private readonly int _serviceId;
  private int _lastTimestamp = -1;
  private int _sequence = 0;

  // Using smaller bits to fit within int.MaxValue (2,147,483,647)
  private const int ServiceIdBits = 2;     // Supports up to 4 different services
  private const int SequenceBits = 6;      // Supports up to 64 IDs per second
  private const int TimestampBits = 23;    // About 97 days worth of seconds

  private const int MaxServiceId = (1 << ServiceIdBits) - 1;
  private const int MaxSequence = (1 << SequenceBits) - 1;

  private const int ServiceIdShift = SequenceBits;
  private const int TimestampShift = ServiceIdBits + SequenceBits;

  public SnowflakeId(int serviceId)
  {
    if (serviceId > MaxServiceId || serviceId < 0)
    {
      throw new ArgumentException($"Service ID must be between 0 and {MaxServiceId}");
    }

    _serviceId = serviceId;
  }

  public int GenerateOrderId()
  {
    lock (_lock)
    {
      var currentTimestamp = GetTimestamp();

      if (currentTimestamp < _lastTimestamp)
      {
        throw new Exception("Clock moved backwards. Refusing to generate order ID.");
      }

      if (currentTimestamp == _lastTimestamp)
      {
        _sequence = (_sequence + 1) & MaxSequence;
        if (_sequence == 0)
        {
          currentTimestamp = WaitNextSecond(_lastTimestamp);
        }
      }
      else
      {
        _sequence = 0;
      }

      _lastTimestamp = currentTimestamp;

      // Combine components into an integer
      int id = ((currentTimestamp & ((1 << TimestampBits) - 1)) << TimestampShift) |
              (_serviceId << ServiceIdShift) |
              _sequence;

      return Math.Abs(id); // Ensure positive number
    }
  }

  private static int GetTimestamp()
  {
    return (int)(DateTime.UtcNow - Epoch).TotalSeconds;
  }

  private static int WaitNextSecond(int lastTimestamp)
  {
    var timestamp = GetTimestamp();
    while (timestamp <= lastTimestamp)
    {
      timestamp = GetTimestamp();
    }
    return timestamp;
  }

  public static (DateTime Timestamp, int ServiceId, int Sequence) DeconstructId(int id)
  {
    var sequence = id & MaxSequence;
    var serviceId = (id >> ServiceIdShift) & MaxServiceId;
    var timestamp = (id >> TimestampShift) & ((1 << TimestampBits) - 1);

    return (Epoch.AddSeconds(timestamp), serviceId, sequence);
  }
}
