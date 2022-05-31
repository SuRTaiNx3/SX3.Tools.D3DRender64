namespace SX3.Tools.D3DRender64.Controls.Layouts
{
    /// <summary>
    /// An abstract class which offers methods to apply certain layouts to contents of a container-control
    /// </summary>
    public abstract class Layout
    {
        /// <summary>
        /// Applies a layout to the content of the given container-control
        /// </summary>
        /// <param name="parent"></param>
        public abstract void ApplyLayout(ContentControl parent);
    }
}
