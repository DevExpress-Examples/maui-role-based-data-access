using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.EF;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using WebApi.BusinessObjects;

namespace WebApi.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();

        //Test users and posts are generated here
        ApplicationUser alexUser = CreateUserIfDoesntExist("Alex", "123", () => GetRoleWithEditorPermissions());
        if (ObjectSpace.IsNewObject(alexUser)) {
            CreatePostIfDoesntExist(".NET MAUI New Blog Draft Title", "Draft text", "draftImage", alexUser, isPublished: false);
            CreatePostIfDoesntExist(".NET MAUI — Your Feedback Counts", "Blog text", "feedbackImage", alexUser, true);
            CreatePostIfDoesntExist(".NET MAUI — Upcoming Features", "Blog text", "upcomingFeaturesImage", alexUser, true);
        }
        ApplicationUser antonyUser = CreateUserIfDoesntExist("Antony", "123", () => GetRoleWithEditorPermissions());
        if (ObjectSpace.IsNewObject(antonyUser)) {
            CreatePostIfDoesntExist("Localize Your .NET MAUI MVVM Application", "Blog text", "feedbackImage", antonyUser, true);
            CreatePostIfDoesntExist(".NET MAUI — Support for the Latest .NET 6 Updates & New Learning Materials", "Blog text", "leaningMaterialsImage", antonyUser, true);
        }

        ApplicationUser dennisUser = CreateUserIfDoesntExist("Dennis", "123", () => GetRoleWithEditorPermissions());
        if (ObjectSpace.IsNewObject(dennisUser)) {
            CreatePostIfDoesntExist("DevExpress Multi-Platform App UI Controls Support .NET MAUI GA Release (v22.1)", "Blog text", "releaseImage", dennisUser, true);
            CreatePostIfDoesntExist("DevExpress UI Controls Support the Latest Microsoft .NET MAUI Preview 10", "Blog text", "previewImage", dennisUser, true);
        }

        CreateUserIfDoesntExist("Admin", "", () => CreateAdminRole());
        CreateUserIfDoesntExist("Viewer", "", () => GetRoleWithDefaultPermissions());
        CreateUserIfDoesntExist("Editor", "", () => GetRoleWithEditorPermissions());
        ObjectSpace.CommitChanges();
    }
    private ApplicationUser CreateUserIfDoesntExist(string name, string password, Func<PermissionPolicyRole> createRoleAction) {
        var sampleUser = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == name);
        if (sampleUser == null) {
            sampleUser = ObjectSpace.CreateObject<ApplicationUser>();
            sampleUser.Photo = ObjectSpace.CreateObject<MediaDataObject>();
            sampleUser.Photo.MediaData = GetResourceByName(name);
            sampleUser.UserName = name;
            sampleUser.SetPassword(password);
            sampleUser.Roles.Add(createRoleAction());
            ((ISecurityUserWithLoginInfo)sampleUser).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(sampleUser));
        }
        return sampleUser;
    }
    private Post CreatePostIfDoesntExist(string title, string content, string imageFileName, ApplicationUser author, bool isPublished = false) {
        var samplePost = ObjectSpace.FirstOrDefault<Post>(p => p.Title == title);
        if (samplePost == null) {
            samplePost = ObjectSpace.CreateObject<Post>();
            samplePost.Title = title;
            samplePost.Content = content;
            samplePost.Author = author;
            samplePost.IsPublished = isPublished;
            samplePost.Image = ObjectSpace.CreateObject<MediaDataObject>();
            samplePost.Image.MediaData = GetResourceByName(imageFileName);
        }
        return samplePost;
    }
    private byte[] GetResourceByName(string shortName) {
        string embeddedResourceName = Array.Find(GetType().Assembly.GetManifestResourceNames(), (s) => s.Contains(shortName));
        if (string.IsNullOrEmpty(embeddedResourceName))
            return null;
        var stream = GetType().Assembly.GetManifestResourceStream(embeddedResourceName!);
        if (stream == null) {
            return null;
        }
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
    private PermissionPolicyRole GetRoleWithDefaultPermissions(string name = "Default") {
        var role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == name);
        if (role == null) {
            role = ObjectSpace.CreateObject<PermissionPolicyRole>();
            role.Name = name;
            role.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            role.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
            role.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            role.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            role.AddMemberPermission<ApplicationUser>(SecurityOperations.Read, "UserName;Photo", null, SecurityPermissionState.Allow);
            role.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            role.AddTypePermissionsRecursively<MediaDataObject>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
            role.AddTypePermissionsRecursively<MediaResourceObject>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
            role.AddObjectPermissionFromLambda<Post>(SecurityOperations.Read, p => p.IsPublished, SecurityPermissionState.Allow);
        }
        return role;
    }
    private PermissionPolicyRole GetRoleWithEditorPermissions(string roleName = "Editor") {
        PermissionPolicyRole role = GetRoleWithDefaultPermissions(roleName);
        role.AddTypePermissionsRecursively<Post>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
        return role;
    }

    private PermissionPolicyRole CreateAdminRole() {
        var role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Administrators");
        if (role == null) {
            role = ObjectSpace.CreateObject<PermissionPolicyRole>();
        }
        role.IsAdministrative = true;
        return role;
    }

}
