using System;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [Serializable]
    internal sealed class Tfs2010Issue : IssueTrackerIssue
    {
        public static class DefaultStatusNames
        {
            public static string Active = "Active";
            public static string Resolved = "Resolved";
            public static string Closed = "Closed";
        }

        public Tfs2010Issue(WorkItem workItem, string customReleaseNumberFieldName)
            : base(workItem.Id.ToString(), workItem.State, workItem.Title, workItem.Description, GetReleaseNumber(workItem, customReleaseNumberFieldName))
        {
        }

        private static string GetReleaseNumber(WorkItem workItem, string customReleaseNumberFieldName)
        {
            if (string.IsNullOrEmpty(customReleaseNumberFieldName))
                return workItem.IterationPath + @"\"; // Add a backslash to the end of the iteration path in case our release number shows up at the end.
            else
                return @"\" + workItem.Fields[customReleaseNumberFieldName].Value.ToString().Trim() + @"\";
        }
    }
}
