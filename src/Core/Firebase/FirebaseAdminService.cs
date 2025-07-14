namespace Core.Firebase;

using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public interface IFirebaseAdminService
{
  Task<FirebaseToken> VerifyIdTokenAsync(string idToken);
}

public class FirebaseAdminService : IFirebaseAdminService
{
  private readonly FirebaseApp _app;

  public FirebaseAdminService(IConfiguration configuration)
  {
    _app = FirebaseApp.Create(new AppOptions
    {
      Credential = GetFirebaseCredential(configuration)
    });
  }

  private GoogleCredential GetFirebaseCredential(IConfiguration configuration)
  {
    // Check for JSON file path first
    var jsonFilePath = configuration["Firebase:ServiceAccountKeyPath"] ?? "firebase.json";
    if (!string.IsNullOrEmpty(jsonFilePath) && File.Exists(jsonFilePath))
    {
      return GoogleCredential.FromFile(jsonFilePath);
    }

    // Fallback to JSON string from configuration
    var jsonString = configuration["FirebaseToken"];
    if (!string.IsNullOrEmpty(jsonString))
    {
      return GoogleCredential.FromJson(jsonString);
    }

    // Fallback to default credentials (useful for Google Cloud environments)
    try
    {
      return GoogleCredential.GetApplicationDefault();
    }
    catch
    {
      throw new InvalidOperationException(
        "No Firebase credentials found. Please provide either:\n" +
        "1. Firebase:ServiceAccountKeyPath pointing to a JSON file\n" +
        "2. Firebase:ServiceAccountKey with JSON content\n" +
        "3. Set up Application Default Credentials"
      );
    }
  }

  public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
  {
    var user = await FirebaseAuth.GetAuth(_app).VerifyIdTokenAsync(idToken);

    return user;
  }
}