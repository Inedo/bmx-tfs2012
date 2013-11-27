using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.TeamFoundation.Server;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    public sealed class TeamProjectPicker : DropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamProjectPicker"/> class.
        /// </summary>
        public TeamProjectPicker()
        {
            this.AutoPostBack = true;
        }

        internal void FillItems(string configurationProfileName)
        {
            if (this.Items.Count > 0) 
                return;

            var configurer = TfsConfigurer.GetConfigurer(InedoLib.Util.NullIf(configurationProfileName, string.Empty));
            if (configurer == null) 
                return;

            var collection = TfsActionBase.GetTeamProjectCollection(configurer);
            var structureService = collection.GetService<ICommonStructureService>();
            
            this.Items.AddRange(
                structureService
                    .ListProjects()
                    .Select(p => new ListItem(p.Name, p.Name))
                    .ToArray()
            );
        }

    }
}
