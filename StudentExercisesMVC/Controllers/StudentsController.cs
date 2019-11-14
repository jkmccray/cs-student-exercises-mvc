using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using StudentExercises.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Students
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT s.Id,
                                            s.FirstName,
                                            s.LastName,
                                            s.SlackHandle,
                                            s.CohortId,
                                            c.Name AS CohortName
                                        FROM Students s
                                        LEFT JOIN Cohorts c on s.CohortId = c.Id
                                      ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            }
                        };

                        students.Add(student);
                    }

                    reader.Close();

                    return View(students);
                }
            }
        }

        // GET: Students/Details/5
        public ActionResult Details(int id)
        {
            Student aStudent = GetStudentByIdWithExercises(id);
            return View(aStudent);
        }

        // GET: Students/Create
        [HttpGet]
        public ActionResult Create()
        {
            var viewModel = new StudentCreateViewModel() 
            {
                Cohorts = GetAllCohorts()
            };
            return View(viewModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StudentCreateViewModel model)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Students
                ( FirstName, LastName, SlackHandle, CohortId )
                VALUES
                ( @firstName, @lastName, @slackHandle, @cohortId )";
                    cmd.Parameters.Add(new SqlParameter("@firstName", model.Student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", model.Student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", model.Student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", model.Student.CohortId));
                    cmd.ExecuteNonQuery();

                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            var student = GetStudentByIdWithExercises(id);
            var viewModel = new StudentEditViewModel()
            {
                Cohorts = GetAllCohorts(),
                Exercises = GetAllExercises(),
                Student = student,
                SelectedExerciseIds = student.Exercises.Select(e => e.Id).ToList()
            };
            return View(viewModel);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, StudentEditViewModel model)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                            UPDATE Students
                                            SET FirstName = @firstName, LastName = @lastName, SlackHandle = @slackHandle, CohortId = @cohortId
                                            WHERE Id = @id;
                                            DELETE FROM StudentExercises
                                            WHERE StudentId = @id
                                           ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@firstName", model.Student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", model.Student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", model.Student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", model.Student.CohortId));
                        cmd.ExecuteNonQuery();
                        foreach(int exerciseId in model.SelectedExerciseIds)
                        {
                            AddExerciseToStudent(id, exerciseId);
                        };
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var viewModel = new StudentEditViewModel()
                {
                    Cohorts = GetAllCohorts(),
                    Exercises = GetAllExercises(),
                    Student = model.Student
                };
                return View(viewModel);
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id)
        {
            Student studentToDelete = GetStudentById(id);
            return View(studentToDelete);
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        cmd.CommandText = @"DELETE FROM Students
                                            WHERE id = @id;
                                            DELETE FROM StudentExercises
                                            WHERE StudentId = @id
                                           ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private Student GetStudentById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT s.Id,
                                            s.FirstName,
                                            s.LastName,
                                            s.SlackHandle,
                                            s.CohortId,
                                            c.Name AS CohortName
                                        FROM Students s
                                        LEFT JOIN Cohorts c on s.CohortId = c.Id
                                        WHERE s.Id = @id
                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student aStudent = null;
                    if (reader.Read())
                    {
                        aStudent = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            }
                        };

                    }
                    else
                    {
                        aStudent = null;
                    }

                    reader.Close();

                    return aStudent;
                }
            }
        }
        private Student GetStudentByIdWithExercises(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT s.Id AS StudentId,
                                            s.FirstName,
                                            s.LastName,
                                            s.SlackHandle,
                                            s.CohortId,
                                            c.Name AS CohortName,
											e.Name AS ExerciseName,
											e.Language AS ExerciseLanguage,
											e.Id AS ExerciseId,
											se.Id AS StudentExerciseId
                                        FROM Students s
                                        LEFT JOIN Cohorts c on s.CohortId = c.Id
                                        LEFT JOIN StudentExercises se on s.Id = se.StudentId
										LEFT JOIN Exercises e on e.Id = se.ExerciseId
                                        WHERE s.Id = @id
                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student aStudent = null;
                    while (reader.Read())
                    {
                        if (aStudent == null)
                        {
                            aStudent = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("StudentExerciseId")))
                        {
                            Exercise newExercise = new Exercise()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                            };
                            aStudent.Exercises.Add(newExercise);
                        }
                    }
                    reader.Close();

                    return aStudent;
                }
            }
        }
        private List<Cohort> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Cohorts";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        cohorts.Add(new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        });
                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }
        private IEnumerable<Exercise> GetAllExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Language FROM Exercises";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();
                    while (reader.Read())
                    {
                        exercises.Add(new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        });
                    }

                    reader.Close();

                    return exercises;
                }
            }
        }
        private void AddExerciseToStudent(int studentId, int exerciseId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        INSERT INTO StudentExercises (ExerciseId, StudentId)
                                        VALUES (@exerciseId, @studentId)
                                        ";
                    cmd.Parameters.Add(new SqlParameter("@studentId", studentId));
                    cmd.Parameters.Add(new SqlParameter("@exerciseId", exerciseId));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}