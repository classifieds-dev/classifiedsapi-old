using System.Linq;
using AdsApi.ActionConstraints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace AdsApi.Attributes
{
    public class RequiredFromQuery : FromQueryAttribute, IParameterModelConvention
    {
        public void Apply(ParameterModel parameter)
        {
            if (parameter.Action.Selectors != null && parameter.Action.Selectors.Any())
            {
                parameter.Action.Selectors.Last().ActionConstraints.Add(new RequiredFromQueryActionConstraint(parameter.BindingInfo?.BinderModelName ?? parameter.ParameterName));
            }
        }
    }
}
