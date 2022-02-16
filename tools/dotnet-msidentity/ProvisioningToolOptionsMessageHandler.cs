using Microsoft.DotNet.MSIdentity.Tool;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.DotNet.Scaffolding.Shared.Messaging
{
    internal class ProvisioningToolOptionsMessageHandler : MessageHandlerBase
    {
        private HashSet<string> _messageTypesHandled = new HashSet<string>()
        {
            MessageTypes.ProjectInfoRequest.Value,
            MessageTypes.ProjectInfoResponse.Value
        };
        private ProvisioningToolOptions _provisioningToolOptions;

        public ProvisioningToolOptionsMessageHandler(ProvisioningToolOptions provisioningToolOptions, ILogger logger)
            : base(logger)
        {
            if (provisioningToolOptions == null)
            {
                throw new ArgumentNullException(nameof(provisioningToolOptions));
            }

            _provisioningToolOptions = provisioningToolOptions;
        }

        public ProvisioningToolOptionsMessageHandler(ILogger logger)
            : base(logger)
        {

        }

        public ProvisioningToolOptions ProvisioningToolOptions
        {
            get
            {
                return _provisioningToolOptions;
            }
        }

        public override ISet<string> MessageTypesHandled => _messageTypesHandled;

        protected override bool HandleMessageInternal(IMessageSender sender, Message message)
        {
            if (MessageTypes.ProjectInfoRequest.Value.Equals(message.MessageType, StringComparison.OrdinalIgnoreCase))
            {
                Message response = sender.CreateMessage(MessageTypes.ProjectInfoResponse, _provisioningToolOptions, CurrentProtocolVersion);
                sender.Send(response);
            }
            else if (MessageTypes.ProjectInfoResponse.Value.Equals(message.MessageType, StringComparison.OrdinalIgnoreCase))
            {
                BuildProjectInformation(message);
            }
            else
            {
                return false;
            }

            return true;
        }

        [SuppressMessage("supressing re-throw exception", "CA2200")]
        private void BuildProjectInformation(Message msg)
        {
            try
            {
                _provisioningToolOptions = msg.Payload.ToObject<ProvisioningToolOptions>();
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"{MessageStrings.InvalidProjectInformationMessage}{Environment.NewLine}{msg?.ToString()}");
                throw ex;
            }
        }
    }
}
