using System.Text;
using System.Text.RegularExpressions;
using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.Services.Processing;

/// <summary>
/// Processes SEC filing HTML documents to extract structured sections
/// </summary>
public static class FilingProcessor
{
    private static readonly Regex HeadingRegex = new(@"<h[1-6][^>]*>(.*?)</h[1-6]>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TagCleanerRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Processes filing HTML content and extracts sections with anchor IDs
    /// </summary>
    /// <param name="html">The HTML content of the filing</param>
    /// <returns>List of parsed sections</returns>
    public static Task<List<HtmlSlice>> ProcessFilingContentAsync(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return Task.FromResult(new List<HtmlSlice>());
        }

        var slices = new List<HtmlSlice>();
        var headings = FindHeadings(html);

        if (headings.Count == 0)
        {
            // No headings found, return entire content as single slice
            slices.Add(new HtmlSlice
            {
                Content = html,
                Label = "Complete Document",
                AnchorTargetId = "document",
                StartOffset = 0,
                Length = html.Length
            });
            return Task.FromResult(slices);
        }

        // Process each heading as a section
        for (int i = 0; i < headings.Count; i++)
        {
            var heading = headings[i];
            var startOffset = heading.StartOffset;
            var endOffset = i < headings.Count - 1
                ? headings[i + 1].StartOffset
                : html.Length;

            var sectionContent = html.Substring(startOffset, endOffset - startOffset);
            var label = CleanLabel(heading.Text);
            var anchorId = GenerateAnchorId(label);

            slices.Add(new HtmlSlice
            {
                Content = sectionContent,
                Label = label,
                AnchorTargetId = anchorId,
                StartOffset = startOffset,
                Length = endOffset - startOffset
            });
        }

        return Task.FromResult(slices);
    }

    private static List<HeadingInfo> FindHeadings(string html)
    {
        var headings = new List<HeadingInfo>();
        var matches = HeadingRegex.Matches(html);

        foreach (Match match in matches)
        {
            var text = match.Groups[1].Value;
            var cleanText = TagCleanerRegex.Replace(text, "").Trim();

            if (!string.IsNullOrWhiteSpace(cleanText))
            {
                headings.Add(new HeadingInfo
                {
                    Text = cleanText,
                    StartOffset = match.Index
                });
            }
        }

        return headings;
    }

    private static string CleanLabel(string text)
    {
        // Remove HTML tags
        var clean = TagCleanerRegex.Replace(text, " ");

        // Normalize whitespace
        clean = WhitespaceRegex.Replace(clean, " ");

        return clean.Trim();
    }

    private static string GenerateAnchorId(string label)
    {
        // Convert to lowercase
        var id = label.ToLowerInvariant();

        // Replace spaces and special characters with underscores
        id = Regex.Replace(id, @"[^a-z0-9]+", "_");

        // Remove leading/trailing underscores
        id = id.Trim('_');

        // Limit length
        if (id.Length > 100)
        {
            id = id.Substring(0, 100).TrimEnd('_');
        }

        return id;
    }

    /// <summary>
    /// Converts HTML sections to Markdown format
    /// </summary>
    /// <param name="slices">HTML sections to convert</param>
    /// <returns>List of sections with markdown content</returns>
    public static List<HtmlSlice> ConvertToMarkdown(List<HtmlSlice> slices)
    {
        var converter = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true
        });

        return slices.Select(slice => new HtmlSlice
        {
            Content = converter.Convert(slice.Content),
            Label = slice.Label,
            AnchorTargetId = slice.AnchorTargetId,
            StartOffset = slice.StartOffset,
            Length = slice.Length
        }).ToList();
    }

    private class HeadingInfo
    {
        public string Text { get; set; } = string.Empty;
        public int StartOffset { get; set; }
    }
}
