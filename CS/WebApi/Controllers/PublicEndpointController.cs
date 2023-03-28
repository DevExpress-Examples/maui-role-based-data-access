using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Mvc;
using WebApi.BusinessObjects;

namespace WebAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class PublicEndpointController : ControllerBase {
        private readonly ISecurityProvider _securityProvider;
        private readonly INonSecuredObjectSpaceFactory _publicObjectSpaceFactory;
        public PublicEndpointController(ISecurityProvider securityProvider, INonSecuredObjectSpaceFactory publicObjectSpaceFactory) {
            _securityProvider = securityProvider;
            _publicObjectSpaceFactory = publicObjectSpaceFactory;
        }

        [HttpGet("AuthorImage/{authorID}")]
        public FileContentResult GetAuthorImage(Guid authorID) {
            var objectSpace = _publicObjectSpaceFactory.CreateNonSecuredObjectSpace<ApplicationUser>();
            var author = objectSpace.GetObjectByKey<ApplicationUser>(authorID);
            return File(author.Photo.MediaResource.MediaData, "image/jpeg");
        }
        [HttpGet("PostImage/{postID}")]
        public FileContentResult GetPostImage(int postID) {
            var objectSpace = _publicObjectSpaceFactory.CreateNonSecuredObjectSpace<Post>();
            var post = objectSpace.GetObjectByKey<Post>(postID);
            return File(post.Image.MediaResource.MediaData, "image/jpeg");
        }
    }
}
