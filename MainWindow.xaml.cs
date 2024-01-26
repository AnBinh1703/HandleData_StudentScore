using CsvHelper;
using HandleData_StudentScore.Models;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using CsvHelper;

namespace HandleData_StudentScore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=ANN;Initial Catalog=Data_Score;Persist Security Info=True;User ID=sa;Password=12345;Encrypt=False;TrustServerCertificate=True";

        public MainWindow()
        {
            InitializeComponent();
            LoadSchoolYears(); // Populate the School Year combo box on window load
        }

        private void Button_Browser(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Select CSV File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                filePathTextBox.Text = openFileDialog.FileName;
            }
        }


        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedYear = cmbSchoolYear.SelectedItem?.ToString();

                // Clear data for the selected year
                ClearDataForYear(selectedYear);

                MessageBox.Show("Data cleared successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClearDataForYear(string schoolYear)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString)) // Use class-level ConnectionString
            {
                connection.Open();

                // Retrieve SchoolYearId for the selected school year
                int schoolYearId = GetSchoolYearId(schoolYear, connection);

                // Delete scores for students in the selected school year
                string deleteScoreQuery = "DELETE FROM Score WHERE StudentId IN (SELECT Id FROM Student WHERE SchoolYearId = @SchoolYearId)";
                using (SqlCommand command = new SqlCommand(deleteScoreQuery, connection))
                {
                    command.Parameters.AddWithValue("@SchoolYearId", schoolYearId);
                    command.ExecuteNonQuery();
                }

                // Delete students in the selected school year
                string deleteStudentQuery = "DELETE FROM Student WHERE SchoolYearId = @SchoolYearId";
                using (SqlCommand command = new SqlCommand(deleteStudentQuery, connection))
                {
                    command.Parameters.AddWithValue("@SchoolYearId", schoolYearId);
                    command.ExecuteNonQuery();
                }
            }
        }


        private int GetSchoolYearId(string schoolYear, SqlConnection connection)
        {
            // Implement logic to retrieve SchoolYearId for the selected school year
            // You can use a SELECT statement to query the SchoolYear table based on the provided schoolYear
            string selectQuery = "SELECT Id FROM SchoolYear WHERE Name = @SchoolYear";
            using (SqlCommand command = new SqlCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@SchoolYear", schoolYear);
                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return (int)result;
                }
                else
                {
                    throw new Exception($"School year '{schoolYear}' not found in the database.");
                }
            }
        }

        private void LoadSchoolYears()
        {
            try
            {
                // Clear existing items in the ComboBox
                cmbSchoolYear.Items.Clear();

                // Add years from 2010 to 2024 to the ComboBox
                for (int year = 2010; year <= 2024; year++)
                {
                    cmbSchoolYear.Items.Add(year.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading school years: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Import(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedYear = cmbSchoolYear.SelectedValue?.ToString();

                // Check if data already exists for the selected school year
                if (DataExistsForYear(selectedYear))
                {
                    MessageBox.Show("Data already exists for the selected school year. Import aborted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Read CSV file and insert data into SQL tables
                ImportData(selectedYear, filePathTextBox.Text);

                MessageBox.Show("Data imported successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportData(string selectedYear, string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // Assuming CsvRecord is your class representing the CSV structure
                    IEnumerable<CsvRecord> records = csv.GetRecords<CsvRecord>();

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // Insert school year if it doesn't exist
                        InsertSchoolYear(selectedYear, connection);

                        // Insert subjects if they don't exist
                        InsertSubjects(connection);

                        foreach (var record in records)
                        {
                            // Insert or retrieve student
                            int studentId = InsertStudent(record.SBD, selectedYear, connection);

                            // Insert scores
                            InsertScores(studentId, record, connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while importing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void InsertSchoolYear(string selectedYear, SqlConnection connection)
        {
            string insertQuery = "INSERT INTO SchoolYear (Name, ExamYear, Status) VALUES (@Name, @ExamYear, @Status)";
            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", selectedYear);
                command.Parameters.AddWithValue("@ExamYear", int.Parse(selectedYear)); // Assuming ExamYear is an integer
                command.Parameters.AddWithValue("@Status", "Active"); // Set a default status, adjust as needed

                command.ExecuteNonQuery();
            }
        }

        private void InsertSubjects(SqlConnection connection)
        {
            List<(string Code, string Name)> subjects = new List<(string Code, string Name)>
    {
        ("Toan", "Math"),
        ("Van", "Literature"),
        ("Ly", "Physics"),
        ("Sinh", "Biology"),
        ("Ngoaingu", "Foreign Language"),
        ("Hoa", "Chemistry"),
        ("Lichsu", "History"),
        ("Dialy", "Geography"),
        ("GDCD", "Civic Education")
    };

            foreach (var subject in subjects)
            {
                // Check if the subject already exists
                string selectQuery = "SELECT Id FROM Subject WHERE Code = @Code";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@Code", subject.Code);
                    object result = selectCommand.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        // Subject doesn't exist, so insert it
                        string insertQuery = "INSERT INTO Subject (Code, Name) VALUES (@Code, @Name)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@Code", subject.Code);
                            insertCommand.Parameters.AddWithValue("@Name", subject.Name);

                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private int InsertStudent(string studentCode, string selectedYear, SqlConnection connection)
        {
            // Check if the student already exists
            string selectQuery = "SELECT Id FROM Student WHERE StudentCode = @StudentCode AND SchoolYearId = @SchoolYearId";
            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@StudentCode", studentCode);
                selectCommand.Parameters.AddWithValue("@SchoolYearId", GetSchoolYearId(selectedYear, connection));
                object result = selectCommand.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    // Student doesn't exist, so insert it
                    string insertQuery = "INSERT INTO Student (StudentCode, SchoolYearId, Status) VALUES (@StudentCode, @SchoolYearId, @Status); SELECT SCOPE_IDENTITY()";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@StudentCode", studentCode);
                        insertCommand.Parameters.AddWithValue("@SchoolYearId", GetSchoolYearId(selectedYear, connection)); // GetSchoolYearId method from previous code
                        insertCommand.Parameters.AddWithValue("@Status", "Active"); // Set a default status, adjust as needed

                        return Convert.ToInt32(insertCommand.ExecuteScalar());
                    }
                }
                else
                {
                    // Student already exists, return its ID
                    return (int)result;
                }
            }
        }

        private void InsertScores(int studentId, CsvRecord record, SqlConnection connection)
        {
            Dictionary<string, string> subjectCodeToName = new Dictionary<string, string>
    {
        {"Toan", "Math"},
        {"Van", "Literature"},
        {"Ly", "Physics"},
        {"Sinh", "Biology"},
        {"Ngoaingu", "Foreign Language"},
        {"Hoa", "Chemistry"},
        {"Lichsu", "History"},
        {"Dialy", "Geography"},
        {"GDCD", "Civic Education"}
    };

            foreach (var subjectCode in subjectCodeToName.Keys)
            {
                // Get the corresponding SubjectId for the subject code
                int subjectId = GetSubjectIdByCode(subjectCode, connection);

                // Insert the score only if the subject code exists in the CSV record
                if (record.GetType().GetProperty(subjectCode) != null)
                {
                    // Get the score for the current subject
                    decimal? score = GetScoreForSubject(record, subjectCode);

                    // Check if the score is not null before inserting
                    if (score.HasValue)
                    {
                        string insertScoreQuery = "INSERT INTO Score (StudentId, SubjectId, Score) VALUES (@StudentId, @SubjectId, @Score)";
                        using (SqlCommand insertScoreCommand = new SqlCommand(insertScoreQuery, connection))
                        {
                            insertScoreCommand.Parameters.AddWithValue("@StudentId", studentId);
                            insertScoreCommand.Parameters.AddWithValue("@SubjectId", subjectId);
                            insertScoreCommand.Parameters.AddWithValue("@Score", score);

                            insertScoreCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private int GetSubjectIdByCode(string subjectCode, SqlConnection connection)
        {
            // Retrieve SubjectId based on the subject code
            string selectQuery = "SELECT Id FROM Subject WHERE Code = @Code";
            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@Code", subjectCode);
                object result = selectCommand.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return (int)result;
                }
                else
                {
                    throw new Exception($"Subject with code '{subjectCode}' not found in the database.");
                }
            }
        }

        private decimal? GetScoreForSubject(CsvRecord record, string subjectCode)
        {
            // Implement logic to get the score for the specified subject from the CsvRecord
            // You might want to adjust this based on how scores are represented in your CsvRecord
            switch (subjectCode)
            {
                case "Toan":
                    return record.Toan;
                case "Van":
                    return record.Van;
                case "Ly":
                    return record.Ly;
                case "Sinh":
                    return record.Sinh;
                case "Ngoaingu":
                    return record.Ngoaingu;
                case "Year":
                    return null; // Assuming Year is not a score, return null
                case "Hoa":
                    return record.Hoa;
                case "Lichsu":
                    return record.Lichsu;
                case "Dialy":
                    return record.Dialy;
                case "GDCD":
                    return record.GDCD;
                default:
                    return null; // Handle if the subject code is not recognized
            }
        }


        public class CsvRecord
        {
            public string SBD { get; set; }
            public decimal? Toan { get; set; }
            public decimal? Van { get; set; }
            public decimal? Ly { get; set; }
            public decimal? Sinh { get; set; }
            public decimal? Ngoaingu { get; set; }
            public int? Year { get; set; }
            public decimal? Hoa { get; set; }
            public decimal? Lichsu { get; set; }
            public decimal? Dialy { get; set; }
            public decimal? GDCD { get; set; }
        }


        private bool DataExistsForYear(string? selectedYear)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string selectQuery = "SELECT COUNT(*) FROM SchoolYear WHERE Name = @SchoolYear";
                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@SchoolYear", selectedYear);
                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

       

        private void Button_statistical(object sender, RoutedEventArgs e)
        {
            StatisticalScore statisticalScore = new StatisticalScore();
            statisticalScore.Show();
        }
    }
}









