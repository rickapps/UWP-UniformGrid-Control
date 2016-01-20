/// <summary>
/// Port of WPF UniformGrid control to UWP. In the Microsoft UWP examples on GitHub, see the XamlUIBasics project. They have a custom WrapPanel included in the project.
/// I took that code and combined it with a reverse compile of the the WPF UniformGrid control to get the code below.
/// January 20, 2016
/// RickApps.com
/// </summary>
namespace UniformGridForUWP
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Windows.Foundation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Provides a way to arrange content in a grid where all the cells in the grid have the same size.
    /// </summary>
    public class UniformGrid : Panel
    {
        /// <summary>
        /// A value indicating whether a dependency property change handler
        /// should ignore the next change notification.  This is used to reset
        /// the value of properties without performing any of the actions in
        /// their change handlers.
        /// </summary>
        private bool _ignorePropertyChange;

        private int _rows;

        private int _columns;

        #region public int Rows

        /// <summary>
        /// Gets or sets the number of rows that are in the grid.
        /// </summary>
        /// <returns>The number of rows that are in the grid. The default is 0.</returns>
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(
                "Rows",
                typeof(int),
                typeof(UniformGrid),
                new PropertyMetadata(0, OnRowsColumnsChanged));

        #endregion

        #region public int Columns

        /// <summary>
        /// Gets or sets the number of columns that are in the grid.
        /// </summary>
        /// <returns>The number of columns that are in the grid. The default is 0.</returns>
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
                "Columns",
                typeof(int),
                typeof(UniformGrid),
                new PropertyMetadata(0, OnRowsColumnsChanged));

        #endregion

        #region public int FirstColumn

        /// <summary>
        /// Gets or sets the number of leading blank cells in the first row of the grid.
        /// </summary>
        /// <returns>The number of empty cells that are in the first row of the grid. The default is 0.</returns>
        public int FirstColumn
        {
            get { return (int)GetValue(FirstColumnProperty); }
            set { SetValue(FirstColumnProperty, value); }
        }

        public static readonly DependencyProperty FirstColumnProperty =
            DependencyProperty.Register(
                "FirstColumn",
                typeof(int),
                typeof(UniformGrid),
                new PropertyMetadata(0, OnFirstColumnChanged));

        #endregion

        /// <summary>
        /// Validity check on row or column. For now, just check that it is positive. This code could be much simplified.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnRowsColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformGrid source = (UniformGrid)d;
            int value = (int)e.NewValue;

            // Ignore the change if requested
            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            if (value < 0)
            {
                // Reset the property to its original state before throwing
                source._ignorePropertyChange = true;
                source.SetValue(e.Property, (int)e.OldValue);

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Properties.Resources.UniformGrid_RowsColumnsChanged_InvalidValue",
                    value);
                throw new ArgumentException(message, "value");
            }

            // The length properties affect measuring.
            source.InvalidateMeasure();
        }

        /// <summary>
        /// Check that the offset is positive. This seems to be the only check in the WPF uniform grid.
        /// Might also make the offset be less than the number of items included in the grid.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnFirstColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformGrid source = (UniformGrid)d;
            int value = (int)e.NewValue;

            // Ignore the change if requested
            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            if (value < 0)
            {
                // Reset the property to its original state before throwing
                source._ignorePropertyChange = true;
                source.SetValue(e.Property, (int)e.OldValue);

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Properties.Resources.UniformGrid_OnFirstColumnChanged_InvalidValue",
                    value);
                throw new ArgumentException(message, "value");
            }

            // The length properties affect measuring.
            source.InvalidateMeasure();
        }

        /// <summary>
        /// Initializes a new instance of the UniformGrid.
        /// </summary>
        public UniformGrid()
        {
        }

        /// <summary>
        /// Computes the desired size of the System.Windows.Controls.Primitives.UniformGrid by measuring all of the child elements.
        /// </summary>
        /// <param name="constraint">The System.Windows.Size of the available area for the grid.</param>
        /// <returns>The desired System.Windows.Size based on the child content of the grid and the constraint parameter.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "Compat with WPF.")]
        protected override Size MeasureOverride(Size constraint)
        {
            this.UpdateComputedValues();
            Size availableSize = new Size(constraint.Width / (double)this._columns, constraint.Height / (double)this._rows);
            double num = 0.0;
            double num2 = 0.0;
            int i = 0;
            int count = base.Children.Count;
            while (i < count)
            {
                UIElement element = base.Children[i];
                element.Measure(availableSize);
                Size desiredSize = element.DesiredSize;
                if (num < desiredSize.Width)
                {
                    num = desiredSize.Width;
                }
                if (num2 < desiredSize.Height)
                {
                    num2 = desiredSize.Height;
                }
                i++;
            }
            return new Size(num * (double)this._columns, num2 * (double)this._rows);
        }

        /// <summary>
        /// Defines the layout of the System.Windows.Controls.Primitives.UniformGrid by distributing space evenly among all of the child elements.
        /// </summary>
        /// <param name="finalSize">The System.Windows.Size of the area for the grid to use.</param>
        /// <returns>The actual System.Windows.Size of the grid that is rendered to display the child elements that are visible.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect finalRect = new Rect(0.0, 0.0, finalSize.Width / (double)this._columns, finalSize.Height / (double)this._rows);
            double width = finalRect.Width;
            double num = finalSize.Width - 1.0;
            finalRect.X += finalRect.Width * (double)this.FirstColumn;
            foreach (UIElement element in base.Children)
            {
                element.Arrange(finalRect);
                if (element.Visibility != Visibility.Collapsed)
                {
                    finalRect.X += width;
                    if (finalRect.X >= num)
                    {
                        finalRect.Y += finalRect.Height;
                        finalRect.X = 0.0;
                    }
                }
            }
            return finalSize;
        }

        /// <summary>
        /// If no row count or column count is specified, use the smallest square number that will contain all the items
        /// as our dimensions. Otherwise, calculate the number of rows or columns needed to contain all items.
        /// </summary>
        private void UpdateComputedValues()
        {
            this._columns = this.Columns;
            this._rows = this.Rows;
            if (this.FirstColumn >= this._columns)
            {
                this.FirstColumn = 0;
            }
            if (this._rows == 0 || this._columns == 0)
            {
                int num = 0;
                int i = 0;
                int count = base.Children.Count;
                while (i < count)
                {
                    if (base.Children[i].Visibility != Visibility.Collapsed)
                    {
                        num++;
                    }
                    i++;
                }
                if (num == 0)
                {
                    num = 1;
                }
                if (this._rows == 0)
                {
                    if (this._columns > 0)
                    {
                        this._rows = (num + this.FirstColumn + (this._columns - 1)) / this._columns;
                        return;
                    }
                    this._rows = (int)Math.Sqrt((double)num);
                    if (this._rows * this._rows < num)
                    {
                        this._rows++;
                    }
                    this._columns = this._rows;
                    return;
                }
                else if (this._columns == 0)
                {
                    this._columns = (num + (this._rows - 1)) / this._rows;
                }
            }
        }
    }
}

