using System;
using System.Linq;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web;

[assembly: ExtensionConfigurer(typeof(Inedo.BuildMasterExtensions.TFS2012.TfsConfigurer))]

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [CustomEditor(typeof(TfsConfigurerEditor))]
    public sealed class TfsConfigurer : ExtensionConfigurerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TfsConfigurer"/> class.
        /// </summary>
        public TfsConfigurer()
        {
        }

        public static readonly string ConfigurerName = typeof(TfsConfigurer).FullName + "," + typeof(TfsConfigurer).Assembly.GetName().Name;

        /// <summary>
        /// The base url of the TFS store, should not include collection name, e.g. "http://server:port/tfs"
        /// </summary>
        [Persistent]
        public string BaseUrl { get; set; }
        /// <summary>
        /// The username used to connect to the server
        /// </summary>
        [Persistent]
        public string UserName { get; set; }
        /// <summary>
        /// The password used to connect to the server
        /// </summary>
        [Persistent]
        public string Password { get; set; }
        /// <summary>
        /// The domain of the server
        /// </summary>
        [Persistent]
        public string Domain { get; set; }
        /// <summary>
        /// Returns true if BuildMaster should connect to TFS using its own account, false if the credentials are specified
        /// </summary>
        [Persistent]
        public bool UseSystemCredentials { get; set; }

        public Uri BaseUri { get { return new Uri(this.BaseUrl); } }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Empty;
        }

        internal static TfsConfigurer GetConfigurer(string profileName = null)
        {
            var profiles = StoredProcs
                .ExtensionConfiguration_GetConfigurations(TfsConfigurer.ConfigurerName)
                .Execute();

            var configurer =
                profiles.FirstOrDefault(p => string.Equals(profileName, p.Profile_Name, System.StringComparison.OrdinalIgnoreCase)) ??
                    profiles.FirstOrDefault(p => p.Default_Indicator.Equals(Domains.YN.Yes));

            if (configurer == null)
                return null;

            return (TfsConfigurer)Util.Persistence.DeserializeFromPersistedObjectXml(configurer.Extension_Configuration);
        }
    }
}
