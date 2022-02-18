// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.MSIdentity;
using Microsoft.DotNet.MSIdentity.Tool;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Web.CodeGeneration.Tools;

namespace Microsoft.VisualStudio.Web.CodeGeneration.Design
{
    public class Program
    {
        public const string TOOL_NAME = "dotnet-aspnet-codegenerator-design";
        private const string APPNAME = "Code Generation";
        public const string MSIDENTITY_TOOL_NAME = "dotnet-msidentity";
        public const string MSIDENTITY = "msidentity";
        private static ConsoleLogger _logger;

        public static async Task Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            _logger = new ConsoleLogger();
            _logger.LogMessage($"Command Line: {string.Join(" ", args)}", LogMessageLevel.Trace);

            if (args.Length >= 4)
            {
                if (args[0] == MSIDENTITY)
                {
                    await ExecuteMsIdentity(args, _logger);
                    return;
                }
            }
            Execute(args, _logger);
            return;
        }

/*        private static Dictionary<string, string> mapping = new Dictionary<string, string>
        {
            {"--port-number", nameof(MsIdentityCmdLineArgs.PortNumber)},
            {"msidentity", nameof(MsIdentityCmdLineArgs.IsMsIdentity)},
            {Commands.LIST_AAD_APPS_COMMAND, nameof(MsIdentityCmdLineArgs.LIST_AAD_APPS_COMMAND) },
            {Commands.LIST_SERVICE_PRINCIPALS_COMMAND, nameof(MsIdentityCmdLineArgs.LIST_SERVICE_PRINCIPALS_COMMAND) },
            {Commands.LIST_TENANTS_COMMAND, nameof(MsIdentityCmdLineArgs.LIST_TENANTS_COMMAND) }
        };*/

        private static async Task ExecuteMsIdentity(string[] args, ConsoleLogger logger)
        {
            //MsIdentityCmdLineArgs msIdentityCmdLineArgs = new MsIdentityCmdLineArgs();
           /* ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCommandLine(args, mapping);
            IConfiguration configuration = configurationBuilder.Build();
            ConfigurationBinder.Bind(configuration, msIdentityCmdLineArgs);*/
            try
            {
                var portNumber = int.Parse(args[4]);
                using (var client = await ScaffoldingClient.Connect(portNumber, logger))
                {
                    var messageOrchestrator = new MessageOrchestrator(client, logger);
                    var provisioningToolOptions = messageOrchestrator.GetProvisioningToolOptions();
                    if (string.Equals(args[1], Commands.LIST_AAD_APPS_COMMAND, StringComparison.OrdinalIgnoreCase))
                    {
                        IMsAADTool msAADTool = MsAADToolFactory.CreateTool(Commands.LIST_AAD_APPS_COMMAND, provisioningToolOptions);
                        await msAADTool.Run();
                    }
                    /*string projectAssetsFile = ProjectModelHelper.GetProjectAssetsFile(projectInformation);
                    //fix package dependencies sent from VS
                    projectInformation = projectInformation.AddPackageDependencies(projectAssetsFile);
                    var codeGenArgs = ToolCommandLineHelper.FilterExecutorArguments(args);
                    var isSimulationMode = ToolCommandLineHelper.IsSimulationMode(args);
                    CodeGenCommandExecutor executor = new CodeGenCommandExecutor(projectInformation,
                        codeGenArgs,
                        "Debug",
                        logger,
                        isSimulationMode);
*/
                    //int exitCode = executor.Execute((changes) => messageOrchestrator.SendFileSystemChangeInformation(changes));

                    messageOrchestrator.SendScaffoldingCompletedMessage();
                }
            }
            catch (Exception)
            { }
        }

        private static void Execute(string[] args, ConsoleLogger logger)
        {
            //System.Diagnostics.Debugger.Launch();
            var app = new CommandLineApplication(false)
            {
                Name = APPNAME,
                Description = Resources.AppDesc
            };

            // Define app Options;
            app.HelpOption("-h|--help");
            var projectPath = app.Option("-p|--project", Resources.ProjectPathOptionDesc, CommandOptionType.SingleValue);
            var appConfiguration = app.Option("-c|--configuration", Resources.ConfigurationOptionDesc, CommandOptionType.SingleValue);
            var framework = app.Option("-tfm|--target-framework", Resources.TargetFrameworkOptionDesc, CommandOptionType.SingleValue);
            var buildBasePath = app.Option("-b|--build-base-path", "", CommandOptionType.SingleValue);
            var dependencyCommand = app.Option("--no-dispatch", "", CommandOptionType.NoValue);
            var port = app.Option("--port-number", "", CommandOptionType.SingleValue);
            var noBuild = app.Option("--no-build", "", CommandOptionType.NoValue);
            var simMode = app.Option("--simulation-mode", Resources.SimulationModeOptionDesc, CommandOptionType.NoValue);

#if DEBUG
            if (args.Contains("--debug", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Attach a debugger to processID: {System.Diagnostics.Process.GetCurrentProcess().Id} and hit enter.");
                Console.ReadKey();
            }
#endif

            app.OnExecute(async () =>
            {
                var exitCode = 1;
                try
                {
                    CodeGenerationEnvironmentHelper.SetupEnvironment();
                    string project = projectPath.Value();
                    if (string.IsNullOrEmpty(project))
                    {
                        project = Directory.GetCurrentDirectory();
                    }
                    project = Path.GetFullPath(project);
                    var configuration = appConfiguration.Value();

                    var portNumber = int.Parse(port.Value());
                    using (var client = await ScaffoldingClient.Connect(portNumber, logger))
                    {
                        var messageOrchestrator = new MessageOrchestrator(client, logger);
                        var projectInformation = messageOrchestrator.GetProjectInformation();
                        string projectAssetsFile = ProjectModelHelper.GetProjectAssetsFile(projectInformation);
                        //fix package dependencies sent from VS
                        projectInformation = projectInformation.AddPackageDependencies(projectAssetsFile);
                        var codeGenArgs = ToolCommandLineHelper.FilterExecutorArguments(args);
                        var isSimulationMode = ToolCommandLineHelper.IsSimulationMode(args);
                        CodeGenCommandExecutor executor = new CodeGenCommandExecutor(projectInformation,
                            codeGenArgs,
                            configuration,
                            logger,
                            isSimulationMode);

                        exitCode = executor.Execute((changes) => messageOrchestrator.SendFileSystemChangeInformation(changes));

                        messageOrchestrator.SendScaffoldingCompletedMessage();
                    }
                }
                catch(Exception ex)
                {
                    logger.LogMessage(Resources.GenericErrorMessage, LogMessageLevel.Error);
                    logger.LogMessage(ex.Message, LogMessageLevel.Error);
                    logger.LogMessage(ex.StackTrace, LogMessageLevel.Trace);
                    if (!logger.IsTracing)
                    {
                        logger.LogMessage(Resources.EnableTracingMessage);
                    }

                }
                return exitCode;
                
            });

            app.Execute(args);
        }
    }
}
