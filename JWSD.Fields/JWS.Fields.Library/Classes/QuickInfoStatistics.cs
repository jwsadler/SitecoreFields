using JWS.Fields.Library.Fields;
using Sitecore.Configuration;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;

namespace JWS.Fields.Library.Classes
{
    public class QuickInfoStatistics
    {
        private const string ALWAYSOPEN = "StatsInfo.AlwaysOpen";
        private const string SECTION_NAME = "StatsInfo";
        private const string DISPLAY_NAME = "Stats Info";
        private const string ICON = "office/16x16/test_card.png";

        public void Process(RenderContentEditorArgs args)
        {
            args.EditorFormatter.RenderSectionBegin(args.Parent, SECTION_NAME, SECTION_NAME, DISPLAY_NAME,
                ICON, !Settings.GetBoolSetting(ALWAYSOPEN, false), true);

            if (args.Item == null) return;
            var stats = new Statistics {ItemId = args.Item.ID.ToString()};
            args.EditorFormatter.AddLiteralControl(args.Parent, stats.Render());
            args.EditorFormatter.RenderSectionEnd(args.Parent, true, true);
        }
    }
}