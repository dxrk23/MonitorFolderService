using System;
namespace EmailService
{
    public class MessageDTO
    {
        public Guid Id { get; set; }
        public String Type { get; set; }
        public string JsonContent { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
