using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ChessRocks")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ChessRocks")]
[assembly: AssemblyCopyright("Copyright ©  2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("dc6e842f-ac8f-4520-89f4-4508789d519b")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.5.0.0")]
// added Chess960 support

//[assembly: AssemblyVersion("1.4.0.0")]
//added chess.com's CurrentPosition tag

//[assembly: AssemblyVersion("1.3.0.0")]
//added comment braces if not done by user
//fixed saving position setup file to be able to be reloaded as games
//made the PGNHeader and GameList dialogs smaller

//[assembly: AssemblyVersion("1.2.0.0")]
//added main menu at the top of the window
//added browse buttons for file selection in engine text options
//added load and save of engine options
//fixed bug in filling the engine combobox options
//added engine logo images
//changed warning button test to yellow for better visibility of the text
//fixed pagedown bug in the move list - hardcocded value removed
//instead of having to drag and drop a piece you can now just touch it and the destination square
//fix knight move anomaly - applies possibly to other pieces also
//smaller (800x576) possible window size and more consistent resizing

//[assembly: AssemblyVersion("1.1.0.0")]
//lost version

//[assembly: AssemblyVersion("1.0.0.0")]
//initial release

[assembly: AssemblyFileVersion("1.0.0.0")]
