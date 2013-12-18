using System;
using System.Collections.Generic;
using System.Linq;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [Serializable]
    internal sealed class Tfs2010Category : IssueTrackerCategory
    {
        public enum CategoryTypes { Collection, Project, AreaPath }

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
            var areaPaths = new List<Tfs2010Category>();

            foreach (Node area in project.AreaRootNodes)
            {
                areaPaths.Add(new Tfs2010Category(area.Path, area.Path, new Tfs2010Category[0], CategoryTypes.AreaPath));

                foreach (Node item in area.ChildNodes)
                {
                    areaPaths.Add(new Tfs2010Category(item.Path, item.Path, new Tfs2010Category[0], CategoryTypes.AreaPath));
                }
            }

            return new Tfs2010Category(project.Name,
                project.Name,
                areaPaths.ToArray(),
                CategoryTypes.Project);
        }
    }
}
