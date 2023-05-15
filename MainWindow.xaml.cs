using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace eindproject_trim2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class MediaFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public MediaFile(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }

        public string getFileName()
        {
            return FileName;
        }

        virtual public string WhatAreYou()
        {
            return "I don't know";
        }
    }
    public class MusicFile : MediaFile
    {
        public string Genre { get; set; }

        public MusicFile(string fileName, string filePath, string genre) : base(fileName, filePath)
        {
            Genre = genre;
        }

        public override string WhatAreYou()
        {
            return "I am Music";
        }

    }

    public class MovieFile : MediaFile
    {
        public string Genre { get; set; }

        public MovieFile(string fileName, string filePath, string genre) : base(fileName, filePath)
        {
            Genre = genre;
        }
        public override string WhatAreYou()
        {
            return "I am Movie";
        }

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string directoryPath = @"C:\Users\nolfm\Desktop\oop\project\muziek map"; // Vervang dit pad door het pad naar jouw specifieke map
        private bool userIsDraggingSlider = false;
        private bool mediaPlayerIsPlaying = false;
        private List<MediaFile> items = new List<MediaFile>();
        private string jsonFilePath = "data.json"; // Vervang dit door het gewenste pad naar het JSON-bestand


        public MainWindow()
        {
            InitializeComponent();
            LoadItemsFromDirectory();

            // DispatcherTimer voor het bijwerken van de slider
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveItemsToJson();
        }

        private void LoadItemsFromDirectory()
        {
            if (Directory.Exists(directoryPath))
            {
                string[] extensions = { "*.mp3", "*.mp4" };
                List<string> items_str = new List<string>();

                foreach (string ext in extensions)
                {
                    string[] files = Directory.GetFiles(directoryPath, ext);

                    foreach (string file in files)
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        if (ext.Contains("mp3"))
                        {
                            items.Add(new MusicFile(fileName, file, "Genre"));
                            items_str.Add(fileName);
                        }
                        else if (ext.Contains("mp4"))
                        {
                            items.Add(new MovieFile(fileName, file, "Genre"));
                            items_str.Add(fileName);
                        }
                    }
                }

                listBox.ItemsSource = items_str;

            }
            else
            {
                MessageBox.Show("De opgegeven map bestaat niet!");
            }
        }

        private void SaveItemsToJson()
        {
            string jsonData = JsonSerializer.Serialize(items);
            System.IO.File.WriteAllText(jsonFilePath, jsonData);
        }

        private void LoadItemsFromJson()
        {
            if (System.IO.File.Exists(jsonFilePath))
            {
                string jsonData = System.IO.File.ReadAllText(jsonFilePath);
                items = JsonSerializer.Deserialize<List<MediaFile>>(jsonData);
                listBox.ItemsSource = items.Select(file => file.FileName);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem is string selectedValue)
            {
                string filePath = System.IO.Path.Combine(directoryPath, selectedValue);
                foreach (MediaFile file in items)
                {
                    if (file.getFileName().Contains(selectedValue))
                    {
                        lblWhatAreYou.Content = file.WhatAreYou();
                    }
                }
                mePlayer.Source = new Uri(filePath, UriKind.RelativeOrAbsolute);
            }
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
            mediaPlayerIsPlaying = false;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (mePlayer.Volume < 1)
                    mePlayer.Volume += 0.1;
            }
            else
            {
                if (mePlayer.Volume > 0)
                    mePlayer.Volume -= 0.1;
            }
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan ts = TimeSpan.FromSeconds(sliProgress.Value);
            mePlayer.Position = ts;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;

                // Update the time label
                TimeSpan currentPosition = mePlayer.Position;
                lblProgressStatus.Text = mePlayer.Position.ToString(@"hh\:mm\:ss");
            }
        }
    }
}
