using System.Collections.Generic;

namespace SwaMe.Pipeline
{
    /// <summary>
    /// Denotes something that can be rendered to mzQC output.
    /// </summary>
    public interface IMzqcMetrics
    {
        IDictionary<string, dynamic> RenderableMetrics { get; }
    }
}
