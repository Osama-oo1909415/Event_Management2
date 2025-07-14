using System;
using System.ComponentModel.DataAnnotations;

namespace EventWebsite.ViewModels
{
    public class CreateEventViewModel
    {
        [Required]
        [Display(Name = "Event Title")]
        public string EventTitle { get; set; } = string.Empty; // <-- ADD THIS

        [Required]
        [Display(Name = "Event Date and Time")]
        [DataType(DataType.DateTime)]
        public DateTime EventDateTime { get; set; }

        [Required]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty; // <-- ADD THIS

        [Display(Name = "Event Category (e.g., music, sport)")]
        public string Category { get; set; } = string.Empty; // <-- ADD THIS
    }
}