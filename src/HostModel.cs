using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HostSyncer
{
    public class HostModel
    {
        public HostModel()
        {
            this.IgnoreDomain = new string[] { };
            this.OnlyDomain = null;
        }

        private static string EtcPath
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.System);
                path = Path.Combine(path, "drivers");
                return Path.Combine(path, "etc");
            }
        }
        public static string LocalHost => Path.Combine(EtcPath, "hosts");
        public static string LocalHostBak => Path.Combine(EtcPath, "hosts.bak");

        public string Repo { get; set; }
        public string TempFileSubfix { get; set; }

        public string SrcFile => $@"https://github.com/{this.Repo}/raw/master/hosts";
        public string CommitInfoLink => $@"https://api.github.com/repos/{this.Repo}/commits?path=/hosts";

        public string[] IgnoreDomain { get; internal set; }
        public string[] OnlyDomain { get; internal set; }

        public string DestPath => Path.Combine(HostModel.EtcPath, $@"hosts_{this.TempFileSubfix}");
    }
}
