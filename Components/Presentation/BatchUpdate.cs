using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sends a batch of requests to a presentation in one call. Press the button to run.
    public class BatchUpdateComponent : ButtonComponent
    {
        public BatchUpdateComponent()
          : base("Batch Update", "Batch",
              "Apply a list of request JSON to a presentation in a single batchUpdate call.",
              PluginUtilities.TabName, PluginUtilities.CategoryABPresentation)
        { }

        public override string ButtonLabel => "Send";

        // object IDs this component has already created, per presentation. Pressing Send again
        // without the upstream builders recomputing would re-send the same auto-generated IDs and
        // fail the whole batch with "object ID already exists" — see RequestBuilders.RemapCreatedIds.
        // Not persisted: reopening the .gh recomputes upstream and generates fresh IDs anyway.
        private readonly Dictionary<string, HashSet<string>> _sentIds = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Presentation ID", "PID", "ID of the presentation to update.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Requests JSON", "RQS", "Request JSON from the Requests Aggregator (or straight from a builder) — a list or a tree; a tree is flattened branch by branch and sent as ONE batchUpdate.", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Replies", "RP", "Per-request replies (JSON).", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Operation status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw API response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered) return;

            string token = "";
            string presentationId = "";
            var requests = new List<string>();

            DA.GetData(0, ref token);
            DA.GetData(1, ref presentationId);
            // tree in, one flat ordered list out — one API call per press, never one per branch
            if (DA.GetDataTree(2, out GH_Structure<GH_String> tree))
                foreach (var branch in tree.Branches)
                    foreach (var item in branch)
                        if (item != null && !string.IsNullOrWhiteSpace(item.Value)) requests.Add(item.Value);

            if (!_sentIds.TryGetValue(presentationId ?? "", out var sentForDeck))
                _sentIds[presentationId ?? ""] = sentForDeck = new HashSet<string>(StringComparer.Ordinal);

            var toSend = RequestBuilders.RemapCreatedIds(requests, sentForDeck, out var remapped);
            if (remapped.Count > 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    remapped.Count + " object ID(s) in this batch were already created by an earlier Send from this component — regenerated so this push adds a second copy instead of failing with \"object ID already exists\". Upstream Slide ID / Object ID outputs still refer to the first copy; the new IDs are in Replies.");

            var client = new SlidesConnectClient(token);
            var result = client.BatchUpdateAsync(presentationId, toSend).GetAwaiter().GetResult();

            if (!result.Item1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Item3);
                DA.SetData(1, "Failed");
                DA.SetData(2, result.Item3);
                return;
            }

            foreach (var id in RequestBuilders.CreatedIds(toSend)) sentForDeck.Add(id);

            var replies = new List<string>();
            try
            {
                var arr = JObject.Parse(result.Item2)["replies"] as JArray;
                if (arr != null)
                    foreach (var r in arr) replies.Add(r.ToString(Formatting.None));
            }
            catch { }

            DA.SetDataList(0, replies);
            DA.SetData(1, "Applied " + requests.Count + " request(s)");
            DA.SetData(2, result.Item2);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_BatchUpdate;
        public override Guid ComponentGuid => new Guid("d4e7ca10-2dbe-476b-af56-1e49a0b89a60");
    }
}
