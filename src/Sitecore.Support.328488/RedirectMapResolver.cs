using Sitecore.XA.Feature.Redirects.Pipelines.HttpRequest;

namespace Sitecore.Support.XA.Feature.Redirects.Pipelines.HttpRequest
{
  public class RedirectMapResolver : Sitecore.XA.Feature.Redirects.Pipelines.HttpRequest.RedirectMapResolver
  {
    protected override RedirectMapping FindMapping(string filePath)
    {
      char[] slash = { '/' };
      foreach (var mapping in MappingsMap)
      {
        if ((!mapping.IsRegex && mapping.Pattern == filePath) || (mapping.IsRegex && mapping.Regex.IsMatch(filePath)) || (Context?.RawUrl != null && mapping.Pattern.TrimEnd(slash) == Context.RawUrl.ToLower().TrimEnd(slash)))
        {
          return mapping;
        }
      }
      return null;
    }
  }
}
