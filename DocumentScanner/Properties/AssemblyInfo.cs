using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ssepan.Application;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DocumentScanner")]
[assembly: AssemblyDescription("Document scanning, packing, and transfer.\n\nThanks to:\n~ IC#Code for their #Zip (SharpZip) library.\n~ NETMaster on CodeProject.com for his original TwainLib.\n~ 'Sciolist' on stackoverflow.com for workaround to vexing Image.Save file-locking problem.\n\nSteve Sepan takes credit for:\n~ Designing and coding this application.\n~ Designing and coding the Ssepan.* libraries.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Free Software Foundation, Inc.")]
[assembly: AssemblyProduct("DocumentScanner")]
[assembly: AssemblyCopyright("Copyright (C) 1989, 1991 Free Software Foundation, Inc.  \n59 Temple Place - Suite 330, Boston, MA  02111-1307, USA")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("689e5041-ba14-417f-8867-fcefb705dd6f")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("0.12")]


#region " Helper class to get information for the About form. "
/// <summary>
/// This class uses the System.Reflection.Assembly class to
/// access assembly meta-data
/// This class is ! a normal feature of AssemblyInfo.cs
/// </summary>
public class AssemblyInfo : AssemblyInfoBase
{
    // Used by Helper Functions to access information from Assembly Attributes
    public AssemblyInfo()
    {
        base.myType = typeof(DocumentScanner.DocumentViewer);
    }
}
#endregion
