namespace EntityLayer;

[Serializable]
public class ResponseMessageDTO
{
    public ResponseMessageDTO(string response)
    {
        Response = response;
    }
    public string Response { get; set; }
}