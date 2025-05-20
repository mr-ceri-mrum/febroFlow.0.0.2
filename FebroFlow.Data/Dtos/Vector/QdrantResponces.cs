namespace FebroFlow.Data.Dtos.Vector;

public class QdrantCollectionResponse
{
    public QdrantResult Result { get; set; }
    public string Status { get; set; }
    public double Time { get; set; }
}

public class QdrantResult
{
    public List<CollectionDescription> Collections { get; set; }
}

public class CollectionDescription
{
    public string Name { get; set; }
}

public class QdrantPoint
{
    public string Id { get; set; }  // можно Guid.ToString()
    public float[] Vector { get; set; } // размер должен совпадать с размером коллекции (например, 512)
    public Dictionary<string, object> Payload { get; set; } // любые дополнительные данные
}