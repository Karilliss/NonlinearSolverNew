namespace NonlinearSolver
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtEquations;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.TextBox txtRules;
        private System.Windows.Forms.TextBox txtIterations;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.ComboBox cmbEpsilon;
        private System.Windows.Forms.Label lblMethod;
        private System.Windows.Forms.Label lblEpsilon;
        private System.Windows.Forms.Label lblIterations;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnSolve;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnClearField;
        private System.Windows.Forms.Button btnComplexityCheck;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnVisualize;
        private System.Windows.Forms.Label lblLineCount;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtEquations = new System.Windows.Forms.TextBox();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.txtRules = new System.Windows.Forms.TextBox();
            this.txtIterations = new System.Windows.Forms.TextBox();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.cmbEpsilon = new System.Windows.Forms.ComboBox();
            this.lblMethod = new System.Windows.Forms.Label();
            this.lblEpsilon = new System.Windows.Forms.Label();
            this.lblIterations = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnSolve = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClearField = new System.Windows.Forms.Button();
            this.btnComplexityCheck = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnVisualize = new System.Windows.Forms.Button();
            this.lblLineCount = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.lblLineCount.Location = new System.Drawing.Point(20, 10);
            this.lblLineCount.Size = new System.Drawing.Size(100, 22);
            this.lblLineCount.Text = "Lines: 0/10";
            this.lblLineCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLineCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.txtEquations.Location = new System.Drawing.Point(20, 30);
            this.txtEquations.Multiline = true;
            this.txtEquations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtEquations.Size = new System.Drawing.Size(400, 120);
            this.txtEquations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEquations.TextChanged += new System.EventHandler(this.txtEquations_TextChanged);
            this.txtEquations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEquations_KeyDown);
            this.txtEquations.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtEquations_KeyUp);
            this.txtEquations.Enter += new System.EventHandler(this.txtEquations_Enter);
            this.txtEquations.Leave += new System.EventHandler(this.txtEquations_Leave);
            this.txtEquations.AcceptsReturn = true;
            this.txtEquations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.txtResult.Location = new System.Drawing.Point(20, 240);
            this.txtResult.Multiline = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResult.Size = new System.Drawing.Size(400, 100);
            this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResult.ReadOnly = true;
            this.txtResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.txtRules.Location = new System.Drawing.Point(440, 30);
            this.txtRules.Multiline = true;
            this.txtRules.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRules.Size = new System.Drawing.Size(300, 280);
            this.txtRules.ReadOnly = true;
            this.txtRules.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRules.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMethod.Items.AddRange(new object[] { "Newton Method", "Secant Method" });
            this.cmbMethod.Location = new System.Drawing.Point(100, 160);
            this.cmbMethod.Size = new System.Drawing.Size(150, 24);
            this.cmbMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbMethod.SelectedIndex = 0;
            this.cmbMethod.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEpsilon.Location = new System.Drawing.Point(100, 185);
            this.cmbEpsilon.Size = new System.Drawing.Size(120, 24);
            this.cmbEpsilon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbEpsilon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.lblMethod.Location = new System.Drawing.Point(20, 160);
            this.lblMethod.Size = new System.Drawing.Size(80, 22);
            this.lblMethod.Text = "Method:";
            this.lblMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMethod.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.lblEpsilon.Location = new System.Drawing.Point(20, 185);
            this.lblEpsilon.Size = new System.Drawing.Size(40, 22);
            this.lblEpsilon.Text = "ε:";
            this.lblEpsilon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEpsilon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.lblIterations.Location = new System.Drawing.Point(20, 210);
            this.lblIterations.Size = new System.Drawing.Size(80, 22);
            this.lblIterations.Text = "Max Iter:";
            this.lblIterations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblIterations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.txtIterations.Location = new System.Drawing.Point(100, 210);
            this.txtIterations.Size = new System.Drawing.Size(80, 22);
            this.txtIterations.Text = "1000";
            this.txtIterations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.txtIterations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnGenerate.Location = new System.Drawing.Point(20, 360);
            this.btnGenerate.Size = new System.Drawing.Size(90, 30);
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            this.btnGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnSolve.Location = new System.Drawing.Point(110, 360);
            this.btnSolve.Size = new System.Drawing.Size(80, 30);
            this.btnSolve.Text = "Solve";
            this.btnSolve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSolve.Click += new System.EventHandler(this.btnSolve_Click);
            this.btnSolve.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnClear.Location = new System.Drawing.Point(200, 360);
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.Text = "Clear All";
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            this.btnClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnClearField.Location = new System.Drawing.Point(290, 360);
            this.btnClearField.Size = new System.Drawing.Size(110, 30);
            this.btnClearField.Text = "Clear Field";
            this.btnClearField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClearField.Click += new System.EventHandler(this.btnClearField_Click);
            this.btnClearField.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnComplexityCheck.Location = new System.Drawing.Point(400, 360);
            this.btnComplexityCheck.Size = new System.Drawing.Size(170, 30);
            this.btnComplexityCheck.Text = "Check Complexity";
            this.btnComplexityCheck.Visible = false;
            this.btnComplexityCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnComplexityCheck.Click += new System.EventHandler(this.btnComplexityCheck_Click);
            this.btnComplexityCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnVisualize.Location = new System.Drawing.Point(580, 360);
            this.btnVisualize.Size = new System.Drawing.Size(100, 30);
            this.btnVisualize.Text = "Visualize";
            this.btnVisualize.Visible = false;
            this.btnVisualize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnVisualize.Click += new System.EventHandler(this.btnVisualize_Click);
            this.btnVisualize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnSave.Location = new System.Drawing.Point(690, 360);
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.Text = "Save";
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.ClientSize = new System.Drawing.Size(780, 410);
            this.MinimumSize = new System.Drawing.Size(600, 430);
            this.Controls.Add(this.txtEquations);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.txtRules);
            this.Controls.Add(this.txtIterations);
            this.Controls.Add(this.cmbMethod);
            this.Controls.Add(this.cmbEpsilon);
            this.Controls.Add(this.lblMethod);
            this.Controls.Add(this.lblEpsilon);
            this.Controls.Add(this.lblIterations);
            this.Controls.Add(this.lblLineCount);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnSolve);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnClearField);
            this.Controls.Add(this.btnComplexityCheck);
            this.Controls.Add(this.btnVisualize);
            this.Controls.Add(this.btnSave);
            this.Text = "Nonlinear Solver";
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}