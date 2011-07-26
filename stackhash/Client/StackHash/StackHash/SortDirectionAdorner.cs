using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows;

namespace StackHash
{
    /// <summary>
    /// Adorner used to indicate the sort direction of a ListView column
    /// </summary>
    public class SortDirectionAdorner : Adorner
    {
        private readonly static Geometry DescendingGeometry =
                Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

        private readonly static Geometry AscendingGeometry =
            Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

        private ListSortDirection _direction;
        private Brush _fill;

        /// <summary>
        /// Adorner used to indicate the sort direction of a ListView column
        /// </summary>
        /// <param name="element">Element to be adorned</param>
        /// <param name="direction">Current sort direction</param>
        public SortDirectionAdorner(UIElement element, ListSortDirection direction)
            : base(element)
        {
            _direction = direction;

            Color fillColor = new Color();
            fillColor.ScA = 0.5F;
            fillColor.ScR = 0.0F;
            fillColor.ScG = 0.0F;
            fillColor.ScB = 0.0F;

            _fill = new SolidColorBrush(fillColor);
        }

        /// <summary />
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (drawingContext == null) throw new ArgumentNullException("drawingContext");

            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            drawingContext.PushTransform(
                new TranslateTransform(
                  AdornedElement.RenderSize.Width - 15,
                  (AdornedElement.RenderSize.Height - 5) / 2));

            drawingContext.DrawGeometry(_fill, null,
                _direction == ListSortDirection.Ascending ?
                  AscendingGeometry : DescendingGeometry);

            drawingContext.Pop();
        }
    }
}
