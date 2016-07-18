using System;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Web.Script.Serialization;

namespace racaljkhost
{
    partial class CGitHost : ServiceBase
    {
        private string EtcPath { get; set; }
        private string LocalHost { get; set; }
        private string LocalHostBak { get; set; }
        private string CommitInfoLink { get; set; }
        private string HostFileLink { get; set; }
        private int Interval => 1 * 60 * 60 * 1000;
        private JavaScriptSerializer Serializer { get; set; }

        public CGitHost()
        {
            InitializeComponent();
            this.EtcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc");
            this.LocalHost = Path.Combine(EtcPath, "hosts");
            this.LocalHostBak = Path.Combine(EtcPath, "hosts.bak");
            this.CommitInfoLink = "https://api.github.com/repos/racaljk/hosts/commits?path=/hosts";
            this.HostFileLink = "https://raw.githubusercontent.com/racaljk/hosts/master/hosts";
            this.Serializer = new JavaScriptSerializer();
        }

        protected override void OnStart(string[] args)
        {
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            //Get current local hosts file update date;
            var lastWriteTime = File.GetLastWriteTimeUtc(LocalHost);
            //Get current server latest hosts file update date;
            var request = (HttpWebRequest)WebRequest.Create(CommitInfoLink);
            request.UserAgent = "Chrome/51.0.2704.106";
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            var commitTxt = reader.ReadToEnd();
            dynamic commit = this.Serializer.Deserialize<dynamic>(commitTxt);
            var lastCommitDate = (DateTime)commit.First.commit.committer.date.Value;
            reader.Close();
            stream.Close();
            response.Close();
            //check if require to update the hosts;
            //return if needn`t;
            if (lastCommitDate < lastWriteTime.AddMilliseconds(this.Interval)) return;
            //check current hosts backup file existing;
            //if backup haven`t stored, rename current file as backup;
            if (!File.Exists(LocalHostBak)) File.Move(LocalHost, LocalHostBak);
            //download remote latest hosts file and store on "driver\etc\hosts"
            WebClient client = new WebClient();
            client.DownloadFile(HostFileLink, LocalHost);
        }

        protected override void OnStop()
        {
            _timer.Stop();
        }
    }
}
