using System;
using System.Linq;
using ZBuildLights.Core.Models.CruiseControl;

namespace ZBuildLights.Web.Models.Admin
{
    public class EditProjectCruiseServerViewModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public CcProjectViewModel[] Projects { get; set; }

        public CcProjectViewModel[] ProjectsByProjectAndName
        {
            get { return Projects.OrderBy(x => x.ProjectAndName).ToArray(); }
        }
    }
}