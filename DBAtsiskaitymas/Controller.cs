using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DBAtsiskaitymas.Contexts;
using DBAtsiskaitymas.Migrations;
using DBAtsiskaitymas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;

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
                Console.WriteLine("= [1]  - CREAT NEW STUDENT WITH DEPARTMENT =");
                Console.WriteLine("= [2]  - CREAT NEW STUDENT WITH DEPARTMENT AND LECTURES =");
                Console.WriteLine("= [3]  - ADD NEW DEPARTMENT =");
                Console.WriteLine("= [4]  - ADD NEW LECTURE =");
                Console.WriteLine("= [5]  - CHANGE STUDENT DEPARTMENT WITH LECTURES =");
                Console.WriteLine("= [6]  - LIST DEPARTMENTS STUDENTS =");
                Console.WriteLine("= [7]  - LIST DEPARTMENTS LECTURES =");
                Console.WriteLine("= [8]  - LIST STUDENTS LECTURES =");
                Console.WriteLine("= [9]  - DELETE STUDENT =");
                Console.WriteLine("= [12] - EXIT PROGRAME =\n");
                var input = Input();
                switch (input)
                {
                    case 1:
                        AddNewStudentToDepartment();
                        break;
                    case 2:
                        AddNewStudent();
                        break;
                    case 3:
                        AddNewDepartment();
                        break;
                    case 4:
                        AddNewLecture();
                        break;
                    case 5:
                        ChangeStudentDepartment();
                        break;
                    case 6:
                        ShowDepartmentsStudents();
                        break;
                    case 7:
                        ShowDepartmentsLectures();
                        break;
                    case 8:
                        ShowStudentsLectures();
                        break;
                    case 9:
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

        // 2. Pridėti studentus/paskaitas į jau egzistuojantį departamentą. ==
        public void AddNewStudentToDepartment()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Enter new student name:");
            string studentName = Console.ReadLine();
            Console.WriteLine("Enter new student surname:");
            string studentSurname = Console.ReadLine();
            Console.WriteLine();

            // Listing all available Departments and adding Department ID to list
            Console.WriteLine("Select which department student will attend (by typing its number):");
            var listOfDepartmentsIds = new List<string>();
            ListingAndAddingToList(listOfDepartmentsIds, "Departments");

            // Selecting departmentId
            string departmentId = SelectingId(listOfDepartmentsIds);

            var department = dbContext.Departments.Single(x => x.Id == new Guid(departmentId));
            
            // Creating Student in DB
            dbContext.Students.Add(new Student { Name = studentName, Surname = studentSurname, Departments = new List<Department> { department } });

            dbContext.SaveChanges();

            Console.WriteLine($"\nStudent {studentName} {studentSurname} is created and added to {department.Name} department!");
        }

        // 4. Sukurti studentą, jį pridėti prie egzistuojančio departamento ir priskirti jam egzistuojančias paskaitas. ==
        public void AddNewStudent()
        {
            using var dbContext = new AplicationContext();

            Console.WriteLine("Enter new student name:");
            string studentName = Console.ReadLine();
            Console.WriteLine("Enter new student surname:");
            string studentSurname = Console.ReadLine();

            // Listing all available Departments and adding Department ID to list
            Console.WriteLine("Select which department student will attend (by typing its number):");
            var listOfDepartmentsIds = new List<string>();
            ListingAndAddingToList(listOfDepartmentsIds, "Departments");

            // Selecting departmentId
            string departmentId = SelectingId(listOfDepartmentsIds);

            var listOfLecturesInDepartment = dbContext.Departments.Include(x => x.Lectures).Where(i => i.Id == new Guid(departmentId)).ToList();
            var studentLectureList = new List<Lecture>();

            for (int i = 0; i < listOfLecturesInDepartment.Count; i++)
            {
                foreach (var item in listOfLecturesInDepartment[i].Lectures)
                {
                    studentLectureList.Add(dbContext.Lectures.Single(v => v.Id == item.Id));
                }
            }

            var department = dbContext.Departments.Single(x => x.Id == new Guid(departmentId));
            
            // Creating Student in DB
            dbContext.Students.Add(new Student { Name = studentName, Surname = studentSurname, Departments = new List<Department> { department }, Lectures = new List<Lecture>(studentLectureList) });

            dbContext.SaveChanges();

            Console.WriteLine($"\nStudent {studentName} {studentSurname} is created and added to {department.Name} department! Studens Lectures are:");
            ListingAndAddingToList(listOfDepartmentsIds, "Lectures");
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // 8. Atvaizduoti visas paskaitas pagal studentą.

        public void ShowStudentsLectures()
        {
            using var dbContext = new AplicationContext();

            // Listing all available students
            Console.WriteLine("Select Student which lectures you want to see (by typing its number):\n");
            var listOfStudents = new List<string>();
            ListingAndAddingToList(listOfStudents, "Students");

            // Adding selected Department 
            Console.WriteLine("\nSelect Student Number:");
            string studenttId = SelectingId(listOfStudents);

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

            // Listing all available students
            Console.WriteLine("Select which Student will change Department:\n");
            var listOfStudents = new List<string>();
            ListingAndAddingToList(listOfStudents, "Students");

            // Adding selected Student 
            Console.WriteLine("Select Student Number:");
            string studenttId = SelectingId(listOfStudents);
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
            ListingAndAddingToList(listOfDepartments, "Departments");

            // Adding selected Department
            string newDepartmentId = SelectingId(listOfDepartments);
            Console.WriteLine();

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

            // Listing all available students
            Console.WriteLine("Select which Student to Delete:\n");
            var listOfStudents = new List<string>();
            ListingAndAddingToList(listOfStudents, "Students");

            // Adding selected Department 
            Console.WriteLine("\nSelect Student Number:");
            string studenttId = SelectingId(listOfStudents);
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
            ListingEntries("Lectures");

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

            // Listing all available Departments
            Console.WriteLine("Select Departments which students you want to see:\n");
            var listOfDepartments = new List<string>();
            ListingAndAddingToList(listOfDepartments, "Departments");

            // Adding selected Department 
            Console.WriteLine("Select Deparment Number:");
            string departmentId = SelectingId(listOfDepartments);

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
            ListingAndAddingToList(listOfDepartments, "Departments");

            Console.WriteLine("Select Deparment Number:");
            // Adding selected Department 
            string departmentId = SelectingId(listOfDepartments);

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
            ListingEntries("Departments");

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

        // = Handling Listing all available entries and adding IDs to list =
        public void ListingAndAddingToList (List<string> listOfAllIds, string dbName)
        {
            using var dbContext = new AplicationContext();

            if (dbName == "Departments")
            {
                    foreach (var (item, index) in dbContext.Departments.WithIndex())
                    {
                        Console.WriteLine($"{index + 1}. {item.Name}");
                        listOfAllIds.Add(item.Id.ToString());
                    }
                Console.WriteLine();
            }
            else if (dbName == "Students")
            {
                foreach (var (item, index) in dbContext.Students.WithIndex())
                {
                    Console.WriteLine($"{index + 1}. {item.Name} {item.Surname}");
                    listOfAllIds.Add(item.Id.ToString());
                }
                Console.WriteLine();
            }
            else if(dbName == "Lectures")
            {
                foreach (var (item, index) in dbContext.Lectures.WithIndex())
                {
                    Console.WriteLine($"{index + 1}. {item.Name}");
                    listOfAllIds.Add(item.Id.ToString());
                }
                Console.WriteLine();
            }

        }

        // = Handling Listing all available entries =
        public void ListingEntries(string dbName)
        {
            using var dbContext = new AplicationContext();

            if (dbName == "Departments")
            {
                foreach (var (item, index) in dbContext.Departments.WithIndex())
                {
                    Console.WriteLine($"{index + 1}. {item.Name}");
                }
                Console.WriteLine();
            }
            else if (dbName == "Students")
            {
                foreach (var (item, index) in dbContext.Students.WithIndex())
                {
                    Console.WriteLine($"{index + 1}. {item.Name} {item.Surname}");
                }
                Console.WriteLine();
            }
            else if (dbName == "Lectures")
            {
                foreach (var (item, index) in dbContext.Lectures.WithIndex())
                {
                    Console.WriteLine($"{index + 1}. {item.Name}");
                }
                Console.WriteLine();
            }
        }

        // = Handling Adding selected Departments ID =
        public static string SelectingId(List<string> listOfIds)
        {
            using var dbContext = new AplicationContext();

            string selectedId = "";
            var input = Input();
            for (int i = 0; i < listOfIds.Count; i++)
            {
                if (input == i + 1)
                {
                    selectedId = listOfIds[i];
                }
            }

            return selectedId;
        }

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
