namespace TfsEventWorkflowsConfiguration
{
  partial class Form1
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.lblHeader = new System.Windows.Forms.Label();
      this.txtWorkflowActivitiesGlobalCollection = new System.Windows.Forms.TextBox();
      this.grpWorkflowActivities = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.txtWorkflowActivitiesGlobalPath = new System.Windows.Forms.TextBox();
      this.grpWorkflows = new System.Windows.Forms.GroupBox();
      this.label6 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.txtWorkflowsProjectReleativePath = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.txtWorkflowsConfigFileNameJobAgent = new System.Windows.Forms.TextBox();
      this.txtWorkflowsConfigFileNameWebService = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.txtWorkflowsGlobalPath = new System.Windows.Forms.TextBox();
      this.txtWorkflowsGlobalCollection = new System.Windows.Forms.TextBox();
      this.grpCollectionSpecific = new System.Windows.Forms.GroupBox();
      this.cbCollection = new System.Windows.Forms.ComboBox();
      this.label9 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.txtWorkflowsCollectionPath = new System.Windows.Forms.TextBox();
      this.grpServer = new System.Windows.Forms.GroupBox();
      this.label12 = new System.Windows.Forms.Label();
      this.txtServerUrl = new System.Windows.Forms.TextBox();
      this.btnConnect = new System.Windows.Forms.Button();
      this.btnSave = new System.Windows.Forms.Button();
      this.grpWorkflowActivities.SuspendLayout();
      this.grpWorkflows.SuspendLayout();
      this.grpCollectionSpecific.SuspendLayout();
      this.grpServer.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblHeader
      // 
      this.lblHeader.AutoSize = true;
      this.lblHeader.Location = new System.Drawing.Point(29, 25);
      this.lblHeader.Name = "lblHeader";
      this.lblHeader.Size = new System.Drawing.Size(339, 13);
      this.lblHeader.TabIndex = 0;
      this.lblHeader.Text = "Use this tool to configure the TfsEventWorkflows version control paths";
      // 
      // txtWorkflowActivitiesGlobalCollection
      // 
      this.txtWorkflowActivitiesGlobalCollection.Location = new System.Drawing.Point(139, 23);
      this.txtWorkflowActivitiesGlobalCollection.Name = "txtWorkflowActivitiesGlobalCollection";
      this.txtWorkflowActivitiesGlobalCollection.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowActivitiesGlobalCollection.TabIndex = 1;
      // 
      // grpWorkflowActivities
      // 
      this.grpWorkflowActivities.Controls.Add(this.label3);
      this.grpWorkflowActivities.Controls.Add(this.label2);
      this.grpWorkflowActivities.Controls.Add(this.txtWorkflowActivitiesGlobalPath);
      this.grpWorkflowActivities.Controls.Add(this.txtWorkflowActivitiesGlobalCollection);
      this.grpWorkflowActivities.Enabled = false;
      this.grpWorkflowActivities.Location = new System.Drawing.Point(32, 138);
      this.grpWorkflowActivities.Name = "grpWorkflowActivities";
      this.grpWorkflowActivities.Size = new System.Drawing.Size(718, 83);
      this.grpWorkflowActivities.TabIndex = 2;
      this.grpWorkflowActivities.TabStop = false;
      this.grpWorkflowActivities.Text = "version control path to custom workflow activities - these settings apply to all " +
    "collections and projects";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 52);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(29, 13);
      this.label3.TabIndex = 2;
      this.label3.Text = "Path";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 26);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(53, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Collection";
      // 
      // txtWorkflowActivitiesGlobalPath
      // 
      this.txtWorkflowActivitiesGlobalPath.Location = new System.Drawing.Point(139, 49);
      this.txtWorkflowActivitiesGlobalPath.Name = "txtWorkflowActivitiesGlobalPath";
      this.txtWorkflowActivitiesGlobalPath.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowActivitiesGlobalPath.TabIndex = 1;
      // 
      // grpWorkflows
      // 
      this.grpWorkflows.Controls.Add(this.label6);
      this.grpWorkflows.Controls.Add(this.label8);
      this.grpWorkflows.Controls.Add(this.label7);
      this.grpWorkflows.Controls.Add(this.txtWorkflowsProjectReleativePath);
      this.grpWorkflows.Controls.Add(this.label4);
      this.grpWorkflows.Controls.Add(this.txtWorkflowsConfigFileNameJobAgent);
      this.grpWorkflows.Controls.Add(this.txtWorkflowsConfigFileNameWebService);
      this.grpWorkflows.Controls.Add(this.label5);
      this.grpWorkflows.Controls.Add(this.txtWorkflowsGlobalPath);
      this.grpWorkflows.Controls.Add(this.txtWorkflowsGlobalCollection);
      this.grpWorkflows.Enabled = false;
      this.grpWorkflows.Location = new System.Drawing.Point(32, 233);
      this.grpWorkflows.Name = "grpWorkflows";
      this.grpWorkflows.Size = new System.Drawing.Size(718, 157);
      this.grpWorkflows.TabIndex = 3;
      this.grpWorkflows.TabStop = false;
      this.grpWorkflows.Text = "version control path to global workflows - these settings apply to all collection" +
    "s and projects";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(6, 78);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(107, 13);
      this.label6.TabIndex = 5;
      this.label6.Text = "Project Relative Path";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(6, 130);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(107, 13);
      this.label8.TabIndex = 5;
      this.label8.Text = "Job Agent Config File";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(6, 104);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(121, 13);
      this.label7.TabIndex = 5;
      this.label7.Text = "Web Service Config File";
      // 
      // txtWorkflowsProjectReleativePath
      // 
      this.txtWorkflowsProjectReleativePath.Location = new System.Drawing.Point(139, 75);
      this.txtWorkflowsProjectReleativePath.Name = "txtWorkflowsProjectReleativePath";
      this.txtWorkflowsProjectReleativePath.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowsProjectReleativePath.TabIndex = 4;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(6, 52);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(62, 13);
      this.label4.TabIndex = 2;
      this.label4.Text = "Global Path";
      // 
      // txtWorkflowsConfigFileNameJobAgent
      // 
      this.txtWorkflowsConfigFileNameJobAgent.Location = new System.Drawing.Point(139, 127);
      this.txtWorkflowsConfigFileNameJobAgent.Name = "txtWorkflowsConfigFileNameJobAgent";
      this.txtWorkflowsConfigFileNameJobAgent.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowsConfigFileNameJobAgent.TabIndex = 4;
      // 
      // txtWorkflowsConfigFileNameWebService
      // 
      this.txtWorkflowsConfigFileNameWebService.Location = new System.Drawing.Point(139, 101);
      this.txtWorkflowsConfigFileNameWebService.Name = "txtWorkflowsConfigFileNameWebService";
      this.txtWorkflowsConfigFileNameWebService.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowsConfigFileNameWebService.TabIndex = 4;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(6, 26);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(53, 13);
      this.label5.TabIndex = 2;
      this.label5.Text = "Collection";
      // 
      // txtWorkflowsGlobalPath
      // 
      this.txtWorkflowsGlobalPath.Location = new System.Drawing.Point(139, 49);
      this.txtWorkflowsGlobalPath.Name = "txtWorkflowsGlobalPath";
      this.txtWorkflowsGlobalPath.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowsGlobalPath.TabIndex = 1;
      // 
      // txtWorkflowsGlobalCollection
      // 
      this.txtWorkflowsGlobalCollection.Location = new System.Drawing.Point(139, 23);
      this.txtWorkflowsGlobalCollection.Name = "txtWorkflowsGlobalCollection";
      this.txtWorkflowsGlobalCollection.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowsGlobalCollection.TabIndex = 1;
      // 
      // grpCollectionSpecific
      // 
      this.grpCollectionSpecific.Controls.Add(this.cbCollection);
      this.grpCollectionSpecific.Controls.Add(this.label9);
      this.grpCollectionSpecific.Controls.Add(this.label10);
      this.grpCollectionSpecific.Controls.Add(this.txtWorkflowsCollectionPath);
      this.grpCollectionSpecific.Enabled = false;
      this.grpCollectionSpecific.Location = new System.Drawing.Point(32, 398);
      this.grpCollectionSpecific.Name = "grpCollectionSpecific";
      this.grpCollectionSpecific.Size = new System.Drawing.Size(718, 83);
      this.grpCollectionSpecific.TabIndex = 2;
      this.grpCollectionSpecific.TabStop = false;
      this.grpCollectionSpecific.Text = "version control path to collection specific workflows";
      // 
      // cbCollection
      // 
      this.cbCollection.FormattingEnabled = true;
      this.cbCollection.Location = new System.Drawing.Point(139, 22);
      this.cbCollection.Name = "cbCollection";
      this.cbCollection.Size = new System.Drawing.Size(561, 21);
      this.cbCollection.TabIndex = 3;
      this.cbCollection.SelectedIndexChanged += new System.EventHandler(this.cbCollection_SelectedIndexChanged);
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(6, 52);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(120, 13);
      this.label9.TabIndex = 2;
      this.label9.Text = "Collection Relative Path";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(6, 26);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(53, 13);
      this.label10.TabIndex = 2;
      this.label10.Text = "Collection";
      // 
      // txtWorkflowsCollectionPath
      // 
      this.txtWorkflowsCollectionPath.Location = new System.Drawing.Point(139, 49);
      this.txtWorkflowsCollectionPath.Name = "txtWorkflowsCollectionPath";
      this.txtWorkflowsCollectionPath.Size = new System.Drawing.Size(561, 20);
      this.txtWorkflowsCollectionPath.TabIndex = 1;
      this.txtWorkflowsCollectionPath.TextChanged += new System.EventHandler(this.txtWorkflowsCollectionPath_TextChanged);
      // 
      // grpServer
      // 
      this.grpServer.Controls.Add(this.label12);
      this.grpServer.Controls.Add(this.txtServerUrl);
      this.grpServer.Location = new System.Drawing.Point(32, 52);
      this.grpServer.Name = "grpServer";
      this.grpServer.Size = new System.Drawing.Size(718, 54);
      this.grpServer.TabIndex = 2;
      this.grpServer.TabStop = false;
      this.grpServer.Text = "Team Foundation Server";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(6, 26);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(20, 13);
      this.label12.TabIndex = 2;
      this.label12.Text = "Url";
      // 
      // txtServerUrl
      // 
      this.txtServerUrl.Location = new System.Drawing.Point(139, 23);
      this.txtServerUrl.Name = "txtServerUrl";
      this.txtServerUrl.Size = new System.Drawing.Size(561, 20);
      this.txtServerUrl.TabIndex = 1;
      this.txtServerUrl.Text = "  http://localhost:8080/tfs";
      // 
      // btnConnect
      // 
      this.btnConnect.Location = new System.Drawing.Point(767, 72);
      this.btnConnect.Name = "btnConnect";
      this.btnConnect.Size = new System.Drawing.Size(124, 23);
      this.btnConnect.TabIndex = 4;
      this.btnConnect.Text = "Connect";
      this.btnConnect.UseVisualStyleBackColor = true;
      this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
      // 
      // btnSave
      // 
      this.btnSave.Enabled = false;
      this.btnSave.Location = new System.Drawing.Point(767, 444);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(124, 23);
      this.btnSave.TabIndex = 4;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(924, 504);
      this.Controls.Add(this.btnSave);
      this.Controls.Add(this.btnConnect);
      this.Controls.Add(this.grpWorkflows);
      this.Controls.Add(this.grpCollectionSpecific);
      this.Controls.Add(this.grpServer);
      this.Controls.Add(this.grpWorkflowActivities);
      this.Controls.Add(this.lblHeader);
      this.Name = "Form1";
      this.Text = "TfsEventWorkflows Configuration Tool";
      this.grpWorkflowActivities.ResumeLayout(false);
      this.grpWorkflowActivities.PerformLayout();
      this.grpWorkflows.ResumeLayout(false);
      this.grpWorkflows.PerformLayout();
      this.grpCollectionSpecific.ResumeLayout(false);
      this.grpCollectionSpecific.PerformLayout();
      this.grpServer.ResumeLayout(false);
      this.grpServer.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.TextBox txtWorkflowActivitiesGlobalCollection;
    private System.Windows.Forms.GroupBox grpWorkflowActivities;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox txtWorkflowActivitiesGlobalPath;
    private System.Windows.Forms.GroupBox grpWorkflows;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox txtWorkflowsGlobalPath;
    private System.Windows.Forms.TextBox txtWorkflowsGlobalCollection;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox txtWorkflowsProjectReleativePath;
    private System.Windows.Forms.TextBox txtWorkflowsConfigFileNameJobAgent;
    private System.Windows.Forms.TextBox txtWorkflowsConfigFileNameWebService;
    private System.Windows.Forms.GroupBox grpCollectionSpecific;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox txtWorkflowsCollectionPath;
    private System.Windows.Forms.ComboBox cbCollection;
    private System.Windows.Forms.GroupBox grpServer;
    private System.Windows.Forms.TextBox txtServerUrl;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Button btnConnect;
    private System.Windows.Forms.Button btnSave;
  }
}

