namespace Application.DTOs.Analytics;

public class MuseumAnalyticsOverview
{
  public int TotalVisitors { get; set; }
  public int TotalArticles { get; set; }
  public int TotalEvents { get; set; }
  public int TotalTourOnlines { get; set; }
  public double AverageRating { get; set; }
  public int TotalFeedbacks { get; set; }
  public double TotalRevenue { get; set; }
  public int TotalArtifacts { get; set; }
}

public class AdminAnalyticsOverview
{
  public int TotalMuseums { get; set; }
  public int TotalPendingRequests { get; set; }
  public int TotalUsers { get; set; }
  public int TotalEvents { get; set; }
  public int TotalTours { get; set; }

  public List<AnalyticsMuseumCategorize> MuseumsByCategory { get; set; } = [];
}

public class AnalyticsMuseumCategorize
{
  public string Category { get; set; } = string.Empty;
  public int Count { get; set; }
}