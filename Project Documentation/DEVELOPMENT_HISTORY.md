# Epic Markdown Manager - Development History

## Initial Requirements Analysis
- User requested a Windows desktop todo-list app with markdown support
- Key requirement: Variable line heights (headings larger than regular text)
- Specific styling: Bold shown as red, italic as yellow
- Dark theme (Obsidian/Dracula style)
- Single window editing (not split view)

## Development Timeline

### Phase 1: Project Setup and Architecture Design
1. **Technology Selection**
   - Chose C# with WPF for Windows native development
   - Selected .NET 7.0 initially, later upgraded to .NET 8.0
   - Decided on AvalonEdit for text editing capabilities

2. **Initial Structure Created**
   - Created project file with package references
   - Set up dark theme in App.xaml
   - Created settings.json for configuration

### Phase 2: Core Components Development
1. **AppSettings System**
   - Implemented singleton pattern for global settings access
   - Added FileSystemWatcher for live settings reload
   - Created typed settings classes for different aspects

2. **File Explorer Sidebar**
   - Built TreeView-based file browser
   - Added filtering for .md and .txt files
   - Implemented FileSystemWatcher for auto-refresh

3. **Initial Editor Attempt with AvalonEdit**
   - Created MarkdownEditor.cs with split view
   - Left pane: AvalonEdit for editing
   - Right pane: FlowDocument for preview
   - Created MarkdownParser for rendering

### Phase 3: First Major Pivot - Syntax Highlighting Issues
**Problem:** AvalonEdit version incompatibility
- Initial code used HighlightingDefinition API not available in v6.1.0
- Attempted to use DocumentColorizingTransformer instead
- Discovered limitation: Can't change font sizes in AvalonEdit colorizers

**Solution Attempts:**
1. Tried using VisualLineElementGenerator for custom rendering
2. Attempted FormattedTextRun for variable heights
3. Found that AvalonEdit expects uniform line heights

### Phase 4: Second Major Pivot - RichTextBox Implementation
**Critical Realization:** User wanted single view with inline rendering, not split view

**New Approach:**
1. **Removed AvalonEdit completely**
   - Deleted MarkdownEditor.cs
   - Removed AvalonEdit package reference

2. **Created RichMarkdownEditor.cs**
   - Used native WPF RichTextBox
   - Implemented live markdown parsing
   - Added timer-based re-rendering (500ms debounce)

3. **Rendering Strategy**
   - Parse text line-by-line
   - Create Paragraph objects with appropriate styling
   - Apply font sizes directly to Run elements
   - Preserve markdown syntax while applying colors

### Phase 5: Integration and Polish
1. **MainWindow Updates**
   - Switched from MarkdownEditor to RichMarkdownEditor
   - Updated all event handlers for RichTextBox API
   - Fixed Save/Load functionality

2. **Keyboard Shortcuts**
   - Implemented Ctrl+B for bold insertion
   - Implemented Ctrl+I for italic insertion
   - Added Ctrl+V for image paste from clipboard

3. **Build Configuration**
   - Upgraded from .NET 7.0 to .NET 8.0
   - Created build.bat for standalone exe generation
   - Set up self-contained publishing

### Phase 6: Bug Fixes and Compilation Issues
1. **Namespace Issues**
   - Fixed missing TextRun imports
   - Resolved RelayCommand not found errors
   - Corrected property name references

2. **API Compatibility**
   - Adjusted for RichTextBox Selection API
   - Fixed text extraction methods
   - Updated caret position handling

## Key Technical Decisions

### Why RichTextBox over AvalonEdit
- **AvalonEdit Limitations:**
  - Designed for uniform line heights
  - Syntax highlighting doesn't support font size changes
  - Complex API for simple requirements

- **RichTextBox Advantages:**
  - Native support for variable font sizes
  - Built-in paragraph and run styling
  - Simpler API for rich text formatting

### Rendering Approach Evolution
1. **Initial:** Tried to modify AvalonEdit's rendering pipeline
2. **Intermediate:** Attempted custom TextRun implementations
3. **Final:** Direct FlowDocument manipulation in RichTextBox

### Color Scheme Decision
- Instead of traditional bold/italic rendering:
  - Bold (`**text**`) → Red color
  - Italic (`*text*`) → Yellow color
- This unique approach maintains markdown visibility

## Lessons Learned

1. **Component Selection:** Sometimes native controls (RichTextBox) are better than specialized libraries (AvalonEdit) for specific use cases

2. **User Requirements:** The initial split-view approach was over-engineered; user wanted simple inline rendering

3. **API Compatibility:** Always verify library capabilities match requirements before deep implementation

4. **Iterative Development:** Multiple pivots led to cleaner, simpler solution

## Final Achievement
Successfully created a markdown editor that:
- Renders different heading sizes inline (H1: 26pt, H2: 18pt, H3: 16pt)
- Shows markdown syntax with color coding
- Maintains minimal line spacing
- Provides dark theme throughout
- Supports image paste and special characters
- Auto-saves and live-reloads settings