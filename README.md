# Role-Based Data Access with the DevExpress Web API Service

This example shows how to use our free [.NET App Security Library & Web API Service](https://www.devexpress.com/products/net/application_framework/security-web-api-service.xml) to implement authentication and role-based data access in your .NET MAUI application. A wizard helps you generate a ready-to-use authentication service. This service uses the [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) ORM to access a database.    

<img src="https://user-images.githubusercontent.com/12169834/228174231-0d1f2d88-8b2b-4db4-8969-fd9b2379d8ce.png" width="30%"/>


You can find more information about our Web API Service's access restrictions in the following resources:

[Create a Standalone Web API Application](https://docs.devexpress.com/eXpressAppFramework/403401/backend-web-api-service/create-new-application-with-web-api-service?p=net6)

[A 1-Click Solution for CRUD Web API with Role-based Access Control via EF Core & ASP.NET](https://www.youtube.com/watch?v=T7y4gwc1n4w&list=PL8h4jt35t1wiM1IOux04-8DiofuMEB33G)

## Prerequisites

[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads), if you run this solution on Windows.

## Run Projects

1. Run Visual Studio as an administrator so that the IDE can create the database as defined in `appsettings.json`. Open the `WebAPI` solution.

2. Choose the `WebApi` item in the **debug** dropdown menu. This selection will debug the project on the [Kestrel](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-7.0) web server.

    ![Run Settings](images/authenticate-run-settings.png)

    If you prefer IIS Express to Kestrel, select **IIS Express** from the **debug** drop-down menu, and use an external text editor to add the following code to `.vs\MAUI_WebAPI\config\applicationhost.config`:

    ```xaml
    <sites>
        <site name="WebSite1" id="1" serverAutoStart="true">
        <!-* ... -->
            <bindings>
                <binding protocol="http" bindingInformation="*:65201:*" />
                <binding protocol="https" bindingInformation="*:44317:*" />
                <binding protocol="https" bindingInformation="*:44317:localhost" />
                <binding protocol="http" bindingInformation="*:65201:localhost" />
            </bindings>
        </site>
        <!-* ... -->
    </sites>
    ```

3. Right-click the MAUI project, choose `Set as Startup Project`, and select your emulator. Note that physical devices that are attached over USB do not allow you to access your machine's localhost.
4. Right-click the `WebAPI` project and select `Debug > Start new instance`.
5. Right-click the `MAUI` project and select `Debug > Start new instance`.

## Implementation Details

### Service and Communication

* DevExpress Web API Service uses JSON Web Tokens (JWT) to authorize users. Call `WebAPI`'s **Authenticate** endpoint and pass a username and password to the endpoint from the .NET MAUI application. In this example, token generation logic is implemented in the `WebAPIService.RequestTokenAsync` method:

    ```csharp
      private async Task<HttpResponseMessage> RequestTokenAsync(string userName, string password) {
            return await HttpClient.PostAsync($"{ApiUrl}Authentication/Authenticate",
                                                new StringContent(JsonSerializer.Serialize(new { userName, password = $"{password}" }), Encoding.UTF8,
                                                ApplicationJson));
      }
    ```

    Include the token in [HttpClient.DefaultRequestHeaders.Authorization](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.httprequestheaders.authorization?view=net-7.0). All subsequent requests can then access private endpoints and data: 

    ```csharp
    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await tokenResponse.Content.ReadAsStringAsync());
    ```

  File to Look At: [WebAPIService.cs](CS/MAUI/Services/WebAPIService.cs)

* WebApi service contains the following custom endpoints:

    * The **CanDeletePost** endpoint allows you to send a request from a mobile device to the service and check whether the current user can delete posts. This allows you to show/hide the delete button in the UI.

        File to Look At: [Updater.cs](CS/WebAPI/Controllers/CustomEndpointController.cs)

    * The **CurrentUser** endpoint retrieves the current user. You can use it to get the avatar of the logged-in user.

        File to Look At: [Updater.cs](CS/WebAPI/Controllers/CustomEndpointController.cs)

    * The **GetAuthorImage** endpoint retrieves an image by a user ID. A user image does not come with the user object to allow you to load them separately.

        File to Look At: [Updater.cs](CS/WebAPI/Controllers/PublicEndpointController.cs)

    * The **GetAuthorImage** endpoint retrieves a post image by a post ID. A post image does not come with the user object to allow you to load them separately.

        File to Look At: [Updater.cs](CS/WebAPI/Controllers/PublicEndpointController.cs)


* To create users and specify their passwords, use the `Updater.UpdateDatabaseAfterUpdateSchema` method. You can modify a user's password directly in the database or use the full version of our [XAF UI](https://docs.devexpress.com/eXpressAppFramework/112649/data-security-and-safety/security-system/authentication/passwords-in-the-security-system).

    File to Look At: [Updater.cs](CS/WebAPI/DatabaseUpdate/Updater.cs)

* Use the `PermissionPolicyRole` objects in the `Updater` class to add a user permissions. The following code snippet calls the `AddObjectPermissionFromLambda` method to configure the "Viewer" role that allows the user to read published posts:

    ```csharp
    role.AddObjectPermissionFromLambda(SecurityOperations.Read, p => p.IsPublished, SecurityPermissionState.Allow);
    ```

    File to Look At: [Updater.cs](CS/WebAPI/DatabaseUpdate/Updater.cs)

* The `AddTypePermissionsRecursively` method adds CRUD (create, read, update, delete) permissions for the `Post` type to the "Editor" users (Alex, Antony, and Dennis):
    
    ```csharp
    role.AddTypePermissionsRecursively<Post>(SecurityOperations.Read | SecurityOperations.Write | SecurityOperations.Create | SecurityOperations.DeleteObject, SecurityPermissionState.Allow);
    ```

    File to Look At: [Updater.cs](CS/WebAPI/DatabaseUpdate/Updater.cs)

### Login UI and View Model

* Use the [TextEdit.StartIcon](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.EditBase.StartIcon) and [PasswordEdit.StartIcon](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.EditBase.StartIcon) properties to display icons in the [TextEdit](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.TextEdit) and [PasswordEdit](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.PasswordEdit) controls.

    ```xaml
    <dxe:TextEdit LabelText="Login" StartIcon="editorsname" .../>
    <dxe:PasswordEdit LabelText="Password" StartIcon="editorspassword" .../>
    ```

    File to Look At: [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)
    
* To validate the [PasswordEdit](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.PasswordEdit) control's value, use the [EditBase.HasError](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.EditBase.HasError) and [EditBase.ErrorText](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.EditBase.ErrorText) inherited properties.

    ```xaml
    <dxe:PasswordEdit ... HasError="{Binding HasError}" ErrorText="{Binding ErrorText}"/>
    ```

    File to Look At: [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)

    ```csharp
    public class LoginViewModel : BaseViewModel {
        // ...
        string errorText;
        bool hasError;
        // ...

        public string ErrorText {
            get => errorText;
            set => SetProperty(ref errorText, value);
        }

        public bool HasError {
            get => hasError;
            set => SetProperty(ref hasError, value);
        }
        async void OnLoginClicked() {
            /// ...
            string response = await DataStore.Authenticate(userName, password);
            if (!string.IsNullOrEmpty(response)) {
                ErrorText = response;
                HasError = true;
                return;
            }
            HasError = false;
            await Navigation.NavigateToAsync<SuccessViewModel>();
        }
    }
    ```

    File to Look At: [LoginViewModel.cs](CS/MAUI/ViewModels/LoginViewModel.cs)

* Specify the [TextEdit.ReturnType](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.EditBase.ReturnType) inherited property to focus the [PasswordEdit](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.PasswordEdit) control after the [TextEdit](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.TextEdit) control's value is edited.
* Bind the [PasswordEdit.ReturnCommand](https://docs.devexpress.com/MAUI/DevExpress.Maui.Editors.EditBase.ReturnCommand) property to the **Login** command to execute the command when a user enters the password:

    ```xaml
    <dxe:PasswordEdit ReturnCommand="{Binding LoginCommand}"/>
    ```

    File to Look At: [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)
    
    ```csharp
    public class LoginViewModel : BaseViewModel {
        // ...
        public LoginViewModel() {
            LoginCommand = new Command(OnLoginClicked);
            SignUpCommand = new Command(OnSignUpClicked);
            PropertyChanged +=
                (_, __) => LoginCommand.ChangeCanExecute();

        }
        // ...
        public Command LoginCommand { get; }
        public Command SignUpCommand { get; }
        // ...
    }
    ```

    File to Look At: [LoginViewModel.cs](CS/MAUI/ViewModels/LoginViewModel.cs)

* In the UI, the Image object caches its displayed image.  because we pass a string with a Uri to the Image. Image caching is described in the [MAUI documentation](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/image?view=net-maui-7.0#load-a-remote-image). To create a Uri, we use a [MultiBinding](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/multibinding?view=net-maui-7.0) that gets the host name and the author or post ID:

    ```xaml
    <Image>
        <Image.Source>
            <MultiBinding StringFormat="{}{0}PublicEndpoint/PostImage/{1}">
                <Binding Source="{x:Static webService:WebAPIService.ApiUrl}"/>
                <Binding Path="PostId"/>
            </MultiBinding>
        </Image.Source>
    </Image>
    ```

    File to Look At: [ItemsPage.xaml](CS/MAUI/Views/ItemsPage.xaml)


## Debug Specifics

Android emulator and iOS simulator request a certificate to access a service over HTTPS. In this example, we switch to HTTP in debug mode:

```csharp
#if !DEBUG
    app.UseHttpsRedirection();
#endif
```

#### MAUI - Android

```xml
<network-security-config>
    <domain-config cleartextTrafficPermitted="true">
        <domain includeSubdomains="true">10.0.2.2</domain>
    </domain-config>
</network-security-config>
```

#### MAUI - iOS

```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSAllowsLocalNetworking</key>
    <true/>
</dict>
```

This allows you to bypass the certificate check without the need to create a development certificate or implement HttpClient handlers.

For more information, please refer to [Connect to local web services from Android emulators and iOS simulators](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services?view=net-maui-7.0#android-network-security-configuration).

**We recommend that you use HTTP only when you develop/debug your application. In production, use HTTPS for security reasons.**

## Files to Look At

* [WebAPIService.cs](CS/MAUI/Services/WebAPIService.cs)
* [Updater.cs](CS/WebAPI/DatabaseUpdate/Updater.cs)
* [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)
* [ItemsPage.xaml](CS/MAUI/Views/ItemsPage.xaml)
* [LoginViewModel.cs](CS/MAUI/ViewModels/LoginViewModel.cs)

## Documentation

* [Featured Scenario: Role-Based Data Access](https://docs.devexpress.com/MAUI/404316)
* [Featured Scenarios](https://docs.devexpress.com/MAUI/404291)
* [Create a Standalone Web API Application](https://docs.devexpress.com/eXpressAppFramework/403401/backend-web-api-service/create-new-application-with-web-api-service?p=net6)

## More Examples

* [Authenticate Users with the WebAPI Service](https://github.com/DevExpress-Examples/maui-authenticate/)
* [DevExpress Mobile UI for .NET MAUI](https://github.com/DevExpress-Examples/maui-demo-app/)
