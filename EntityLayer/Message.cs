using System;
using System.Text.Json;
using EntityLayer;

namespace EntityLayer
{
    [Serializable]
    public class Message
    {
        public string Action { get; set; }
        public int MemberId { get; set; }
        public int BookId { get; set; }
        public int LoanId { get; set; }

        public Message(string action, int memberId, int bookId, int loanId)
        {
            Action = action;
            MemberId = memberId;
            BookId = bookId;
            LoanId = loanId;
        }
    }
}