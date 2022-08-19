using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Qlik.Engine;
using Qlik.Sense.Client;
using Qlik.Sense.Client.Visualizations;

namespace LayoutEvaluator
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "<url>";
            var apiKey = "<apiKey>";
            var appId = "<appId>";

            var location = QcsLocation.FromUri(url);
            location.AsApiKey(apiKey);

            using (var app = location.App(appId))
            {
                Run(app).Wait();
            }
        }

        private static async Task Run(IApp app)
        {
            var sheetList = await app.GetSheetListAsync();
            var sheetListLayout = await sheetList.GetLayoutAsync();

            foreach (var listItem in sheetListLayout.AppObjectList.Items)
            {
                await AnalyzeSheet(app, listItem);
            }
        }

        private static async Task AnalyzeSheet(IApp app, SheetObjectViewListContainer listItem)
        {
            WriteLine($"Analyzing sheet: {listItem.Data.Title}");
            _indentation++;
            var sheet = await app.GetGenericObjectAsync(listItem.Info.Id);
            var children = await sheet.GetChildInfosAsync();
            foreach (var nxInfo in children)
            {
                await AnalyzeObject(app, nxInfo);
            }
            _indentation--;
        }

        private static async Task AnalyzeObject(IApp app, NxInfo nxInfo)
        {
            Write($"GetLayout for {nxInfo.Id}: ");
            var o = await app.GetGenericObjectAsync(nxInfo.Id);
            var sw = new Stopwatch();
            sw.Start();
            var layout = (await o.GetLayoutAsync()).As<VisualizationBaseLayout>();
            sw.Stop();
            WriteLine(sw.Elapsed.ToString() + $" (Title: \"{layout.Title}\")");
        }

        private static int _indentation = 0;
        private static bool _newLine = true;

        private static void WriteLine(string s)
        {
            Write(s + Environment.NewLine);
            _newLine = true;
        }

        private static void Write(string s)
        {
            var indentation = _newLine ? MakeIndentation() : string.Empty;
            Console.Write(indentation + s);
        }

        private static string MakeIndentation()
        {
            return string.Concat(Enumerable.Repeat("\t", _indentation));
        }
    }
}
