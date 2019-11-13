namespace StudentExercises.Models
{
    public class Instructor : NSSPerson
    {
        public void AssignStudentAnExercise(Student student, Exercise exercise) {
            student.Exercises.Add(exercise);
        }
    }
}