using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Windows.Media;

namespace EpicMarkdownManager
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static AppSettings? _instance;
        private FileSystemWatcher? _watcher;
        private string _settingsPath;

        public static AppSettings Instance => _instance ??= new AppSettings();

        public ThemeSettings Theme { get; set; } = new();
        public FontSettings Fonts { get; set; } = new();
        public ColorSettings Colors { get; set; } = new();
        public EditorSettings Editor { get; set; } = new();
        public ImageSettings Images { get; set; } = new();

        private AppSettings()
        {
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            LoadSettings();
            WatchSettingsFile();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonConvert.DeserializeObject<AppSettingsData>(json);
                    if (settings != null)
                    {
                        Theme = settings.Theme ?? new ThemeSettings();
                        Fonts = settings.Fonts ?? new FontSettings();
                        Colors = settings.Colors ?? new ColorSettings();
                        Editor = settings.Editor ?? new EditorSettings();
                        Images = settings.Images ?? new ImageSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private void WatchSettingsFile()
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (directory != null)
            {
                _watcher = new FileSystemWatcher(directory)
                {
                    Filter = "settings.json",
                    NotifyFilter = NotifyFilters.LastWrite
                };

                _watcher.Changed += (s, e) =>
                {
                    System.Threading.Thread.Sleep(100); // Small delay to ensure file is written
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        LoadSettings();
                        OnPropertyChanged(string.Empty);
                    });
                };

                _watcher.EnableRaisingEvents = true;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Color GetColor(string hexColor)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(hexColor);
            }
            catch
            {
                return System.Windows.Media.Colors.White;
            }
        }

        public SolidColorBrush GetBrush(string hexColor)
        {
            return new SolidColorBrush(GetColor(hexColor));
        }
    }

    public class AppSettingsData
    {
        [JsonProperty("theme")]
        public ThemeSettings? Theme { get; set; }

        [JsonProperty("fonts")]
        public FontSettings? Fonts { get; set; }

        [JsonProperty("colors")]
        public ColorSettings? Colors { get; set; }

        [JsonProperty("editor")]
        public EditorSettings? Editor { get; set; }

        [JsonProperty("images")]
        public ImageSettings? Images { get; set; }
    }

    public class ThemeSettings
    {
        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; } = "#1e1e1e";

        [JsonProperty("textColor")]
        public string TextColor { get; set; } = "#d4d4d4";

        [JsonProperty("selectionBackground")]
        public string SelectionBackground { get; set; } = "#264f78";

        [JsonProperty("lineNumberColor")]
        public string LineNumberColor { get; set; } = "#858585";

        [JsonProperty("currentLineBackground")]
        public string CurrentLineBackground { get; set; } = "#2a2a2a";
    }

    public class FontSettings
    {
        [JsonProperty("fontFamily")]
        public string FontFamily { get; set; } = "Consolas";

        [JsonProperty("regularTextSize")]
        public double RegularTextSize { get; set; } = 12;

        [JsonProperty("heading3Size")]
        public double Heading3Size { get; set; } = 16;

        [JsonProperty("heading2Size")]
        public double Heading2Size { get; set; } = 18;

        [JsonProperty("heading1Size")]
        public double Heading1Size { get; set; } = 26;
    }

    public class ColorSettings
    {
        [JsonProperty("regularText")]
        public string RegularText { get; set; } = "#d4d4d4";

        [JsonProperty("heading1")]
        public string Heading1 { get; set; } = "#569cd6";

        [JsonProperty("heading2")]
        public string Heading2 { get; set; } = "#4ec9b0";

        [JsonProperty("heading3")]
        public string Heading3 { get; set; } = "#dcdcaa";

        [JsonProperty("italic")]
        public string Italic { get; set; } = "#ffeb3b";

        [JsonProperty("bold")]
        public string Bold { get; set; } = "#f44336";

        [JsonProperty("link")]
        public string Link { get; set; } = "#4fc3f7";

        [JsonProperty("bulletPoint")]
        public string BulletPoint { get; set; } = "#808080";

        [JsonProperty("codeBlock")]
        public string CodeBlock { get; set; } = "#2d2d30";
    }

    public class EditorSettings
    {
        [JsonProperty("wordWrap")]
        public bool WordWrap { get; set; } = true;

        [JsonProperty("showLineNumbers")]
        public bool ShowLineNumbers { get; set; } = true;

        [JsonProperty("tabSize")]
        public int TabSize { get; set; } = 4;

        [JsonProperty("autoIndent")]
        public bool AutoIndent { get; set; } = true;

        [JsonProperty("highlightCurrentLine")]
        public bool HighlightCurrentLine { get; set; } = true;
    }

    public class ImageSettings
    {
        [JsonProperty("imageFolder")]
        public string ImageFolder { get; set; } = "EpicMarkdownManager_Images";

        [JsonProperty("defaultWidth")]
        public int DefaultWidth { get; set; } = 400;

        [JsonProperty("enableResize")]
        public bool EnableResize { get; set; } = true;
    }
}