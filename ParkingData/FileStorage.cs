using System.IO;
using System.Text.Json;

namespace ParkingData;

public class FileStorage
{
    public void Save<T>(string path, T data)
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;

        string json = JsonSerializer.Serialize(data, options);

        File.WriteAllText(path + ".tmp", json);
        File.Move(path + ".tmp", path, true);
    }

    public T? Read<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<T>(json);
    }
}