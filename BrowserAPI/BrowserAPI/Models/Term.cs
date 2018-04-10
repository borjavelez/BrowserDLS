using System;
using System.ComponentModel.DataAnnotations;

using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrowserAPI.Models
{
    public class Term
    {
        public int Id { get; set; }
        [Required]
        public string Value { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public TimeSpan Time { get; set; }
    }
}

