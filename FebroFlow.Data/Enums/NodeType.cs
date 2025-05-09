namespace FebroFlow.Data.Enums;

public enum NodeType
{
    // Input nodes
    TelegramTrigger = 1,
    WebhookTrigger = 2,
    ScheduleTrigger = 3,
    
    // Processing nodes
    OpenAI = 10,
    VectorStoreRetriever = 11,
    ChainRetrievalQA = 12,
    MemoryManager = 13,
    Switch = 14,
    Code = 15,
    SequentialThinking = 16,
    IfCondition = 17,
    Translator = 18,
    
    // Output nodes
    TelegramMessage = 20,
    HttpRequest = 21,
    DatabaseWrite = 22,
    FileOperation = 23,
    
    // AI specific nodes
    EmbeddingsOpenAI = 30,
    PineconeVectorStore = 31,
    OpenAIChatModel = 32,
    ImageAnalysis = 33,
    AudioTranscription = 34,
    TextToSpeech = 35
}