using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("GifWin.UWP")]
[assembly: AssemblyDescription("GifWin is a reimagining of the excellent GifWit, for Windows.")]
[assembly: AssemblyProduct("GifWin")]
[assembly: AssemblyCopyright("Copyright The GifWin Team © 2016-2017")]

[assembly: AssemblyVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor
)]

[assembly: AssemblyFileVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch
)]

[assembly: AssemblyInformationalVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch + "-" +
    ThisAssembly.Git.Branch + "+" +
    ThisAssembly.Git.Commit
)]
