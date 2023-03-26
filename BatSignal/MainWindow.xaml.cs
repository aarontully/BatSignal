using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BatSignal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileSystemWatcher? _watcher;
        private FileSystemWatcher? _destWatcher;

        public MainWindow()
        {
            InitializeComponent();
            StartWatcherBtn.IsEnabled = false;
            StopWatcherBtn.IsEnabled = false;
            FolderPathTextBox.TextChanged += FolderPathTextBox_TextChanged;
        }

        private void FolderPathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                StartWatcherBtn.IsEnabled = false;
            }
            else
            {
                StartWatcherBtn.IsEnabled = true;
            }
        }

        private void StartWatcherBtn_Click(object sender, RoutedEventArgs e)
        {
            // Get path from the text box
            if(!string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                // disable StartWatcherBtn and enable StopWatcherBtn
                StartWatcherBtn.IsEnabled = false;
                StopWatcherBtn.IsEnabled = true;
                GreenLight.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009900"));
                RedLight.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d3d3"));

                string folderPath = FolderPathTextBox.Text;

                // Create a new instance of the filesystemwatcher class
                _watcher = new()
                {
                    // Set the properties of the watcher
                    Path = folderPath,
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true
                };

                // Create another instance for the destination path
                _destWatcher = new()
                {
                    Path = DestinationFolderPath.Text,
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true
                };

                // Register event handlers for the watcher events
                _watcher.Created += OnFileCreated;
                _watcher.Deleted += OnFileDeleted;
                _watcher.Renamed += OnWatcherRenamed;

                // Register event handlers for destination watcher events
                _destWatcher.Created += _destWatcher_Created;
                _destWatcher.Deleted += _destWatcher_Deleted;
                _destWatcher.Renamed += _destWatcher_Renamed;

                // Log that watcher has started
                Dispatcher.Invoke(() =>
                {
                    var sb = new StringBuilder(MessageTextBlock.Text);
                    sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - BatSignal has been switched on\n");
                    MessageTextBlock.Text = sb.ToString();
                });
            }
            else
            {
                MessageBox.Show("You must enter a folder location");
            }
        }

        private void _destWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var sb = new StringBuilder(MessageTextBlock.Text);
                sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - File renamed from {e.OldName} to {e.Name}\n");
                MessageTextBlock.Text = sb.ToString();
            });
        }

        private void _destWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            // Perform actions on the deleted file
            Dispatcher.Invoke(() =>
            {
                var sb = new StringBuilder(MessageTextBlock.Text);
                sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - File deleted: {e.Name}\n");
                MessageTextBlock.Text = sb.ToString();
            });
        }

        private void _destWatcher_Created(object sender, FileSystemEventArgs e)
        {
            // Perform actions on the created file
            Dispatcher.Invoke(() =>
            {
                var sb = new StringBuilder(MessageTextBlock.Text);
                sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - File appeared: {e.Name}\n");
                MessageTextBlock.Text = sb.ToString();
            });
        }

        private void OnWatcherRenamed(object sender, RenamedEventArgs e)
        {
            // Perform actions on the renamed file
            Dispatcher.Invoke(() =>
            {
                var sb = new StringBuilder(MessageTextBlock.Text);
                sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - File renamed from {e.OldName} to {e.Name}\n");
                MessageTextBlock.Text = sb.ToString();
            });
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            // Perform actions on the deleted file
            Dispatcher.Invoke(() =>
            {
                var sb = new StringBuilder(MessageTextBlock.Text);
                sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - File deleted: {e.Name}\n");
                MessageTextBlock.Text = sb.ToString();
            });
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            string fileName = string.Empty;
            Dispatcher.Invoke(() => fileName = e.Name);
            string filePath = e.FullPath;

            // Perform actions on the created file
            Dispatcher.Invoke(() =>
            {
                var sb = new StringBuilder(MessageTextBlock.Text);
                sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - File appeared: {e.Name}\n");
                MessageTextBlock.Text = sb.ToString();
            });

            Dispatcher.Invoke(() =>
            {
                if(!string.IsNullOrWhiteSpace(DestinationFolderPath.Text) && Directory.Exists(DestinationFolderPath.Text))
                {
                    string destinationPath = Path.Combine(DestinationFolderPath.Text, fileName);
                    if(!File.Exists(destinationPath))
                    {
                        File.Move(filePath, destinationPath);
                        var sb = new StringBuilder(MessageTextBlock.Text);
                        MessageTextBlock.Text += $"{DateTime.Now:dddd, dd MMMM yyyy} - Moved file {fileName} to {destinationPath}\n";
                        MessageTextBlock.Text = sb.ToString();
                    }
                    else
                    {
                        var sb = new StringBuilder(MessageTextBlock.Text);
                        MessageTextBlock.Text += $"{DateTime.Now:dddd, dd MMMM yyyy} - File {fileName} already exists in {destinationPath}\n";
                        MessageTextBlock.Text = sb.ToString();
                    }
                }
                else
                {
                    var sb = new StringBuilder(MessageTextBlock.Text);
                    MessageTextBlock.Text += $"{DateTime.Now:dddd, dd MMMM yyyy} - File {fileName} processed but not moved\n";
                    MessageTextBlock.Text = sb.ToString();
                }
            });
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var selectedFolder = dialog.FileName;
                FolderPathTextBox.Text = selectedFolder;
            }
        }

        private void StopWatcherBtn_Click(object sender, RoutedEventArgs e)
        {
            // Stop the watcher if it is running
            if(_watcher is not null)
            {
                // Log that watcher has stopped
                Dispatcher.Invoke(() =>
                {
                    var sb = new StringBuilder(MessageTextBlock.Text);
                    sb.Insert(0, $"{DateTime.Now:dddd, dd MMMM yyyy} - BatSignal has been switched off\n");
                    MessageTextBlock.Text = sb.ToString();
                });

                RedLight.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D40000"));
                GreenLight.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d3d3"));

                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;

                //disable StopWatcherBtn and if there is text in FolderPathTextBox.Text enable StartWatcherBtn otherwise keep it disabled
                StopWatcherBtn.IsEnabled = false;
                if(!string.IsNullOrEmpty(FolderPathTextBox.Text))
                {
                    StartWatcherBtn.IsEnabled = true;
                }
                else
                {
                    StartWatcherBtn.IsEnabled = false;
                }
            }
        }

        private void DestBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var selectedFolder = dialog.FileName;
                DestinationFolderPath.Text = selectedFolder;
            }
        }
    }
}
