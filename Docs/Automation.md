# Git Credential Manager for Windows

 The [Git Credential Manager for Windows](https://github.com/Microsoft/Git-Credential-Manager-for-Windows) (GCM) provides secure Git credential storage for Windows. GCM provides multi-factor authentication support for [Visual Studio Team Services](https://www.visualstudio.com/), [Team Foundation Server](Faq.md#q-i-thought-microsoft-was-maintaining-this-why-does-the-gcm-not-work-as-expected-with-tfs), and [GitHub](https://www.github.com).

## Build Agents and Automation

 Build agents, and other automation, often require specialized setup and configuration. While there is detailed documentation on [GCM configuration options](Docs/Configuration.md), below are common recommendations for settings agents often require to operate.
 
### Recommendations

 Build agents cannot manage modal dialogs, therefore we recommended the following configuration.

     git config --global credential.interactive never

 Build agents often need to minimize the amount of network traffic they generate.

 To avoid Microsoft Account vs. Azure Active Directory look-up against a Visual Studio Team Services account use...
 
 ... for Azure Directory backed authentication:

     git config --global credential.authority Azure
     
 ... for Microsoft Account backed authentication:

     git config --global credential.authority Microsoft
     
 If your agents rely on an on premise instance of Team Foundation Server and [Windows Domain Authentication](https://msdn.microsoft.com/en-us/library/ee253152(v=bts.10).aspx), use:
 
     git config --global credential.authority Windows

 To avoid unnecessary service account credential validation, when relying on Microsoft Account or Azure Active Directory use:

     git config --global credential.validate false