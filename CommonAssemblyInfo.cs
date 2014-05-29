using System.Reflection;
using System.Runtime.InteropServices;

#if(DEBUG)
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("SimpleFX")]
[assembly: AssemblyProduct("SimpleFX")]
[assembly: AssemblyCopyright("Copyright © Mark Rendle, Vagif Abilov 2011-2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
