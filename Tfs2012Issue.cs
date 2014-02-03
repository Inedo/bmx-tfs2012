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

        private bool allowHtml;

        public Tfs2010Issue(WorkItem workItem, string customReleaseNumberFieldName, bool allowHtml)
            : base(workItem.Id.ToString(), workItem.State, workItem.Title, workItem.Description, GetReleaseNumber(workItem, customReleaseNumberFieldName))
        {
            this.allowHtml = allowHtml;
        }

        public override IssueTrackerIssue.RenderMode IssueDescriptionRenderMode
        {
            get { return this.allowHtml ? RenderMode.Html : RenderMode.Text; }
        }

        private static string GetReleaseNumber(WorkItem workItem, string customReleaseNumberFieldName)
        {
            if (string.IsNullOrEmpty(customReleaseNumberFieldName))
                return workItem.IterationPath.Substring(workItem.IterationPath.LastIndexOf('\\') + 1);
            else
                return workItem.Fields[customReleaseNumberFieldName].Value.ToString().Trim();
        }
    }
}
