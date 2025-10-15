# Epic Markdown Manager - Project Overview

## Architecture Summary

This is a WPF-based markdown editor for Windows that renders markdown with custom styling inline. The application uses a RichTextBox for live rendering with variable font sizes and colors.

## Technology Stack
- **Language:** C# (.NET 8.0)
- **UI Framework:** WPF (Windows Presentation Foundation)
- **Target Platform:** Windows x64
- **Key Libraries:**
  - AvalonEdit 6.1.0 (initially used, now removed)
  - Newtonsoft.Json 13.0.3 (for settings management)
  - System.Drawing.Common 7.0.0 (for image handling)

## Project File Structure

### Core Application Files
- **`App.xaml`/`App.xaml.cs`** - Application entry point and global resources (dark theme styles)
- **`MainWindow.xaml`/`MainWindow.xaml.cs`** - Main application window containing menu bar, toolbar, file explorer, and editor area

### Core Components

#### RichMarkdownEditor.cs
- **Purpose:** The main editor component using RichTextBox
- **Key Features:**
  - Live markdown rendering with variable font sizes
  - Inline color styling (red for bold, yellow for italic)
  - Auto-rendering on text change with debounce timer
  - Image paste support from clipboard
- **Key Classes:**
  - `RichMarkdownEditor` - Main editor grid control
  - `RelayCommand` - ICommand implementation for key bindings

#### FileExplorer.cs
- **Purpose:** Left sidebar file tree for project navigation
- **Features:**
  - Displays folder hierarchy
  - Filters for .md and .txt files
  - FileSystemWatcher for auto-refresh
  - Double-click to open files

#### AppSettings.cs
- **Purpose:** Configuration management with live reload
- **Key Classes:**
  - `AppSettings` - Singleton for settings access
  - `ThemeSettings` - Colors for UI theme
  - `FontSettings` - Font sizes for different heading levels
  - `ColorSettings` - Colors for markdown elements
  - `EditorSettings` - Editor behavior options
  - `ImageSettings` - Image handling configuration
- **Features:**
  - Auto-reload when settings.json changes
  - Property change notifications for UI updates

#### MarkdownParser.cs (Legacy)
- **Note:** Originally used for split-view preview, now unused
- Contains FlowDocument rendering logic that could be repurposed

### Configuration Files
- **`settings.json`** - User-configurable settings:
  - Theme colors (dark mode)
  - Font sizes (H1: 26pt, H2: 18pt, H3: 16pt)
  - Markdown element colors
  - Editor preferences
- **`EpicMarkdownManager.csproj`** - Project configuration:
  - .NET 8.0 Windows target
  - Self-contained single-file publish settings
  - Package references

### Build Files
- **`build.bat`** - Windows batch script for building standalone exe

## Key Design Patterns

1. **MVVM Light** - Property change notifications for data binding
2. **Singleton Pattern** - AppSettings instance
3. **Observer Pattern** - FileSystemWatcher for file changes
4. **Command Pattern** - RelayCommand for keyboard shortcuts

## Rendering Approach

The application uses a **RichTextBox** with live markdown parsing:
1. User types markdown syntax
2. Timer debounces changes (500ms)
3. Text is parsed line-by-line
4. Paragraphs are rebuilt with appropriate:
   - Font sizes for headings
   - Colors for markdown elements
   - Text decorations (underline, bold)
5. Caret position is preserved during re-render

## Color Scheme
- Background: `#1e1e1e` (dark)
- Regular text: `#d4d4d4` (light gray)
- Bold (`**text**`): `#f44336` (red)
- Italic (`*text*`): `#ffeb3b` (yellow)
- Links: `#4fc3f7` (light blue)
- Headings: Various blues/greens

## Important Notes for Maintenance

1. **RichTextBox Limitations**: The editor rebuilds the document on each render, which may cause performance issues with very large files
2. **Markdown Syntax Preservation**: The app shows raw markdown syntax with color highlighting, not WYSIWYG
3. **Settings Auto-reload**: Changes to settings.json apply immediately without restart
4. **Image Storage**: Pasted images save to `EpicMarkdownManager_Images` subfolder

## Entry Points for Debugging
- Start at `MainWindow.xaml.cs` constructor
- Text rendering happens in `RichMarkdownEditor.RenderMarkdown()`
- Settings loading in `AppSettings.LoadSettings()`