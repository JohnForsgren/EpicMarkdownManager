using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EpicMarkdownManager
{
    public class FileExplorer : TreeView, INotifyPropertyChanged
    {
        private string? _rootFolder;
        private FileSystemWatcher? _watcher;

        public event EventHandler<string>? FileSelected;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? RootFolder
        {
            get => _rootFolder;
            set
            {
                _rootFolder = value;
                OnPropertyChanged();
                LoadFileTree();
            }
        }

        public FileExplorer()
        {
            Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));
            Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212));
            BorderThickness = new Thickness(0);
            Padding = new Thickness(5);

            MouseDoubleClick += OnMouseDoubleClick;
            KeyDown += OnKeyDown;
        }

        private void LoadFileTree()
        {
            Items.Clear();

            if (string.IsNullOrEmpty(_rootFolder) || !Directory.Exists(_rootFolder))
                return;

            var rootItem = CreateDirectoryNode(new DirectoryInfo(_rootFolder));
            Items.Add(rootItem);
            rootItem.IsExpanded = true;

            SetupFileWatcher();
        }

        private void SetupFileWatcher()
        {
            _watcher?.Dispose();

            if (!string.IsNullOrEmpty(_rootFolder))
            {
                _watcher = new FileSystemWatcher(_rootFolder)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                                   NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Created += (s, e) => Dispatcher.Invoke(LoadFileTree);
                _watcher.Deleted += (s, e) => Dispatcher.Invoke(LoadFileTree);
                _watcher.Renamed += (s, e) => Dispatcher.Invoke(LoadFileTree);

                _watcher.EnableRaisingEvents = true;
            }
        }

        private TreeViewItem CreateDirectoryNode(DirectoryInfo directory)
        {
            var node = new TreeViewItem
            {
                Header = CreateNodeHeader(directory.Name, true),
                Tag = directory.FullName,
                FontWeight = FontWeights.Normal
            };

            try
            {
                // Add subdirectories
                foreach (var subDir in directory.GetDirectories().OrderBy(d => d.Name))
                {
                    if (!subDir.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        node.Items.Add(CreateDirectoryNode(subDir));
                    }
                }

                // Add markdown files
                foreach (var file in directory.GetFiles("*.md").OrderBy(f => f.Name))
                {
                    node.Items.Add(CreateFileNode(file));
                }

                // Add text files
                foreach (var file in directory.GetFiles("*.txt").OrderBy(f => f.Name))
                {
                    node.Items.Add(CreateFileNode(file));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }

            return node;
        }

        private TreeViewItem CreateFileNode(FileInfo file)
        {
            var node = new TreeViewItem
            {
                Header = CreateNodeHeader(file.Name, false),
                Tag = file.FullName,
                FontWeight = FontWeights.Normal
            };

            return node;
        }

        private StackPanel CreateNodeHeader(string text, bool isDirectory)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // Add icon
            var icon = new TextBlock
            {
                Text = isDirectory ? "📁" : "📄",
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            panel.Children.Add(icon);

            // Add text
            var textBlock = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center
            };
            panel.Children.Add(textBlock);

            return panel;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                if (File.Exists(path))
                {
                    FileSelected?.Invoke(this, path);
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SelectedItem is TreeViewItem item && item.Tag is string path)
                {
                    if (File.Exists(path))
                    {
                        FileSelected?.Invoke(this, path);
                    }
                    else if (Directory.Exists(path))
                    {
                        item.IsExpanded = !item.IsExpanded;
                    }
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshTree()
        {
            LoadFileTree();
        }
    }
}