using System;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using DatabaseDesigner.Core.Models;
using DatabaseDesigner.Wasm.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace DatabaseDesigner.Wasm.Pages
{
    public partial class Index : IDisposable
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public BlazorDiagram Diagram { get; } = new BlazorDiagram();

        public void Dispose()
        {
            // Unsubscribe from the events
            Diagram.Links.Added -= OnLinkAdded;
            Diagram.Links.Removed -= OnLinkRemoved;
        }

        protected override void OnInitialized()
        {
            // Make sure there are no parameters in the method signature
            base.OnInitialized();

            // Configure options directly on the Diagram object
            Diagram.Options.GridSize = 40;
            Diagram.Options.AllowMultiSelection = false;

            // FIX: Create new instances of the router and path generator
            Diagram.Options.Links.DefaultRouter = new OrthogonalRouter();
            Diagram.Options.Links.DefaultPathGenerator = new StraightPathGenerator();

            Diagram.RegisterComponent<Table, TableNode>();
            Diagram.Nodes.Add(new Table());

            // Subscribe to the global link events
            Diagram.Links.Added += OnLinkAdded;
            Diagram.Links.Removed += OnLinkRemoved;
        }

        // **FIX 2:** The event handler now receives a BaseLinkModel.
        // The logic for when a link is attached has been moved here.
        private void OnLinkAdded(BaseLinkModel link)
        {
            // This event fires when the link is fully connected and added to the diagram.
            // We can now add the label here.
            link.Labels.Add(new LinkLabelModel(link, "1..*", -40, new Point(0, -30)));
            link.Refresh();

            var targetPort = (link.Target.Model) as ColumnPort;
            targetPort?.Column.Refresh();
        }

        // **FIX 3:** The event handler now receives a BaseLinkModel.
        private void OnLinkRemoved(BaseLinkModel link)
        {
            link.TargetPortChanged -= OnLinkTargetPortChanged;

            if (!link.IsAttached)
                return;

            // Access ports through the Source and Target anchors
            var sourceCol = (link.Source.Model as ColumnPort)?.Column;
            var targetCol = (link.Target.Model as ColumnPort)?.Column;

            if (sourceCol != null && targetCol != null)
            {
            (sourceCol.Primary ? targetCol : sourceCol).Refresh();
        }
        }

        private void NewTable()
        {
            Diagram.Nodes.Add(new Table());
        }

        private async Task ShowJson()
        {
            var json = JsonConvert.SerializeObject(new
            {
                Nodes = Diagram.Nodes.Cast<object>(),
                Links = Diagram.Links.Cast<object>()
            }, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await JSRuntime.InvokeVoidAsync("console.log", json);
        }

        private void Debug()
        {
            Console.WriteLine("Debug Port Positions:");
            foreach (var port in Diagram.Nodes.ToList()[0].Ports)
            {
                Console.WriteLine(port.Position);
        }
    }
}
}