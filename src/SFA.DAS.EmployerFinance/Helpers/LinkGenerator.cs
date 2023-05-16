using Microsoft.AspNetCore.Routing;

namespace SFA.DAS.EmployerFinance.Helpers
{
    public interface ILinkGeneratorWrapper
    {
        string GetPathByName(string name, object values);
    }

    public class LinkGeneratorWrapper : ILinkGeneratorWrapper
    {
        private readonly LinkGenerator _linkGenerator;

        public LinkGeneratorWrapper(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public string GetPathByName(string name, object values)
        {
            return _linkGenerator.GetPathByName(name, values);
        }
    }

}
