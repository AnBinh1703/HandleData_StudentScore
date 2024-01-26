using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
namespace HandleData_StudentScore
{
    /// <summary>
    /// Interaction logic for StatisticalScore.xaml
    /// </summary>
    public partial class StatisticalScore : Window
    {
        private ObservableCollection<DataItem> _dataItems;
        private int _totalCount;
        private double _totalSum;
        private double _totalAverage;

        public ObservableCollection<DataItem> DataItems
        {
            get { return _dataItems; }
            set { _dataItems = value; }
        }

        public int TotalCount
        {
            get { return _totalCount; }
            set { _totalCount = value; }
        }

        public double TotalSum
        {
            get { return _totalSum; }
            set { _totalSum = value; }
        }

        public double TotalAverage
        {
            get { return _totalAverage; }
            set { _totalAverage = value; }
        }
        public class DataItem
        {
            public int Year { get; set; }
            public int Student { get; set; }
            public int Mathematics { get; set; }
            public int Literature { get; set; }
            public int Physics { get; set; }
            public int Biology { get; set; }
            public int English { get; set; }
            public int Chemistry { get; set; }
            public int History { get; set; }
            public int Geography { get; set; }
            public int CivicEducation { get; set; }
        }
        public class StatisticalScore
        {
            public int Count { get; set; }
            public decimal TotalScore { get; set; }
            public decimal AverageScore { get; set; }
        }

        public StatisticalScore GetStatistics()
        {
            int count = 0;
            decimal totalScore = 0;
            decimal averageScore = 0;

            using (SqlConnection connection = new SqlConnection("your_connection_string_here"))
            {
                connection.Open();

                string query = @"
            SELECT COUNT(*) AS Count, SUM(Score) AS TotalScore, AVG(Score) AS AverageScore
            FROM Score
            INNER JOIN Student ON Score.StudentId = Student.Id
            INNER JOIN SchoolYear ON Student.SchoolYearId = SchoolYear.Id
            INNER JOIN Subject ON Score.SubjectId = Subject.Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            count = reader.GetInt32(0);
                            totalScore = reader.GetDecimal(1);
                            averageScore = reader.GetDecimal(2);
                        }
                    }
                }
            }

            return new StatisticalScore { Count = count, TotalScore = totalScore, AverageScore = averageScore };
        }
    }
}
        
    


