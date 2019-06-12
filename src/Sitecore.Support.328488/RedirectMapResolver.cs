using Sitecore.Pipelines.HttpRequest;
using Sitecore.XA.Feature.Redirects.Pipelines.HttpRequest;
using Sitecore.XA.Foundation.Multisite.Extensions;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Sitecore.Support.XA.Feature.Redirects.Pipelines.HttpRequest
{
  public class RedirectMapResolver : Sitecore.XA.Feature.Redirects.Pipelines.HttpRequest.RedirectMapResolver
  {
    private string ResolvedMappingsPrefix => "SXA-Redirect-ResolvedMappings-" + Context.Database.Name + "-" + Context.Site.Name;
    private string AllMappingsPrefix => "SXA-Redirect-AllMappings-" + Context.Database.Name + "-" + Context.Site.Name;

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

    public override void Process(HttpRequestArgs args)
    {
      if (Context.Item != null || Context.Database == null || !Context.Site.IsSxaSite() || IsFile(Context.Request.FilePath))
      {
        return;
      }
      #region FIX bug #328488
      string text = EnsureSlashes(this.Context.RawUrl.ToLower());
      #endregion
      RedirectMapping redirectMapping = GetResolvedMapping(text);
      bool flag = redirectMapping != null;
      if (redirectMapping == null)
      {
        redirectMapping = FindMapping(text);
      }
      if (redirectMapping != null && !flag)
      {
        Dictionary<string, RedirectMapping> dictionary = (HttpRuntime.Cache[ResolvedMappingsPrefix] as Dictionary<string, RedirectMapping>) ?? new Dictionary<string, RedirectMapping>();
        dictionary[text] = redirectMapping;
        HttpRuntime.Cache.Add(ResolvedMappingsPrefix, dictionary, null, DateTime.UtcNow.AddMinutes(CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
      }
      if (redirectMapping != null && HttpContext.Current != null)
      {
        string targetUrl = GetTargetUrl(redirectMapping, text);
        if (redirectMapping.RedirectType == RedirectType.Redirect301)
        {
          Redirect301(HttpContext.Current.Response, targetUrl);
        }
        if (redirectMapping.RedirectType == RedirectType.Redirect302)
        {
          HttpContext.Current.Response.Redirect(targetUrl, endResponse: true);
        }
        if (redirectMapping.RedirectType == RedirectType.ServerTransfer)
        {
          HttpContext.Current.Server.TransferRequest(targetUrl);
        }
      }
    }
    private string EnsureSlashes(string text)
    {
      return StringUtil.EnsurePostfix('/', StringUtil.EnsurePrefix('/', text));
    }
  }
}
