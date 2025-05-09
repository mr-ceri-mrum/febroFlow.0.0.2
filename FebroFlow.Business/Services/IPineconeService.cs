namespace FebroFlow.Business.Services;

/// <summary>
/// Интерфейс для взаимодействия с векторной базой данных Pinecone
/// </summary>
public interface IPineconeService
{
    /// <summary>
    /// Добавление вектора в базу данных
    /// </summary>
    /// <param name="id">Идентификатор вектора</param>
    /// <param name="vector">Вектор</param>
    /// <param name="metadata">Метаданные</param>
    /// <param name="nameSpace">Namespace для вектора</param>
    /// <returns>Результат операции</returns>
    Task<bool> UpsertVectorAsync(string id, float[] vector, Dictionary<string, object> metadata, string nameSpace = "");
    
    /// <summary>
    /// Добавление векторов в базу данных пакетно
    /// </summary>
    /// <param name="vectors">Словарь векторов: ключ - id, значение - массив координат</param>
    /// <param name="metadata">Словарь метаданных для каждого вектора</param>
    /// <param name="nameSpace">Namespace для векторов</param>
    /// <returns>Результат операции</returns>
    Task<bool> UpsertVectorsAsync(Dictionary<string, float[]> vectors, Dictionary<string, Dictionary<string, object>> metadata, string nameSpace = "");
    
    /// <summary>
    /// Поиск ближайших векторов
    /// </summary>
    /// <param name="vector">Вектор для поиска</param>
    /// <param name="topK">Количество ближайших векторов для возврата</param>
    /// <param name="nameSpace">Namespace для поиска</param>
    /// <param name="filter">Фильтр для поиска</param>
    /// <returns>Результаты поиска с метаданными</returns>
    Task<List<PineconeSearchResult>> QueryAsync(float[] vector, int topK = 10, string nameSpace = "", Dictionary<string, object> filter = null);
    
    /// <summary>
    /// Удаление вектора из базы данных
    /// </summary>
    /// <param name="id">Идентификатор вектора</param>
    /// <param name="nameSpace">Namespace вектора</param>
    /// <returns>Результат операции</returns>
    Task<bool> DeleteVectorAsync(string id, string nameSpace = "");
    
    /// <summary>
    /// Удаление множества векторов с фильтрацией
    /// </summary>
    /// <param name="filter">Фильтр для удаления</param>
    /// <param name="nameSpace">Namespace для удаления</param>
    /// <returns>Результат операции</returns>
    Task<bool> DeleteVectorsAsync(Dictionary<string, object> filter, string nameSpace = "");
}

/// <summary>
/// Класс для результатов поиска в Pinecone
/// </summary>
public class PineconeSearchResult
{
    /// <summary>
    /// Идентификатор вектора
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Значение сходства (от 0 до 1)
    /// </summary>
    public float Score { get; set; }
    
    /// <summary>
    /// Метаданные вектора
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }
}