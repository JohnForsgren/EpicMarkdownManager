using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace EpicMarkdownManager
{
    public partial class MainWindow : Window
    {
        private string? _currentProjectFolder;
        private DispatcherTimer _autoSaveTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Setup file explorer event
            fileExplorer.FileSelected += OnFileSelected;

            // Setup editor property change notifications
            markdownEditor.PropertyChanged += OnEditorPropertyChanged;

            // Setup keyboard shortcuts
            SetupKeyboardShortcuts();

            // Setup auto-save timer
            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(2)
            };
            _autoSaveTimer.Tick += (s, e) => AutoSave();

            // Show folder selection dialog on startup
            Loaded += (s, e) => ShowFolderSelectionDialog();
        }

        private void SetupKeyboardShortcuts()
        {
            // File operations
            CommandBindings.Add(new CommandBinding(
                new RoutedCommand("OpenFolder", typeof(MainWindow),
                    new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) }),
                (s, e) => OpenFolder_Click(s, e)));

            CommandBindings.Add(new CommandBinding(
                new RoutedCommand("NewFile", typeof(MainWindow),
                    new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) }),
                (s, e) => NewFile_Click(s, e)));

            CommandBindings.Add(new CommandBinding(
                new RoutedCommand("SaveFile", typeof(MainWindow),
                    new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) }),
                (s, e) => Save_Click(s, e)));

            CommandBindings.Add(new CommandBinding(
                new RoutedCommand("SaveAs", typeof(MainWindow),
                    new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift) }),
                (s, e) => SaveAs_Click(s, e)));

            // Refresh
            CommandBindings.Add(new CommandBinding(
                new RoutedCommand("Refresh", typeof(MainWindow),
                    new InputGestureCollection { new KeyGesture(Key.F5) }),
                (s, e) => Refresh_Click(s, e)));
        }

        private void ShowFolderSelectionDialog()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select your markdown project folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenProjectFolder(dialog.SelectedPath);
            }
        }

        private void OpenProjectFolder(string folderPath)
        {
            _currentProjectFolder = folderPath;
            fileExplorer.RootFolder = folderPath;
            StatusText.Text = $"Opened: {Path.GetFileName(folderPath)}";
            Title = $"Epic Markdown Manager - {Path.GetFileName(folderPath)}";

            // Start auto-save
            _autoSaveTimer.Start();
        }

        private void OnFileSelected(object? sender, string filePath)
        {
            if (CheckSaveChanges())
            {
                markdownEditor.LoadFile(filePath);
                UpdateStatusBar();
            }
        }

        private void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RichMarkdownEditor.IsDirty) ||
                e.PropertyName == nameof(RichMarkdownEditor.CurrentFilePath))
            {
                UpdateStatusBar();
            }
        }

        private void UpdateStatusBar()
        {
            FilePathText.Text = markdownEditor.CurrentFilePath ?? "Untitled";
            ModifiedIndicator.Text = markdownEditor.IsDirty ? "●" : "";
        }

        private bool CheckSaveChanges()
        {
            if (markdownEditor.IsDirty)
            {
                var result = MessageBox.Show(
                    "Do you want to save your changes?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Save_Click(null, null);
                    return !markdownEditor.IsDirty;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private void AutoSave()
        {
            if (markdownEditor.IsDirty && !string.IsNullOrEmpty(markdownEditor.CurrentFilePath))
            {
                try
                {
                    File.WriteAllText(markdownEditor.CurrentFilePath, markdownEditor.GetPlainText());
                    StatusText.Text = "Auto-saved";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Auto-save failed: {ex.Message}";
                }
            }
        }

        // Menu Event Handlers
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderSelectionDialog();
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSaveChanges())
            {
                markdownEditor.NewFile();
                UpdateStatusBar();
            }
        }

        private void Save_Click(object? sender, RoutedEventArgs? e)
        {
            markdownEditor.Editor.Focus();
            ApplicationCommands.Save.Execute(null, markdownEditor);
            UpdateStatusBar();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".md",
                InitialDirectory = _currentProjectFolder
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, markdownEditor.GetPlainText());
                    markdownEditor.LoadFile(dialog.FileName);
                    UpdateStatusBar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save file: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSaveChanges())
            {
                Close();
            }
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            var selection = markdownEditor.Editor.Selection;
            if (!selection.IsEmpty)
            {
                selection.Text = $"**{selection.Text}**";
            }
            else
            {
                markdownEditor.Editor.CaretPosition.InsertTextInRun("**text**");
            }
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            var selection = markdownEditor.Editor.Selection;
            if (!selection.IsEmpty)
            {
                selection.Text = $"*{selection.Text}*";
            }
            else
            {
                markdownEditor.Editor.CaretPosition.InsertTextInRun("*text*");
            }
        }

        private void PasteImage_Click(object sender, RoutedEventArgs e)
        {
            // This will be handled by the RichMarkdownEditor's paste special method
            // triggered through Ctrl+V
            markdownEditor.Editor.Paste();
        }

        private void InsertH1_Click(object sender, RoutedEventArgs e)
        {
            InsertAtLineStart("# ");
        }

        private void InsertH2_Click(object sender, RoutedEventArgs e)
        {
            InsertAtLineStart("## ");
        }

        private void InsertH3_Click(object sender, RoutedEventArgs e)
        {
            InsertAtLineStart("### ");
        }

        private void InsertList_Click(object sender, RoutedEventArgs e)
        {
            InsertAtLineStart("- ");
        }

        private void InsertLink_Click(object sender, RoutedEventArgs e)
        {
            var linkDialog = new Window
            {
                Title = "Insert Link",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = AppSettings.Instance.GetBrush(AppSettings.Instance.Theme.BackgroundColor)
            };

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

            var textLabel = new System.Windows.Controls.TextBlock
            {
                Text = "Link Text:",
                Margin = new Thickness(10, 10, 10, 5),
                Foreground = AppSettings.Instance.GetBrush(AppSettings.Instance.Theme.TextColor)
            };
            var textBox = new System.Windows.Controls.TextBox
            {
                Margin = new Thickness(10, 0, 10, 10)
            };

            var urlLabel = new System.Windows.Controls.TextBlock
            {
                Text = "URL:",
                Margin = new Thickness(10, 10, 10, 5),
                Foreground = AppSettings.Instance.GetBrush(AppSettings.Instance.Theme.TextColor)
            };
            var urlBox = new System.Windows.Controls.TextBox
            {
                Margin = new Thickness(10, 0, 10, 10)
            };

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(5),
                IsDefault = true
            };
            okButton.Click += (s, ev) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text) && !string.IsNullOrWhiteSpace(urlBox.Text))
                {
                    InsertMarkdown($"[{textBox.Text}](", $"{urlBox.Text})");
                    linkDialog.Close();
                }
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 75,
                Margin = new Thickness(5),
                IsCancel = true
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            System.Windows.Controls.Grid.SetRow(textLabel, 0);
            System.Windows.Controls.Grid.SetRow(textBox, 1);
            System.Windows.Controls.Grid.SetRow(urlLabel, 2);
            System.Windows.Controls.Grid.SetRow(urlBox, 3);
            System.Windows.Controls.Grid.SetRow(buttonPanel, 4);

            grid.Children.Add(textLabel);
            grid.Children.Add(textBox);
            grid.Children.Add(urlLabel);
            grid.Children.Add(urlBox);
            grid.Children.Add(buttonPanel);

            linkDialog.Content = grid;
            linkDialog.ShowDialog();
        }

        private void TogglePreview_Click(object sender, RoutedEventArgs e)
        {
            // This would toggle the preview pane visibility
            // Implementation depends on your layout preferences
        }

        private void ToggleFileExplorer_Click(object sender, RoutedEventArgs e)
        {
            fileExplorer.Visibility = ToggleFileExplorerMenuItem.IsChecked ?
                Visibility.Visible : Visibility.Collapsed;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            fileExplorer.RefreshTree();
            if (!string.IsNullOrEmpty(markdownEditor.CurrentFilePath))
            {
                markdownEditor.LoadFile(markdownEditor.CurrentFilePath);
            }
        }

        private void StartPomodoroTimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Try to find and launch the Pomodoro Timer app
                var possiblePaths = new[]
                {
                    Path.Combine(Environment.CurrentDirectory, "PomodoroTimer.exe"),
                    Path.Combine(Environment.CurrentDirectory, "..", "PomodoroTimer", "PomodoroTimer.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PomodoroTimer", "PomodoroTimer.exe")
                };

                bool launched = false;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                        launched = true;
                        break;
                    }
                }

                if (!launched)
                {
                    MessageBox.Show(
                        "Pomodoro Timer application not found. Please ensure it's installed in the same directory or configure the path.",
                        "Timer Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch Pomodoro Timer: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                Process.Start(new ProcessStartInfo(settingsPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertMarkdown(string prefix, string suffix)
        {
            var selection = markdownEditor.Editor.Selection;
            if (!selection.IsEmpty)
            {
                selection.Text = $"{prefix}{selection.Text}{suffix}";
            }
            else
            {
                markdownEditor.Editor.CaretPosition.InsertTextInRun($"{prefix}text{suffix}");
            }
        }

        private void InsertAtLineStart(string text)
        {
            markdownEditor.Editor.CaretPosition.InsertTextInRun(text);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CheckSaveChanges())
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
    }
}