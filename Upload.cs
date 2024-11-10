// Decompiled with JetBrains decompiler
// Type: FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Upload
// Assembly: FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload, Version=38.55.1083.0, Culture=neutral, PublicKeyToken=null
// MVID: BB3DAF7E-8B46-46B6-B579-7126E2E7434E
// Assembly location: C:\Program Files (x86)\FedEx\ShipManager\BIN\FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.dll

using FedEx.Gsm.Cafe.ApplicationEngine.Gui.Data;
using FedEx.Gsm.Cafe.ApplicationEngine.Gui.Eventing;
using FedEx.Gsm.Cafe.ApplicationEngine.Gui.UtilityFunctions;
using FedEx.Gsm.Common.Logging;
using FedEx.Gsm.ShipEngine.DataAccess;
using FedEx.Gsm.ShipEngine.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

#nullable disable
namespace FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload
{
  public class Upload : HelpFormBase
  {
    private IContainer components;
    private Label Title;
    private CheckBox chkShippingData;
    private CheckBox chkGroundEPDIFiles;
    private CheckBox chkCallTags;
    private Button btnOK;
    private Button btnSelectAll;
    private Button btnUnselectAll;
    private Button btnCancel;
    private PictureBox pictureBox1;
    private CheckBox chkSmartPostData;

    public static event TopicDelegate UpdateStatusBar;

    public Upload()
    {
      this.InitializeComponent();
      this.SetupEvents();
    }

    private void btnSelectAll_Click(object sender, EventArgs e)
    {
      this.CheckUnCheckAllCheckboxes(true);
    }

    private void SetupEvents()
    {
      GuiData.EventBroker.AddPublisher(EventBroker.Events.UpdateStatusBar, (object) this, "UpdateStatusBar");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        try
        {
          GuiData.EventBroker.RemovePublisher(EventBroker.Events.UpdateStatusBar, (object) this, "UpdateStatusBar");
        }
        catch (Exception ex)
        {
          FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Warning, FxLogger.AppCode_GUI, "Upload.Dispose() Remove events", ex.ToString());
        }
      }
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void CheckUnCheckAllCheckboxes(bool check)
    {
      foreach (Control control in (ArrangedElementCollection) this.Controls)
      {
        if (control.GetType() == typeof (CheckBox))
          ((CheckBox) control).Checked = check;
      }
    }

    private void btnUnselectAll_Click(object sender, EventArgs e)
    {
      this.CheckUnCheckAllCheckboxes(false);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      if (!this.ItemChecked())
      {
        this.DialogResult = DialogResult.None;
        int num = (int) MessageBox.Show(FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Resources.UploadDownload.IDS_SELECT_ITEMS_TO_UPLOAD);
      }
      else
      {
        RevenueServiceRequest state = new RevenueServiceRequest();
        if (this.chkShippingData.Checked)
          state.IncludeExpress = true;
        if (this.chkGroundEPDIFiles.Checked)
          state.IncludeGround = true;
        if (this.chkCallTags.Checked)
          state.IncludeCallTag = true;
        if (this.chkSmartPostData.Checked)
          state.IncludeSmartPost = true;
        ThreadPool.QueueUserWorkItem(new WaitCallback(Upload.RequestUpload), (object) state);
        int num = (int) MessageBox.Show((IWin32Window) this, GuiData.Languafier.Translate("UploadRequestInitiated"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        this.DialogResult = DialogResult.OK;
      }
    }

    private bool ItemChecked()
    {
      bool flag = false;
      foreach (Control control in (ArrangedElementCollection) this.Controls)
      {
        if (control.GetType() == typeof (CheckBox) && ((CheckBox) control).Checked)
        {
          flag = true;
          break;
        }
      }
      return flag;
    }

    public static void RequestUpload(object uploadRequest)
    {
      Upload sender = new Upload();
      RevenueServiceRequest revenueServiceRequest = (RevenueServiceRequest) uploadRequest;
      StatusBarEventArgs args = new StatusBarEventArgs(FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Resources.UploadDownload.UploadStart);
      FedEx.Gsm.Common.Languafier.Languafier languafier = new FedEx.Gsm.Common.Languafier.Languafier();
      if (Upload.UpdateStatusBar != null)
        Upload.UpdateStatusBar((object) sender, (EventArgs) args);
      try
      {
        RevenueServiceResponse revenueServiceResponse = GuiData.AppController.ShipEngine.DemandUpload(revenueServiceRequest);
        if (revenueServiceResponse.IsOperationOk)
          return;
        if (Upload.UpdateStatusBar != null)
          Upload.UpdateStatusBar((object) sender, (EventArgs) new StatusBarEventArgs(languafier.TranslateError(revenueServiceResponse.ErrorCode)));
        FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, FxLogger.AppCode_GUI, "DemandUpload", " return code" + revenueServiceResponse.Error.Code.ToString());
      }
      catch (Exception ex)
      {
        if (Upload.UpdateStatusBar != null)
          Upload.UpdateStatusBar((object) sender, (EventArgs) new StatusBarEventArgs(ex.Message));
        FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, FxLogger.AppCode_GUI, "DemandUpload", " return message" + ex.Message);
      }
    }

