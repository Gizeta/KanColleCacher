﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Models.Data.Xml;
using Grabacr07.KanColleViewer;
using Livet;
using Debug = System.Diagnostics.Debug;

namespace d_f_32.KanColleCacher
{
    [Serializable]
    public class Settings :NotificationObject
    {
        private static readonly string filePath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "grabacr.net",
            "KanColleViewer",
            "KanColleCacher.xml");

        public static Settings Current { get; private set; }

        /// <summary>
        /// 加载插件设置
        /// </summary>
        public static void Load()
        {
            try
            {
                Current = filePath.ReadXml<Settings>();

                if (!Directory.Exists(Current.CacheFolder))
                {
                    try 
					{
						Directory.CreateDirectory(Current.CacheFolder);
					}
					catch (Exception ex)
					{
						Current.CacheFolder= Directory.GetCurrentDirectory() + @"\MyCache";
						Log.Exception(ex.InnerException, ex, "设置文件中CacheFolder不存在，试图创建时发生异常");
					}
                }
            }
            catch (Exception ex)
            {
                Current = new Settings();
				Log.Exception(ex.InnerException, ex, "读取设置文件时出现异常");
            }
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        public static void Save()
        {
            try
            {
                Current.WriteXml(filePath);
            }
            catch (Exception ex)
            {
				Log.Exception(ex.InnerException, ex, "保存设置文件时出现异常");
            }
        }

        
        public Settings ()
        {       
                _CacheFolder = Directory.GetCurrentDirectory() + @"\MyCache";
                _CacheEnabled = true;
                _HackEnabled = true;
                _HackTitleEnabled = true;

                _CacheEntryFiles = 2;
                _CachePortFiles = 2;
                _CacheSceneFiles = 2;
                _CacheResourceFiles = 2;
                _CacheSoundFiles = 2;

				//_UpdateNoModifiedTimeFile = false;
				_CheckFiles = 1;
         }
       


        private string _CacheFolder;
        public string CacheFolder
        {
            get { return this._CacheFolder; }
            set
            {
                if (this._CacheFolder != value)
                {
                    this._CacheFolder = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private bool _CacheEnabled;
        public bool CacheEnabled
        {
            get { return this._CacheEnabled; }
            set
            {
                if (this._CacheEnabled != value)
                {
                    this._CacheEnabled = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private bool _HackEnabled;
        public bool HackEnabled
        {
            get { return this._HackEnabled; }
            set
            {
                if (this._HackEnabled != value)
                {
                    this._HackEnabled = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private bool _HackTitleEnabled;
        public bool HackTitleEnabled
        {
            get { return this._HackTitleEnabled; }
            set
            {
                if (this._HackTitleEnabled != value)
                {
                    this._HackTitleEnabled = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private int _CacheEntryFiles;
        public int CacheEntryFiles
        {
            get { return this._CacheEntryFiles; }
            set
            {
                if (this._CacheEntryFiles != value)
                {
                    this._CacheEntryFiles = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private int _CachePortFiles;
        public int CachePortFiles
        {
            get { return this._CachePortFiles; }
            set
            {
                if (this._CachePortFiles != value)
                {
                    this._CachePortFiles = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private int _CacheSceneFiles;
        public int CacheSceneFiles
        {
            get { return this._CacheSceneFiles; }
            set
            {
                if (this._CacheSceneFiles != value)
                {
                    this._CacheSceneFiles = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private int _CacheResourceFiles;
        public int CacheResourceFiles
        {
            get { return this._CacheResourceFiles; }
            set
            {
                if (this._CacheResourceFiles != value)
                {
                    this._CacheResourceFiles = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private int _CacheSoundFiles;
        public int CacheSoundFiles
        {
            get { return this._CacheSoundFiles; }
            set
            {
                if (this._CacheSoundFiles != value)
                {
                    this._CacheSoundFiles = value;
                    this.RaisePropertyChanged();
                }
            }
        }

		//0-不检查 1-不检查资源文件 2-检查所有文件
		private int _CheckFiles;
		public int CheckFiles
		{
			get { return this._CheckFiles; }
			set
			{
				if (this._CheckFiles != value)
				{
					this._CheckFiles = value;
					this.RaisePropertyChanged();
				}
			}
		}

		//private bool _UpdateNoModifiedTimeFile;
		//public bool UpdateNoModifiedTimeFile
		//{
		//	get { return this._UpdateNoModifiedTimeFile; }
		//	set
		//	{
		//		if (this._UpdateNoModifiedTimeFile != value)
		//		{
		//			this._UpdateNoModifiedTimeFile = value;
		//			this.RaisePropertyChanged();
		//		}
		//	}
		//}
    }
}