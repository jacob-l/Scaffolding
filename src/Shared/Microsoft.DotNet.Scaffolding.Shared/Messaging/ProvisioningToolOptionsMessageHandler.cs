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
            MessageTypes.ProvisioningToolOptionsRequest.Value,
            MessageTypes.ProvisioningToolOptionsResponse.Value
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
            _provisioningToolOptions = new ProvisioningToolOptions();
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
            if (MessageTypes.ProvisioningToolOptionsRequest.Value.Equals(message.MessageType, StringComparison.OrdinalIgnoreCase))
            {
                Message response = sender.CreateMessage(MessageTypes.ProvisioningToolOptionsResponse, _provisioningToolOptions, CurrentProtocolVersion);
                sender.Send(response);
            }
            else if (MessageTypes.ProvisioningToolOptionsResponse.Value.Equals(message.MessageType, StringComparison.OrdinalIgnoreCase))
            {
                BuildProvisioningToolOptions(message);
            }
            else
            {
                return false;
            }

            return true;
        }

        [SuppressMessage("supressing re-throw exception", "CA2200")]
        private void BuildProvisioningToolOptions(Message msg)
        {
            try
            {
                _provisioningToolOptions = msg.Payload.ToObject<ProvisioningToolOptions>() ?? throw new Exception("bah");
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Not working booo{Environment.NewLine}{msg?.ToString()}");
                throw ex;
            }
        }
    }
}
