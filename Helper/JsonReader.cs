using Newtonsoft.Json.Linq;

namespace Client_Api.Helper;

public static class JsonReader
{
    public static string GetFieldFromJsonFile(string field)
    {
        string jsonFilePath = "secret.json";
        if (File.Exists(jsonFilePath))
        {
            var json = File.ReadAllText(jsonFilePath);
            var jsonObject = JObject.Parse(json);
            return jsonObject[field].ToString();
        }
        else
        {
            throw new FileNotFoundException("File not found.");
        }
    }
}