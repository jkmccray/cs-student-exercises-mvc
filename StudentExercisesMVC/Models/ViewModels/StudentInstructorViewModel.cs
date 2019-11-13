using StudentExercises.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Models.ViewModels
{
    public class StudentInstructorViewModel
    {
        public List<Student> Students { get; set; }
        public List<Instructor> Instructors { get; set; }
    }
}