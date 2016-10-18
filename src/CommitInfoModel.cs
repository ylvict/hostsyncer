using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HostSyncer
{
    public class CommitInfoModel
    {
        public CommitModel Commit { get; set; }

        public class CommitModel
        {
            public CommitterModel Committer { get; set; }

            public class CommitterModel
            {
                public DateTime Date { get; set; }
            }
        }
    }
}
