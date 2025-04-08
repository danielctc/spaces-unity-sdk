

[System.Serializable]
public class FirebaseUserData
{
    // Firebase Auth Fields
    //-------------------------------------------------------------------------
    public string uid;
    public string accessToken;
    public string email;
    public string photoURL;



    // Firebase User Collection fields, these are all passed in a strings.
    //-------------------------------------------------------------------------
    // These should match with firebase field names (case sensetive).
    public string name;
    public string rpmURL;
    public string Nickname;
    public string height_cm;
    public string created_on;
}