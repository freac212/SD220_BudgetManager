using System;
using System.Reflection;

namespace SD220_Deliverable_1_DGrouette.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}