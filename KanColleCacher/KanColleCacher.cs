using Grabacr07.KanColleViewer.Composition;
using System.ComponentModel.Composition;
using Debug = System.Diagnostics.Debug;
using File = System.IO.File;
using System.Windows;
using System.Windows.Media;

namespace d_f_32.KanColleCacher
{
	[Export(typeof(IPlugin))]
    [Export(typeof(ITool))]
    [ExportMetadata("Guid", "DA0FF655-A2CA-40DC-A78B-6DC85C2D448B")]
    [ExportMetadata("Title", AssemblyInfo.Name)]
	[ExportMetadata("Description", AssemblyInfo.Description)]
	[ExportMetadata("Version", AssemblyInfo.Version)]
	[ExportMetadata("Author", AssemblyInfo.Author)]
	public class KanColleCacher : IPlugin, ITool
    {
		const string name = "缓存工具";
		static bool isInitialized = false;
		static CacherToolView view;

        public KanColleCacher()
        {
            Initialize();
        }

		public void Initialize()
		{
			if (isInitialized) return;
			isInitialized = true;

#if DEBUG
			Debug.WriteLine(@"
KanColleCacher
=================================================
CACHR>	初始化开始：{0}
", System.DateTime.Now);
#endif
			
			Settings.Load();
			view = new CacherToolView();

			//Debug.WriteLine(@"CACHR>	GraphList加入规则");

			////只有当列表文件不存在时才打印列表
			GraphList.AppendRule();

			//Debug.WriteLine(@"CACHR>	GraphList加入规则完成");

			//Debug.WriteLine(@"CACHR>	Fiddler初始化开始");
			FiddlerRules.Initialize();
            //Debug.WriteLine(@"CACHR>	Fiddler初始化完成");

            //Debug.WriteLine(@"CACHR>	初始化完成");
		}

		~KanColleCacher()
		{
			Settings.Save();
			Debug.Flush();
		}

		public string Name
		{
			get { return name; }
		}

		public object View
		{
			get { return view; }
		}
	}
}
