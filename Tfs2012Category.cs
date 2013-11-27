using System;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [Serializable]
    internal sealed class Tfs2010Category : IssueTrackerCategory
    {
        public enum CategoryTypes { Collection, Project }

        public CategoryTypes CategoryType { get; private set; }

        private Tfs2010Category(string categoryId, string categoryName, IssueTrackerCategory[] subCategories, CategoryTypes categoryType)
            : base(categoryId, categoryName, subCategories) 
        { 
            CategoryType = categoryType;
        }

        internal static Tfs2010Category CreateCollection(TeamProjectCollection projectCollection, Tfs2010Category[] projectCategories)
        {
            return new Tfs2010Category(projectCollection.Name, 
                projectCollection.Name, 
                projectCategories, 
                CategoryTypes.Collection);
        }

        internal static Tfs2010Category CreateProject(Project project)
        {
            return new Tfs2010Category(project.Name,
                project.Name,
                new Tfs2010Category[0],
                CategoryTypes.Project);
        }
    }
}
