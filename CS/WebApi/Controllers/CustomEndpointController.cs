using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.BusinessObjects;

namespace WebAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomEndpointController : ControllerBase {
        private readonly ISecurityProvider _securityProvider;
        private readonly IObjectSpaceFactory _securedObjectSpaceFactory;
        public CustomEndpointController(ISecurityProvider securityProvider, IObjectSpaceFactory securedObjectSpaceFactory) {
            _securityProvider = securityProvider;
            _securedObjectSpaceFactory = securedObjectSpaceFactory;
        }

        [HttpGet(nameof(CanDeletePost))]
        public IActionResult CanDeletePost(string typeName) {
            var securityStrategy = (SecurityStrategy)_securityProvider.GetSecurity();
            var objectType = securityStrategy.TypesInfo.PersistentTypes.First(info => info.Name == typeName).Type;
            return Ok(securityStrategy.CanDelete(objectType));
        }

        [HttpGet(nameof(CurrentUser))]
        public object CurrentUser() {
            var securityStrategy = (SecurityStrategy)_securityProvider.GetSecurity();
            var currentUser = (ApplicationUser)securityStrategy.User;
            return new { ID = currentUser.ID, UserName = currentUser.UserName };
        }
    }
}
