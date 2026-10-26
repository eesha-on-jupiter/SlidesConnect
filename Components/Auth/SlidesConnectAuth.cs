using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using System;

namespace SlidesConnect
{
    // Validates a Google OAuth access token against the tokeninfo endpoint and reports its scope.
    public class SlidesConnectAuthComponent : GH_Component
    {
        public SlidesConnectAuthComponent()
          : base("Google Slides Auth", "Auth",
              "Validate a Google OAuth access token and confirm it can reach the Slides API.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAAuth)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token (scope https://www.googleapis.com/auth/presentations).", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "S", "Connection status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw tokeninfo response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string token = "";

            // 2. DA.GetData calls
            if (!DA.GetData(0, ref token)) return;

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(token))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Token is empty.");
                DA.SetData(0, "No token");
                return;
            }

            // 4. Logic
            var client = new SlidesConnectClient(token);
            var result = client.CheckTokenAsync().GetAwaiter().GetResult();

            string status;
            if (result.Item1)
            {
                string scope = "";
                try { scope = (string)JObject.Parse(result.Item2)["scope"] ?? ""; } catch { }
                status = "Connected" + (string.IsNullOrEmpty(scope) ? "" : " — scope: " + scope);
            }
            else
            {
                status = "Failed: " + result.Item3;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, status);
            }

            // 5. DA.SetData calls
            DA.SetData(0, status);
            DA.SetData(1, result.Item1 ? result.Item2 : result.Item3);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_SlidesConnectAuth;
        public override Guid ComponentGuid => new Guid("ef2acbe5-cc5a-4506-a2a5-93531e9070b1");
    }
}
