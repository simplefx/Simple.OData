using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Simple.OData.Client")]
[assembly: AssemblyDescription(".NET4/WinRT OData client library.")]
[assembly: AssemblyCompany("Vagif Abilov")]
[assembly: AssemblyProduct("Simple.OData.Client")]
[assembly: AssemblyCopyright("Copyright © Vagif Abilov 2012-2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if(DEBUG)
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c39637fa-a4e4-4ca4-a0a8-f393f35e9a72")]

[assembly: InternalsVisibleTo("Simple.OData.Client.Tests")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
