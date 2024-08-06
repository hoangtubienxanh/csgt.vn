using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace VietnamTrafficPolice.Apis.Page;

internal static partial class TrafficIncidentPage
{
    private const string EmptyContentSelector =
        """div[style="width:97%; height:auto; float:left; text-align:center; padding: 0px 10px; color:red;"]""";

    [GeneratedRegex("\\s+")]
    private static partial Regex HasTrailingWhitespaceRegex();

    internal static IEnumerable<TrafficIncidentTableContent> CrawlTrafficIncidentData(IHtmlDocument document)
    {
        var root = document.QuerySelector("#bodyPrint123");
        ArgumentNullException.ThrowIfNull(root);

        if (document.QuerySelector(EmptyContentSelector) is not null)
        {
            return [];
        }

        List<TrafficIncidentTableContent> incidents = [];

        var rows = root.QuerySelectorAll("hr")
            .OfType<IHtmlHrElement>()
            .Where(ContainsStyle);

        foreach (var row in rows)
        {
            JsonObject dictionary = new();
            List<JsonNode> contactAddress = [];

            foreach (var (header, value) in row.ParseContent())
            {
                // Handles newlines in contact address row
                var normalized = HasTrailingWhitespaceRegex().Replace(value, " ").Trim();

                switch (header)
                {
                    case "Nơi giải quyết vụ việc: ":
                        // Ignored because there's no content
                        break;
                    case "":
                        contactAddress.Insert(0, normalized);
                        break;
                    default:
                        dictionary.Add(header, normalized);
                        break;
                }
            }

            dictionary.Add("Nơi giải quyết vụ việc: ", new JsonArray(contactAddress.ToArray()));
            incidents.Add(dictionary.Deserialize(VietnamTrafficPoliceContext.Default.TrafficIncidentTableContent)!);
        }

        return incidents;
    }

    private static bool ContainsStyle(IHtmlHrElement element)
    {
        var style = element.Attributes.FirstOrDefault(attribute =>
            attribute is { Name: "style", Value: "margin-bottom: 25px;" });

        return style is not null;
    }

    private static IEnumerable<(string, string)> ParseContent(this IHtmlHrElement root)
    {
        return from element in TraverseBackward(root)
            let @group = element.QuerySelector(".row")
            let item1 = @group?.QuerySelector("label > span")?.TextContent ?? string.Empty
            let item2 = @group?.QuerySelector("div > span")?.TextContent ??
                        @group?.QuerySelector("div")?.TextContent ?? element.TextContent
            select (item1, item2);
    }

    private static IEnumerable<IElement> TraverseBackward(this IElement startElement)
    {
        for (var current = startElement.PreviousElementSibling;
             current is IHtmlDivElement { ClassName: "form-group" };
             current = current.PreviousElementSibling)
        {
            yield return current;
        }
    }
}