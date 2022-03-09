using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace EDM.Infohub.Integration
{
    public class CustomInspectMessage : IClientMessageInspector
    {
        private string _token;

        public CustomInspectMessage(string token)
        {
            _token = token;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            request.Headers.Add(new MessageHeader<string>(_token).GetUntypedHeader("_SecureConnect_TokenCredential", String.Empty));
            return null;
        }

    }

    public class BtgSecurityBehavior : IEndpointBehavior
    {
        private string _token;

        public BtgSecurityBehavior(string token)
        {
            _token = token;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            var customInspectMessage = new CustomInspectMessage(_token);
            clientRuntime.ClientMessageInspectors.Add(customInspectMessage);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}