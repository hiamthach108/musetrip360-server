namespace Application.Shared.Type;

public class CursorPagination
{
  public int Limit { get; set; }
  public string? Cursor { get; set; }
}

public class CursorPagination<T>
{
  public IEnumerable<T> Data { get; set; } = [];
  public string? NextCursor { get; set; }
  public string? PreviousCursor { get; set; }
  public bool HasNext { get; set; }
  public bool HasPrevious { get; set; }
  public int Total { get; set; }
}