using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds replaceAllText requests: find-and-replace across the whole presentation, one request per
    // record — a list of {{tokens}} and a list of values from a CSV column fills a template deck.
    public class ReplaceAllTextComponent : GH_Component
    {
        public ReplaceAllTextComponent()
          : base("Replace All Text", "Replace",
              "Build requests that replace all occurrences of each Find Text with its Replace Text across the presentation — one per record, index-matched.",
              PluginUtilities.TabName, PluginUtilities.CategoryAFText)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Find Text", "FT", "Text to find, one per record (e.g. a list of {{tokens}}).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Replace Text", "RT", "Replacement text per record.", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("Match Case", "MC", "Whether the search is case-sensitive.", GH_ParamAccess.list, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "replaceAllText batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var findTexts = new List<string>();
            var replaceTexts = new List<string>();
            var matchCases = new List<bool>();

            DA.GetDataList(0, findTexts);
            DA.GetDataList(1, replaceTexts);
            DA.GetDataList(2, matchCases);

            int n = PluginUtilities.RecordCount(findTexts, replaceTexts, matchCases);
            PluginUtilities.WarnIfRagged(this, n, ("Find Text", findTexts.Count), ("Replace Text", replaceTexts.Count), ("Match Case", matchCases.Count));
            findTexts = PluginUtilities.MatchLength(findTexts, n, "");
            replaceTexts = PluginUtilities.MatchLength(replaceTexts, n, "");
            matchCases = PluginUtilities.MatchLength(matchCases, n, false);

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrEmpty(findTexts[i]))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"[{i}] Find Text is empty — the API rejects an empty search string; skipped.");
                    continue;
                }
                requests.Add(RequestBuilders.BuildReplaceAllText(findTexts[i], replaceTexts[i], matchCases[i]));
            }
            if (requests.Count == 0) return;

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ReplaceAllText;
        public override Guid ComponentGuid => new Guid("b2b417d6-a837-4f7e-a6c2-f12104bea52f");
    }
}
