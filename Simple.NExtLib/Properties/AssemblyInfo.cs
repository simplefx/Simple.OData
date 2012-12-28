using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if WINDOWS_PHONE
[assembly: AssemblyTitle("NExtLib Windows Phone")]
#elif SILVERLIGHT
[assembly: AssemblyTitle("NExtLib Silverlight")]
#elif PocketPC
[assembly: AssemblyTitle("NExtLib Compact")]
#elif PORTABLE
[assembly: AssemblyTitle("NExtLib Portable")]
#elif NETFX_CORE
[assembly: AssemblyTitle("NExtLib WinRT")]
#elif NET20
[assembly: AssemblyTitle("NExtLib .NET 2.0")]
#elif NET35
[assembly: AssemblyTitle("NExtLib .NET 3.5")]
#elif NET40
[assembly: AssemblyTitle("NExtLib .NET 4.0")]
#else
[assembly: AssemblyTitle("NExtLib")]
#endif

[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("NExtLib")]
[assembly: AssemblyCopyright("Copyright © Mark Rendle 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if !PORTABLE
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4ad3854e-d691-4546-9bf7-637fcbf035dc")]
#endif

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
[assembly: AssemblyVersion("0.1.0.0")]
#if !PocketPC
[assembly: AssemblyFileVersion("0.1.0.0")]
#endif

[assembly: CLSCompliant(true)]
