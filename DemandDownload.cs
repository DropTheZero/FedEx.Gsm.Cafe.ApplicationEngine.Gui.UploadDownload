// Decompiled with JetBrains decompiler
// Type: FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.DemandDownload
// Assembly: FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload, Version=38.55.1083.0, Culture=neutral, PublicKeyToken=null
// MVID: BB3DAF7E-8B46-46B6-B579-7126E2E7434E
// Assembly location: C:\Program Files (x86)\FedEx\ShipManager\BIN\FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.dll

using FedEx.Gsm.Cafe.ApplicationEngine.Gui.Data;
using FedEx.Gsm.Cafe.ApplicationEngine.Gui.Eventing;
using FedEx.Gsm.Cafe.ApplicationEngine.Gui.UserControls;
using FedEx.Gsm.Cafe.ApplicationEngine.Gui.UtilityFunctions;
using FedEx.Gsm.Common.Logging;
using FedEx.Gsm.ShipEngine.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

#nullable disable
namespace FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload
{
  public class DemandDownload : HelpFormBase
  {
    private bool _OnlyResetProfile;
    private string notCurrentVersionText;
    private IContainer components;
    private ColorGroupBox OptionsGroup;
    private CheckBox chkSoftware;
    private CheckBox chkMaintenance;
    private CheckBox chkURSATable;
    private CheckBox chkCountryList;
    private Button btnOK;
    private Button btnSelectAll;
    private Button btnUnselectAll;
    private Button btnCancel;
    private BackgroundWorker downloadWorker;
    private CheckBox chkServiceBullitenBoard;
    private FlowLayoutPanel OptionsFlowLayout;
    private LinkLabel lnkSoftwareUpdatesDisclaimer;
    private CheckBox chkIATATable;
    private DisablingTreeView RatesTrackingNumberTreeView;
    private CheckBox chkHazMatTable;
    private CheckBox chkHALTable;
    private CheckBox chkCurrencyTable;
    private Button btnCheckUpdate;
    private CheckBox chkNewRates;

    public event TopicDelegate UpdateStatusBar;

    public event TopicDelegate DownloadFinished;

    public event EventHandler<DownloadStartedEventArgs> DownloadStarted;

    public DemandDownload()
    {
      this.InitializeComponent();
      this.SetupEvents();
      this.VerifyCheckboxes();
      this.downloadWorker.DoWork += new DoWorkEventHandler(this.downloadWorker_DoWork);
      this.downloadWorker.ProgressChanged += new ProgressChangedEventHandler(this.downloadWorker_ProgressChanged);
      this.downloadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.downloadWorker_RunWorkerCompleted);
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      foreach (TreeNode node1 in this.RatesTrackingNumberTreeView.Nodes)
      {
        node1.Text = GuiData.Languafier.Translate(nameof (DemandDownload) + node1.Name + "Node") ?? node1.Text;
        foreach (TreeNode node2 in node1.Nodes)
          node2.Text = GuiData.Languafier.Translate(nameof (DemandDownload) + node2.Name + "Node") ?? node2.Text;
      }
      this.Realigncheckboxes();
    }

