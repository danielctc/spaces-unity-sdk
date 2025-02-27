

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







    // // Method to convert created_on string to DateTime
    // //-------------------------------------------------------------------------
    // public DateTime GetCreatedOnDate()
    // {
    //     if (DateTime.TryParse(created_on, out DateTime dateValue))
    //     {
    //         return dateValue;
    //     }
    //     else
    //     {
    //         // Handle the case where the date string is not in a valid format
    //         Debug.LogWarning("Invalid date format for created_on");
    //         return DateTime.MinValue; // Return a default value or handle as needed
    //     }
    // }

}