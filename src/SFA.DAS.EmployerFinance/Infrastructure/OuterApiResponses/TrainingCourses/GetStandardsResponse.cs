using Newtonsoft.Json;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.TrainingCourses;

public class GetStandardsResponse
{
    [JsonProperty("standards")]
    public List<StandardResponse> Standards { get; set; }
}

public class StandardResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("learningType")]
    public string LearningType { get; set; }
}