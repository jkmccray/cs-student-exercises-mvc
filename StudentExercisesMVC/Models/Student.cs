using System.Collections.Generic;

namespace StudentExercises.Models
{
    public class Student : NSSPerson
    {
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}