using System;
using System.ComponentModel.DataAnnotations;

namespace PROJLRR.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        public string Message { get; set; } = string.Empty;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
        
        public string? ModifiePar { get; set; }
    }
}