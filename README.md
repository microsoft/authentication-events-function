# Microsoft Entra ID Custom authentication extension sample


Within Microsoft Entra ID and Microsoft Entra External ID's sign-up and sign-in user flow, there are built-in authentication events. You can also add [custom authentication extensions](https://learn.microsoft.com/entra/identity-platform/custom-extension-overview) at specific points within the authentication flow. A custom authentication extension is essentially an event listener that, when activated, makes an HTTP call to a REST API endpoint where you define a workflow action. For example, you could use a custom claims provider to add external user data to the token before the token is issued.


This Azure Function (WebJob) sample code demonstrates how to create a web API for Entra ID custom authentication extension. 

> [!NOTE]
>
> The [Microsoft.Azure.WebJobs.Extensions.AuthenticationEvents](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.AuthenticationEvents) NuGet library is currently in preview. Steps in this article are subject to change. For general availability implementation of implementing a token issuance start event, you can do so using the [Azure portal](https://learn.microsoft.com/entra/identity-platform/custom-extension-tokenissuancestart-setup?tabs=visual-studio-code%2Cazure-portal&pivots=azure-portal).

## Prerequisites

- A basic understanding of the concepts covered in [Custom authentication extensions overview](https://learn.microsoft.com/entra/identity-platform/custom-extension-overview).
- An Azure subscription with the ability to create Azure Functions. If you don't have an existing Azure account, sign up for a [free trial](https://azure.microsoft.com/free/dotnet/) or use your [Visual Studio Subscription](https://visualstudio.microsoft.com/subscriptions/) benefits when you [create an account](https://account.windowsazure.com/Home/Index).
- A Microsoft Entra ID tenant. You can use either a customer or workforce tenant for this how-to guide.
- One of the following IDEs and configurations:
    - Visual Studio Code, with the [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) extension enabled.
    - Visual Studio with [Azure Development workload for Visual Studio](https://learn.microsoft.com/en-us/dotnet/azure/configure-visual-studio) configured.

## Other samples

This Azure Function (WebJob) sample code demonstrates how to create a web API without using the [Microsoft.Azure.WebJobs.Extensions.AuthenticationEvents](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.AuthenticationEvents) NuGet library (currently in preview). For the code sample with NuGet the library, please check the [Azure Function](https://github.com/microsoft/custom-authentication-extension-sdk) sample. 