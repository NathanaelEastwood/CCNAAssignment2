using Entities;

namespace EntityLayer;

[Serializable]
public class ResponseMessageDTO
{
    public int ResponseCode { get; set; }
    public List<Book>? Book { get; set; }
    public List<Loan>? Loan { get; set; }
    public List<Member>? Member { get; set; }
}