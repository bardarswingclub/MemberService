using System;
using System.ComponentModel.DataAnnotations;

namespace MemberService.Data
{
    public class Payment
    {
        public string Id { get; set; }

        [Required]
        public MemberUser User { get; set; }

        [Required]
        public DateTime PayedAt { get; set; }
    }
}