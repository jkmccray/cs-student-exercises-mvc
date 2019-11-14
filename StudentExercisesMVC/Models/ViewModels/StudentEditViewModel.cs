using Microsoft.AspNetCore.Mvc.Rendering;
using StudentExercises.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Models.ViewModels
{
    public class StudentEditViewModel
    {
        public Student Student { get; set; }
        public List<Cohort> Cohorts { get; set; } = new List<Cohort>();

        public List<SelectListItem> CohortOptions
        {
            get
            {
                if (Cohorts == null) return null;

                return Cohorts
                    .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                    .ToList();
            }
        }
        public IEnumerable<Exercise> Exercises { get; set; }
        public List<SelectListItem> ExerciseOptions 
        { 
            get
            {
                if (Exercises == null) return null;
                return Exercises
                    .Select(e => new SelectListItem($"{e.Name} ({e.Language})", e.Id.ToString()))
                    .ToList();
            }
        }
        public List<int> SelectedExerciseIds { get; set; }
    }
}
