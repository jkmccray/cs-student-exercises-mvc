using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace StudentExercises.Models.ViewModels
{
    public class InstructorCreateViewModel
    {
        public List<SelectListItem> Cohorts { get; set; }
        public Instructor Instructor { get; set; }
    }
}