    private void downloadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      StatusBarEventArgs userState = e.UserState as StatusBarEventArgs;
      if (this.UpdateStatusBar == null || userState == null)
        return;
      this.UpdateStatusBar((object) this, (EventArgs) userState);
    }

    private void downloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      try
      {
        DownloadFinishedEventArgs result = e.Result as DownloadFinishedEventArgs;
        if (e.Error != null)
        {
          if (result != null)
            result.Success = false;
          this.OnDownloadFinished((object) this, result);
        }
        else
          this.OnDownloadFinished((object) this, result);
      }
      finally
      {
        this.DialogResult = DialogResult.OK;
      }
    }

    private void downloadWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      Error error = new Error();
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      DemandDownload.DownloadData downloadData = (DemandDownload.DownloadData) e.Argument;
      DownloadFinishedEventArgs finishArgs = downloadData.FinishArgs;
      finishArgs.Success = true;
      backgroundWorker.ReportProgress(0, (object) new StatusBarEventArgs(FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Resources.UploadDownload.DownloadStart));
      int code;
      if (downloadData.DownloadRequest != null)
      {
        ProcessDownloadResponse downloadResponse = GuiData.AppController.ShipEngine.ProcessDetailedDownloadRequest(downloadData.DownloadRequest);
        if (!downloadResponse.IsOperationOk || downloadResponse.AdminDownloadResponse != null && downloadResponse.AdminDownloadResponse.Count > 0)
        {
          if (downloadResponse.HasError)
          {
            finishArgs.Success = false;
            finishArgs.CommFailure = downloadResponse.ErrorCode == 320005;
          }
          string appCodeGui = FxLogger.AppCode_GUI;
          code = downloadResponse.Error.Code;
          string inMessage = " return code" + code.ToString();
          FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, appCodeGui, "ShipEngine.ProcessDownloadRequests", inMessage);
          if (downloadResponse.AdminDownloadResponse != null)
          {
            finishArgs.Success = false;
            foreach (ProcessDownloadResponseItem downloadResponseItem in downloadResponse.AdminDownloadResponse)
            {
              if (downloadResponseItem.Response != null)
                FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, FxLogger.AppCode_GUI, "ShipEngine.ProcessDownloadRequests", "Request " + downloadResponseItem.Request.ToString() + " returned errors " + string.Join<int>(", ", downloadResponseItem.Response.Select<ServiceResponse, int>((Func<ServiceResponse, int>) (r => r.ErrorCode))));
              else
                FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, FxLogger.AppCode_GUI, "ShipEngine.ProcessDownloadRequests", "Request " + downloadResponseItem.Request.ToString() + " returned no response list");
            }
            finishArgs.Responses = downloadResponse.AdminDownloadResponse;
          }
          if (downloadResponse.Error.Code == 295033)
          {
            int num = (int) Utility.DisplayError(GuiData.Languafier.TranslateError(downloadResponse.Error.Code), Error.ErrorType.Failure);
            GuiData.AppController.ShipEngine.InstallFedExService();
          }
        }
        backgroundWorker.ReportProgress(0, (object) new StatusBarEventArgs(GuiData.Languafier.TranslateError(downloadResponse.ErrorCode) ?? downloadResponse.ErrorMessage));
      }
      if (downloadData.RunInstalls)
      {
        ServiceResponse serviceResponse = GuiData.AppController.ShipEngine.DownloadInstalls((List<AdminInstallInfo>) null);
        if (!serviceResponse.IsOperationOk)
        {
          if (serviceResponse.HasError)
            finishArgs.Success = false;
          string appCodeGui = FxLogger.AppCode_GUI;
          code = serviceResponse.Error.Code;
          string inMessage = " return code" + code.ToString();
          FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, appCodeGui, "AdminUtil.ExecuteAllAvailable", inMessage);
          backgroundWorker.ReportProgress(0, (object) new StatusBarEventArgs(GuiData.Languafier.TranslateError(serviceResponse.ErrorCode) ?? serviceResponse.ErrorMessage));
        }
        else
          finishArgs.Software = true;
      }
      this.DownloadFinished((object) downloadData.DownloadForm, (EventArgs) downloadData.FinishArgs);
      e.Result = (object) finishArgs;
    }

    protected override void OnClosing(CancelEventArgs e) => base.OnClosing(e);

    public void SelectAll()
    {
      this.chkSoftware.Enabled = false;
      this.CheckUnCheckAllCheckboxes(true);
    }

    public void UnselectAll()
    {
      this.CheckUnCheckAllCheckboxes(false);
      this.chkSoftware.Enabled = true;
    }

    private void btnSelectAll_Click(object sender, EventArgs e) => this.SelectAll();

    private void btnUnselectAll_Click(object sender, EventArgs e) => this.UnselectAll();

    private void CheckUnCheckAllCheckboxes(bool check)
    {
      foreach (Control control in (ArrangedElementCollection) this.OptionsFlowLayout.Controls)
      {
        if (control.GetType() == typeof (CheckBox) && control.Enabled)
          ((CheckBox) control).Checked = check;
      }
      foreach (TreeNode node in this.RatesTrackingNumberTreeView.Nodes)
      {
        node.Checked = check;
        this.CheckAllChildNodes(node, check);
      }
    }

    private void Realigncheckboxes()
    {
      foreach (Control control in (ArrangedElementCollection) this.OptionsFlowLayout.Controls)
      {
        if (control is CheckBox)
          control.Margin = new Padding(this.RatesTrackingNumberTreeView.Indent + 3, 0, 0, 0);
      }
    }

    private void SetupEvents()
    {
      GuiData.EventBroker.AddPublisher(EventBroker.Events.UpdateStatusBar, (object) this, "UpdateStatusBar");
      GuiData.EventBroker.AddPublisher(EventBroker.Events.DownloadStarted, (object) this, "DownloadStarted");
      GuiData.EventBroker.AddPublisher(EventBroker.Events.DownloadFinished, (object) this, "DownloadFinished");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        try
        {
          GuiData.EventBroker.RemovePublisher(EventBroker.Events.UpdateStatusBar, (object) this, "UpdateStatusBar");
          GuiData.EventBroker.RemovePublisher(EventBroker.Events.DownloadFinished, (object) this, "DownloadFinished");
        }
        catch (Exception ex)
        {
          FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Warning, FxLogger.AppCode_GUI, "DemandDownload.Dispose() Remove events", ex.ToString());
        }
      }
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    public void OnDownloadFinished(object sender, DownloadFinishedEventArgs e)
    {
      if (this._OnlyResetProfile)
      {
        int num1 = (int) MessageBox.Show(FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Resources.UploadDownload.IDS_DSMS_PROFILE_RESET, "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
      else
      {
        List<string> stringList = (List<string>) null;
        string id = e.Success ? "m38006" : "m38001";
        if (e.Success && e.Software)
          id = "m38007";
        if (e.Responses != null)
        {
          stringList = e.Responses.Where<ProcessDownloadResponseItem>((Func<ProcessDownloadResponseItem, bool>) (r => r.Response != null && r.Response.Any<ServiceResponse>((Func<ServiceResponse, bool>) (rs => rs.HasError)))).Select<ProcessDownloadResponseItem, string>((Func<ProcessDownloadResponseItem, string>) (r => GuiData.Languafier.Translate("DemandDownloadRequestName_" + r.Request.ToString("d")))).Where<string>((Func<string, bool>) (s => !string.IsNullOrEmpty(s))).Distinct<string>().ToList<string>();
          if (stringList.Count > 0 && id == "m38001")
            id = "DownloadFailedSpecific";
        }
        string text = e.CommFailure ? GuiData.Languafier.TranslateError(320005) : GuiData.Languafier.TranslateMessage(id);
        MessageBoxIcon icon = e.CommFailure ? MessageBoxIcon.Hand : MessageBoxIcon.Asterisk;
        if (stringList != null && stringList.Count > 0 && !e.CommFailure)
        {
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.AppendLine(text);
          foreach (string str in stringList)
            stringBuilder.AppendLine("\t" + str);
          text = stringBuilder.ToString();
        }
        if (this.InvokeRequired)
          this.Invoke((Delegate) new EventHandler<DownloadFinishedEventArgs>(this.OnDownloadFinished), (object) this, (object) e);
        else if (!this.IsDisposed)
        {
          int num2 = (int) MessageBox.Show((IWin32Window) this, text, string.Empty, MessageBoxButtons.OK, icon);
        }
        else
        {
          int num3 = (int) MessageBox.Show(text, string.Empty, MessageBoxButtons.OK, icon);
        }
      }
    }

    private void AddAccountMeter(ref AdminDownloadRequest adminDownloadRequest)
    {
      adminDownloadRequest.Account = GuiData.CurrentAccount;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      DemandDownload.DownloadData downloadData = new DemandDownload.DownloadData();
      downloadData.RunInstalls = false;
      downloadData.DownloadRequest = (List<AdminDownloadRequest>) null;
      downloadData.DownloadForm = this;
      this._OnlyResetProfile = false;
      if (!this.AreAnyItemsChecked)
      {
        this.DialogResult = DialogResult.None;
        int num = (int) MessageBox.Show(GuiData.Languafier.TranslateMessage(38005));
      }
      else
      {
        this.OptionsGroup.Enabled = false;
        this.EnableControlButtons(false);
        if (this.chkURSATable.Checked)
        {
          if (Control.ModifierKeys == Keys.Shift)
          {
            try
            {
              this.EnableButtons(true);
              this._OnlyResetProfile = true;
              this.Close();
            }
            catch (Exception ex)
            {
              FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, FxLogger.AppCode_GUI, "Reset DSMS", " return error" + ex.Message);
            }
          }
        }
        List<AdminDownloadRequest> adminDownloadRequestList = new List<AdminDownloadRequest>();
        downloadData.FinishArgs = new DownloadFinishedEventArgs();
        string empty = string.Empty;
        string countryCode = GuiData.CurrentAccount.Address.CountryCode;
        if (!this._OnlyResetProfile)
        {
          if (this.chkNewRates.Checked)
          {
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.DLL;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "ExpressDomesticListRates") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["ExpressDomesticListRates"].Checked)
          {
            downloadData.FinishArgs.DomesticListRatings = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = string.Compare(countryCode, "CA", true) == 0 || string.Compare(countryCode, "MX", true) == 0 ? AdminDownloadRequest.RequestType.DomesticNonUsList : AdminDownloadRequest.RequestType.ListRates;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "ExpressDomesticDiscounts") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["ExpressDomesticDiscounts"].Checked)
          {
            downloadData.FinishArgs.DomesticRatings = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = string.Compare(countryCode, "CA", true) == 0 || string.Compare(countryCode, "MX", true) == 0 ? AdminDownloadRequest.RequestType.DomesticNonUS : AdminDownloadRequest.RequestType.DomesticRates;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "ExpressEarnedDiscounts") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["ExpressEarnedDiscounts"].Checked)
          {
            downloadData.FinishArgs.EarnedDiscounts = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.EarnedDiscounts;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "ExpressInternationalDiscounts") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["ExpressInternationalDiscounts"].Checked)
          {
            downloadData.FinishArgs.InternationalRatings = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.IntlRates;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "GroundListRates") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundListRates"].Checked)
          {
            downloadData.FinishArgs.GroundListRates = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.GroundListRates;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "GroundDiscounts") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundDiscounts"].Checked)
          {
            downloadData.FinishArgs.GroundDiscounts = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.GroundDiscount;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "GroundEarnedDiscounts") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundEarnedDiscounts"].Checked)
          {
            downloadData.FinishArgs.GroundEarnedDiscounts = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.GroundEarnedDiscount;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "SmartPostRates") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostRates"].Checked)
          {
            downloadData.FinishArgs.SmartPostRates = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.SmartPostRates;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "SmartPostListRates") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostListRates"].Checked)
          {
            downloadData.FinishArgs.SmartPostRates = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.SmartPostListRates;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "SmartPostEarnedDiscounts") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostEarnedDiscounts"].Checked)
          {
            downloadData.FinishArgs.SmartPostEarnedDiscounts = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.SmartPostEarnedDiscounts;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Rate", "OtherRates") && this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["OtherRates"].Checked)
          {
            downloadData.FinishArgs.OtherRates = true;
            AdminDownloadRequest adminDownloadRequest1 = new AdminDownloadRequest();
            adminDownloadRequest1.DownloadType = AdminDownloadRequest.RequestType.OtherRates;
            this.AddAccountMeter(ref adminDownloadRequest1);
            adminDownloadRequestList.Add(adminDownloadRequest1);
            AdminDownloadRequest adminDownloadRequest2 = new AdminDownloadRequest();
            adminDownloadRequest2.DownloadType = AdminDownloadRequest.RequestType.AccountOneRate;
            this.AddAccountMeter(ref adminDownloadRequest2);
            adminDownloadRequestList.Add(adminDownloadRequest2);
            AdminDownloadRequest adminDownloadRequest3 = new AdminDownloadRequest();
            adminDownloadRequest3.DownloadType = AdminDownloadRequest.RequestType.ListOneRate;
            this.AddAccountMeter(ref adminDownloadRequest3);
            adminDownloadRequestList.Add(adminDownloadRequest3);
          }
          if (this.ChildNodeExist("Tracking", "ExpressTrackingNumbers") && this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"].Nodes["ExpressTrackingNumbers"].Checked)
          {
            downloadData.FinishArgs.TrackingNumbers = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.ExpressTrackingNumberRange;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.ChildNodeExist("Tracking", "SmartPostTrackingNumbers") && this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"].Nodes["SmartPostTrackingNumbers"].Checked)
          {
            downloadData.FinishArgs.SmartPostTrackingNumbers = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.SmartPostTrackingNumberRange;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.chkURSATable.Checked)
          {
            downloadData.FinishArgs.URSA = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.Ursa_Generic;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.chkMaintenance.Checked)
          {
            downloadData.FinishArgs.Maintenance = true;
            AdminDownloadRequest adminDownloadRequest4 = new AdminDownloadRequest();
            adminDownloadRequest4.DownloadType = AdminDownloadRequest.RequestType.McAfee;
            this.AddAccountMeter(ref adminDownloadRequest4);
            adminDownloadRequestList.Add(adminDownloadRequest4);
            downloadData.FinishArgs.PolicyGrid = true;
            AdminDownloadRequest adminDownloadRequest5 = new AdminDownloadRequest();
            adminDownloadRequest5.DownloadType = AdminDownloadRequest.RequestType.PolicyGrid;
            this.AddAccountMeter(ref adminDownloadRequest5);
            adminDownloadRequestList.Add(adminDownloadRequest5);
            downloadData.FinishArgs.PolicyGrid = true;
            AdminDownloadRequest adminDownloadRequest6 = new AdminDownloadRequest();
            adminDownloadRequest6.DownloadType = AdminDownloadRequest.RequestType.CommodityData;
            this.AddAccountMeter(ref adminDownloadRequest6);
            adminDownloadRequestList.Add(adminDownloadRequest6);
            downloadData.FinishArgs.PolicyGrid = true;
            AdminDownloadRequest adminDownloadRequest7 = new AdminDownloadRequest();
            adminDownloadRequest7.DownloadType = AdminDownloadRequest.RequestType.ProhibitionAndWaiverAdvisory;
            this.AddAccountMeter(ref adminDownloadRequest7);
            adminDownloadRequestList.Add(adminDownloadRequest7);
            downloadData.FinishArgs.ProductBrandMaster = true;
            AdminDownloadRequest adminDownloadRequest8 = new AdminDownloadRequest();
            adminDownloadRequest8.DownloadType = AdminDownloadRequest.RequestType.ProductBrandMaster;
            this.AddAccountMeter(ref adminDownloadRequest8);
            adminDownloadRequestList.Add(adminDownloadRequest8);
          }
          if (this.chkServiceBullitenBoard.Checked)
          {
            downloadData.FinishArgs.ServiceBullitenBoard = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.SBB;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.chkIATATable.Checked)
          {
            downloadData.FinishArgs.IATATable = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.IATA;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.chkHazMatTable.Checked)
          {
            downloadData.FinishArgs.HazMatTable = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.HazMatTable;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.chkHALTable.Checked)
          {
            downloadData.FinishArgs.HazMatTable = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.HoldAtLocationTable;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
          if (this.chkCurrencyTable.Checked)
          {
            downloadData.FinishArgs.CurrencyTable = true;
            AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
            adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.CurrencyConversionTable;
            this.AddAccountMeter(ref adminDownloadRequest);
            adminDownloadRequestList.Add(adminDownloadRequest);
          }
        }
        else
        {
          downloadData.FinishArgs.URSA = true;
          AdminDownloadRequest adminDownloadRequest = new AdminDownloadRequest();
          adminDownloadRequest.DownloadType = AdminDownloadRequest.RequestType.NotSupported;
          this.AddAccountMeter(ref adminDownloadRequest);
          adminDownloadRequest.ResetProfile = true;
          adminDownloadRequestList.Add(adminDownloadRequest);
        }
        StatusBarEventArgs args = new StatusBarEventArgs(FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Resources.UploadDownload.DownloadStart);
        if (this.UpdateStatusBar != null)
          this.UpdateStatusBar((object) this, (EventArgs) args);
        try
        {
          if (adminDownloadRequestList.Count > 0)
            downloadData.DownloadRequest = adminDownloadRequestList;
          downloadData.RunInstalls = this.chkSoftware.Checked;
          if (downloadData.DownloadRequest != null || downloadData.RunInstalls)
          {
            this.downloadWorker.RunWorkerAsync((object) downloadData);
            this.OnDownloadStarted(downloadData);
            this.DialogResult = DialogResult.None;
          }
          else
          {
            this.DialogResult = DialogResult.Cancel;
            this.EnableButtons(true);
          }
        }
        catch (Exception ex)
        {
          string msgDnloadFailure = FedEx.Gsm.Cafe.ApplicationEngine.Gui.UploadDownload.Resources.UploadDownload.IDS_MSG_DNLOAD_FAILURE;
          FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, FxLogger.AppCode_GUI, "Winform.DemandDownload.btnOK", " return message" + ex.Message);
          this.DialogResult = DialogResult.Cancel;
          this.EnableButtons(true);
        }
      }
    }

    private void OnDownloadStarted(DemandDownload.DownloadData downloadData)
    {
      EventHandler<DownloadStartedEventArgs> downloadStarted = this.DownloadStarted;
      if (downloadStarted == null)
        return;
      downloadStarted((object) this, new DownloadStartedEventArgs());
    }

    private void EnableControlButtons(bool enabled)
    {
      this.btnCancel.Enabled = enabled;
      this.btnOK.Enabled = enabled;
      this.btnSelectAll.Enabled = enabled;
      this.btnUnselectAll.Enabled = enabled;
    }

    private void EnableButtons(bool enabled)
    {
      foreach (Control control in (ArrangedElementCollection) this.Controls)
      {
        if (control is Button)
          control.Enabled = enabled;
      }
    }

    private bool AreAnyItemsChecked
    {
      get
      {
        foreach (Control control in (ArrangedElementCollection) this.OptionsFlowLayout.Controls)
        {
          if (control is CheckBox && ((CheckBox) control).Checked)
            return true;
        }
        foreach (TreeNode node1 in this.RatesTrackingNumberTreeView.Nodes)
        {
          if (node1.Checked)
            return true;
          foreach (TreeNode node2 in node1.Nodes)
          {
            if (node2.Checked)
              return true;
          }
        }
        return false;
      }
    }

    public void CheckTrackingNumbers(bool bCheck)
    {
      this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"].Nodes["ExpressTrackingNumbers"].Checked = bCheck;
      if (!this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"].Nodes["ExpressTrackingNumbers"].Checked)
        return;
      this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"].ExpandAll();
    }

    private void VerifyCheckboxes()
    {
      string str1 = "N";
      string str2 = "N";
      GuiData.ConfigManager.GetProfileString("SHIPNET2000/ADMINSVC/SETTINGS", "NeverDownloadGroundDiscounts", out str1);
      GuiData.ConfigManager.GetProfileString("SHIPNET2000/GUI/SETTINGS", "DISABLESMARTPOSTEARNEDDISCOUNTDOWNLOAD", out str2);
      if (GuiData.CurrentAccount.IsGroundEnabled)
      {
        if ("Y".Equals(str1, StringComparison.InvariantCultureIgnoreCase))
        {
          if (this.ChildNodeExist("Rate", "GroundDiscounts"))
            this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundDiscounts"].Remove();
          if (this.ChildNodeExist("Rate", "GroundEarnedDiscounts"))
            this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundEarnedDiscounts"].Remove();
        }
      }
      else
      {
        if (this.ChildNodeExist("Rate", "GroundDiscounts"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundDiscounts"].Remove();
        if (this.ChildNodeExist("Rate", "GroundEarnedDiscounts"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundEarnedDiscounts"].Remove();
      }
      if (!GuiData.CurrentAccount.IsGroundEnabled || !GuiData.CurrentAccount.UseListRates)
      {
        if (GuiData.CurrentAccount.IsGroundEnabled && !GuiData.CurrentAccount.UseListRates)
        {
          if (this.ChildNodeExist("Rate", "GroundListRates"))
            this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundListRates"].Remove();
        }
        else if (this.ChildNodeExist("Rate", "GroundListRates"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["GroundListRates"].Remove();
      }
      if (!GuiData.CurrentAccount.UseListRates && this.ChildNodeExist("Rate", "ExpressDomesticListRates"))
        this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["ExpressDomesticListRates"].Remove();
      this.chkCountryList.Visible = false;
      this.chkCountryList.Enabled = false;
      if (GuiData.CurrentAccount.IsSmartPostEnabled)
      {
        if (str2.ToUpper() == "Y" && this.ChildNodeExist("Rate", "SmartPostEarnedDiscounts"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostEarnedDiscounts"].Remove();
        if (!GuiData.CurrentAccount.UseListRates && this.ChildNodeExist("Rate", "SmartPostListRates"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostListRates"].Remove();
      }
      else
      {
        if (this.ChildNodeExist("Rate", "SmartPostRates"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostRates"].Remove();
        if (this.ChildNodeExist("Rate", "SmartPostListRates"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostListRates"].Remove();
        if (this.ChildNodeExist("Rate", "SmartPostEarnedDiscounts"))
          this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes["SmartPostEarnedDiscounts"].Remove();
        if (this.ChildNodeExist("Tracking", "SmartPostTrackingNumbers"))
          this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"].Nodes["SmartPostTrackingNumbers"].Remove();
      }
      this.chkHALTable.Enabled = true;
      if (this.chkHALTable.Enabled)
        return;
      this.chkHALTable.Checked = false;
    }

    private bool NodeExists(params string[] path)
    {
      TreeNodeCollection nodes = this.RatesTrackingNumberTreeView.Nodes;
      for (int index = 0; index < path.Length; ++index)
      {
        string key = path[index];
        if (!nodes.ContainsKey(key))
          return false;
        if (index == path.Length - 1)
          return true;
        nodes = nodes[key].Nodes;
      }
      return false;
    }

    private bool ChildNodeExist(string type, string name)
    {
      bool flag = false;
      if (this.RatesTrackingNumberTreeView.Nodes.Count > 0)
      {
        if (type.Equals("Rate"))
        {
          foreach (TreeNode node in this.RatesTrackingNumberTreeView.Nodes["Rates"].Nodes)
          {
            if (node.Name == name)
            {
              flag = true;
              break;
            }
          }
        }
        else
        {
          foreach (TreeNode node in this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"].Nodes)
          {
            if (node.Name == name)
            {
              flag = true;
              break;
            }
          }
        }
      }
      return flag;
    }

    private void Checkbox_CheckedChanged(object sender, EventArgs e)
    {
      if (this.chkSoftware.Checked)
      {
        this.EnableDisableCheckboxes(false);
        this.CheckUnCheckAllCheckboxes(false);
        this.chkSoftware.Enabled = true;
      }
      else
      {
        this.EnableDisableCheckboxes(true);
        this.chkSoftware.Enabled = !this.AreAnyItemsChecked;
      }
    }

    private void EnableDisableCheckboxes(bool enable)
    {
      foreach (Control control in (ArrangedElementCollection) this.OptionsFlowLayout.Controls)
      {
        if (control is CheckBox)
          control.Enabled = enable;
      }
      this.RatesTrackingNumberTreeView.Enabled = enable;
      if (enable)
        return;
      foreach (TreeNode node in this.RatesTrackingNumberTreeView.Nodes)
      {
        node.Checked = false;
        this.CheckAllChildNodes(node, false);
      }
    }

    private void RatesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
    {
      if (e.Action == TreeViewAction.Unknown)
        return;
      if (e.Node.Nodes.Count > 0)
        this.CheckAllChildNodes(e.Node, e.Node.Checked);
      if (e.Node == this.RatesTrackingNumberTreeView.Nodes["Rates"] || e.Node == this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"])
        this.UpdateIndeterminateState(e.Node);
      else if (e.Node.Parent == this.RatesTrackingNumberTreeView.Nodes["Rates"] || e.Node.Parent == this.RatesTrackingNumberTreeView.Nodes["TrackingNumbers"])
        this.UpdateIndeterminateState(e.Node.Parent);
      this.chkSoftware.Enabled = !this.AreAnyItemsChecked;
    }

    private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
    {
      foreach (TreeNode node in treeNode.Nodes)
      {
        node.Checked = nodeChecked;
        if (node.Nodes.Count > 0)
          this.CheckAllChildNodes(node, nodeChecked);
      }
    }

    private void RatesTreeView_Click(object sender, EventArgs e)
    {
    }

    private void UpdateIndeterminateState(TreeNode node)
    {
      int num = 0;
      foreach (TreeNode node1 in node.Nodes)
      {
        if (node1.Checked)
          ++num;
      }
      if (num == 0)
      {
        node.Checked = false;
        this.ToggleIndeterminateState(node, false);
      }
      else if (num != 0 && num != node.Nodes.Count)
      {
        node.Checked = false;
        this.ToggleIndeterminateState(node, true);
      }
      else
      {
        if (num != node.Nodes.Count)
          return;
        node.Checked = true;
        this.ToggleIndeterminateState(node, false);
      }
    }

    private void ToggleIndeterminateState(TreeNode node, bool indeterminate)
    {
      if (indeterminate)
      {
        if (node.Text.StartsWith("*"))
          return;
        node.Text = "*" + node.Text;
      }
      else
      {
        if (!node.Text.StartsWith("*") || node.Text.Length <= 1)
          return;
        node.Text = node.Text.Substring(1);
      }
    }

    private void setDefaultCheckBoxState()
    {
      if (this.chkURSATable.Font.Style == FontStyle.Bold)
      {
        this.chkURSATable.Font = new Font(this.chkURSATable.Font, FontStyle.Regular);
        this.chkURSATable.Text = this.chkURSATable.Text.Replace(this.notCurrentVersionText, string.Empty);
      }
      if (this.chkIATATable.Font.Style == FontStyle.Bold)
      {
        this.chkIATATable.Font = new Font(this.chkIATATable.Font, FontStyle.Regular);
        this.chkIATATable.Text = this.chkIATATable.Text.Replace(this.notCurrentVersionText, string.Empty);
      }
      if (this.chkHazMatTable.Font.Style == FontStyle.Bold)
      {
        this.chkHazMatTable.Font = new Font(this.chkHazMatTable.Font, FontStyle.Regular);
        this.chkHazMatTable.Text = this.chkHazMatTable.Text.Replace(this.notCurrentVersionText, string.Empty);
      }
      if (this.chkHALTable.Font.Style == FontStyle.Bold)
      {
        this.chkHALTable.Font = new Font(this.chkHALTable.Font, FontStyle.Regular);
        this.chkHALTable.Text = this.chkHALTable.Text.Replace(this.notCurrentVersionText, string.Empty);
      }
      if (this.chkCurrencyTable.Font.Style == FontStyle.Bold)
      {
        this.chkCurrencyTable.Font = new Font(this.chkCurrencyTable.Font, FontStyle.Regular);
        this.chkCurrencyTable.Text = this.chkCurrencyTable.Text.Replace(this.notCurrentVersionText, string.Empty);
      }
      if (this.chkServiceBullitenBoard.Font.Style == FontStyle.Bold)
      {
        this.chkServiceBullitenBoard.Font = new Font(this.chkServiceBullitenBoard.Font, FontStyle.Regular);
        this.chkServiceBullitenBoard.Text = this.chkServiceBullitenBoard.Text.Replace(this.notCurrentVersionText, string.Empty);
      }
      if (this.chkNewRates.Font.Style != FontStyle.Bold)
        return;
      this.chkNewRates.Visible = false;
      this.chkNewRates.Font = new Font(this.chkNewRates.Font, FontStyle.Regular);
    }

    private void btnCheckUpdate_Click(object sender, EventArgs e)
    {
      this.Cursor = Cursors.WaitCursor;
      List<ServiceRequest> accountList = new List<ServiceRequest>();
      List<AdminDownloadRequest> availableList = new List<AdminDownloadRequest>();
      accountList.Add(new ServiceRequest()
      {
        Account = GuiData.CurrentAccount
      });
      try
      {
        GuiData.AppController.ShipEngine.ListAvailableDownloads(accountList, out availableList);
        this.notCurrentVersionText = GuiData.Languafier.Translate("DemandDownloadNotCurrentVersion");
        this.setDefaultCheckBoxState();
        if (availableList.FindAll((Predicate<AdminDownloadRequest>) (f => f.DownloadStatus == AdminDownloadRequest.DownloadState.NotDownloaded)).Count == 0)
        {
          int num = (int) MessageBox.Show((IWin32Window) this, GuiData.Languafier.TranslateMessage("m7105"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
        else
        {
          foreach (AdminDownloadRequest adminDownloadRequest in availableList)
          {
            if (adminDownloadRequest.DownloadStatus == AdminDownloadRequest.DownloadState.NotDownloaded)
            {
              switch (adminDownloadRequest.DownloadType)
              {
                case AdminDownloadRequest.RequestType.Ursa_Generic:
                  this.chkURSATable.Text += this.notCurrentVersionText;
                  this.chkURSATable.Font = new Font(this.chkURSATable.Font, FontStyle.Bold);
                  continue;
                case AdminDownloadRequest.RequestType.SBB:
                  this.chkServiceBullitenBoard.Text += this.notCurrentVersionText;
                  this.chkServiceBullitenBoard.Font = new Font(this.chkServiceBullitenBoard.Font, FontStyle.Bold);
                  continue;
                case AdminDownloadRequest.RequestType.DLL:
                  this.chkNewRates.Font = new Font(this.chkNewRates.Font, FontStyle.Bold);
                  this.chkNewRates.Visible = true;
                  continue;
                case AdminDownloadRequest.RequestType.IATA:
                  this.chkIATATable.Text += this.notCurrentVersionText;
                  this.chkIATATable.Font = new Font(this.chkIATATable.Font, FontStyle.Bold);
                  continue;
                case AdminDownloadRequest.RequestType.HazMatTable:
                  this.chkHazMatTable.Text += this.notCurrentVersionText;
                  this.chkHazMatTable.Font = new Font(this.chkHazMatTable.Font, FontStyle.Bold);
                  continue;
                case AdminDownloadRequest.RequestType.HoldAtLocationTable:
                  this.chkHALTable.Text += this.notCurrentVersionText;
                  this.chkHALTable.Font = new Font(this.chkHALTable.Font, FontStyle.Bold);
                  continue;
                case AdminDownloadRequest.RequestType.CurrencyConversionTable:
                  this.chkCurrencyTable.Text += this.notCurrentVersionText;
                  this.chkCurrencyTable.Font = new Font(this.chkCurrencyTable.Font, FontStyle.Bold);
                  continue;
                default:
                  continue;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        FxLogger.LogMessage(FedEx.Gsm.Common.Logging.LogLevel.Error, FxLogger.AppCode_GUI, nameof (btnCheckUpdate_Click), "Exception: " + ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    public void CheckMaintenanceRelease(bool bCheck) => this.chkMaintenance.Checked = bCheck;

    private void InitializeComponent()
    {
      this.components = (IContainer) new System.ComponentModel.Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (DemandDownload));
      this.OptionsGroup = new ColorGroupBox();
      this.lnkSoftwareUpdatesDisclaimer = new LinkLabel();
      this.btnCheckUpdate = new Button();
      this.OptionsFlowLayout = new FlowLayoutPanel();
      this.chkSoftware = new CheckBox();
      this.chkURSATable = new CheckBox();
      this.chkMaintenance = new CheckBox();
      this.chkServiceBullitenBoard = new CheckBox();
      this.chkIATATable = new CheckBox();
      this.chkHazMatTable = new CheckBox();
      this.chkHALTable = new CheckBox();
      this.chkCountryList = new CheckBox();
      this.chkCurrencyTable = new CheckBox();
      this.chkNewRates = new CheckBox();
      this.RatesTrackingNumberTreeView = new DisablingTreeView();
      this.btnOK = new Button();
      this.btnSelectAll = new Button();
      this.btnUnselectAll = new Button();
      this.btnCancel = new Button();
      this.downloadWorker = new BackgroundWorker();
      this.OptionsGroup.SuspendLayout();
      this.OptionsFlowLayout.SuspendLayout();
      this.SuspendLayout();
      componentResourceManager.ApplyResources((object) this.OptionsGroup, "OptionsGroup");
      this.OptionsGroup.BorderThickness = 1f;
      this.OptionsGroup.Controls.Add((Control) this.lnkSoftwareUpdatesDisclaimer);
      this.OptionsGroup.Controls.Add((Control) this.btnCheckUpdate);
      this.OptionsGroup.Controls.Add((Control) this.OptionsFlowLayout);
      this.OptionsGroup.GroupTitleFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
      this.OptionsGroup.Name = "OptionsGroup";
      this.OptionsGroup.RoundCorners = 5;
      this.OptionsGroup.TabStop = false;
      componentResourceManager.ApplyResources((object) this.lnkSoftwareUpdatesDisclaimer, "lnkSoftwareUpdatesDisclaimer");
      this.lnkSoftwareUpdatesDisclaimer.Name = "lnkSoftwareUpdatesDisclaimer";
      this.lnkSoftwareUpdatesDisclaimer.TabStop = true;
      componentResourceManager.ApplyResources((object) this.btnCheckUpdate, "btnCheckUpdate");
      this.btnCheckUpdate.Name = "btnCheckUpdate";
      this.btnCheckUpdate.UseVisualStyleBackColor = true;
      this.btnCheckUpdate.Click += new EventHandler(this.btnCheckUpdate_Click);
      componentResourceManager.ApplyResources((object) this.OptionsFlowLayout, "OptionsFlowLayout");
      this.OptionsFlowLayout.Controls.Add((Control) this.chkSoftware);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkURSATable);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkMaintenance);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkServiceBullitenBoard);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkIATATable);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkHazMatTable);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkHALTable);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkCountryList);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkCurrencyTable);
      this.OptionsFlowLayout.Controls.Add((Control) this.chkNewRates);
      this.OptionsFlowLayout.Controls.Add((Control) this.RatesTrackingNumberTreeView);
      this.OptionsFlowLayout.Name = "OptionsFlowLayout";
      this.helpProvider1.SetShowHelp((Control) this.OptionsFlowLayout, (bool) componentResourceManager.GetObject("OptionsFlowLayout.ShowHelp"));
      this.helpProvider1.SetHelpKeyword((Control) this.chkSoftware, componentResourceManager.GetString("chkSoftware.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkSoftware, (HelpNavigator) componentResourceManager.GetObject("chkSoftware.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkSoftware, "chkSoftware");
      this.chkSoftware.Name = "chkSoftware";
      this.helpProvider1.SetShowHelp((Control) this.chkSoftware, (bool) componentResourceManager.GetObject("chkSoftware.ShowHelp"));
      this.chkSoftware.UseVisualStyleBackColor = true;
      this.chkSoftware.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkURSATable, componentResourceManager.GetString("chkURSATable.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkURSATable, (HelpNavigator) componentResourceManager.GetObject("chkURSATable.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkURSATable, "chkURSATable");
      this.chkURSATable.Name = "chkURSATable";
      this.helpProvider1.SetShowHelp((Control) this.chkURSATable, (bool) componentResourceManager.GetObject("chkURSATable.ShowHelp"));
      this.chkURSATable.Tag = (object) "Express URSA Table";
      this.chkURSATable.UseVisualStyleBackColor = true;
      this.chkURSATable.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkMaintenance, componentResourceManager.GetString("chkMaintenance.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkMaintenance, (HelpNavigator) componentResourceManager.GetObject("chkMaintenance.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkMaintenance, "chkMaintenance");
      this.chkMaintenance.Name = "chkMaintenance";
      this.helpProvider1.SetShowHelp((Control) this.chkMaintenance, (bool) componentResourceManager.GetObject("chkMaintenance.ShowHelp"));
      this.chkMaintenance.UseVisualStyleBackColor = true;
      this.chkMaintenance.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkServiceBullitenBoard, componentResourceManager.GetString("chkServiceBullitenBoard.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkServiceBullitenBoard, (HelpNavigator) componentResourceManager.GetObject("chkServiceBullitenBoard.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkServiceBullitenBoard, "chkServiceBullitenBoard");
      this.chkServiceBullitenBoard.Name = "chkServiceBullitenBoard";
      this.helpProvider1.SetShowHelp((Control) this.chkServiceBullitenBoard, (bool) componentResourceManager.GetObject("chkServiceBullitenBoard.ShowHelp"));
      this.chkServiceBullitenBoard.Tag = (object) "Service Bulletin Board";
      this.chkServiceBullitenBoard.UseVisualStyleBackColor = true;
      this.chkServiceBullitenBoard.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkIATATable, componentResourceManager.GetString("chkIATATable.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkIATATable, (HelpNavigator) componentResourceManager.GetObject("chkIATATable.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkIATATable, "chkIATATable");
      this.chkIATATable.Name = "chkIATATable";
      this.helpProvider1.SetShowHelp((Control) this.chkIATATable, (bool) componentResourceManager.GetObject("chkIATATable.ShowHelp"));
      this.chkIATATable.Tag = (object) "IATA Table";
      this.chkIATATable.UseVisualStyleBackColor = true;
      this.chkIATATable.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkHazMatTable, componentResourceManager.GetString("chkHazMatTable.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkHazMatTable, (HelpNavigator) componentResourceManager.GetObject("chkHazMatTable.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkHazMatTable, "chkHazMatTable");
      this.chkHazMatTable.Name = "chkHazMatTable";
      this.helpProvider1.SetShowHelp((Control) this.chkHazMatTable, (bool) componentResourceManager.GetObject("chkHazMatTable.ShowHelp"));
      this.chkHazMatTable.Tag = (object) "HazMat Table";
      this.chkHazMatTable.UseVisualStyleBackColor = true;
      this.chkHazMatTable.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkHALTable, componentResourceManager.GetString("chkHALTable.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkHALTable, (HelpNavigator) componentResourceManager.GetObject("chkHALTable.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkHALTable, "chkHALTable");
      this.chkHALTable.Name = "chkHALTable";
      this.helpProvider1.SetShowHelp((Control) this.chkHALTable, (bool) componentResourceManager.GetObject("chkHALTable.ShowHelp"));
      this.chkHALTable.Tag = (object) "Hold at Location Table";
      this.chkHALTable.UseVisualStyleBackColor = true;
      this.chkHALTable.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkCountryList, componentResourceManager.GetString("chkCountryList.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkCountryList, (HelpNavigator) componentResourceManager.GetObject("chkCountryList.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkCountryList, "chkCountryList");
      this.chkCountryList.Name = "chkCountryList";
      this.helpProvider1.SetShowHelp((Control) this.chkCountryList, (bool) componentResourceManager.GetObject("chkCountryList.ShowHelp"));
      this.chkCountryList.Tag = (object) "";
      this.chkCountryList.UseVisualStyleBackColor = true;
      this.chkCountryList.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      this.helpProvider1.SetHelpKeyword((Control) this.chkCurrencyTable, componentResourceManager.GetString("chkCurrencyTable.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this.chkCurrencyTable, (HelpNavigator) componentResourceManager.GetObject("chkCurrencyTable.HelpNavigator"));
      componentResourceManager.ApplyResources((object) this.chkCurrencyTable, "chkCurrencyTable");
      this.chkCurrencyTable.Name = "chkCurrencyTable";
      this.helpProvider1.SetShowHelp((Control) this.chkCurrencyTable, (bool) componentResourceManager.GetObject("chkCurrencyTable.ShowHelp"));
      this.chkCurrencyTable.Tag = (object) "Currency Conversion Table";
      this.chkCurrencyTable.UseVisualStyleBackColor = true;
      this.chkCurrencyTable.CheckedChanged += new EventHandler(this.Checkbox_CheckedChanged);
      componentResourceManager.ApplyResources((object) this.chkNewRates, "chkNewRates");
      this.chkNewRates.Name = "chkNewRates";
      this.helpProvider1.SetShowHelp((Control) this.chkNewRates, (bool) componentResourceManager.GetObject("chkNewRates.ShowHelp"));
      this.chkNewRates.Tag = (object) "New Rates Available - Please Download";
      this.chkNewRates.UseVisualStyleBackColor = true;
      this.RatesTrackingNumberTreeView.BackColor = SystemColors.Control;
      this.RatesTrackingNumberTreeView.BorderStyle = BorderStyle.None;
      this.RatesTrackingNumberTreeView.CheckBoxes = true;
      componentResourceManager.ApplyResources((object) this.RatesTrackingNumberTreeView, "RatesTrackingNumberTreeView");
      this.RatesTrackingNumberTreeView.Name = "RatesTrackingNumberTreeView";
      this.RatesTrackingNumberTreeView.Nodes.AddRange(new TreeNode[2]
      {
        (TreeNode) componentResourceManager.GetObject("RatesTrackingNumberTreeView.Nodes"),
        (TreeNode) componentResourceManager.GetObject("RatesTrackingNumberTreeView.Nodes1")
      });
      this.helpProvider1.SetShowHelp((Control) this.RatesTrackingNumberTreeView, (bool) componentResourceManager.GetObject("RatesTrackingNumberTreeView.ShowHelp"));
      this.RatesTrackingNumberTreeView.ShowLines = false;
      this.RatesTrackingNumberTreeView.AfterCheck += new TreeViewEventHandler(this.RatesTreeView_AfterCheck);
      this.RatesTrackingNumberTreeView.Click += new EventHandler(this.RatesTreeView_Click);
      componentResourceManager.ApplyResources((object) this.btnOK, "btnOK");
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      componentResourceManager.ApplyResources((object) this.btnSelectAll, "btnSelectAll");
      this.btnSelectAll.Name = "btnSelectAll";
      this.btnSelectAll.UseVisualStyleBackColor = true;
      this.btnSelectAll.Click += new EventHandler(this.btnSelectAll_Click);
      componentResourceManager.ApplyResources((object) this.btnUnselectAll, "btnUnselectAll");
      this.btnUnselectAll.Name = "btnUnselectAll";
      this.btnUnselectAll.UseVisualStyleBackColor = true;
      this.btnUnselectAll.Click += new EventHandler(this.btnUnselectAll_Click);
      componentResourceManager.ApplyResources((object) this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.downloadWorker.WorkerReportsProgress = true;
      this.AcceptButton = (IButtonControl) this.btnOK;
      componentResourceManager.ApplyResources((object) this, "$this");
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnUnselectAll);
      this.Controls.Add((Control) this.btnSelectAll);
      this.Controls.Add((Control) this.btnOK);
      this.Controls.Add((Control) this.OptionsGroup);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.HelpButton = false;
      this.helpProvider1.SetHelpKeyword((Control) this, componentResourceManager.GetString("$this.HelpKeyword"));
      this.helpProvider1.SetHelpNavigator((Control) this, (HelpNavigator) componentResourceManager.GetObject("$this.HelpNavigator"));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (DemandDownload);
      this.helpProvider1.SetShowHelp((Control) this, (bool) componentResourceManager.GetObject("$this.ShowHelp"));
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.OptionsGroup.ResumeLayout(false);
      this.OptionsFlowLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    private class DownloadData
    {
      public DemandDownload DownloadForm;
      public bool RunInstalls;
      public DownloadFinishedEventArgs FinishArgs;
      public List<AdminDownloadRequest> DownloadRequest;
    }
  }
}
