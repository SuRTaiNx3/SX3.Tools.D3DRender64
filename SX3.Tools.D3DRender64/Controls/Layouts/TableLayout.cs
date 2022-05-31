namespace SX3.Tools.D3DRender64.Controls.Layouts
{
    public class TableLayout : Layout
    {
        #region Globals

        private static TableLayout _instanceTwoColumns = new TableLayout(2);
        private static TableLayout _instanceThreeColumns = new TableLayout(3);

        #endregion

        #region Properties

        public static Layout TwoColumns { get { return _instanceTwoColumns; } }

        public static Layout ThreeColumns { get { return _instanceThreeColumns; } }

        public int Columns { get; set; }

        public float[] ColumnWidth { get; private set; }

        #endregion

        #region Constructor

        public TableLayout() : this(2) { }

        public TableLayout(int columns)
            : base()
        {
            this.Columns = columns;
            this.ColumnWidth = new float[columns];
            for (int i = 0; i < columns; i++)
                this.ColumnWidth[i] = 1f / this.Columns;
        }

        #endregion

        #region Methods

        public override void ApplyLayout(ContentControl parent)
        {
            float height = 0;

            for (int i = 0; i < parent.ChildControls.Count; i++)
            {
                var control = parent.ChildControls[i];
                if (!control.Visible)
                    continue;

                float width = parent.Width * this.ColumnWidth[i % this.Columns];
                float xSum = 0f;
                for (int x = 0; x < i % Columns; x++)
                    xSum += this.ColumnWidth[x];

                if (control.FillParent)
                    control.Width = width - parent.MarginLeft - parent.MarginRight - control.MarginLeft - control.MarginRight;

                //if (i % 2 == 0)
                //    control.X = control.MarginLeft + parent.MarginLeft;
                //else
                //    control.X = control.MarginLeft + parent.MarginLeft + parent.Width / 2f;

                control.X = control.MarginLeft + parent.MarginLeft + parent.Width * xSum;


                if (i == 0)
                {
                    control.Y = control.MarginTop;
                }
                else
                {
                    var lastControl = parent.ChildControls[i - 1];
                    if (i % Columns == 0)
                    {
                        control.Y = height; //lastControl.Y + lastControl.Height + lastControl.MarginBottom + control.MarginTop;
                    }
                    else
                    {
                        control.Y = lastControl.Y;
                    }
                }
                if (control.Y + control.Height + control.MarginBottom + control.MarginTop > height)
                    height = control.Y + control.Height + control.MarginBottom + control.MarginTop;
            }
        }

        #endregion
    }
}
