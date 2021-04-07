# ExtremeDumper
.NET Assembly Dumper

## Description
### View Processes
![](./Images/ProcessView.png)

The default page is process view. You can right click here to dump all .NET modules and view modules in selected process. And also you can click "Inject Dll" to inject a .NET assembly into any process. Any process which contains desktop clr or coreclr will be marked as green.

### View Modules
![](./Images/ModuleView.png)

This page show all modules in select process and you can just view .NET modules by click "Only .NET Modules". Any .NET module will be marked as green.

### View Exported Functions
![](./Images/ExportFunctionView.png)

This page shou exported functions for unmanaged dlls.

### Inject .NET assemblies
![](./Images/InjectManagedDll.png)

Currently the injector supports any .NET Framework assembly with any platform target. And you can pass a string argument to injection main method in injector. In the future it will supports .NET Core.

## Downloads
GitHub: [Latest release](https://github.com/wwh1004/ExtremeDumper/releases/latest/download/ExtremeDumper.zip)

AppVeyor: [![Build status](https://ci.appveyor.com/api/projects/status/f6kyx4yv68lwain0?svg=true)](https://ci.appveyor.com/project/wwh1004/extremedumper)
