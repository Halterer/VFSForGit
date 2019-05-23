using CommandLine;
using GVFS.Common;
using GVFS.Common.FileSystem;
using GVFS.Common.Git;
using GVFS.Common.Http;
using GVFS.Common.NamedPipes;
using GVFS.Common.Tracing;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GVFS.CommandLine
{
    [Verb(TestVerb.TestVerbName, HelpText = "Print a test message to stdout")]
    public class TestVerb : GVFSVerb
    {
        private const string TestVerbName = "test";

        public override string EnlistmentRootPathParameter { get; set; }

        protected override string VerbName
        {
            get { return TestVerbName; }
        }

        public override void Execute()
        {
            this.Output.WriteLine("Hello world!");
            Environment.Exit(0);
        }
    }
}
