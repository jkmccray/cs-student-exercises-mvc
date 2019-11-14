using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace StudentExercises.Models.ViewModels
{
    public class StudentCreateViewModel
    {
        public Student Student { get; set; }
        public List<Cohort> Cohorts { get; set; } = new List<Cohort>();

        public List<SelectListItem> CohortOptions
        {
            get
            {
                if (Cohorts == null) return null;

                List<SelectListItem> selectItems = Cohorts
                    .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                    .ToList();
                selectItems.Insert(0, new SelectListItem
                {
                    Text = "Choose cohort...",
                    Value = "0"
                });

                return selectItems;
            }
        }
    }
}