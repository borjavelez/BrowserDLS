using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MonitorAPI.Models
{
    public class LogMonitor
    {
        public int Id { get; set; }
        [Required]
        public string Origin { get; set; }
        [Required]
        public TimeSpan Time { get; set; }
        [Required]
        public string Message { get; set; }
    }
}