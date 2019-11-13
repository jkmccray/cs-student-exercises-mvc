using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace StudentExercises.Models.ViewModels
{
    public class StudentCreateViewModel
    {
        public List<SelectListItem> Cohorts { get; set; }
        public Student Student { get; set; }
    }
}