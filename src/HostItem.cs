using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HostSyncer
{
    public class HostItem
    {
        public string IP { get; set; }
        public string Domain { get; set; }

        public HostItem(string section)
        {
            var meta = section.Trim().Split(' ', '\t');
            if (meta.Length < 2) throw new NotSupportedException("\"" + section + "\" cannot be resolved!");
            this.IP = meta.First().Trim();
            this.Domain = meta.Last().Trim();
        }
    }

    public class HostItemComparer : IEqualityComparer<HostItem>
    {
        public bool Equals(HostItem x, HostItem y)
        {
            return x.Domain == y.Domain;
        }

        public int GetHashCode(HostItem obj)
        {
            string hCode = obj.IP + "^" + obj.Domain;
            return hCode.GetHashCode();
        }
    }
}
