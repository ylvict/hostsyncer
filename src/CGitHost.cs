using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Timers;
using System.Web.Script.Serialization;

namespace HostSyncer
{
    partial class CGitHost : ServiceBase
    {
        private string EtcPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc");

        private string LocalHost => Path.Combine(EtcPath, "hosts");

        private string LocalHostBak => Path.Combine(EtcPath, "hosts.bak");

        private string CommitInfoLink => ConfigurationManager.AppSettings["CommitInfoLink"];

        private string HostFileLink => ConfigurationManager.AppSettings["HostFileSrc"];

        private JavaScriptSerializer Serializer => new JavaScriptSerializer();

        private WebClient WebClient => new WebClient();

        private Timer Timer { get; set; }

        private int Interval
        {
            get
            {
                var interval = default(int);
                int.TryParse(ConfigurationManager.AppSettings["Interval"], out interval);
                return interval;
            }
        }

        public CGitHost()
        {
            InitializeComponent();

            this.Timer = new Timer(this.Interval);
            this.Timer.Enabled = true;
            this.Timer.AutoReset = true;
            this.Timer.Elapsed += new ElapsedEventHandler(this.Timer_Elapsed);
        }

        protected override void OnStart(string[] args) => this.WebClient.DownloadFile(HostFileLink, LocalHost);

        protected override void OnStop() => this.Timer.Stop();

        protected void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Get current local hosts file update date;
            var lastWriteTime = File.GetLastWriteTimeUtc(LocalHost);
            //Get current server latest hosts file update date;
            var lastCommitDate = this.GetLatestCommitTime();
            //check if require to update the hosts;
            //return if needn`t;
            if (lastCommitDate < lastWriteTime.AddMilliseconds(this.Timer.Interval)) return;
            //check current hosts backup file existing;
            //if backup haven`t stored, rename current file as backup;
            if (!File.Exists(LocalHostBak)) File.Move(LocalHost, LocalHostBak);
            //download remote latest hosts file and store on "driver\etc\hosts"
            this.WebClient.DownloadFile(HostFileLink, LocalHost);
        }

        private DateTime GetLatestCommitTime()
        {
            var request = (HttpWebRequest)WebRequest.Create(CommitInfoLink);
            request.UserAgent = "Chrome/51.0.2704.106";
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            var commitTxt = reader.ReadToEnd();
            dynamic commit = Serializer.Deserialize<dynamic>(commitTxt);
            DateTime lastCommitDate = Convert.ToDateTime(commit[0]["commit"]["committer"]["date"]);
            reader.Close();
            stream.Close();
            response.Close();
            return lastCommitDate.ToUniversalTime();
        }
    }
}
