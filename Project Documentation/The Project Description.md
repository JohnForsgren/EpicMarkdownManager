


Name suggestion: EpicMarkdownManager

I need you to design a todo-list app for windows as per these rules. 

It needs to be written in an appropriate language and it needs to be in the end assembled into an exe file which can be run.

Side-note / Guidelines: 
It is probably best for the AI to first create a plan of the project in the first prompt, and then build the prompt in the next message. When defining the project scope, I am NOT talking about repeating all of the features here. Rather, it is about: 
(1) General considerations, e.g which language it is written in, managing different line-heights, performance concerns, libraries, etc.
(2) Overview of the scripts/code-files to use.

At certain points during the development, you (the ai) need to pause the development for me to test the core features you have implemented to make sure that you are not continuing with bugged code. But these milestones should not be too frequent; you will be the judge of when you feel that you have made sufficient edits to begin testing. 

Core app feature
The app is largely similar to notepad/notepad++ in that it is a program capable of opening raw text files, and interpreting them as per markdown language. I am currently only planning on using this for a todo-list. 

I am looking for a good tool for making todo lists, mainly focusing on the features: (1) Able to work in a markdown file, where i can make long and complex bullet lists (this is not handled well in many tools like word and Gdocs since the page length is very limited. (2) Able to add color to the text. 

Language & Setup
- Consider making it in C# (since my timer application is in that language; perhaps they could be merged into one app). If there is another language that is clearly more practical for this topic, that could be used instead. 

General considerations to help development:
- Regarding a concern for lag suggested by an AI, namely: "Building a robust, performant parser that can live-render complex markdown (especially with custom styling and variable line heights) is the most significant challenge." - On this point it should be noted that the .md files are not intended to be particularly long; we are talking about a few hundred lines of text with relatively little text on each line, so aside from images, we are not talking about text files that are 40 kb+ in size. 

- Since this is a small project, it will likely be easier to handle by reducing the amount of code-files; while it is a standard to have one file per class divided up in many different folders (as for an enterprise application), this is not necessary; smaller classes and functions should (if feasible) be put in the same file to reduce the amount of files.   

Overview: Example case of how the app is used 

This is to give some context of how a basic setup of the main features i want the program to be able to read. Namely: 
Headings in different fonts using # 
Indents using tabs
Ability to make Links and colored text through markdown italics and bold, i.e Yellow Text (*), and Red Text (**)

THE EXAMPLE 
# Heading 1 - Example task 
## Milestone 1 
- Task: 
	- Write the script
		- (R) Ask Michael if he has any recommendations 
		- Begin by watching this video for inspiration: [Link](https://www.youtube.com/watch?v=ipyu58LSBts)
	- Make the powerpoint. 
		- **ISSUE**: I am unable to start Powerpoint; Contact IT support. 
		- *IMPORTANT*: The presentation cannot be submitted before it has been reviewed by Jessica. 
## Milestone 2
[...]

Specific Features 

General basic window functionality
Similarly to Obsidian, you open the program by selecting a folder, after which it will be presented in a sort of "Project Explorer" sidebar with different files and folders in your "project folder". 

"Bold" and "Italic"
Pressing Ctrl+B should add "**" around the selection, making it "bold". Likewise Ctrl + I for Italics. This doesn't actually change the boldness in the interface, but makes it red an yellow, respectively (see the color section elsewhere). 

Other basic features of a markdown should also be present
I am not sure of all details here, but the ones that come to mind are: 
- Automatic indents, i.e if i have already indented once and press enter, the cursor should be at the same end. 
- General shortcuts/functionality for these sorts of text programs, like Ctrl+S for saving (Presumably there are reliable libraries for this sort of thing) 
- Links should be clickable. 

Making lines the paragraph lines shorter than the headings
This was a MAJOR but important issue that made me unable to use notepad++, which is why I need this separate app to be created. Notepad++ was unable to have variable line height; all lines are at minimum as high as the tallest headline. The reasoning for this was that the program will be bugged if multiple different text styles are incorporated in the same line, but this will not happen in this program, since the only way to make text larger is by adding one or more "#" in the start, making the entire text a headline. 

In general, I dislike spacing and it should not be present, other than a few pixles.

The specific Fonts / Colors
FONT: Doesn't matter; I use Arial but there might be a cooler one available.

TEXT SIZE
Regular Text: 12
Heading ### : 16, Underline
Heading ## :  18, Bold
Heading # : 26, Underline 

BONUS: Make these be read from a json file, so that they can more easily be edited later on. Ideally, the interface should update automatically upon saving an edit in this json file.  

COLOR: Regarding the color, I have a reference image here. It isn't crucial, but worth reminding if i forget attaching it. 

Link color: light blue (as they typically are this color in most apps). 

THEME: Dark Mode / Dracula / Obsidian
I need the theme, i.e background to be of a Dark Mode / Dracula / Obsidian sort of theme, i.e a dark background with white text. This is bascially already answered in the color image i put above. 

Miscellaneous side-comments 

Side note: Google Drive Desktop App
Not sure if this has any relevance, but I have occasionally have had issues with applications (Audacity/Filmora comes to mind) not being able to read or save files to google drive due to permission issues - not specifically for these todo-list files, but it might be something to consider. 

Special Characters
The interface should be able to handle special characters such as "🔥". 

Reference to The timer app
There should be an available button called something like "Start Promodoros Timer" which later will connect to my existing Promodoros Timer app. 


Other "Nice-to-have"-features

Incorporating Images
A nice feature that I used when making todo-lists in obsidian and would like in this app if possible:  You can simply paste (with ctrl+V) images from your clipboard. This causes the image to be stored in a sub-folder at the same location, i.e if the project folder "Todo-Lists" is the root folder that has been opened and is visible in the "Project Explorer", it will have a separate folder called some suitable name, such as "[App Name] Images", containing all of the images, which is the same solution as the Obsidian app. 

Bonus: Resizing images
If not too difficult to implement, 
- Simple solution: If the image is clicked, a "px" text bar comes up, where the user can set the px for the image (see below). 
- More advanced solution: it would be cool if the image can be clicked, at which point a border appears, indicating that the user can drag along the edges to resize the image. When resized, it will have a corresponding px added to it, like this : "|300px"
[[File:The_Inserted_Image.png|300px]]; this being based on what I have seen on wiki pages, where images can be resized by adding the "|???px" in the end. 

Settings window (Less important)

Potentially, there could be a settings window. If implementing this, it is important that the app as a whole should still be simple to use, so it shouldn't have too many features. 

Notable things to include in the settings window would be: 
- Changing the Font (Might as well be done for the whole application, instead of just a single element such as heading 2).
- Changing the colors of the things that have color.
- Toggling on and off if the text breaks into new lines when reaching the end of the window (or if there is no max line length). 



