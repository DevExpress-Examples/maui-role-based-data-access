using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace WebApi.BusinessObjects {
    [DomainComponent]
    [DefaultClassOptions]
    public class Post : IXafEntityObject, IObjectSpaceLink {
        [DevExpress.ExpressApp.Data.Key]
        public virtual int PostId { get; set; }
        public virtual string Title { get; set; }
        public virtual string Content { get; set; }
        public virtual bool IsPublished { get; set; }
        public virtual MediaDataObject Image { get; set; }

        public virtual ApplicationUser Author { get; set; }

        IObjectSpace IObjectSpaceLink.ObjectSpace { get; set; }

        void IXafEntityObject.OnCreated() {
            var objectSpace = ((IObjectSpaceLink)this).ObjectSpace;
            if (objectSpace.IsNewObject(this)) {
                Author = objectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("ID=CurrentUserId()"));
            }
        }

        void IXafEntityObject.OnSaving() { }

        void IXafEntityObject.OnLoaded() { }
    }
}
