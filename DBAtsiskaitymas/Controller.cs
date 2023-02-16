using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAtsiskaitymas.Contexts;
using DBAtsiskaitymas.Migrations;
using DBAtsiskaitymas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DBAtsiskaitymas
{
    internal class Controller
    {
        public void InitiateController()
        {
            bool isActive = true;
            while (isActive)
            {
                Console.WriteLine("\n== WELCOME TO STUDENT INFORMATION SYSTEM ==");
                Console.WriteLine("= [1]  - CREAT NEW STUDENT =");
                Console.WriteLine("= [2]  - ADD NEW DEPARTMENT =");
                Console.WriteLine("= [3]  - ADD NEW LECTURE =");
                Console.WriteLine("= [4]  - CHANGE STUDENT DEPARTMENT WITH LECTURES =");
                Console.WriteLine("= [5]  - LIST DEPARTMENTS STUDENTS =");
                Console.WriteLine("= [6]  - LIST DEPARTMENTS LECTURES =");
                Console.WriteLine("= [7]  - LIST STUDENTS LECTURES =");
                Console.WriteLine("= [8]  - DELETE STUDENT =");
                Console.WriteLine("= [12] - EXIT PROGRAME =\n");
                var input = Input();
                switch (input)
                {
                    case 1:
                        AddNewStudent();
                        break;
                    case 2:
                        AddNewDepartment();
                        break;
                    case 3:
                        AddNewLecture();
                        break;
                    case 4:
                        ChangeStudentDepartment();
                        break;
                    case 5:
                        ShowDepartmentsStudents();
                        break;
                    case 6:
                        ShowDepartmentsLectures();
                        break;
                    case 7:
                        ShowStudentsLectures();
                        break;
                    case 8:
                        DeleteStudent();
                        break;
                    case 12:
                        isActive = false;
                        break;
                }
            }

        }

        // === FUNCTIONS ===============================================================================================

        // === HANDLING STUDENTS FUNC ===============================================================================================
        // 4. Sukurti studentą, jį pridėti prie egzistuojančio departamento ir priskirti jam egzistuojančias paskaitas. DONE ==
        public void AddNewStudent()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Enter new student name:");
            string studentName = Console.ReadLine();

            Console.WriteLine("Enter new student surname:");
            string studentSurname = Console.ReadLine();

            Console.WriteLine("Select which department student will attend (by typing its number):");
            // Listing all available Departments
            var listOfDepartments = new List<string>();
            foreach (var (item, index) in dbContext.Departments.WithIndex())
            {
                Console.WriteLine($"{index+1}. {item.Name}");
                listOfDepartments.Add(item.Id.ToString());
            }
            Console.WriteLine();
            // Adding selected Department 
            string departmentId = "";
            var input = Input();
            for (int i = 0; i < listOfDepartments.Count; i++)
            {
                if(input == i + 1)
                {
                    departmentId = listOfDepartments[i];
                }
            }

            //Some code which lets to chose Lectures for the student
                // Listing all Lectures which are related to Department
                    //Console.WriteLine("\nSelect which Lectures Student will attend in Department by selecting a numbers seperating them by ','.");
            var listOfLecturesInDepartment = dbContext.Departments.Include(x => x.Lectures).Where(i => i.Id == new Guid(departmentId)).ToList();
                    //for (int i = 0; i < listOfLecturesInDepartment.Count; i++)
                    //{
                    //    for (int x = 0; x < listOfLecturesInDepartment[i].Lectures.Count; x++)
                    //    {
                    //        Console.WriteLine($"{x+1}. {listOfLecturesInDepartment[i].Lectures[x].Name}");

                    //    }
                    //}
            Console.WriteLine();

            // Adding Lectures which coresponds to selected Department to student
                //Some code which lets to chose Lectures for the student
                    //var lectureSelection = Console.ReadLine();
                    //string[] selectedLectures = lectureSelection.Split(',');

            var studentLectureList = new List<Lecture>();

            for (int i = 0; i < listOfLecturesInDepartment.Count; i++)
            {
                foreach (var item in listOfLecturesInDepartment[i].Lectures)
                {
                    studentLectureList.Add(dbContext.Lectures.Single(v => v.Id == item.Id));
                }
                    //Some code which lets to chose Lectures for the student  
                        //for (int x = 0; x < listOfLecturesInDepartment[i].Lectures.Count; x++)
                        //{
                        //    foreach (var item in selectedLectures)
                        //    {
                        //        if (Int32.Parse(item) - 1 == x)
                        //        {
                        //            studentLectureList.Add(dbContext.Lectures.Single(v => v.Id == listOfLecturesInDepartment[i].Lectures[x].Id));
                        //        }
                        //    }
                        //}
            }

            // Creating Student in DB
            var department = dbContext.Departments.Single(x => x.Id == new Guid(departmentId));
            dbContext.Students.Add(new Student { Name = studentName, Surname = studentSurname, Departments = new List<Department> { department }, Lectures = new List<Lecture>(studentLectureList) });

            dbContext.SaveChanges();

            Console.WriteLine($"\nStudent {studentName} {studentSurname} is added!");
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // 8. Atvaizduoti visas paskaitas pagal studentą.

        public void ShowStudentsLectures()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Select Student which lectures you want to see:\n");
            // Listing all available students
            var listOfStudents = new List<string>();
            foreach (var (item, index) in dbContext.Students.WithIndex())
            {
                Console.WriteLine($"{index + 1}. {item.Name} {item.Surname}");
                listOfStudents.Add(item.Id.ToString());
            }
            Console.WriteLine();

            Console.WriteLine("\nSelect Student Number:");
            // Adding selected Department 
            string studenttId = "";
            var input = Input();
            for (int i = 0; i < listOfStudents.Count; i++)
            {
                if (input == i + 1)
                {
                    studenttId = listOfStudents[i];
                }
            }

            // Finding lectures which relates to the Student
            var listOfLectures = dbContext.Students.Include(x => x.Lectures).Where(i => i.Id == new Guid(studenttId)).ToList();

            // Listing Students Lecture
            foreach (var item in listOfLectures)
            {
                Console.WriteLine($"\n{item.Name} {item.Surname} lectures:");
                foreach (var (lecture, index) in item.Lectures.WithIndex())
                {
                    Console.WriteLine($"  {index + 1}. {lecture.Name}.");
                }
            }
            Console.WriteLine();
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // 5. Perkelti studentą į kitą departamentą(bonus points jei pakeičiamos ir jo paskaitos).

        public void ChangeStudentDepartment()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Select which Student will change Department:\n");
            // Listing all available students
            var listOfStudents = new List<string>();
            foreach (var (item, index) in dbContext.Students.WithIndex())
            {
                Console.WriteLine($"{index + 1}. {item.Name} {item.Surname}");
                listOfStudents.Add(item.Id.ToString());
            }
            Console.WriteLine();

            Console.WriteLine("Select Student Number:");
            // Adding selected Department 
            string studenttId = "";
            var input = Input();
            for (int i = 0; i < listOfStudents.Count; i++)
            {
                if (input == i + 1)
                {
                    studenttId = listOfStudents[i];
                }
            }
            Console.WriteLine();

            // Selecting Departments
            var currentStudentDepartment = dbContext.Students.Include(x => x.Departments).Where(i => i.Id == new Guid(studenttId)).ToList();

            // Showing current students department
            string currentDepartment = "";
            foreach (var item in currentStudentDepartment)
            {
                Console.WriteLine($"Curent {item.Name} {item.Surname} department is -- {item.Departments.Select(x => x.Name).First()}\n");
                currentDepartment = item.Departments.Select(x => x.Name).First();
            }

            Console.WriteLine("Select New department from the list:");
            // Listing new Departments for the student and adding to list
            var listOfDepartments = new List<string>();
            int count = 0;
            foreach (var item in dbContext.Departments)
            {
                if (item.Name != currentDepartment)
                {
                    count++;
                    Console.WriteLine($"{count}. {item.Name}");
                    listOfDepartments.Add(item.Id.ToString());
                } 
            }
            Console.WriteLine();

            // Adding selected Department
            string newDepartmentId = "";
            var departmentInput = Input();
            for (int i = 0; i < listOfDepartments.Count; i++)
            {
                if (departmentInput == i + 1)
                {
                    newDepartmentId = listOfDepartments[i];
                }
            }
            
            // Adding new Lectures to the student and listing them
            Console.WriteLine("New Lecture list:");
            var newLecturesList = dbContext.Departments.Include(x => x.Lectures).Where(i => i.Id == new Guid(newDepartmentId)).ToList();
            var newStudentLectureList = new List<Lecture>();

            foreach (var item in newLecturesList)
            {
                foreach (var (lectures, index) in item.Lectures.WithIndex())
                {
                    newStudentLectureList.Add(dbContext.Lectures.Single(v => v.Id == lectures.Id));
                    Console.WriteLine($"{index+1}. {lectures.Name} ");
                }            
            }

            var departmentNew = new List<Department> { dbContext.Departments.Single(x => x.Id == new Guid(newDepartmentId)) };
            var oldLectureList = dbContext.Students.Include(x => x.Lectures).Where(i => i.Id == new Guid(studenttId)).ToList();


            foreach (var item in currentStudentDepartment)
            {
                item.Departments = departmentNew;
            }

            foreach (var item in oldLectureList)
            {
                item.Lectures = newStudentLectureList;
            }

            dbContext.SaveChanges();

            Console.WriteLine("Student Department and Lectures are changed!");
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // Delete Student extra

        public void DeleteStudent()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Select which Student to Delete:\n");
            // Listing all available students
            var listOfStudents = new List<string>();
            foreach (var (item, index) in dbContext.Students.WithIndex())
            {
                Console.WriteLine($"{index + 1}. {item.Name} {item.Surname}");
                listOfStudents.Add(item.Id.ToString());
            }
            Console.WriteLine();

            Console.WriteLine("\nSelect Student Number:");
            // Adding selected Department 
            string studenttId = "";
            var input = Input();
            for (int i = 0; i < listOfStudents.Count; i++)
            {
                if (input == i + 1)
                {
                    studenttId = listOfStudents[i];
                }
            }
            Console.WriteLine();

            // Deleting student
            var studentDelete = dbContext.Students.Where(x => x.Id == new Guid(studenttId)).Include(x => x.Departments).First();
            dbContext.Students.Remove(studentDelete);
            dbContext.SaveChanges();
        }

        // === HANDLING DEPARTMENTS FUNC ===============================================================================================
        // 1. Sukurti departamentą ir į jį pridėti "studentus?", paskaitas(papildomi points jei pridedamos paskaitos jau egzistuojančios duomenų bazėje).

        public void AddNewDepartment()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Enter new Department name:");
            string departmentName = Console.ReadLine();

            Console.WriteLine("\nSelect Lectures to add to the new Department.");
            Console.WriteLine("Select by typing lecture number and seperating by ',' - LIKE: 1,2,etc.. .\n");

            // Listing all the Lectures in DB
            foreach (var (item, index) in dbContext.Lectures.WithIndex())
            {
                Console.WriteLine($"{index+1}. {item.Name}");
            }
            Console.WriteLine();

            // Adding selected Lectures to list
            var lectureList = new List<Lecture>();
            // Reading Selection
            var lectureSelection = Console.ReadLine();
            string[] selectedLectures = lectureSelection.Split(',');

            // Parsing the Lecture DB and putting selected lectures to list
            foreach (var (item, index) in dbContext.Lectures.WithIndex())
            {
                foreach (var selected in selectedLectures)
                {
                    if (Int32.Parse(selected) == index + 1)
                    {
                        lectureList.Add(dbContext.Lectures.Single(x => x.Id == item.Id));
                    }
                }
            }

            // Saving new Department to the DB
            dbContext.Departments.Add(new Department { Name = departmentName, Lectures = new List<Lecture>(lectureList) });
            dbContext.SaveChanges();
            Console.WriteLine($"{departmentName} is added!");
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // 6. Atvaizduoti visus departamento studentus.

        public void ShowDepartmentsStudents()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Select Departments which students you want to see:\n");
            // Listing all available Departments
            var listOfDepartments = new List<string>();
            foreach (var (item, index) in dbContext.Departments.WithIndex())
            {
                Console.WriteLine($"{index + 1}. {item.Name}");
                listOfDepartments.Add(item.Id.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Select Deparment Number:");
            // Adding selected Department 
            string departmentId = "";
            var input = Input();
            for (int i = 0; i < listOfDepartments.Count; i++)
            {
                if (input == i + 1)
                {
                    departmentId = listOfDepartments[i];
                }
            }

            // Finding students which relates to the Department
            var listOfStudentsInDepartment = dbContext.Departments.Include(x => x.Students).Where(i => i.Id == new Guid(departmentId)).ToList();

            // Listing Students in Department
            foreach (var item in listOfStudentsInDepartment)
            {
                Console.WriteLine($"\n{item.Name} students:");
                foreach (var (student, index) in item.Students.WithIndex())
                {
                    Console.WriteLine($"  {index+1}. {student.Name} {student.Surname}.");
                }
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // 7. Atvaizduoti visas departamento paskaitas.

        public void ShowDepartmentsLectures()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Select Departments which Lectures you want to see:\n");
            // Listing all available Departments
            var listOfDepartments = new List<string>();
            foreach (var (item, index) in dbContext.Departments.WithIndex())
            {
                Console.WriteLine($"{index + 1}. {item.Name}");
                listOfDepartments.Add(item.Id.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Select Deparment Number:");
            // Adding selected Department 
            string departmentId = "";
            var input = Input();
            for (int i = 0; i < listOfDepartments.Count; i++)
            {
                if (input == i + 1)
                {
                    departmentId = listOfDepartments[i];
                }
            }

            // Finding lectures which relates to the Department
            var listOfLecturesInDepartment = dbContext.Departments.Include(x => x.Lectures).Where(i => i.Id == new Guid(departmentId)).ToList();

            // Listing Lectures in Department
            foreach (var item in listOfLecturesInDepartment)
            {
                Console.WriteLine($"\n{item.Name} lectures:");
                foreach (var (student, index) in item.Lectures.WithIndex())
                {
                    Console.WriteLine($"  {index + 1}. {student.Name}.");
                }
            }
        }

        // === HANDLING LECTURE FUNC ===============================================================================================
        // 3. Sukurti paskaitą ir ją priskirti prie departamento.

        public void AddNewLecture()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Enter new Lecture name:");
            string  lectureName = Console.ReadLine();

            Console.WriteLine("\nSelect Departments to add to the new Lecture.");
            Console.WriteLine("Select by typing department number and seperating by ',' - LIKE: 1,2,etc.. .\n");

            // Listing all the Departments in DB
            foreach (var (item, index) in dbContext.Departments.WithIndex())
            {
                Console.WriteLine($"{index + 1}. {item.Name}");
            }
            Console.WriteLine();

            // Adding selected Departments to list
            var departmentList = new List<Department>();
            // Reading Selection
            var departmentSelection = Console.ReadLine();
            string[] selectedDepartments = departmentSelection.Split(',');

            // Parsing the Departments DB and putting selected Departments to list
            foreach (var (item, index) in dbContext.Departments.WithIndex())
            {
                foreach (var selected in selectedDepartments)
                {
                    if (Int32.Parse(selected) == index + 1)
                    {
                        departmentList.Add(dbContext.Departments.Single(x => x.Id == item.Id));
                    }
                }
            }

            // Saving new Lecture to the DB
            dbContext.Lectures.Add(new Lecture { Name = lectureName, Departments = new List<Department>(departmentList) });
            dbContext.SaveChanges();
            Console.WriteLine($"{lectureName} is added!");

        }

        // === SUPPORT FUNC ===============================================================================================
        // = Handling input =
        public static int Input()
        {
            bool isCorrect = Int32.TryParse(Console.ReadLine(), out int result);
            if (isCorrect)
            {
                return result;
            }
            Console.WriteLine("Please input a number for your selection");
            return 0;
        }
    }
        // Handling indexes in forEach
        public static class EnumExtension
        {
            public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
               => self.Select((item, index) => (item, index)).ToList();
        }
}
