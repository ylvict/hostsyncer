using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Timers;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace HostSyncer
{
    partial class CGitHost : ServiceBase
    {
        private List<HostModel> HostRepos => new List<HostModel>
        {
            new HostModel { Repo = "racaljk/hosts", TempFileSubfix = "racaljk", IgnoreDomain = new string[] { ".googlevideo.com", ".youtube.com" } },
            new HostModel { Repo = "fengixng/google-hosts", TempFileSubfix = "fengixng", OnlyDomain = new string[] { ".googlevideo.com", ".youtube.com" } },
        };

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

        protected override void OnStart(string[] args) => this.Handle();

        protected override void OnStop() => this.Timer.Stop();

        protected void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Get current local hosts file update date;
            var lastWriteTime = File.GetLastWriteTimeUtc(HostModel.LocalHost);

            //Get current server latest hosts file update date;
            DateTime lastCommitDate = this.HostRepos
                .Select(repo => this.GetLatestCommitTime(repo.CommitInfoLink))
                .OrderBy(x => x).LastOrDefault();

            //check if require to update the hosts;
            //return if needn`t;
            if (lastCommitDate < lastWriteTime.AddMilliseconds(this.Timer.Interval))
                return;

            //check current hosts backup file existing;
            //if backup haven`t stored, rename current file as backup;
            if (!File.Exists(HostModel.LocalHostBak))
                File.Move(HostModel.LocalHost, HostModel.LocalHostBak);

            //download remote latest hosts file and store on "driver\etc\hosts"
            this.Handle();
        }

        private void Handle()
        {
            this.HostRepos.ForEach(repo => this.WebClient.DownloadFile(repo.SrcFile, repo.DestPath));
            this.Compose();
#if !DEBUG
            this.HostRepos.ForEach(repo => File.Delete(repo.DestPath));
#endif
        }

        private void Compose()
        {
            var items = this.HostRepos.SelectMany(repo =>
            {
                return File.ReadAllLines(repo.DestPath)
                    .Where(x => !x.Trim().StartsWith("#"))
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Select(x => new HostItem(x))
                    .Where(x => (repo.OnlyDomain == null) || (repo.OnlyDomain.Length > 0 && repo.OnlyDomain.Count(d => x.Domain.EndsWith(d)) > 0))
                    .Where(x => repo.IgnoreDomain.Count(ig => x.Domain.EndsWith(ig)) <= 0);

            }).Select(x => $"{x.IP}\t{x.Domain}");

#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            File.WriteAllLines(HostModel.LocalHost, items);
        }

        private DateTime GetLatestCommitTime(string commitInfoLink)
        {
            var request = (HttpWebRequest)WebRequest.Create(commitInfoLink);
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
