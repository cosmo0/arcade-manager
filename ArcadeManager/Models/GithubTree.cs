using System;
using System.Collections.Generic;

namespace ArcadeManager.Models
{
    public class GithubTree
    {
        public class File
        {
            public string path { get; set; }
            public string mode { get; set; }
            public string type { get; set; }
            public string sha { get; set; }
            public int size { get; set; }
            public string url { get; set; }
        }

        public string sha { get; set; }
        public string url { get; set; }
        public List<File> tree { get; set; }
        public bool truncated { get; set; }
    }
}
