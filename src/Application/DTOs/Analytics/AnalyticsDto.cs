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