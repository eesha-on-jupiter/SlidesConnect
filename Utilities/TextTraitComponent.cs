using Grasshopper.Kernel;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Base for the one-trait text/paragraph styling components in the Text Style tab (Bold Text,
    // Font Size Text, Aligned Text, ...) — the Slides counterparts of DocsConnect's atomic Text and
    // Paragraph components. Docs inserts text at an index; Slides styles text that already lives in
    // an object, so every one of these takes Object ID(s) plus its single trait, index-matched, and
    // styles the whole object's text. Subclasses register the trait input and build one request per
    // record. (Per-letter / per-word styling lives in Styled Text Box's Same Box mode, where the
    // component computes the ranges itself.)
    public abstract class TextTraitComponent<T> : GH_Component
    {
        protected TextTraitComponent(string name, string nickname, string description)
            : base(name, nickname, description, PluginUtilities.TabName, PluginUtilities.CategoryAGTextStyle)
        { }

        // display name of the trait input, for the ragged-list warning
        protected abstract string TraitName { get; }

        // registers the trait input — always index 1, always list access
        protected abstract void RegisterTraitParam(GH_InputParamManager pManager);

        // value used for records beyond the trait list's length
        protected abstract T Fallback { get; }

        // one request for one record; null = nothing to send for this record
        protected abstract string BuildOne(string objectId, T value);

        // reads the trait list (index 1). Colour traits override this to read a Colour list and hand
        // back its Color JSON form, so the rest of the pipeline only ever sees strings.
        protected virtual List<T> ReadTrait(IGH_DataAccess DA)
        {
            var traits = new List<T>();
            DA.GetDataList(1, traits);
            return traits;
        }

        protected virtual string RequestTypeName => "updateTextStyle";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape whose text to style — from Create Shape / Styled Text Box's Object ID output, or Create Slide's Title/Body Object ID.", GH_ParamAccess.list, "");
            RegisterTraitParam(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", RequestTypeName + " batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            DA.GetDataList(0, objectIds);
            var traits = ReadTrait(DA);

            int n = PluginUtilities.RecordCount(objectIds, traits);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), (TraitName, traits.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            traits = PluginUtilities.MatchLength(traits, n, Fallback);

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(objectIds[i])) continue;
                string request = BuildOne(objectIds[i], traits[i]);
                if (request != null) requests.Add(request);
            }

            if (requests.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Object ID supplied.");
                return;
            }
            DA.SetDataList(0, requests);
        }
    }
}
