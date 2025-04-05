using Entities;

namespace EntityLayer;

public class Message : IEntity
{
    public Message(string messageContent)
    {
        MessageContent = messageContent;
    }
    public string MessageContent { get; set; }
}