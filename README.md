[![Build status](https://ci.appveyor.com/api/projects/status/eka9d0tkw1gh70i5/branch/master?svg=true)](https://ci.appveyor.com/project/rahulpnath/asmspy/branch/master)

AsmSpy
------

A simple command line tool to view assembly references.

## Install

Install [from Chocolatey package](https://chocolatey.org/packages/asmspy):

    C:\> choco install asmspy

Or download [AsmSpy as a .zip here](https://ci.appveyor.com/project/rahulpnath/asmspy/branch/master/artifacts). The .zip file contains AsmSpy.exe.

## How it works

Simply run AsmSpy giving it a path to your bin directory (the folder where your project's assemblies live).

    AsmSpy D:\Source\sutekishop\Suteki.Shop\Suteki.Shop\bin

It will output a list of all conflicting assembly references. That is where different assemblies in your bin folder reference different versions of the same assembly.

### Switches:
| Switch | Description |
| --- | --- |
| all | List all assemblies and references.<br> Supported formats:  -a, --all |
| nonsystem | Ignore system assemblies. <br> Supported formats:  -n, --nonsystem |
| noconsole | Do not show reference output on console.<br> Supported formats:  -nc, --noconsole |
| silent | Do not show any output, only warnings and errors will be shown.<br> Supported formats:  -s, --silent |
| dgml | Export dependency graph to a dgml file.<br> Supported formats:  -dg \<filename\> |
| dot | Export dependency graph to a [DOT](https://en.wikipedia.org/wiki/DOT_(graph_description_language)) file.<br> Supported formats:  -dt \<filename\> |
| xml | Export dependency graph to a xml file.<br> Supported formats:  -x \<filename\> |
| rsw | Will only analyze assemblies if their referenced assemblies starts with the given value.<br> Supported formats:  -rsw \<string\>, --referencedstartswith \<string\> |
| tree | Write a dependency tree to the console.<br>Supported formats: -tr --tree |
| i | Include subdirectories in search.<br> Supported formats:  -i, --includesub |
| c | Use the binding redirects of the given configuration file (Web.config or App.config) <br> Supported formats: -c \<string>, --configurationFile \<string> |
| f | Whether to exit with an error code when AsmSpy detected Assemblies which could not be found <br> Supported formats. -f, --failOnMissing |

### Examples
To see a list of all assemblies and all references, just add the 'all' flag:

    AsmSpy D:\Source\sutekishop\Suteki.Shop\Suteki.Shop\bin --all

To check only a single assembly provide a path to the file:

    AsmSpy D:\Source\sutekishop\Suteki.Shop\Suteki.Shop\bin\Suteki.Shop.dll

To ignore system assemblies, add the 'nonsystem' flag.

The output looks something like this:


	Reference: System.Runtime.Serialization
		3.0.0.0 by Microsoft.ServiceModel.Samples.XmlRpc
		3.0.0.0 by Microsoft.Web.Mvc
		4.0.0.0 by Suteki.Shop
	Reference: System.Web.Mvc
		2.0.0.0 by Microsoft.Web.Mvc
		3.0.0.0 by MvcContrib
		3.0.0.0 by MvcContrib.FluentHtml
		3.0.0.0 by Suteki.Common
		2.0.0.0 by Suteki.Common
		3.0.0.0 by Suteki.Shop
		2.0.0.0 by Suteki.Shop
	Reference: System.ServiceModel.Web
		3.5.0.0 by Microsoft.Web.Mvc
	Reference: System.Web.Abstractions
		3.5.0.0 by Microsoft.Web.Mvc


You can see that System.Web.Mvc is referenced by 7 assemblies in my bin folder. Some reference
version 2.0.0.0 and some version 3.0.0.0. I can now resolve any conflicts.

Color coding is used to more easily distinguish any problems.
* Green - referenced assembly found locally, in the specified directory
* Yellow - referenced assembly not found locally, but found installed in the [Global Assembly Cache](https://msdn.microsoft.com/en-us/library/yf1d93sz(v=vs.110).aspx)
* Red - referenced assembly missing

### Configure AsmSpy as an external tool in Visual Studio

[Blog post here](http://mikehadlow.blogspot.co.uk/2018/01/configure-asmspy-as-external-tool-in.html)
 
