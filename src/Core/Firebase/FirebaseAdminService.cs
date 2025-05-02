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

  public FirebaseAdminService()
  {
    _app = FirebaseApp.Create(new AppOptions
    {
      Credential = GoogleCredential.FromFile("firebase.json")
    });
  }

  public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
  {
    var user = await FirebaseAuth.GetAuth(_app).VerifyIdTokenAsync(idToken);

    return user;
  }
}