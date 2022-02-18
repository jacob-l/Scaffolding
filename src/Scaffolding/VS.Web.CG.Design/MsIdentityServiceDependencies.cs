using Microsoft.DotNet.MSIdentity.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Web.CodeGeneration.Design
{
    internal class MsIdentityServiceDependencies
    {
        private ServiceProvider _serviceProvider;

        public MsIdentityServiceDependencies()
        {
            _serviceProvider = new ServiceProvider();
        }
        public ServiceProvider AddCodeGenerationServices()
        {
            if (_serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(_serviceProvider));
            }

            IFileSystem fileSystem = (IFileSystem)DefaultFileSystem.Instance;
            //Ordering of services is important here
            _serviceProvider.Add(typeof(IFileSystem), fileSystem);
            _serviceProvider.Add(typeof(IFilesLocator), new FilesLocator());

            _serviceProvider.AddServiceWithDependencies<ICodeGeneratorAssemblyProvider, DefaultCodeGeneratorAssemblyProvider>();
            _serviceProvider.AddServiceWithDependencies<ICodeGeneratorLocator, CodeGeneratorsLocator>();
            _serviceProvider.AddServiceWithDependencies<CodeGenCommand, CodeGenCommand>();
            _serviceProvider.AddServiceWithDependencies<IPackageInstaller, PackageInstaller>();
            _serviceProvider.AddServiceWithDependencies<ICodeGeneratorActionsService, CodeGeneratorActionsService>();
            _serviceProvider.AddServiceWithDependencies<IMsAADTool, MsAADTool>();
            return _serviceProvider;
        }

    }
}
