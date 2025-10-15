using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EpicMarkdownManager
{
    // Simple command implementation for key bindings
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }

    public class RichMarkdownEditor : Grid, INotifyPropertyChanged
    {
        private RichTextBox _editor;
        private string? _currentFilePath;
        private bool _isDirty;
        private bool _isUpdating;
        private AppSettings _settings;
        private string? _baseDirectory;
        private DispatcherTimer _renderTimer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CurrentFilePath
        {
            get => _currentFilePath;
            private set
            {
                _currentFilePath = value;
                OnPropertyChanged();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                _isDirty = value;
                OnPropertyChanged();
            }
        }

        public RichTextBox Editor => _editor;

        public RichMarkdownEditor()
        {
            _settings = AppSettings.Instance;

            // Create rich text editor
            _editor = CreateRichTextBox();
            Children.Add(_editor);

            // Initialize base directory
            _baseDirectory = Environment.CurrentDirectory;

            // Setup render timer for live markdown rendering
            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _renderTimer.Tick += (s, e) =>
            {
                _renderTimer.Stop();
                RenderMarkdown();
            };

            // Setup keyboard shortcuts
            SetupKeyboardShortcuts();

            // Listen to settings changes
            _settings.PropertyChanged += (s, e) => ApplySettings();
        }

        private RichTextBox CreateRichTextBox()
        {
            var editor = new RichTextBox
            {
                Background = _settings.GetBrush(_settings.Theme.BackgroundColor),
                Foreground = _settings.GetBrush(_settings.Theme.TextColor),
                FontFamily = new FontFamily(_settings.Fonts.FontFamily),
                FontSize = _settings.Fonts.RegularTextSize,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10),
                AcceptsTab = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            // Setup text changed handler
            editor.TextChanged += OnTextChanged;

            // Set up paragraph spacing
            editor.Document.LineHeight = 1;

            return editor;
        }

        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (!_isUpdating)
            {
                IsDirty = true;
                _renderTimer.Stop();
                _renderTimer.Start();
            }
        }

        private void RenderMarkdown()
        {
            if (_isUpdating) return;

            _isUpdating = true;
            var caretPosition = _editor.CaretPosition;

            try
            {
                // Get plain text
                var text = new TextRange(_editor.Document.ContentStart, _editor.Document.ContentEnd).Text;

                // Clear and rebuild document
                _editor.Document.Blocks.Clear();

                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (var line in lines)
                {
                    var paragraph = ProcessLine(line);
                    _editor.Document.Blocks.Add(paragraph);
                }

                // Try to restore caret position
                try
                {
                    _editor.CaretPosition = caretPosition;
                }
                catch
                {
                    // If position is invalid, move to end
                    _editor.CaretPosition = _editor.Document.ContentEnd;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private Paragraph ProcessLine(string line)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 2, 0, 2),
                LineHeight = 1
            };

            // Check for headings
            var headingMatch = Regex.Match(line, @"^(#{1,3})\s+(.*)$");
            if (headingMatch.Success)
            {
                var level = headingMatch.Groups[1].Value.Length;
                var text = line;

                var run = new Run(text);

                switch (level)
                {
                    case 1:
                        run.FontSize = _settings.Fonts.Heading1Size;
                        run.Foreground = _settings.GetBrush(_settings.Colors.Heading1);
                        run.TextDecorations = TextDecorations.Underline;
                        paragraph.LineHeight = _settings.Fonts.Heading1Size * 0.1;
                        break;
                    case 2:
                        run.FontSize = _settings.Fonts.Heading2Size;
                        run.Foreground = _settings.GetBrush(_settings.Colors.Heading2);
                        run.FontWeight = FontWeights.Bold;
                        paragraph.LineHeight = _settings.Fonts.Heading2Size * 0.1;
                        break;
                    case 3:
                        run.FontSize = _settings.Fonts.Heading3Size;
                        run.Foreground = _settings.GetBrush(_settings.Colors.Heading3);
                        run.TextDecorations = TextDecorations.Underline;
                        paragraph.LineHeight = _settings.Fonts.Heading3Size * 0.1;
                        break;
                }

                paragraph.Inlines.Add(run);
            }
            // Check for list items
            else if (Regex.IsMatch(line.TrimStart(), @"^[-*]\s"))
            {
                // Count indentation
                var indent = line.TakeWhile(c => c == ' ' || c == '\t').Count();
                paragraph.TextIndent = indent * 20;

                // Add bullet
                var bulletRun = new Run("• ")
                {
                    Foreground = _settings.GetBrush(_settings.Colors.BulletPoint)
                };
                paragraph.Inlines.Add(bulletRun);

                // Process the rest of the line
                var content = Regex.Replace(line.TrimStart(), @"^[-*]\s+", "");
                ParseInlineMarkdown(paragraph.Inlines, content);
            }
            else
            {
                // Regular text with inline formatting
                if (!string.IsNullOrWhiteSpace(line))
                {
                    ParseInlineMarkdown(paragraph.Inlines, line);
                }
                else
                {
                    paragraph.Inlines.Add(new Run(" ")); // Empty line placeholder
                }
            }

            return paragraph;
        }

        private void ParseInlineMarkdown(InlineCollection inlines, string text)
        {
            var pattern = @"(\*\*[^*]+\*\*|\*[^*]+\*|\[([^\]]+)\]\(([^)]+)\)|`[^`]+`|[^*\[\]`]+)";
            var matches = Regex.Matches(text, pattern);

            foreach (Match match in matches)
            {
                var value = match.Value;

                if (value.StartsWith("**") && value.EndsWith("**"))
                {
                    // Bold (red)
                    var content = value.Substring(2, value.Length - 4);
                    inlines.Add(new Run(value)
                    {
                        Foreground = _settings.GetBrush(_settings.Colors.Bold),
                        FontWeight = FontWeights.Bold
                    });
                }
                else if (value.StartsWith("*") && value.EndsWith("*") && !value.StartsWith("**"))
                {
                    // Italic (yellow)
                    var content = value.Substring(1, value.Length - 2);
                    inlines.Add(new Run(value)
                    {
                        Foreground = _settings.GetBrush(_settings.Colors.Italic),
                        FontStyle = FontStyles.Italic
                    });
                }
                else if (match.Groups[2].Success)
                {
                    // Link
                    var linkText = match.Groups[2].Value;
                    var linkUrl = match.Groups[3].Value;

                    var hyperlink = new Hyperlink(new Run($"[{linkText}]({linkUrl})"))
                    {
                        NavigateUri = new Uri(linkUrl, UriKind.RelativeOrAbsolute),
                        Foreground = _settings.GetBrush(_settings.Colors.Link),
                        TextDecorations = null
                    };

                    hyperlink.RequestNavigate += (sender, e) =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
                            {
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to open link: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    };

                    inlines.Add(hyperlink);
                }
                else if (value.StartsWith("`") && value.EndsWith("`"))
                {
                    // Inline code
                    var content = value.Substring(1, value.Length - 2);
                    inlines.Add(new Run(value)
                    {
                        Background = _settings.GetBrush(_settings.Colors.CodeBlock),
                        FontFamily = new FontFamily("Consolas")
                    });
                }
                else
                {
                    // Regular text
                    inlines.Add(new Run(value)
                    {
                        Foreground = _settings.GetBrush(_settings.Colors.RegularText)
                    });
                }
            }
        }

        private void SetupKeyboardShortcuts()
        {
            _editor.InputBindings.Add(new KeyBinding(
                new RelayCommand(Save),
                new KeyGesture(Key.S, ModifierKeys.Control)));

            _editor.InputBindings.Add(new KeyBinding(
                new RelayCommand(InsertBold),
                new KeyGesture(Key.B, ModifierKeys.Control)));

            _editor.InputBindings.Add(new KeyBinding(
                new RelayCommand(InsertItalic),
                new KeyGesture(Key.I, ModifierKeys.Control)));

            _editor.InputBindings.Add(new KeyBinding(
                new RelayCommand(() => PasteSpecial()),
                new KeyGesture(Key.V, ModifierKeys.Control)));

            // Prevent default bold/italic formatting
            _editor.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.B && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    InsertBold();
                }
                else if (e.Key == Key.I && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    InsertItalic();
                }
            };
        }

        public void LoadFile(string filePath)
        {
            try
            {
                CurrentFilePath = filePath;
                _baseDirectory = Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory;

                var content = File.ReadAllText(filePath);

                _isUpdating = true;
                _editor.Document.Blocks.Clear();
                _editor.Document.Blocks.Add(new Paragraph(new Run(content)));
                _isUpdating = false;

                RenderMarkdown();
                IsDirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NewFile()
        {
            CurrentFilePath = null;
            _editor.Document.Blocks.Clear();
            IsDirty = false;
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                SaveAs();
            }
            else
            {
                try
                {
                    var text = new TextRange(_editor.Document.ContentStart, _editor.Document.ContentEnd).Text;
                    File.WriteAllText(CurrentFilePath, text);
                    IsDirty = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save file: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".md"
            };

            if (dialog.ShowDialog() == true)
            {
                CurrentFilePath = dialog.FileName;
                Save();
            }
        }

        private void InsertBold()
        {
            var selection = _editor.Selection;
            if (!selection.IsEmpty)
            {
                selection.Text = $"**{selection.Text}**";
            }
            else
            {
                _editor.CaretPosition.InsertTextInRun("**text**");
            }
        }

        private void InsertItalic()
        {
            var selection = _editor.Selection;
            if (!selection.IsEmpty)
            {
                selection.Text = $"*{selection.Text}*";
            }
            else
            {
                _editor.CaretPosition.InsertTextInRun("*text*");
            }
        }

        private void PasteSpecial()
        {
            if (Clipboard.ContainsImage() && !string.IsNullOrEmpty(_baseDirectory))
            {
                try
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        // Create images directory
                        var imagesDir = Path.Combine(_baseDirectory, _settings.Images.ImageFolder);
                        Directory.CreateDirectory(imagesDir);

                        // Generate unique filename
                        var fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var filePath = Path.Combine(imagesDir, fileName);

                        // Save image
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            encoder.Save(fileStream);
                        }

                        // Insert markdown reference
                        var relativePath = Path.Combine(_settings.Images.ImageFolder, fileName)
                            .Replace('\\', '/');
                        _editor.CaretPosition.InsertTextInRun($"![Image]({relativePath})");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to paste image: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (Clipboard.ContainsText())
            {
                _editor.CaretPosition.InsertTextInRun(Clipboard.GetText());
            }
        }

        public string GetPlainText()
        {
            return new TextRange(_editor.Document.ContentStart, _editor.Document.ContentEnd).Text;
        }

        private void ApplySettings()
        {
            _editor.FontFamily = new FontFamily(_settings.Fonts.FontFamily);
            _editor.FontSize = _settings.Fonts.RegularTextSize;
            _editor.Background = _settings.GetBrush(_settings.Theme.BackgroundColor);
            _editor.Foreground = _settings.GetBrush(_settings.Theme.TextColor);

            RenderMarkdown();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}