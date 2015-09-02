using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using d_f_32.KanColleCacher;
// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle(AssemblyInfo.Title)]
[assembly: AssemblyDescription(AssemblyInfo.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(AssemblyInfo.Name)]
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。  如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
//[assembly: Guid("7909a3f7-15a8-4ee5-afc5-11a7cfa40576")]
[assembly: Guid("DA0FF655-A2CA-40DC-A78B-6DC85C2D448B")]

// 程序集的版本信息由下面四个值组成: 
//
//      主版本
//      次版本 
//      生成号
//      修订号
//
// 可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值，
// 方法是按如下所示使用“*”: 
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.Version)]


namespace d_f_32.KanColleCacher
{
	public static class AssemblyInfo
	{

		public const string Name = "KanColleCacher";
		public const string Version = "2.3.0.38";
		public const string Author = "d.f.32";
		public const string Copyright = "©2014 - d.f.32";
#if DEBUG
		public const string Title = "提督很忙！缓存工具 (DEBUG)";
#else
		public const string Title = "提督很忙！缓存工具";
#endif
		public const string Description = "通过创建本地缓存以加快游戏加载速度（并支持魔改）";
	}
}