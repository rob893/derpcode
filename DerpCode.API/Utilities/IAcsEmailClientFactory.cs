using Azure.Communication.Email;
using Azure.Core;

namespace DerpCode.API.Utilities;

public interface IAcsEmailClientFactory
{
    EmailClient CreateClient(TokenCredential? tokenCredential = null);
}