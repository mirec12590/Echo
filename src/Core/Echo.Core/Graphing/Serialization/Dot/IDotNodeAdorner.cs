using System.Collections.Generic;

namespace Echo.Core.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides members for adorning a node in a graph.
    /// </summary>
    public interface IDotNodeAdorner
    {
        /// <summary>
        /// Obtains the adornments that should be added to the node. 
        /// </summary>
        /// <param name="node">The node to adorn.</param>
        /// <returns>The adornments.</returns>
        IDictionary<string, string> GetNodeAttributes(INode node);
    }
}