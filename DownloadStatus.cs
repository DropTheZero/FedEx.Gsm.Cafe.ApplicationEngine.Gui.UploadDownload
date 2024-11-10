// Decompiled with JetBrains decompiler
// Type: FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.DownloadStatus
// Assembly: FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload, Version=38.55.1083.0, Culture=neutral, PublicKeyToken=null
// MVID: BB3DAF7E-8B46-46B6-B579-7126E2E7434E
// Assembly location: C:\Program Files (x86)\FedEx\ShipManager\BIN\FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.dll

using FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Properties;
using FedEx.Gsm.Cafe.ApplicationEngine.Gui.UtilityFunctions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload
{
  public class DownloadStatus : HelpFormBase
  {
    private Bitmap imgPending;
    private Bitmap imgSuccess;
    private Bitmap imgFailure;
    private IContainer components;
    private Label StatusHeader;
    private Label DownloadHeader;
    private Label StatusUrsaTable;
    private Label StatusDomesticRates;
    private Label StatusInternationalRates;
    private Label StatusTrackingNumbers;
    private PictureBox imageUrsaTable;
    private PictureBox imageDomesticRates;
    private PictureBox imageTrackingNumbers;
    private Button btnOK;
    private PictureBox imageInternationalRates;

    public DownloadStatus()
    {
      this.InitializeComponent();
      this.imgPending = Resources.dd_pending;
      this.imgSuccess = Resources.dd_success;
      this.imgFailure = Resources.dd_failure;
    }

    private void DownloadStatus_Load(object sender, EventArgs e)
    {
      this.SetImage(this.imageUrsaTable, Status.Pending);
      this.SetImage(this.imageTrackingNumbers, Status.Pending);
      this.SetImage(this.imageInternationalRates, Status.Pending);
      this.SetImage(this.imageDomesticRates, Status.Pending);
    }

    public void ChangeStatus(DownloadTypes downloadType, Status status)
    {
      switch (downloadType)
      {
        case DownloadTypes.URSA_Table:
          this.SetImage(this.imageUrsaTable, status);
          break;
        case DownloadTypes.Domestic_Rates:
          this.SetImage(this.imageDomesticRates, status);
          break;
        case DownloadTypes.International_Rates:
          this.SetImage(this.imageInternationalRates, status);
          break;
        case DownloadTypes.Tracking_Numbers:
          this.SetImage(this.imageTrackingNumbers, status);
          break;
      }
      this.Focus();
    }

    private void SetImage(PictureBox pictureBox, Status status)
    {
      pictureBox.Image = this.GetImage(status);
      pictureBox.Tag = (object) status.ToString();
    }

    private Image GetImage(Status status)
    {
      if (status == Status.Success)
        return (Image) this.imgSuccess;
      return status == Status.Failure ? (Image) this.imgFailure : (Image) this.imgPending;
    }

    public Status GetCurrentStatus(DownloadTypes downloadType)
    {
      PictureBox pictureBox = new PictureBox();
      switch (downloadType)
      {
        case DownloadTypes.URSA_Table:
          pictureBox = this.imageUrsaTable;
          break;
        case DownloadTypes.Domestic_Rates:
          pictureBox = this.imageDomesticRates;
          break;
        case DownloadTypes.International_Rates:
          pictureBox = this.imageInternationalRates;
          break;
        case DownloadTypes.Tracking_Numbers:
          pictureBox = this.imageTrackingNumbers;
          break;
      }
      return (Status) Enum.Parse(typeof (Status), pictureBox.Tag.ToString());
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (DownloadStatus));
      this.StatusHeader = new Label();
      this.DownloadHeader = new Label();
      this.StatusTrackingNumbers = new Label();
      this.StatusInternationalRates = new Label();
      this.StatusDomesticRates = new Label();
      this.StatusUrsaTable = new Label();
      this.imageUrsaTable = new PictureBox();
      this.imageDomesticRates = new PictureBox();
      this.imageTrackingNumbers = new PictureBox();
      this.btnOK = new Button();
      this.imageInternationalRates = new PictureBox();
      ((ISupportInitialize) this.imageUrsaTable).BeginInit();
      ((ISupportInitialize) this.imageDomesticRates).BeginInit();
      ((ISupportInitialize) this.imageTrackingNumbers).BeginInit();
      ((ISupportInitialize) this.imageInternationalRates).BeginInit();
      this.SuspendLayout();
      componentResourceManager.ApplyResources((object) this.StatusHeader, "StatusHeader");
      this.StatusHeader.Name = "StatusHeader";
      componentResourceManager.ApplyResources((object) this.DownloadHeader, "DownloadHeader");
      this.DownloadHeader.Name = "DownloadHeader";
      componentResourceManager.ApplyResources((object) this.StatusTrackingNumbers, "StatusTrackingNumbers");
      this.StatusTrackingNumbers.Name = "StatusTrackingNumbers";
      componentResourceManager.ApplyResources((object) this.StatusInternationalRates, "StatusInternationalRates");
      this.StatusInternationalRates.Name = "StatusInternationalRates";
      componentResourceManager.ApplyResources((object) this.StatusDomesticRates, "StatusDomesticRates");
      this.StatusDomesticRates.Name = "StatusDomesticRates";
      componentResourceManager.ApplyResources((object) this.StatusUrsaTable, "StatusUrsaTable");
      this.StatusUrsaTable.Name = "StatusUrsaTable";
      componentResourceManager.ApplyResources((object) this.imageUrsaTable, "imageUrsaTable");
      this.imageUrsaTable.Name = "imageUrsaTable";
      this.imageUrsaTable.TabStop = false;
      componentResourceManager.ApplyResources((object) this.imageDomesticRates, "imageDomesticRates");
      this.imageDomesticRates.Name = "imageDomesticRates";
      this.imageDomesticRates.TabStop = false;
      componentResourceManager.ApplyResources((object) this.imageTrackingNumbers, "imageTrackingNumbers");
      this.imageTrackingNumbers.Name = "imageTrackingNumbers";
      this.imageTrackingNumbers.TabStop = false;
      this.btnOK.DialogResult = DialogResult.OK;
      componentResourceManager.ApplyResources((object) this.btnOK, "btnOK");
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      componentResourceManager.ApplyResources((object) this.imageInternationalRates, "imageInternationalRates");
      this.imageInternationalRates.Name = "imageInternationalRates";
      this.imageInternationalRates.TabStop = false;
      componentResourceManager.ApplyResources((object) this, "$this");
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.btnOK);
      this.Controls.Add((Control) this.imageTrackingNumbers);
      this.Controls.Add((Control) this.imageInternationalRates);
      this.Controls.Add((Control) this.imageDomesticRates);
      this.Controls.Add((Control) this.imageUrsaTable);
      this.Controls.Add((Control) this.StatusTrackingNumbers);
      this.Controls.Add((Control) this.StatusInternationalRates);
      this.Controls.Add((Control) this.StatusDomesticRates);
      this.Controls.Add((Control) this.StatusUrsaTable);
      this.Controls.Add((Control) this.DownloadHeader);
      this.Controls.Add((Control) this.StatusHeader);
      this.HelpButton = false;
      this.helpProvider1.SetHelpKeyword((Control) this, componentResourceManager.GetString("$this.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this, (HelpNavigator) componentResourceManager.GetObject("$this.HelpNavigator"));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (DownloadStatus);
      this.helpProvider1.SetShowHelp((Control) this, (bool) componentResourceManager.GetObject("$this.ShowHelp"));
      this.ShowIcon = false;
      this.Load += new EventHandler(this.DownloadStatus_Load);
      ((ISupportInitialize) this.imageUrsaTable).EndInit();
      ((ISupportInitialize) this.imageDomesticRates).EndInit();
      ((ISupportInitialize) this.imageTrackingNumbers).EndInit();
      ((ISupportInitialize) this.imageInternationalRates).EndInit();
      this.ResumeLayout(false);
    }
  }
}
