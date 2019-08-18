using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberService.Data
{
    public class Program
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public ProgramType Type { get; set; }

        public DateTime? SignupOpensAt { get; set; }

        public DateTime? SignupClosesAt { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();

        [Required]
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public MemberUser CreatedByUser { get; set; }

        public bool Archived { get; set; }
    }
}
