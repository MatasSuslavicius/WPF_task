using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Data.OleDb;
using System.IO;

namespace UVS_task
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Random random = new Random();
        private List<Thread> threads = new List<Thread>();
        private bool isRunning = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartThreadsClick(object sender, RoutedEventArgs e)
        {
            ListViewLinesData.Items.Clear();
            CleanDatabase();
            isRunning = true;

            int selectedThreadCount = (int)ThreadSlider.Value;

            for (int i = 0; i < selectedThreadCount; i++)
            {
                int threadNumber = i + 1;
                Thread thread = new Thread(() => GenerateLines(threadNumber));
                threads.Add(thread);
                thread.Start();
            }

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        private void StopThreadsClick(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void GenerateLines(int threadNumber)
        {
            while (isRunning)
            {
                int intervalMilliseconds = random.Next(500, 2001);
                Thread.Sleep(intervalMilliseconds);

                int lineLength = random.Next(5, 11);
                string line= "";

                for (int i = 0; i < lineLength; i++)
                {
                    line += (char)random.Next(65, 91);
                }

                GeneratedLine generatedLine = new GeneratedLine
                {
                    ThreadId = threadNumber,
                    Line = line,
                    GenerationDate = DateTime.Now,
                };

                Dispatcher.Invoke(() =>
                {
                    ListViewLinesData.Items.Add(generatedLine);

                    if (ListViewLinesData.Items.Count > 20)
                    {
                        ListViewLinesData.Items.RemoveAt(0);
                    }
                });

                SaveGeneratedLineToDatabase(generatedLine);
            }
            return;
        }
        private static void SaveGeneratedLineToDatabase(GeneratedLine line)
        {
            using OleDbConnection connection = new OleDbConnection($"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\\Data.mdb");
            connection.Open();

            using OleDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO GeneratedData (ThreadID, [Time], Data) VALUES (?, ?, ?)";
            cmd.Parameters.AddWithValue("ThreadID", line.ThreadId);
            cmd.Parameters.AddWithValue("Time", line.GenerationDate.ToString("MM/dd/yyyy HH:mm:ss"));
            cmd.Parameters.AddWithValue("Data", line.Line);
            cmd.ExecuteNonQuery();
        }
        private static void CleanDatabase()
        {
            using OleDbConnection connection = new OleDbConnection($"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\\Data.mdb");
            connection.Open();

            using OleDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM GeneratedData";
            cmd.ExecuteNonQuery();
        }

        public class GeneratedLine
        {
            public int ThreadId { get; set; }
            public string Line { get; set; }
            public DateTime GenerationDate { get; set; }
        }
    }
}
