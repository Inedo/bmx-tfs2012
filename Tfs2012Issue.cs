using System;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [Serializable]
    internal sealed class Tfs2010Issue : Issue
    {
        public static class DefaultStatusNames
        {
            public static string Active = "Active";
            public static string Resolved = "Resolved";
            public static string Closed = "Closed";
        }

        public Tfs2010Issue(WorkItem workItem, string customReleaseNumberFieldName)
        {
            this.IssueDescription = workItem.Description;
            this.IssueId = workItem.Id.ToString();
            this.IssueStatus = workItem.State;
            this.IssueTitle = workItem.Title;
            if (String.IsNullOrEmpty(customReleaseNumberFieldName))
                this.ReleaseNumber = workItem.IterationPath.Substring(workItem.IterationPath.LastIndexOf('\\') + 1);
            else
                this.ReleaseNumber = workItem.Fields[customReleaseNumberFieldName].Value.ToString().Trim();
        }
    }
}