    private void Upload_Load(object sender, EventArgs e)
    {
      DataTable output;
      GuiData.AppController.ShipEngine.GetDataList((object) null, GsmDataAccess.ListSpecification.Sender_List, out output, new List<GsmFilter>()
      {
        new GsmFilter("IsSmartPostSender", "=", (object) "1")
      }, (List<GsmSort>) null, (List<string>) null, new Error());
      if (output != null && output.Rows.Count > 0 || GuiData.CurrentAccount.IsSmartPostEnabled)
        this.chkSmartPostData.Enabled = true;
      else
        this.chkSmartPostData.Enabled = false;
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Upload));
      this.Title = new Label();
      this.chkShippingData = new CheckBox();
      this.chkGroundEPDIFiles = new CheckBox();
      this.chkCallTags = new CheckBox();
      this.btnOK = new Button();
      this.btnSelectAll = new Button();
      this.btnUnselectAll = new Button();
      this.btnCancel = new Button();
      this.pictureBox1 = new PictureBox();
      this.chkSmartPostData = new CheckBox();
      ((ISupportInitialize) this.pictureBox1).BeginInit();
      this.SuspendLayout();
      componentResourceManager.ApplyResources((object) this.Title, "Title");
      this.Title.Name = "Title";
      this.helpProvider1.SetShowHelp((Control) this.Title, (bool) componentResourceManager.GetObject("Title.ShowHelp"));
      this.helpProvider1.SetHelpKeyword((Control) this.chkShippingData, componentResourceManager.GetString("chkShippingData.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkShippingData, (HelpNavigator) componentResourceManager.GetObject("chkShippingData.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkShippingData, "chkShippingData");
      this.chkShippingData.Name = "chkShippingData";
      this.helpProvider1.SetShowHelp((Control) this.chkShippingData, (bool) componentResourceManager.GetObject("chkShippingData.ShowHelp"));
      this.chkShippingData.UseVisualStyleBackColor = true;
      this.helpProvider1.SetHelpKeyword((Control) this.chkGroundEPDIFiles, componentResourceManager.GetString("chkGroundEPDIFiles.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkGroundEPDIFiles, (HelpNavigator) componentResourceManager.GetObject("chkGroundEPDIFiles.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkGroundEPDIFiles, "chkGroundEPDIFiles");
      this.chkGroundEPDIFiles.Name = "chkGroundEPDIFiles";
      this.helpProvider1.SetShowHelp((Control) this.chkGroundEPDIFiles, (bool) componentResourceManager.GetObject("chkGroundEPDIFiles.ShowHelp"));
      this.chkGroundEPDIFiles.UseVisualStyleBackColor = true;
      componentResourceManager.ApplyResources((object) this.chkCallTags, "chkCallTags");
      this.helpProvider1.SetHelpKeyword((Control) this.chkCallTags, componentResourceManager.GetString("chkCallTags.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkCallTags, (HelpNavigator) componentResourceManager.GetObject("chkCallTags.HelpNavigator"));
      this.chkCallTags.Name = "chkCallTags";
      this.helpProvider1.SetShowHelp((Control) this.chkCallTags, (bool) componentResourceManager.GetObject("chkCallTags.ShowHelp"));
      this.chkCallTags.UseVisualStyleBackColor = true;
      componentResourceManager.ApplyResources((object) this.btnOK, "btnOK");
      this.btnOK.Name = "btnOK";
      this.helpProvider1.SetShowHelp((Control) this.btnOK, (bool) componentResourceManager.GetObject("btnOK.ShowHelp"));
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      componentResourceManager.ApplyResources((object) this.btnSelectAll, "btnSelectAll");
      this.btnSelectAll.CausesValidation = false;
      this.btnSelectAll.Name = "btnSelectAll";
      this.helpProvider1.SetShowHelp((Control) this.btnSelectAll, (bool) componentResourceManager.GetObject("btnSelectAll.ShowHelp"));
      this.btnSelectAll.UseVisualStyleBackColor = true;
      this.btnSelectAll.Click += new EventHandler(this.btnSelectAll_Click);
      componentResourceManager.ApplyResources((object) this.btnUnselectAll, "btnUnselectAll");
      this.btnUnselectAll.CausesValidation = false;
      this.btnUnselectAll.Name = "btnUnselectAll";
      this.helpProvider1.SetShowHelp((Control) this.btnUnselectAll, (bool) componentResourceManager.GetObject("btnUnselectAll.ShowHelp"));
      this.btnUnselectAll.UseVisualStyleBackColor = true;
      this.btnUnselectAll.Click += new EventHandler(this.btnUnselectAll_Click);
      componentResourceManager.ApplyResources((object) this.btnCancel, "btnCancel");
      this.btnCancel.CausesValidation = false;
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.helpProvider1.SetShowHelp((Control) this.btnCancel, (bool) componentResourceManager.GetObject("btnCancel.ShowHelp"));
      this.btnCancel.UseVisualStyleBackColor = true;
      componentResourceManager.ApplyResources((object) this.pictureBox1, "pictureBox1");
      this.pictureBox1.Image = (Image) FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Properties.Resources.dd_comm;
      this.pictureBox1.Name = "pictureBox1";
      this.helpProvider1.SetShowHelp((Control) this.pictureBox1, (bool) componentResourceManager.GetObject("pictureBox1.ShowHelp"));
      this.pictureBox1.TabStop = false;
      this.helpProvider1.SetHelpKeyword((Control) this.chkSmartPostData, componentResourceManager.GetString("chkSmartPostData.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkSmartPostData, (HelpNavigator) componentResourceManager.GetObject("chkSmartPostData.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkSmartPostData, "chkSmartPostData");
      this.chkSmartPostData.Name = "chkSmartPostData";
      this.helpProvider1.SetShowHelp((Control) this.chkSmartPostData, (bool) componentResourceManager.GetObject("chkSmartPostData.ShowHelp"));
      this.chkSmartPostData.UseVisualStyleBackColor = true;
      this.AcceptButton = (IButtonControl) this.btnOK;
      componentResourceManager.ApplyResources((object) this, "$this");
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.Controls.Add((Control) this.chkSmartPostData);
      this.Controls.Add((Control) this.pictureBox1);
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnUnselectAll);
      this.Controls.Add((Control) this.btnSelectAll);
      this.Controls.Add((Control) this.btnOK);
      this.Controls.Add((Control) this.chkCallTags);
      this.Controls.Add((Control) this.chkGroundEPDIFiles);
      this.Controls.Add((Control) this.chkShippingData);
      this.Controls.Add((Control) this.Title);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.HelpButton = false;
      this.helpProvider1.SetHelpKeyword((Control) this, componentResourceManager.GetString("$this.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this, (HelpNavigator) componentResourceManager.GetObject("$this.HelpNavigator"));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (Upload);
      this.helpProvider1.SetShowHelp((Control) this, (bool) componentResourceManager.GetObject("$this.ShowHelp"));
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.TopMost = true;
      this.Load += new EventHandler(this.Upload_Load);
      ((ISupportInitialize) this.pictureBox1).EndInit();
      this.ResumeLayout(false);
    }
  }
}
