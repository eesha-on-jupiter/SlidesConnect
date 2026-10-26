using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Gathers many Request JSON items into one Requests JSON list for a single BatchUpdate call.
    // Tree access on purpose: a tree of requests (one branch per floor, per slide, per whatever
    // drove the graph) is flattened branch by branch into ONE ordered list, so the whole deck goes
    // out in one batchUpdate instead of one call per branch.
    public class RequestsAggregatorComponent : GH_Component
    {
        public RequestsAggregatorComponent()
          : base("Requests Aggregator", "Requests",
              "Collect many Request JSON items into one Requests JSON list for BatchUpdate.",
              PluginUtilities.TabName, PluginUtilities.CategoryAJAggregate)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "One or more batchUpdate request JSON items — a list or a tree; a tree is flattened branch by branch into one ordered list.", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Requests JSON", "RQS", "Ordered list of request JSON for BatchUpdate.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<GH_String> tree)) return;

            var cleaned = new List<string>();
            foreach (var branch in tree.Branches)
                foreach (var item in branch)
                    if (item != null && !string.IsNullOrWhiteSpace(item.Value)) cleaned.Add(item.Value);

            if (cleaned.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No request JSON supplied.");
                return;
            }

            DA.SetDataList(0, cleaned);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_RequestsAggregator;
        public override Guid ComponentGuid => new Guid("5d8313d0-40dc-4442-ae8a-3406c49a4b2b");
    }
}
