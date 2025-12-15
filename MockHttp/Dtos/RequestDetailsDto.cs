namespace MockHttp.Dtos;

public class RequestDetailsDto
{
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, string> FormData { get; set; } = new Dictionary<string, string>();
    public string BodyContent { get; set; } = string.Empty;
    public long BodySize { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Scheme { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
