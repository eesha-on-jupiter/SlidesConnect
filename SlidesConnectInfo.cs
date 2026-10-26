using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SlidesConnect
{
    public class SlidesConnectInfo : GH_AssemblyInfo
    {
        public override string Name => "SlidesConnect";
        // Same 16×16 logo as the category icon — shown in Grasshopper's loaded-libraries list
        public override Bitmap Icon => Properties.Resources.GS_SlidesConnectLogo;
        public override string Description => "Grasshopper components for the Google Slides API — build slides, shapes, text, tables and images, and push styled decks from Rhino model data. Made with the PleaseREST Claude Code plugin.";
        public override Guid Id => new Guid("81c11d8f-2f54-42c8-81f5-f93bdc895600");
        public override string AuthorName => "Eesha Jain";
        public override string AuthorContact => "toeesha@gmail.com";
        // Single source of truth is the .csproj <Version> — never a second hardcoded string here
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString(3);
    }

    public class SlidesConnectCategoryIcon : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("SlidesConnect", Properties.Resources.GS_SlidesConnectLogo);
            Instances.ComponentServer.AddCategorySymbolName("SlidesConnect", 'S');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
