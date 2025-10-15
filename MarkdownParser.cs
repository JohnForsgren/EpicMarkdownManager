using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace EpicMarkdownManager
{
    public class MarkdownParser
    {
        private readonly AppSettings _settings;
        private readonly string _baseDirectory;
        private readonly Action<string>? _imageClickCallback;

        public MarkdownParser(string baseDirectory, Action<string>? imageClickCallback = null)
        {
            _settings = AppSettings.Instance;
            _baseDirectory = baseDirectory;
            _imageClickCallback = imageClickCallback;
        }

        public FlowDocument ParseToFlowDocument(string markdown)
        {
            var document = new FlowDocument
            {
                Background = _settings.GetBrush(_settings.Theme.BackgroundColor),
                Foreground = _settings.GetBrush(_settings.Theme.TextColor),
                FontFamily = new FontFamily(_settings.Fonts.FontFamily),
                FontSize = _settings.Fonts.RegularTextSize,
                LineHeight = 1.2,
                PagePadding = new Thickness(10)
            };

            var lines = markdown.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> currentListBlock = new List<string>();
            bool inList = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Handle lists
                if (IsListItem(line))
                {
                    currentListBlock.Add(line);
                    inList = true;
                }
                else
                {
                    if (inList && currentListBlock.Count > 0)
                    {
                        // Process accumulated list
                        var listBlock = ProcessList(currentListBlock);
                        document.Blocks.Add(listBlock);
                        currentListBlock.Clear();
                        inList = false;
                    }

                    if (!string.IsNullOrWhiteSpace(line) || i == 0 || i == lines.Length - 1)
                    {
                        var block = ParseLine(line);
                        if (block != null)
                        {
                            document.Blocks.Add(block);
                        }
                    }
                }
            }

            // Handle any remaining list items
            if (inList && currentListBlock.Count > 0)
            {
                var listBlock = ProcessList(currentListBlock);
                document.Blocks.Add(listBlock);
            }

            return document;
        }

        private bool IsListItem(string line)
        {
            var trimmed = line.TrimStart();
            return trimmed.StartsWith("- ") || trimmed.StartsWith("* ") ||
                   Regex.IsMatch(trimmed, @"^\d+\.\s");
        }

        private Block ProcessList(List<string> listLines)
        {
            var listParagraph = new Paragraph
            {
                Margin = new Thickness(0, 2, 0, 2),
                LineHeight = _settings.Fonts.RegularTextSize * 1.3
            };

            foreach (var line in listLines)
            {
                // Count indentation level (tabs or 4 spaces = 1 level)
                int indentLevel = 0;
                int charIndex = 0;

                while (charIndex < line.Length)
                {
                    if (line[charIndex] == '\t')
                    {
                        indentLevel++;
                        charIndex++;
                    }
                    else if (charIndex + 3 < line.Length &&
                             line.Substring(charIndex, 4) == "    ")
                    {
                        indentLevel++;
                        charIndex += 4;
                    }
                    else
                    {
                        break;
                    }
                }

                var content = line.Substring(charIndex);

                // Remove list marker
                content = Regex.Replace(content, @"^[\-\*]\s+", "");
                content = Regex.Replace(content, @"^\d+\.\s+", "");

                // Create indentation
                var indent = new string(' ', indentLevel * 4);

                // Add bullet point with proper indentation
                var bulletRun = new Run(indent + "• ")
                {
                    Foreground = _settings.GetBrush(_settings.Colors.BulletPoint),
                    FontSize = _settings.Fonts.RegularTextSize
                };
                listParagraph.Inlines.Add(bulletRun);

                // Parse inline content
                ParseInlineContent(listParagraph.Inlines, content);
                listParagraph.Inlines.Add(new LineBreak());
            }

            return listParagraph;
        }

        private Block? ParseLine(string line)
        {
            // Check for images
            var imageMatch = Regex.Match(line, @"!\[([^\]]*)\]\(([^)]+)\)(?:\|(\d+)px)?");
            if (imageMatch.Success)
            {
                return CreateImageBlock(imageMatch.Groups[2].Value,
                    imageMatch.Groups[3].Success ? imageMatch.Groups[3].Value : null);
            }

            // Check for headings
            if (line.StartsWith("# "))
            {
                return CreateHeading(line.Substring(2), 1);
            }
            else if (line.StartsWith("## "))
            {
                return CreateHeading(line.Substring(3), 2);
            }
            else if (line.StartsWith("### "))
            {
                return CreateHeading(line.Substring(4), 3);
            }

            // Regular paragraph
            if (!string.IsNullOrWhiteSpace(line))
            {
                var paragraph = new Paragraph
                {
                    Margin = new Thickness(0, 2, 0, 2),
                    LineHeight = _settings.Fonts.RegularTextSize * 1.3
                };
                ParseInlineContent(paragraph.Inlines, line);
                return paragraph;
            }

            return null;
        }

        private Block CreateHeading(string text, int level)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5)
            };

            var run = new Run(text);

            switch (level)
            {
                case 1:
                    run.FontSize = _settings.Fonts.Heading1Size;
                    run.Foreground = _settings.GetBrush(_settings.Colors.Heading1);
                    run.TextDecorations = TextDecorations.Underline;
                    paragraph.LineHeight = _settings.Fonts.Heading1Size * 1.3;
                    break;
                case 2:
                    run.FontSize = _settings.Fonts.Heading2Size;
                    run.Foreground = _settings.GetBrush(_settings.Colors.Heading2);
                    run.FontWeight = FontWeights.Bold;
                    paragraph.LineHeight = _settings.Fonts.Heading2Size * 1.3;
                    break;
                case 3:
                    run.FontSize = _settings.Fonts.Heading3Size;
                    run.Foreground = _settings.GetBrush(_settings.Colors.Heading3);
                    run.TextDecorations = TextDecorations.Underline;
                    paragraph.LineHeight = _settings.Fonts.Heading3Size * 1.3;
                    break;
            }

            paragraph.Inlines.Add(run);
            return paragraph;
        }

        private void ParseInlineContent(InlineCollection inlines, string text)
        {
            // Pattern to match bold (**text**), italic (*text*), links, and code blocks
            var pattern = @"(\*\*[^*]+\*\*|\*[^*]+\*|\[([^\]]+)\]\(([^)]+)\)|`[^`]+`|[^*\[\]`]+)";
            var matches = Regex.Matches(text, pattern);

            foreach (Match match in matches)
            {
                var value = match.Value;

                if (value.StartsWith("**") && value.EndsWith("**"))
                {
                    // Bold (shown as red)
                    var content = value.Substring(2, value.Length - 4);
                    var run = new Run(content)
                    {
                        Foreground = _settings.GetBrush(_settings.Colors.Bold),
                        FontWeight = FontWeights.Bold,
                        FontSize = _settings.Fonts.RegularTextSize
                    };
                    inlines.Add(run);
                }
                else if (value.StartsWith("*") && value.EndsWith("*"))
                {
                    // Italic (shown as yellow)
                    var content = value.Substring(1, value.Length - 2);
                    var run = new Run(content)
                    {
                        Foreground = _settings.GetBrush(_settings.Colors.Italic),
                        FontStyle = FontStyles.Italic,
                        FontSize = _settings.Fonts.RegularTextSize
                    };
                    inlines.Add(run);
                }
                else if (match.Groups[2].Success)
                {
                    // Link
                    var linkText = match.Groups[2].Value;
                    var linkUrl = match.Groups[3].Value;

                    var hyperlink = new Hyperlink(new Run(linkText))
                    {
                        NavigateUri = new Uri(linkUrl, UriKind.RelativeOrAbsolute),
                        Foreground = _settings.GetBrush(_settings.Colors.Link),
                        TextDecorations = TextDecorations.Underline
                    };

                    hyperlink.RequestNavigate += (sender, e) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
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
                    var run = new Run(content)
                    {
                        Background = _settings.GetBrush(_settings.Colors.CodeBlock),
                        FontFamily = new FontFamily("Consolas"),
                        FontSize = _settings.Fonts.RegularTextSize
                    };
                    inlines.Add(run);
                }
                else
                {
                    // Regular text
                    var run = new Run(value)
                    {
                        Foreground = _settings.GetBrush(_settings.Colors.RegularText),
                        FontSize = _settings.Fonts.RegularTextSize
                    };
                    inlines.Add(run);
                }
            }
        }

        private Block CreateImageBlock(string imagePath, string? widthStr)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5)
            };

            try
            {
                var fullPath = Path.IsPathRooted(imagePath) ? imagePath :
                    Path.Combine(_baseDirectory, imagePath);

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    var image = new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(5)
                    };

                    // Set width if specified
                    if (!string.IsNullOrEmpty(widthStr) && int.TryParse(widthStr, out int width))
                    {
                        image.Width = width;
                    }
                    else
                    {
                        image.Width = _settings.Images.DefaultWidth;
                    }

                    // Make image clickable for resizing
                    if (_settings.Images.EnableResize)
                    {
                        image.Cursor = Cursors.Hand;
                        image.MouseLeftButtonDown += (s, e) =>
                        {
                            _imageClickCallback?.Invoke(fullPath);
                        };
                    }

                    var container = new InlineUIContainer(image);
                    paragraph.Inlines.Add(container);
                }
                else
                {
                    paragraph.Inlines.Add(new Run($"[Image not found: {imagePath}]")
                    {
                        Foreground = Brushes.Red,
                        FontStyle = FontStyles.Italic
                    });
                }
            }
            catch (Exception ex)
            {
                paragraph.Inlines.Add(new Run($"[Error loading image: {ex.Message}]")
                {
                    Foreground = Brushes.Red,
                    FontStyle = FontStyles.Italic
                });
            }

            return paragraph;
        }
    }
}