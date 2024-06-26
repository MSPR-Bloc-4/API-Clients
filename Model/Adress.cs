using Google.Cloud.Firestore;

namespace Client_Api.Model;

[FirestoreData]
public class Adress
{
    [FirestoreProperty]
    public string Street { get; set; }
    [FirestoreProperty]
    public string City { get; set; }
    [FirestoreProperty]
    public string Country { get; set; }
    [FirestoreProperty]
    public int ZipCode { get; set; }
}