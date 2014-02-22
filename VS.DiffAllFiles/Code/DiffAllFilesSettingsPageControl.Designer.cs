namespace DansKingdom.VS_DiffAllFiles.Code
{
	partial class DiffAllFilesSettingsPageControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.chkCompareFilesNotChanged = new System.Windows.Forms.CheckBox();
			this.chkCompareDeletedFiles = new System.Windows.Forms.CheckBox();
			this.chkCompareNewFiles = new System.Windows.Forms.CheckBox();
			this.chkCompareFilesOneAtATime = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtFileExtensionsToIgnore = new System.Windows.Forms.TextBox();
			this.btnRestoreDefaultSettings = new System.Windows.Forms.Button();
			this.btnConfigureDiffTool = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.grpSettings = new System.Windows.Forms.GroupBox();
			this.grpSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// chkCompareFilesNotChanged
			// 
			this.chkCompareFilesNotChanged.AutoSize = true;
			this.chkCompareFilesNotChanged.Location = new System.Drawing.Point(6, 65);
			this.chkCompareFilesNotChanged.Name = "chkCompareFilesNotChanged";
			this.chkCompareFilesNotChanged.Size = new System.Drawing.Size(257, 17);
			this.chkCompareFilesNotChanged.TabIndex = 0;
			this.chkCompareFilesNotChanged.Text = "Compare files whose contents have not changed";
			this.toolTip1.SetToolTip(this.chkCompareFilesNotChanged, "If files that are checked out, but not actually changed should be compared.");
			this.chkCompareFilesNotChanged.UseVisualStyleBackColor = true;
			this.chkCompareFilesNotChanged.CheckedChanged += new System.EventHandler(this.chkCompareFilesNotChanged_CheckedChanged);
			// 
			// chkCompareDeletedFiles
			// 
			this.chkCompareDeletedFiles.AutoSize = true;
			this.chkCompareDeletedFiles.Location = new System.Drawing.Point(6, 42);
			this.chkCompareDeletedFiles.Name = "chkCompareDeletedFiles";
			this.chkCompareDeletedFiles.Size = new System.Drawing.Size(127, 17);
			this.chkCompareDeletedFiles.TabIndex = 1;
			this.chkCompareDeletedFiles.Text = "Compare deleted files";
			this.toolTip1.SetToolTip(this.chkCompareDeletedFiles, "If files being deleted from source control should be compared.");
			this.chkCompareDeletedFiles.UseVisualStyleBackColor = true;
			this.chkCompareDeletedFiles.CheckedChanged += new System.EventHandler(this.chkCompareDeletedFiles_CheckedChanged);
			// 
			// chkCompareNewFiles
			// 
			this.chkCompareNewFiles.AutoSize = true;
			this.chkCompareNewFiles.Location = new System.Drawing.Point(6, 19);
			this.chkCompareNewFiles.Name = "chkCompareNewFiles";
			this.chkCompareNewFiles.Size = new System.Drawing.Size(112, 17);
			this.chkCompareNewFiles.TabIndex = 2;
			this.chkCompareNewFiles.Text = "Compare new files";
			this.toolTip1.SetToolTip(this.chkCompareNewFiles, "If files being added to source control should be compared.");
			this.chkCompareNewFiles.UseVisualStyleBackColor = true;
			this.chkCompareNewFiles.CheckedChanged += new System.EventHandler(this.chkCompareNewFiles_CheckedChanged);
			// 
			// chkCompareFilesOneAtATime
			// 
			this.chkCompareFilesOneAtATime.AutoSize = true;
			this.chkCompareFilesOneAtATime.Location = new System.Drawing.Point(6, 88);
			this.chkCompareFilesOneAtATime.Name = "chkCompareFilesOneAtATime";
			this.chkCompareFilesOneAtATime.Size = new System.Drawing.Size(182, 17);
			this.chkCompareFilesOneAtATime.TabIndex = 3;
			this.chkCompareFilesOneAtATime.Text = "Try to compare files one at a time";
			this.toolTip1.SetToolTip(this.chkCompareFilesOneAtATime, "If we should try and launch the diff tool one file at a time, or all files at onc" +
        "e. This may or may not work depending on your selected diff tool.");
			this.chkCompareFilesOneAtATime.UseVisualStyleBackColor = true;
			this.chkCompareFilesOneAtATime.CheckedChanged += new System.EventHandler(this.chkCompareFilesOneAtATime_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 120);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(223, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "File Extensions To Ignore (e.g. exe, png, bmp)";
			// 
			// txtFileExtensionsToIgnore
			// 
			this.txtFileExtensionsToIgnore.Location = new System.Drawing.Point(9, 136);
			this.txtFileExtensionsToIgnore.Name = "txtFileExtensionsToIgnore";
			this.txtFileExtensionsToIgnore.Size = new System.Drawing.Size(363, 20);
			this.txtFileExtensionsToIgnore.TabIndex = 5;
			this.toolTip1.SetToolTip(this.txtFileExtensionsToIgnore, "Files with these extensions will not be compared.");
			this.txtFileExtensionsToIgnore.Leave += new System.EventHandler(this.txtFileExtensionsToIgnore_Leave);
			// 
			// btnRestoreDefaultSettings
			// 
			this.btnRestoreDefaultSettings.Location = new System.Drawing.Point(216, 162);
			this.btnRestoreDefaultSettings.Name = "btnRestoreDefaultSettings";
			this.btnRestoreDefaultSettings.Size = new System.Drawing.Size(156, 23);
			this.btnRestoreDefaultSettings.TabIndex = 6;
			this.btnRestoreDefaultSettings.Text = "Restore Default Settings";
			this.btnRestoreDefaultSettings.UseVisualStyleBackColor = true;
			this.btnRestoreDefaultSettings.Click += new System.EventHandler(this.btnRestoreDefaultSettings_Click);
			// 
			// btnConfigureDiffTool
			// 
			this.btnConfigureDiffTool.Location = new System.Drawing.Point(3, 206);
			this.btnConfigureDiffTool.Name = "btnConfigureDiffTool";
			this.btnConfigureDiffTool.Size = new System.Drawing.Size(135, 23);
			this.btnConfigureDiffTool.TabIndex = 7;
			this.btnConfigureDiffTool.Text = "Configure Diff Tool";
			this.btnConfigureDiffTool.UseVisualStyleBackColor = true;
			this.btnConfigureDiffTool.Click += new System.EventHandler(this.btnConfigureDiffTool_Click);
			// 
			// toolTip1
			// 
			this.toolTip1.AutoPopDelay = 60000;
			this.toolTip1.InitialDelay = 500;
			this.toolTip1.ReshowDelay = 100;
			// 
			// grpSettings
			// 
			this.grpSettings.Controls.Add(this.chkCompareNewFiles);
			this.grpSettings.Controls.Add(this.chkCompareFilesNotChanged);
			this.grpSettings.Controls.Add(this.btnRestoreDefaultSettings);
			this.grpSettings.Controls.Add(this.chkCompareDeletedFiles);
			this.grpSettings.Controls.Add(this.txtFileExtensionsToIgnore);
			this.grpSettings.Controls.Add(this.chkCompareFilesOneAtATime);
			this.grpSettings.Controls.Add(this.label1);
			this.grpSettings.Location = new System.Drawing.Point(4, 4);
			this.grpSettings.Name = "grpSettings";
			this.grpSettings.Size = new System.Drawing.Size(380, 196);
			this.grpSettings.TabIndex = 8;
			this.grpSettings.TabStop = false;
			this.grpSettings.Text = "Diff All Files Settings";
			// 
			// DiffAllFilesSettingsPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.grpSettings);
			this.Controls.Add(this.btnConfigureDiffTool);
			this.Name = "DiffAllFilesSettingsPageControl";
			this.Size = new System.Drawing.Size(396, 240);
			this.grpSettings.ResumeLayout(false);
			this.grpSettings.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.CheckBox chkCompareFilesNotChanged;
		private System.Windows.Forms.CheckBox chkCompareDeletedFiles;
		private System.Windows.Forms.CheckBox chkCompareNewFiles;
		private System.Windows.Forms.CheckBox chkCompareFilesOneAtATime;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtFileExtensionsToIgnore;
		private System.Windows.Forms.Button btnRestoreDefaultSettings;
		private System.Windows.Forms.Button btnConfigureDiffTool;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.GroupBox grpSettings;
	}
}
