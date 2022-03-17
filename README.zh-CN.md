# ExtremeDumper [![Build status](https://ci.appveyor.com/api/projects/status/f6kyx4yv68lwain0?svg=true)](https://ci.appveyor.com/project/wwh1004/extremedumper)
.NET程序集Dump工具

## 特性
* 列举所有进程并且高亮.NET进程
* 列举进程中所有模块并且高亮.NET模块
* 通过遍历内存页以Dump进程中全部有效的.NET程序集
* 在模块视图中Dump指定的模块
* 注入.NET程序集到任意进程
* 增强的反反Dump模式
* .NET程序集加载器Hook
* 反标题关键词检测
* 单个可执行文件

## 介绍
### 查看进程
![](./Images/ProcessView.png)

默认页面是进程视图。你可以在此选择进程，右键打开菜单，Dump此进程中所有.NET模块或者查看进程中所有模块。你也可以点击"Inject Dll"以注入.NET程序集到任意进程。所有存在clr模块的进程会被高亮为绿色。

### 查看模块
![](./Images/ModuleView.png)

这个页面会显示选中进程中的所有模块，同时你可以通过点击"Only .NET Modules"选项仅查看.NET模块。所有.NET模块会被高亮为绿色。

### 查看导出函数
![](./Images/ExportFunctionView.png)

这个页面显示了选中模块的导出函数。

### 注入.NET程序集
![](./Images/InjectManagedDll.png)

当前注入器仅支持任意架构下的.NET Framework的程序集。同时你可以在注入器中给Main方法传递字符串参数。计划未来支持.NET Core。

### 反反Dump
开启前:
![](./Images/AntiAntiDump1.png)
![](./Images/AntiAntiDump3.png)
开启后:
![](./Images/AntiAntiDump2.png)
![](./Images/AntiAntiDump4.png)

#### 用法
模块视图中打开右键菜单然后点击"Enable AntiAntiDump"。启用之后你可以很方便地直接Dump任何带有反Dump保护的.NET程序集。

#### 原理
ExtremeDumper会注入核心Dll到目标进程并且从CLR内部对象读取元数据信息。不同于V2版本中的反反Dump，当前版本拥有几乎完美的兼容性。

### .NET程序集加载器Hook
![](./Images/LoaderHook1.png)
![](./Images/LoaderHook2.png)

#### 用法
在主界面点击"Open Loader Hook"按钮，之后会弹出"Loader Hook"窗口。选择一个要Dump的程序，然后点击"Run With Hook"。

#### 高级用法
把"ExtremeDumper.LoaderHook.dll"重命名为"version.dll"，然后把它放在目标程序的根目录下。它将以Dll劫持模式加载。

#### 原理
加载器Hook会在程序启动时挂钩"clr!AssemblyNative::LoadImage"函数。当.NET程序集被"Assembly.Load(byte[])"这些API加载，加载器Hook会将它的原始字节数组保存到磁盘中。

## 下载
GitHub: [Latest release](https://github.com/wwh1004/ExtremeDumper/releases/latest/download/ExtremeDumper.zip)

AppVeyor: [Latest build](https://ci.appveyor.com/api/buildjobs/ytfttpe2ev8kyheu/artifacts/bin%2FRelease%2FExtremeDumper.zip)
