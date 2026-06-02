using HtmlAgilityPack;
using System.Text;

namespace WordSprinter.Services.BookParser;

public class HtmlTextExtractor
{
    private static readonly System.Text.RegularExpressions.Regex PageNumPattern =
        new(@"^\s*-?\s*\d+\s*-?\s*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public string ExtractText(string html, List<string> footnotes)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Remove noise nodes
        var removeSelectors = new[] { "script", "style", "nav", "head" };
        foreach (var sel in removeSelectors)
        {
            foreach (var node in doc.DocumentNode.SelectNodes($"//{sel}") ?? Enumerable.Empty<HtmlNode>())
                node.Remove();
        }

        // Extract footnotes
        var footnoteNodes = doc.DocumentNode.SelectNodes(
            "//aside[@epub:type='footnote'] | //aside[contains(@class,'footnote')] | //div[contains(@class,'footnote')]")
            ?? Enumerable.Empty<HtmlNode>();
        foreach (var fn in footnoteNodes.ToList())
        {
            string fnText = fn.InnerText.Trim();
            if (!string.IsNullOrEmpty(fnText))
                footnotes.Add(fnText);
            fn.Remove();
        }

        var sb = new StringBuilder();
        ExtractTextNodes(doc.DocumentNode, sb);
        return sb.ToString();
    }

    private void ExtractTextNodes(HtmlNode node, StringBuilder sb)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            string text = System.Net.WebUtility.HtmlDecode(node.InnerText);
            if (!string.IsNullOrWhiteSpace(text) && !PageNumPattern.IsMatch(text))
                sb.Append(text).Append(' ');
            return;
        }

        // Block elements — add newline separation
        bool isBlock = node.Name is "p" or "div" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6"
                       or "li" or "blockquote" or "section" or "article";

        foreach (var child in node.ChildNodes)
            ExtractTextNodes(child, sb);

        if (isBlock) sb.Append(' ');
    }
}
