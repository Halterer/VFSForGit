using CommandLine;
using GVFS.Common;
using GVFS.Common.Git;
using GVFS.Common.NamedPipes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GVFS.CommandLine
{
    [Verb(StatisticsVerb.StatisticsVerbName, HelpText = "Get statistics for the health state of the repository")]
    public class StatisticsVerb : GVFSVerb.ForExistingEnlistment
    {
        private const string StatisticsVerbName = "statistics";

        protected override string VerbName
        {
            get { return StatisticsVerbName; }
        }

        protected override void Execute(GVFSEnlistment enlistment)
        {
            using (NamedPipeClient pipeClient = new NamedPipeClient(enlistment.NamedPipeName))
            {
                if (!pipeClient.Connect())
                {
                    this.ReportErrorAndExit("Unable to connect to GVFS.  Try running 'gvfs mount'");
                }

                try
                {
                    pipeClient.SendRequest(NamedPipeMessages.Placeholders.Request);

                    NamedPipeMessages.Message statisticsMessage = pipeClient.ReadResponse();
                    if (!statisticsMessage.Header.Equals(NamedPipeMessages.Placeholders.SuccessResult))
                    {
                        this.Output.WriteLine("Bad response from statistics pipe: " + statisticsMessage.Header);
                        return;
                    }

                    NamedPipeMessages.Message modifiedPathsMessage = new NamedPipeMessages.Message(NamedPipeMessages.ModifiedPaths.ListRequest, "1");
                    pipeClient.SendRequest(modifiedPathsMessage);

                    NamedPipeMessages.Message modifiedPathsResponse = pipeClient.ReadResponse();
                    if (!modifiedPathsResponse.Header.Equals(NamedPipeMessages.ModifiedPaths.SuccessResult))
                    {
                        this.Output.WriteLine("Bad response from modified path pipe: " + modifiedPathsResponse.Header);
                        return;
                    }

                    int trackedFilesCount = 0;
                    int placeholderCount = int.Parse(statisticsMessage.Body);
                    string[] modifiedPathsList = modifiedPathsResponse.Body.Split('\0');
                    modifiedPathsList = modifiedPathsList.Take(modifiedPathsList.Length - 1).ToArray();
                    int modifiedPathsCount = modifiedPathsList.Length;

                    GitProcess gitProcess = new GitProcess(enlistment);
                    GitProcess.Result result = gitProcess.LsTree(
                        GVFSConstants.DotGit.HeadName,
                        line =>
                        {
                            trackedFilesCount++;
                        },
                        recursive: true);

                    string trackedFilesCountFormatted = trackedFilesCount.ToString("N0");
                    string placeholderCountFormatted = placeholderCount.ToString("N0");
                    string modifiedPathsCountFormatted = modifiedPathsCount.ToString("N0");

                    int longest = Math.Max(trackedFilesCountFormatted.Length, placeholderCountFormatted.Length);
                    longest = Math.Max(longest, modifiedPathsCountFormatted.Length);

                    this.Output.WriteLine("\nRepository statistics");
                    this.Output.WriteLine("Total paths tracked by git:     " + trackedFilesCountFormatted.PadLeft(longest) +  " | 100%");
                    this.Output.WriteLine("Total number of placeholders:   " + placeholderCountFormatted.PadLeft(longest) + " | " + this.FormattedPercent(((double)placeholderCount) / trackedFilesCount));
                    this.Output.WriteLine("Total number of modified paths: " + modifiedPathsCountFormatted.PadLeft(longest) + " | " + this.FormattedPercent(((double)modifiedPathsCount) / trackedFilesCount));

                    this.Output.WriteLine("\nTotal hydration percentage:     " + this.FormattedPercent((double)(placeholderCount + modifiedPathsCount) / trackedFilesCount).PadLeft(longest + 7));

                    // this.Output.WriteLine("\n\n" + GVFSConstants.DotGit.HeadName);

                    // Console.ReadLine();
                }
                catch (BrokenPipeException e)
                {
                    this.ReportErrorAndExit("Unable to communicate with GVFS: " + e.ToString());
                }
            }
        }

        private string FormattedPercent(double percent)
        {
            return percent.ToString("P0").PadLeft(4);
        }
    }
}
