using System;

namespace DatingApp.API.Dtos
{
    public class MessageToCreateDto
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public DateTime MessageSendDate { get; set; }
        public string Content { get; set; }
        public MessageToCreateDto()
        {
            MessageSendDate = DateTime.Now;
        }
    }
}