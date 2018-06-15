using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Resources;
using Sitecore.Shell;

namespace Sitecore.Support.Shell.Applications.ContentManager.Galleries.Links
{
  public class GalleryLinksForm: Sitecore.Shell.Applications.ContentManager.Galleries.Links.GalleryLinksForm
  {

    bool IsHidden(Item item)
    {
      while (item != null)
      {
        if (item.Appearance.Hidden)
        {
          return true;
        }

        item = item.Parent;
      }

      return false;
    }

    /// <summary>
    /// Processes the referrers.
    /// </summary>
    /// <param name="item">The item to render information for.</param>
    /// <param name="result">The result of rendering.</param>
    protected override void ProcessReferrers(Item item, StringBuilder result)
    {
      ItemLink[] links = this.GetRefererers(item);
      List<Pair<Item, ItemLink>> pairs = new List<Pair<Item, ItemLink>>();

      foreach (ItemLink link in links)
      {
        Database database = Factory.GetDatabase(link.SourceDatabaseName, false);
        if (database == null)
        {
          continue;
        }

        Item referrer = database.GetItem(link.SourceItemID);
        if (referrer == null || !this.IsHidden(referrer) || UserOptions.View.ShowHiddenItems)
        {
          pairs.Add(new Pair<Item, ItemLink>(referrer, link));
        }
      }

      this.RenderReferrers(result, pairs);
    }

    /// <summary>
    /// Renders the referrers.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="referrers">The referrers.</param>
    private void RenderReferrers(StringBuilder result, List<Pair<Item, ItemLink>> referrers)
    {
      result.Append(this.ReferrersHeader);

      if (referrers.Count == 0)
      {
        result.Append("<div class=\"scNone\">" + Translate.Text(Texts.NONE) + "</div>");
        return;
      }

      result.Append("<div class=\"scRef\">");

      foreach (Pair<Item, ItemLink> pair in referrers)
      {
        Item referrer = pair.Part1;
        ItemLink link = pair.Part2;
        Item sourceItem = null;
        if (link != null)
        {
          sourceItem = link.GetSourceItem();
        }

        if (referrer == null)
        {
          result.Append(string.Format("<div class=\"scLink\">{0} {1}: {2}, {3}</div>", Images.GetImage("Applications/16x16/error.png", 16, 16, "absmiddle", "0px 4px 0px 0px"), Translate.Text("Not found"), link.SourceDatabaseName, link.SourceItemID));
          continue;
        }

        string sourceLanguage = referrer.Language.ToString();
        string sourceVersion = referrer.Version.ToString();
        if (sourceItem != null)
        {
          sourceLanguage = sourceItem.Language.ToString();
          sourceVersion = sourceItem.Version.ToString();
        }

        if (sourceItem != null)
        {
          var sourceField = sourceItem.Fields[link.SourceFieldID];
          if (sourceField != null)
          {
            if (sourceField.HasValue)
            {
              result.Append("<a href=\"#\" class=\"scLink\" onclick='javascript:return scForm.invoke(\"item:load(id=" + referrer.ID + ",language=" + sourceLanguage + ",version=" + sourceVersion + ")\")'>" + Images.GetImage(referrer.Appearance.Icon, 16, 16, "absmiddle", "0px 4px 0px 0px") + referrer.GetUIDisplayName());
              if (link != null && !link.SourceFieldID.IsNull)
              {
                Field field = referrer.Fields[link.SourceFieldID];
                if (!string.IsNullOrEmpty(field.DisplayName))
                {
                  result.Append(" - ");
                  result.Append(field.DisplayName);
                }
              }
              result.Append(" - [" + referrer.Paths.Path + "]</a>");
            }
            }
        }
      }

      result.Append("</div>");
    }


  }
}