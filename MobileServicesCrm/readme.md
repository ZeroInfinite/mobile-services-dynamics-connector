# Azure Mobile Services Connector SDK for Dynamics CRM Online

This folder contains a DomainManager extension to Azure Mobile Services for connecting to Dynamics CRM Online. There is also a sample Mobile Services backend project, **ActivityLoggerBackend**. This project sets up the 'tables' endpoints for several built-in CRM entities, such as Activities, Contacts, Incidents, and Tasks and enables offline sync with the Azure Mobile client SDKs.

If you have any questions on this SDK or Azure Mobile in general, tweet [@AzureMobile](https://twitter.com/AzureMobile) or post on the [MSDN forums](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=azuremobile).

## Table of Contents

  - [Prerequisites](#prerequisites)
  - [Overview](#overview)
  - [Overview of Azure Active Directory configuration](#overview-of-azure-active-directory-configuration)
  - [Project Structure](#project-structure)
  - [0. Connect your Dynamics CRM Online Active Directory tenant to Azure](#0-connect-your-dynamics-crm-online-active-directory-tenant-to-azure)
  - [1. Provision a new Azure Mobile Service](#1-provision-a-new-azure-mobile-service)
  - [2. Publish the backend project to your Mobile Service](#2-publish-the-backend-project-to-your-mobile-service)
  - [3. Register your mobile service with Azure Active Directory](#3-register-your-mobile-service-with-azure-active-directory)
  - [4. (Only for Windows Store client apps) Create a Windows Store Package Security Identifier (SID)](#4-only-for-windows-store-client-apps-create-a-windows-store-package-security-identifier-sid)
  - [5. Register the native client application with Azure Active Directory](#5-register-the-native-client-application-with-azure-active-directory)
  - [6. Configure your Mobile Service settings](#6-configure-your-mobile-service-settings)
  - [7. Verify your Active Directory application registrations](#7-verify-your-active-directory-application-registrations)
  - [8. Configure and run the client app](#8-configure-and-run-the-client-app)

## Prerequisites

- Visual Studio 2013
- [.NET Runtime version 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42643)
- Azure subscription
- Dynamics CRM Online subscription

## Overview

To deploy this sample, you will follow these steps:

1. Provision a new Azure Mobile Service.
2. In Visual Studio, build and deploy the project **ActivityLoggerBackend.sln** to your Mobile Service.
3. Configure the Azure Active Directory authentication settings to enable on-behalf-of access between your iOS mobile client application and the CRM backend.
4. Configure and run your iOS mobile client application.

## Overview of Azure Active Directory configuration

- You will create two Active Directory application entries:
  - One for the Windows client app, called `ActivityLoggerNative`
  - One for the Azure Mobile Services backend, called `ActivityLoggerBackend`

- The native app AAD registration `ActivityLoggerNative` will have the following permissions
  - Azure Active Directory: **Enable sign-on and read users' profiles**
  - ActivityLoggerBackend: **Access ActivityLoggerBackend**

- The Azure Mobile Services backend AAD registration `ActivityLoggerBackend` will have the following permissions:
  - Azure Active Directory: **Enable sign-on and read users' profiles**
  - Dynamics CRM Online: **Access CRM Online as organization users**

Note that the native client app does not need direct permissions to Dynamics CRM. Instead, it will retrieve an authentication token from AAD for the specific logged-in user.

Then, this token is passed to the Azure Mobile Services backend as part of the `LoginAsync` method (or `loginWithProvider` on iOS). Since the Azure Mobile Services backend has access to AAD and has delegated access to Dynamics CRM Online, it can use this user authentication token to securely take actions in Dynamics CRM on-behalf-of the logged in user in the native client application.

## Repository structure:

- [WindowsAzure.Mobile.Service.DynamicsCrm](server/MobileCrmDomainManager/WindowsAzure.Mobile.Service.DynamicsCrm). A custom DomainManager extension for Dynamics CRM Online. Contains code to set up custom entities.

- [ActivityLoggerBackend](server/ActivityLoggerBackend). Mobile Services server project, deploy to your own hosted mobile service. Exposes REST endpoints for built-in CRM entities, such as Activities, Contacts, and Tasks.

- [AzureActivityLogger.iOS](client). iOS client project that connects to an instance of ActivityLoggerBackend.

- [Tools](../Tools/CrmSvcUtil). Codegen tools for CRM.

## 0. Connect your Dynamics CRM Online Active Directory tenant to Azure

- If you don’t have an Azure tenant (account) or you do have one but your Office 365 subscription with Microsoft Dynamics CRM Online is not available in your Azure account, then follow the instructions in the topic [Set up Azure Active Directory access for your Developer Site](https://msdn.microsoft.com/office/office365/HowTo/setup-development-environment).

- For more information, see the section **Register an application with Microsoft Azure** in the document [Walkthrough: Register a CRM app with Active Directory](https://msdn.microsoft.com/en-us/library/dn531010.aspx)

## 1. Provision a new Azure Mobile Service

Follow these steps to create a new mobile service.

1.  Log into the [Management Portal](https://manage.windowsazure.com/). At the bottom of the navigation pane, click **+NEW**. Expand **Compute** and **Mobile Service**, then click **Create**.

  This displays the **Create a Mobile Service** dialog.

2.  In the **Create a Mobile Service** page, type a subdomain name for the new mobile service in the **URL** textbox, select **Create a free 20 MB SQL Database**, select **.NET** runtime, then click **NEXT**.

  This displays the **Specify database settings** page.

  If you already have a database server you would like to use, you can instead choose **Use existing Database** and then select that database.

3.  In **Name**, type the name of the new database, then type **Login name**, which is the administrator login name for the new SQL Database server, type and confirm the password, and click the check button to complete the process.

## 2. Publish the backend project to your Mobile Service

- In Visual Studio, open the solution **ActivityLoggerBackend.sln** and build.

- Right-click the project **ActivityLoggerBackend** and click **Publish**.

- Select **Azure Mobile Services** as the publish target and select the mobile service that you just created.

- Accept the default settings and click **Publish**.

- Once the web deployment has completed, you will see a browser page showing that your backend has been deployed. Make a note of the app URL that is shown on the address bar.

## 3. Register your mobile service with Azure Active Directory

In order for your mobile service backend application to be able to access Dynamics CRM, you need to set up permissions for it and for the mobile client app. In this section, you will register the mobile services backend application with Active Directory.

1. Log on to the [Azure Management Portal](https://manage.windowsazure.com), navigate to your Mobile Service, click the **Identity** tab, then scroll down to the section for **Windows Azure Active Directory**.

  Copy the **App URL** shown there, which will have the form `https://my-mobile-service.azure-mobile.net/login/aad`

2. Navigate to **Active Directory** in the management portal. In the directory list, select your Dynamics CRM Online tenant. Click on the **Domains** tab, and make a note of your directory's default domain.

3. Click the **Applications** tab, click **Add** > **Add an application my organization is developing**.

4. In the Add Application Wizard, enter **ActivityLoggerBackend** for your application and click the **Web Application and/or Web API** type. Then click to continue.

5. In the **Sign-on URL** box, paste the app URL value you copied from your mobile service. Enter the same unique value in the **App ID URI** box, then click to continue.

    ![Set the AAD app properties](./readme-files/mobile-services-add-app-wizard-2-waad-auth.png)

6. After the application **ActivityLoggerBackend** has been added, click on its **Configure** tab. Edit the **Reply URL** value under **Single Sign-on** to be the URL of your mobile service appended with the path, `signin-aad`. For example,  `https://my-mobile-service.azure-mobile.net/signin-aad`. Make sure that you are using the HTTPS scheme.

7. Still in the **Configure** tab, in the **Permissions to Other Applications** section, click **Add application**. Select the **Dynamics CRM Online** application and click the check to close the dialog. Grant the **Access CRM Online as organization users** delegated permission, then click **Save**.

8. Ensure there is another entry in the **Permissions to Other Applications** section for **Windows Azure Active Directory** with the delegated permissions **Enable sign-on and read users' profiles**.

9. Still in your Active Directory application, on the **Configure** tab, copy the **Client ID** for the app.

10. Return to your mobile service's **Identity** tab and paste the copied **Client ID** value for the Windows Azure Active Directory identity provider.

11. In the **Allowed Tenants** list, add the domain of the directory in which you registered the application (e.g. `contoso.onmicrosoft.com`). You can find your default domain name by clicking the **Domains** tab on your Azure Active Directory tenant. Add your domain name to the **Allowed Tenants** list, then click **Save**.  

## 4. (Only for Windows Store client apps) Create a Windows Store Package Security Identifier (SID)

To register a Windows Store app with your Azure Active Directory tenant, you must associate it to the Windows Store and have a package security identifier (SID) for the app. The package SID gets registered with the native application settings in the Azure Active Directory.

###Associate the app with a new store app name

1. In Visual Studio, right click the client app project and click **Store** and **Associate App with the Store**

2. Sign into your Dev Center account.

3. Enter the app name you want to reserve for the app and click **Reserve**.

4. Select the new app name and click **Next**.

5. Click **Associate** to associate the app with the store name.

###Retrieve the package SID for your app.

Now you need to retrieve your package SID which will be configured with the native app settings.

1. Log into the [Microsoft Account Developer Center](https://account.live.com/developers/applications/index) and click the  app name that you just created.

2. Copy your package SID from the **App Settings** section. It will begin with the prefix `ms-app://`

    ![Package SID](./readme-files/package-sid.png)


## 5. Register the native client application with Azure Active Directory

1. Navigate to **Active Directory** in the [Azure Management Portal](https://manage.windowsazure.com).

2. Select the directory you used before, and then select the **Applications** tab at the top. Click **ADD** at the bottom to create a new app registration.

3. Click **Add an application my organization is developing**.

4. In the Add Application Wizard, enter a **Name** for your application such as `ActivityLoggerNativeApp` and click the  **Native Client Application** type. Then click to continue.

5. In the **Redirect URI** box, enter the following:

     - For a Windows Store client, use the **Package SID** that you copied from the Microsoft Account Developer Center, which has the prefix `ms-app://`.

     - For iOS or Xamarin, use the */login/done* endpoint for your mobile service. This value should be similar to  `https://my-mobile-service.azure-mobile.net/login/done`. Make sure that you are using the HTTPS scheme.

6. Once the native application has been added, click on its **Configure** tab. Copy the **Client ID**. You will need this later when you configure your iOS or Windows Store client app.

7. Scroll the page down to the **Permissions to Other Applications** section and click **Add application**.

8. Search for the web application **ActivityLoggerBackend** that you registered earlier and click the plus icon. Then click the check to close the dialog. Make sure you picked the Dynamics CRM Online tenant when you selected the directory.

9. On the new entry you just added, open the **Delegated Permissions** dropdown and select **Access ActivityLoggerBackend**. Then click **Save**.


## 6. Configure your Mobile Service settings

1. Return to the **Applications** tab for your tenant, and select the application **ActivityLoggerBackend**.

2. Under Configure, scroll down to the **Keys** section. You will obtain a Client Secret by generating a new key. Note once you create a key and leave the page, there is no way to get it out of the portal again. Upon creation you must copy and save this value in a secure location. Select a duration for your key, then click save, and copy out the resulting value.

3. In the **Mobile Services** section of the Management Portal, select your mobile service. Navigate to the **Configure** tab, and scroll down to the **App Settings** section. Here you can provide key-value pairs to help you reference the necessary settings.

    * Set `CrmAuthorityUrl` to be the authority endpoint for your AAD tenant. It will be of the form `https://login.windows.net/contoso.onmicrosoft.com`. Copy this value as you will need it later.

    * Set `CrmClientSecret` to be the client secret value you obtained earlier.

    * Set `CrmUrl` to be the URL for your Dynamics CRM Online tenant, which will have the form `https://contoso.crm.dynamics.com`.

    These values will be used in the ActivityLogger backend code using `ApiServices.Settings`.

## 7. Verify your Active Directory application registrations

1. Navigate to the Active Directory section in the Azure portal and select your Dynamics CRM tenant. Select the **Applications** tab.

2. Verify that you have 2 application entries, one for **ActivityLoggerBackend** and one for **ActivityLoggerNativeApp**.

3. Select each application registration and select the **Dashboard** tab. Verify that you have the following permissions listed under **OAuth 2.0 Permission Grants**:

    - **ActivityLoggerBackend** (Mobile Service registration)
        - DYNAMICS CRM ONLINE

          Access CRM Online as organization users

        - WINDOWS AZURE ACTIVE DIRECTORY

          Enable sign-on and read users' profiles

    - **ActivityLoggerNativeApp** (native mobile client app registration)

        - ACTIVITYLOGGERBACKEND

          Access ActivityLoggerBackend

        - WINDOWS AZURE ACTIVE DIRECTORY

          Enable sign-on and read users' profiles


## 8. Configure and run the client app

In this section, you will download the sample iOS client app that uses the backend that you just deployed. Clone or download the git repository to your Mac, and ensure you are using Xcode 6.0 or later.

1. Install [CocoaPods](https://cocoapods.org/):

  ```
  sudo gem install cocoapods
  ```

2. In Terminal, navigate to the directory `MobileServicesCrm/client/AzureActivityLogger.iOS`. Build the dependencies for your project:

  ```
  pod install
  ```

3. Open the *workspace* `Azure Activity Logger.xcworkspace` in Xcode. When using Cocoapods, you must open the Xcode workspace rather than the project.

4. Open `AzureConnector.m` in the folder `Azure Activity Logger/AzureConnector`. Locate the following code near the top of the file:

  ```objective-c
  // endpoint for your hosted mobile service
  NSString *const mobileServiceUrl =    @"INSERT MOBILE SERVICE URI";

  // Application Key, retrieve from your Mobile Service in the Azure Management portal
  NSString *const mobileServiceAppKey = @"INSERT MOBILE SERVICE APP KEY";

  // Active Directory authority, usually of the form "https://login.windows.net/yourtenant/onmicrosoft.com"
  NSString *const aadAuthorityUri =     @"INSERT AUTHORITY URI";

  // client ID for native app registration in Azure Active Directory
  NSString *const activeDirectoryNativeClientId = @"INSERT CLIENT ID";
  ```

  Update the code as follows:

  - To set `mobileServiceUrl`, retrieve your mobile service URL from the Azure Management Portal, which is of the form `https://my-mobile-service.azure-mobile.net`.

  - To set `mobileServiceAppKey`, navigate to your mobile service in the Azure Management Portal. Click **Manage Keys** in the command bar at the bottom. Copy the first key, which is the application key.

  - To set `aadAuthorityUri`, use the authority endpoint for your AAD tenant. It will be the same value that you set for `CrmAuthorityUrl` in your Mobile Service app settings, which is of the form `https://login.windows.net/contoso.onmicrosoft.com`.

  - To set `activeDirectoryNativeClientId`, use the **Client ID** value for your Active Directory application registration **ActivityLoggerNativeApp**.

5. Build and run the app on the iOS simulator or a device.

  - Tap the sync button in the lower right corner to retrieve data from your Dynamics CRM account and sync to the simulator or device.

  - You will be prompted to log in to Azure Active Directory. Verify that the AAD logon screen shows the name of your native app registration, **ActivityLoggerNativeApp**.

  - Once you have successfully authenticated, the app will retreive contacts that are owned by your logged-in username.

  - To make changes, tap on a contact and tap the checkmark on the contact info page. You can add a new check in note with details.

  - Changes will be saved on the device or emulator, but will not be synced with Dynamics CRM until you tap the sync button again. Verify that the new data is not in CRM, then press the sync button.

  - Verify that your new changes are now in Dynamics CRM. Open the new check-in entry in CRM and make modifications to the data, such as changing the checkin date or the title.

  - Press the sync button on the emulator again to retrieve the new data.


# Resources

- Tutorial: [Register your apps to use an Azure Active Directory Account login]

- Tutorial: [iOS: Authenticate your app with Active Directory Authentication Library Single Sign-On]

- Tutorial: [Windows Store: Authenticate your app with Active Directory Authentication Library Single Sign-On]

<!-- links -->

[Azure Management Portal]: https://manage.windowsazure.com/
[Classic Azure Management Portal]: https://manage.windowsazure.com/

[Register your apps to use an Azure Active Directory Account login]: https://azure.microsoft.com/en-us/documentation/articles/mobile-services-how-to-register-active-directory-authentication

[iOS: Authenticate your app with Active Directory Authentication Library Single Sign-On]: https://azure.microsoft.com/en-us/documentation/articles/mobile-services-dotnet-backend-ios-adal-sso-authentication/

[Windows Store: Authenticate your app with Active Directory Authentication Library Single Sign-On]: https://azure.microsoft.com/en-us/documentation/articles/mobile-services-windows-store-dotnet-adal-sso-authentication/
