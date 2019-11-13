using System.ComponentModel.DataAnnotations;


namespace StudentExercises.Models
{
    public class NSSPerson
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength = 2)]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Slack")]
        [StringLength(50, MinimumLength = 3)]
        [Required]
        public string SlackHandle { get; set; }

        [Display(Name = "Cohort")]
        [Required]
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }
    }
}