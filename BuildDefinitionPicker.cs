using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.TeamFoundation.Build.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    public sealed class BuildDefinitionPicker : DropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildDefinitionPicker"/> class.
        /// </summary>
        public BuildDefinitionPicker()
        {
        }

        /// <summary>
        /// Gets or sets the team project.
        /// </summary>
        public string TeamProject { get; set; }

        internal void FillItems(string configurationProfileName)
        {
            if (string.IsNullOrEmpty(this.TeamProject))
                return;

            var configurer = TfsConfigurer.GetConfigurer(InedoLib.Util.NullIf(configurationProfileName, string.Empty));
            if (configurer == null)
                return;

            var collection = TfsActionBase.GetTeamProjectCollection(configurer);

            var buildService = collection.GetService<IBuildServer>();

            this.Items.Clear();
            this.Items.AddRange(
                buildService
                    .QueryBuildDefinitions(this.TeamProject)
                    .Select(d => new ListItem(d.Name, d.Name))
                    .ToArray()
            );
        }

    }
}
