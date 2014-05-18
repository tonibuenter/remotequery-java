//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Org.JGround.Codetable;
using Org.JGround.HWT.Components;
using Org.JGround.HWT.MOM.Docu;
using Org.JGround.MOM;
using Org.JGround.MOM.DB;
using Org.JGround.Util;

namespace Org.JGround.HWT.MOM {

    public class UIReportOverviewPanel : DIV {

        private static Logger logger = Logger.GetLogger(typeof(UIReportOverviewPanel));
        private static HContainer hEmpty = new HContainer();

        //

        private UIFrame uiFrame;
        private Dictionary<String, UIReportWindow> reportWindows;

        public UIReportOverviewPanel(UIFrame uiFrame) {
            this.uiFrame = uiFrame;
            reportWindows = new Dictionary<String, UIReportWindow>();
            H1 titleH1 = new H1("Reports");
            titleH1.SetStyleClass(UIStyles.GetInstance().GetTopLevelTitleStyle(null));
            Add(titleH1);
            Add(new ReportTable(this));
            logger.Debug("UIReportOverviewPanel created");
        }

        public UIReportWindow GetReportWindow(String mrid) {
            //UIReportWindow reportWindow = null;
            //if(!reportWindows.TryGetValue(mrid, out reportWindow)) {
            //    reportWindow = new UIReportWindow(uiFrame, this, mrid);
            //    reportWindows.Put(mrid, reportWindow);
            //}
            //return reportWindow;
            UIReportWindow reportWindow = new UIReportWindow(uiFrame, this, mrid);
            return reportWindow;
        }

        private class ReportTable : TABLE, IHListener {
            private UIReportOverviewPanel o;
            private TR row = new TR();
            private HText nameTx;
            private HLink viewBt;
            private DIV buttonDIV;

            public ReportTable(UIReportOverviewPanel outer) {
                this.o = outer;
                nameTx = new HText();
                viewBt = new HLink(outer.uiFrame, "View Report");
                //
                row.Add(new TD(new DIV(UIStyles.SYNOPSIS, new DIV(UIStyles.UI_CONTROL, new DIV(UIStyles.UI_VALUE, nameTx)))),
                        new TD(buttonDIV = new DIV(HStyles.S100, viewBt)));
                //
                viewBt.AddHListener(this);
            }
            public override int Count() {
                return MOService.GetInstance().GetAllMOReports().Count;
            }

            public override HComponent Get(int index) {
                MOReport moReport = MOService.GetInstance().GetAllMOReports().Get(index);
                nameTx.SetText(moReport.GetName());
                List<MOClass> moClasses = moReport.GetSourceMOClasses();
                MOClass[] moClassesArr = moClasses.ToArray();
                if(MOAccess.GetInstance().CanRead(moClassesArr) && MOAccess.GetInstance().CanRead(moReport)) {
                    buttonDIV.Set(viewBt);
                } else {
                    return hEmpty;
                    //buttonDIV.RemoveAll();
                }
                viewBt.SetSubId(moReport.GetMrid());
                return row;
            }

            public void Arrived(HEvent he) {
                String mrid = viewBt.GetSubId();
                o.uiFrame.SetMainPanel(o.GetReportWindow(mrid));
            }
        }
    }



    public class UIReportWindow : DIV, IHListener {
        //
        //
        //
        private static Logger logger = Logger.GetLogger(typeof(UIReportWindow));
        //
        //
        //
        private UIFrame uiFrame;
        private H1 titleH1;
        private IUIReportPanel uiReportPanel;
        private HLink updateBt;
        private HLink resetBt;
        private HLink closeBt;
        private HLink createReportBt;
        private HLink printBt;
        private UIReportOverviewPanel overviewPanel;
        private HTag docuLink;
        //
        private PrintWindow printPanel;
        //
        private DIV dataDiv = new DIV();

        private static IUIReportPanel Create(UIFrame uiFrame, MOReport moReport) {
            if(moReport.IsProgram()) {
                String t = moReport.GetReportProgramClass();
                System.Type oType = System.Type.GetType(t, true);
                return (IUIReportPanel)System.Activator.CreateInstance(oType, uiFrame);
            } else {
                return new UIReportPanel(uiFrame, moReport);
            }
        }

        public UIReportWindow(UIFrame uiFrame, UIReportOverviewPanel overviewPanel, String mrid) {
            this.uiFrame = uiFrame;
            this.overviewPanel = overviewPanel;
            MOReport moReport = MOService.GetInstance().GetMOReport(mrid);
            //
            // COMPONENTS
            //
            titleH1 = new H1(moReport.GetName());
            titleH1.SetStyleClass(UIStyles.GetInstance().GetTopLevelTitleStyle(null));

            DIV descriptionDIV = new DIV(moReport.GetDescription(), UIStyles.GROUP2);

            uiReportPanel = Create(uiFrame, moReport);
            printPanel = new PrintWindow(uiFrame);
            updateBt = new HLink(uiFrame, "Update Report");
            createReportBt = new HLink(uiFrame, "Create Excel Report");
            printBt = new HLink(uiFrame, "Print...");
            resetBt = new HLink(uiFrame, "Reset Report");
            closeBt = new HLink(uiFrame, "Close Report");
            DIV buttonDIV = new DIV();
            buttonDIV.SetStyleClass(UIStyles.GROUP);
            //
            // LAYOUT
            //
            buttonDIV.Add(updateBt);
            buttonDIV.Add(resetBt);
            if(this.uiReportPanel.IsPrintEnabled()) {
                buttonDIV.Add(printBt);
            }
            buttonDIV.Add(createReportBt);
            buttonDIV.Add(closeBt);
            this.Add(titleH1);
            this.Add(descriptionDIV);
            this.Add(buttonDIV);
            this.Add(dataDiv);
            dataDiv.Set(uiReportPanel.GetView());
            //
            // EVENT HANDLING
            //
            updateBt.AddHListener(this);
            printBt.AddHListener(this);
            createReportBt.AddHListener(this);
            resetBt.AddHListener(this);
            closeBt.AddHListener(this);
        }

        public HComponent GetView() {
            return this;
        }

        public void Arrived(HEvent he) {
            if(he.GetSource() == updateBt) {
                dataDiv.Set(uiReportPanel.GetView());
                uiReportPanel.Update();
            }
            if(he.GetSource() == printBt) {
                printPanel.SetPrintPart(uiReportPanel.GetPrintPart());
            }
            if(he.GetSource() == createReportBt) {
                String fileName = MOSystem.GetUserName() + "-" + DateTimeUtils.NowInMillis() + ".xls";
                fileName = PathUtils.CreateSaveFileName(fileName);
                String filePath = DocuServlet.FindReportFileName(fileName);
                logger.Debug("filePath: " + filePath);
                uiReportPanel.WriteReportToFile(filePath);
                UpdateDocuLink(fileName);
                dataDiv.Set(new DIV(docuLink, UIStyles.MO_NOWRAP2));
            }
            if(he.GetSource() == resetBt) {
                uiReportPanel.Reset();
                dataDiv.Set(uiReportPanel.GetView());
            }
            if(he.GetSource() == closeBt) {
                dataDiv.Set(uiReportPanel.GetView());
                uiReportPanel.Close();
                uiFrame.SetMainPanel(overviewPanel);
            }
        }

        private void UpdateDocuLink(String fileName) {
            docuLink = new HTag(HDTD.Element.A);
            //docuLink.SetAttribute(HDTD.AttName.TARGET, HDTD.AttValue._BLANK);
            String ext = Path.GetExtension(fileName);
            //old String linkValue = DocuServlet.CreateDocuLink(GetDataContext().GetOid(), uiFrame.GetHttpServerUtility().UrlEncode(fileName));
            String linkValue = DocuServlet.CreateReportLink(fileName);
            //, uiFrame.GetHttpServerUtility().UrlEncode(StringUtils.toLetters(fileName)));
            //linkValue = uiFrame.GetHttpServerUtility().UrlPathEncode(linkValue);
            docuLink.SetAttribute(HDTD.AttName.HREF, linkValue);
            uiFrame.GetHWTEvent().GetSession().Add(DocuServlet.SESSION_ID_PREFIX, fileName);


            //docuLink.SetText(fileName);
            docuLink.Add(new IMG(UIStyles.EXCEL_ICON_SRC));
            docuLink.Add(NBSP.NBSP);
            docuLink.Add(new HText(fileName));

        }

    }

    interface IUIReportPanel {
        HComponent GetView();
        HComponent GetPrintPart();
        void Reset();
        void WriteReportToFile(String filePath);
        void Update();
        void Close();
        bool IsPrintEnabled();

    }

    public class UIReportPanel : HTabbedPane, IUIReportPanel {

        private static Logger logger = Logger.GetLogger(typeof(UIReportPanel));



        private UIFrame uiFrame;
        private MOReport moReport;
        private MOSearchCriteria searchCriteria;
        private List<MODataObject> filteredMods = new List<MODataObject>();
        private List<UIReportColumn> uiColumns = new List<UIReportColumn>();
        //
        private DataReportPage dataReportPage;

        public UIReportPanel(UIFrame uiFrame, MOReport moReport)
            : base(uiFrame) {
            this.moReport = moReport;
            logger.Debug("Start Init of UIReportPanel", this.moReport.GetMrid());
            this.uiFrame = uiFrame;
            //
            foreach(MOReportPage moReportPage in moReport.GetMOReportPages()) {
                UIReportPage uiReportPage = new UIReportPage(uiFrame, this, moReportPage);
                AddTab(uiReportPage, moReportPage.GetLabel());
                uiColumns.AddUnique(uiReportPage.GetColumns());
            }
            Reset();

            dataReportPage = new DataReportPage(this.uiFrame, this.uiColumns);
        }

        public bool IsPrintEnabled() {
            return false;
        }

        public void WriteReportToFile(String fileName) {
            Update();
            dataReportPage.WriteReport(fileName, this.filteredMods);
        }

        private void InitSearchCriteria() {
            searchCriteria = new MOSearchCriteria();
            searchCriteria.SetIncludedDataStates(DataState.APPROVED, DataState.STORED);
            searchCriteria.AddIncludedMoids(moReport.GetSourceMoids()[0]);
        }

        public void Reset() {
            InitSearchCriteria();
            filteredMods = MODataObject.Search(searchCriteria);
            foreach(UIReportColumn uiColumn in uiColumns) {
                List<String[]> valuesArray = new List<String[]>();
                uiColumn.UpdateFilter(filteredMods);
            }
        }

        public UIReportPanel ToFirstTab() {
            this.ShowTab(0);
            return this;
        }

        public HComponent GetView() {
            return this;
        }

        public void Close() {
            Reset();
        }

        public void Update() {
            InitSearchCriteria();
            foreach(UIReportColumn uiColumn in uiColumns) {
                uiColumn.UpdateSearchCriteria(searchCriteria);
            }
            filteredMods = MODataObject.Search(searchCriteria);
        }


        public List<MODataObject> GetFilteredData() {
            return filteredMods;
        }

        public HComponent GetPrintPart() {
            UIReportPage up = (UIReportPage)this.GetCurrentTab();
            return up;
        }

    }


    public class DataReportPage {

        private List<UIReportColumn> uiColumns;

        public DataReportPage(UIFrame uiFrame, List<UIReportColumn> uiColumns) {
            this.uiColumns = uiColumns;
        }

        public void WriteReport(String fileName, List<MODataObject> mods) {
            StreamWriter w = null;
            Encoding we = Encoding.Default;
            using(w = new StreamWriter(File.Open(fileName, FileMode.Create), we)) {
                foreach(UIReportColumn uiColumn in uiColumns) {
                    w.WriteExcel(uiColumn.GetHeaderLabel());
                    w.Write("\t");
                }
                w.WriteLine();
                foreach(MODataObject mod in mods) {
                    foreach(UIReportColumn uiColumn in uiColumns) {
                        IEnumerable<String> values = uiColumn.RenderToString(mod, false);
                        w.WriteExcel(IEnumerableUtils.Join(values, ","));
                        w.Write("\t");
                    }
                    w.WriteLine();
                }
            }

        }

    }

    public class UIReportPage : TABLE {

        private UIReportPanel uiReportPanel;
        private MOReportPage moReportPage;

        private UIReportRow reportRow;
        private List<UIReportColumn> uiColumns = new List<UIReportColumn>();

        public UIReportPage(UIFrame uiFrame, UIReportPanel uiReportPanel, MOReportPage moReportPage) {
            this.uiReportPanel = uiReportPanel;
            this.moReportPage = moReportPage;
            //
            SetStyleClass(UIStyles.REPORT_TABLE);
            SetAttribute(HDTD.AttName.CELLPADDING, 0);
            SetAttribute(HDTD.AttName.CELLSPACING, 0);
            SetAttribute(HDTD.AttName.BORDER, 0);
            //
            //String sourceMoid = moReportPage.GetMOReport().GetSourceMoids()[0];
            foreach(MOColumn moColumn in moReportPage.GetMOColumns()) {
                MOAttribute moAttribute = moColumn.GetMOAttribute();
                UIReportColumn uiColumn = new UIReportColumn(uiFrame, uiReportPanel, moColumn);
                this.uiColumns.Add(uiColumn);
            }
            //
            reportRow = new UIReportRow(uiFrame, this.uiColumns, this.uiReportPanel);
        }

        public List<UIReportColumn> GetColumns() {
            return this.uiColumns;
        }

        public override int Count() {
            return 2 + uiReportPanel.GetFilteredData().Count;
        }

        public override HComponent Get(int index) {
            int elmentIndex = index - 2;
            switch(index) {
                case 0:
                    reportRow.SetRenderFilter();
                    return reportRow;
                case 1:
                    reportRow.SetRenderHeader();
                    return reportRow;
                default:
                    reportRow.SetRenderValues(uiReportPanel.GetFilteredData()[elmentIndex], elmentIndex);
                    return reportRow;
            }
        }
    }

    public class UIReportRow : TR, IHListener {

        private static readonly TD emptyTD = new TD();

        private bool renderHeader;
        private bool renderFilter;
        private TD td = new TD();
        private MODataObject mod;
        private int rowIndex;
        private UIReportPanel uiReportPanel;
        private List<UIReportColumn> uiColumns;
        private TD viewButtonTD;
        private HLink viewBt;
        private UIFrame uiFrame;

        public UIReportRow(UIFrame uiFrame, List<UIReportColumn> columns, UIReportPanel uiReportPanel) {
            this.uiFrame = uiFrame;
            this.uiReportPanel = uiReportPanel;
            this.uiColumns = columns;
            this.viewBt = (HLink)new HLink(uiFrame, "View").SetStyleClass(UIStyles.REPORT_VIEWLINK);
            this.viewButtonTD = (TD)new TD(new DIV(viewBt, UIStyles.REPORT_VIEWLINK)).SetStyleClass(UIStyles.REPORT_VIEWLINK);
            viewButtonTD.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            //
            viewBt.AddHListener(this);
        }

        public void SetRenderHeader() {
            this.renderHeader = true;
            this.renderFilter = false;
            td.SetStyleClass(UIStyles.REPORT_CELL_HEADER);
        }

        public void SetRenderFilter() {
            this.renderHeader = false;
            this.renderFilter = true;
            td.SetStyleClass(UIStyles.REPORT_CELL_FILTER);
        }

        public void SetRenderValues(MODataObject mod, int rowIndex) {
            this.renderHeader = false;
            this.renderFilter = false;
            if(rowIndex % 2 == 0) {
                td.SetStyleClass(UIStyles.REPORT_CELL_0);
            } else {
                td.SetStyleClass(UIStyles.REPORT_CELL_1);
            }
            this.mod = mod;
            this.rowIndex = rowIndex;
        }

        public override int Count() {
            return uiColumns.Count + 1;
        }

        public override HComponent Get(int index) {
            if(index == 0) {
                if(renderHeader || renderFilter || mod == null) {
                    return emptyTD;
                }

                viewBt.SetSubId(mod.GetOid().ToString());
                return viewButtonTD;
            }
            //
            index--;
            if(renderHeader) {
                return td.Set(uiColumns.Get(index).GetHeaderComponent());
            } else if(renderFilter) {
                if(uiColumns.Get(index).HasFilterControl()) {
                    return td.Set(uiColumns.Get(index).GetFilterControl().GetUI());
                } else {
                    return td.Set(NBSP.NBSP);
                }
            } else {
                return td.Set(uiColumns.Get(index).GetValueComponent(mod, rowIndex));
            }
        }

        public void Arrived(HEvent he) {
            String oid = viewBt.GetSubId();
            MODataObject mod = MODataObject.GetById(oid);
            if(mod != null)
                uiFrame.OpenDialogOnStack(new UIViewWindowDelegator(uiFrame.GetViewWindow(mod.GetMoid()), mod));
        }
    }


    public class UIReportColumn : IHListener {

        private MOColumn moColumn;
        private String headerLabel;
        private DIV headerDIV;
        private DIV valueDIV;
        private IUIAttributeView uiAttributeView;
        private HLink sortLink;
        private bool isUp;
        private UIReportPanel uiReportPanel;
        private List<MODataObject> dataList;
        private List<Sortable> sortList = new List<Sortable>();
        private IUIReportColumnFilter uiFilterControl;
        private UIFrame uiFrame;

        public UIReportColumn(UIFrame uiFrame, UIReportPanel uiReportPanel, MOColumn moColumn) {
            this.uiFrame = uiFrame;
            this.uiReportPanel = uiReportPanel;
            this.moColumn = moColumn;
            uiAttributeView = UIAttributeViewControlFactory.Create(uiFrame, moColumn);
            headerLabel = moColumn.GetMOAttribute().GetLabel();
            headerDIV = new DIV(headerLabel, UIStyles.REPORT_CELL_HEADER);
            valueDIV = new DIV();
            if(moColumn.IsSortingEnabled()) {
                sortLink = new HLink(uiFrame, headerLabel);
                sortLink.SetStyleClass(UIStyles.REPORT_CELL_HEADER);
                headerDIV.Set(sortLink);
                sortLink.AddHListener(this);
            }
            if(moColumn.HasUIHint(UIHints.WIDTH)) {
                String width = moColumn.GetUIHintValue(UIHints.WIDTH);
                headerDIV.SetAttribute("style", "width: " + width);
            }

            uiFilterControl = UIReportColumnFilterFactory.Create(uiFrame, this);

            //if(moColumn.GetFilterType()!=null) {
            //    uiFilterControl = new UIReportFilterControl(uiFrame, this);
            //}
        }

        public MOColumn GetMOColumn() {
            return moColumn;
        }

        public DIV GetHeaderComponent() {
            return headerDIV;
        }

        public String GetHeaderLabel() {
            return this.headerLabel;
        }

        public HComponent GetValueComponent(MODataObject mod, int rowIndex) {
            IEnumerable<String> renderValues = RenderToString(mod, true);
            valueDIV.RemoveAll();
            foreach(String renderValue in renderValues) {
                valueDIV.Add(new DIV(StringUtils.CutAndEllipsis(renderValue, 64)));
            }
            if(valueDIV.Count() == 0) {
                valueDIV.Add(NBSP.NBSP);
            }
            return valueDIV;
        }

        public IEnumerable<String> RenderToString(MODataObject mod, bool cutWithMaxCols) {
            int maxCols = moColumn.GetMaxCols();
            String[] values = mod.GetCurrentValues(moColumn.GetAttRef());
            uiAttributeView.SetDataContext(mod);
            uiAttributeView.Set(values);

            IEnumerable<String> renderValues = uiAttributeView.RenderToString();
            if(cutWithMaxCols) {
                foreach(String renderValue in renderValues) {
                    yield return StringUtils.CutAndEllipsis(renderValue, maxCols);
                }
            } else {
                foreach(String renderValue in renderValues) {
                    yield return renderValue;
                }
            }
        }

        public bool HasFilterControl() {
            return GetFilterControl() != null;
        }

        public IUIReportColumnFilter GetFilterControl() {
            return uiFilterControl;
        }

        public void Arrived(HEvent he) {
            if(he.GetSource() == sortLink) {
                isUp = !isUp;
                UpdateLists();
            }
        }


        public bool IsUp() {
            return isUp;
        }
        private void UpdateLists() {
            if(ObjectUtils.AreNotEquals(uiReportPanel.GetFilteredData(), dataList)) {
                dataList = uiReportPanel.GetFilteredData();
                sortList = new List<Sortable>();
                foreach(MODataObject mod in dataList) {
                    String compareValue = IEnumerableUtils.Join(RenderToString(mod, false));
                    sortList.Add(new Sortable(mod, StringUtils.RNN(compareValue), this));
                }
            }
            sortList.Sort();
            dataList.Clear();
            foreach(Sortable s in sortList) {
                dataList.Add(s.GetData());
            }


        }
        internal class Sortable : IComparable<Sortable> {

            private IComparable value;
            private MODataObject mod;
            private UIReportColumn uiColumn;

            internal Sortable(MODataObject mod, IComparable value, UIReportColumn uiColumn) {
                this.value = value;
                this.mod = mod;
                this.uiColumn = uiColumn;
            }

            public int CompareTo(Sortable obj) {
                return value.CompareTo(obj.value) * (uiColumn.IsUp() ? 1 : -1);
            }

            public MODataObject GetData() {
                return mod;
            }
        }


        public void UpdateSearchCriteria(MOSearchCriteria searchCriteria) {
            if(HasFilterControl()) {
                GetFilterControl().UpdateSearchCriteria(searchCriteria);
            }

        }

        public void UpdateFilter(List<MODataObject> filteredMods) {
            if(uiFilterControl != null) {
                uiFilterControl.SetFoundObjects(filteredMods);
            }
        }
    }


    public interface IUIReportColumnFilter {
        HComponent GetUI();
        void UpdateSearchCriteria(MOSearchCriteria searchCriteria);
        void SetFoundObjects(List<MODataObject> filteredMods);
    }

    public static class UIReportColumnFilterFactory {

        public static IUIReportColumnFilter Create(UIFrame uiFrame, UIReportColumn uiColumn) {
            String filterType = uiColumn.GetMOColumn().GetFilterType();
            if(filterType.Equals(MOColumn.FT_SELECT)) {
                return new UIReportColumnFilterSelect(uiFrame, uiColumn);
            }
            if(filterType.Equals(MOColumn.FT_STARTSWITH)) {
                return new UIReportColumnFilterText(uiFrame, uiColumn);
            }
            if(filterType.Equals(MOColumn.FT_CONTAINS)) {
                return new UIReportColumnFilterText(uiFrame, uiColumn);
            }
            if(filterType.Equals(MOColumn.FT_YEAR)) {
                if(uiColumn.GetMOColumn().GetMOAttribute().GetMOType() is MOTypeDateTime) {
                    return new UIReportColumnFilterSelect(uiFrame, uiColumn, 4);
                }
            }
            return null;
        }

    }

    public class UIReportColumnFilterSelect : DIV, IUIReportColumnFilter {

        private static Logger logger = Logger.GetLogger(typeof(UIReportColumnFilterSelect));

        private static CCodeTableElement<String[]> EMPTY_CTE = new CCodeTableElement<String[]>("", "-", null);

        //

        private UIReportColumn uiColumn;
        private HList<CCodeTableElement<String[]>> filterValuesHList;
        private List<CCodeTableElement<String[]>> filterValuesCT;
        private String attref;
        private int startsWithLength;

        public UIReportColumnFilterSelect(UIFrame uiFrame, UIReportColumn uiColumn) {
            this.uiColumn = uiColumn;
            this.startsWithLength = -1;
            filterValuesCT = new List<CCodeTableElement<String[]>>();
            filterValuesHList = new HList<CCodeTableElement<String[]>>(uiFrame, filterValuesCT, false, 1);
            attref = uiColumn.GetMOColumn().GetAttRef();
            Add(filterValuesHList);
        }

        public UIReportColumnFilterSelect(UIFrame uiFrame, UIReportColumn uiColumn, int startsWithLength) {
            this.uiColumn = uiColumn;
            this.startsWithLength = startsWithLength;
            filterValuesCT = new List<CCodeTableElement<String[]>>();
            filterValuesHList = new HList<CCodeTableElement<String[]>>(uiFrame, filterValuesCT, false, 1);
            attref = uiColumn.GetMOColumn().GetAttRef();
            Add(filterValuesHList);
        }

        public HComponent GetUI() {
            return this;
        }

        public void UpdateSearchCriteria(MOSearchCriteria searchCriteria) {
            String[] filterSelectedValues = GetSelectedValues();
            if(ArrayUtils.IsEmpty(filterSelectedValues)) {
                return;
            }
            if(ArrayUtils.IsNotEmpty(filterSelectedValues) && startsWithLength == -1) {
                searchCriteria.AddExactMatches(attref, filterSelectedValues);
            } else {
                String value = filterSelectedValues[0];
                if(value.Length >= startsWithLength) {
                    searchCriteria.AddStartsWithMatches(attref, value.Substring(0, 4));
                }
            }
        }

        public void SetFoundObjects(List<MODataObject> filteredMods) {
            Reset();
            if(startsWithLength == -1) {
                List<String[]> valuesArray = new List<String[]>();
                List<MODataObject> mods = new List<MODataObject>();
                foreach(MODataObject mod in filteredMods) {
                    if(valuesArray.AddUnique(mod.GetCurrentValues(attref))) {
                        mods.Add(mod);
                    }
                }
                SetSelectionValues(mods, valuesArray.ToArray());
            } else {
                List<String> valuesArray = new List<String>();
                foreach(MODataObject mod in filteredMods) {
                    String value = mod.GetCurrentValue(attref);
                    if(value != null && value.Length >= startsWithLength) {
                        valuesArray.AddUnique(value.Substring(0, startsWithLength));
                    }
                }
                valuesArray.Sort();
                SetSelectionValues(valuesArray.ToArray());
            }
        }

        public void Reset() {
            filterValuesHList.SetSelected(EMPTY_CTE);
        }

        private String[] GetSelectedValues() {
            IList<CCodeTableElement<String[]>> selected = filterValuesHList.GetSelected();
            if(ICollectionUtils.IsNotEmpty(selected)) {
                return selected[0].GetContextObject();
            }
            return null;
        }

        //private void ClearFilterValues() {
        //    SetFilterValues(null);
        //}

        private void SetSelectionValues(List<MODataObject> mods, String[][] filterValues) {
            filterValuesCT.Clear();
            filterValuesCT.Add(EMPTY_CTE);
            if(filterValues == null) {
                return;
            }
            int index = 0;
            foreach(String[] values in filterValues) {
                MODataObject mod = mods.Get(index);
                String code = ArrayUtils.Join(values);
                String name = IEnumerableUtils.Join(uiColumn.RenderToString(mod, true));
                try {
                    filterValuesCT.AddUnique(new CCodeTableElement<String[]>(code, name, values));
                }
                catch(Exception e) {
                    logger.Error(code, name, values, e);
                }
                index++;
            }
        }

        private void SetSelectionValues(String[] filterValues) {
            filterValuesCT.Clear();
            filterValuesCT.Add(EMPTY_CTE);
            if(filterValues == null) {
                return;
            }
            foreach(String value in filterValues) {
                String code = value;
                String name = value;
                try {
                    filterValuesCT.AddUnique(new CCodeTableElement<String[]>(code, name, new String[] { value }));
                }
                catch(Exception e) {
                    logger.Error(code, name, value, e);
                }
            }
        }

    }


    public class UIReportColumnFilterText : DIV, IUIReportColumnFilter {

        private static Logger logger = Logger.GetLogger(typeof(UIReportColumnFilterText));

        private UIReportColumn uiColumn;
        private HTextField filterTf;
        private String attref;


        public UIReportColumnFilterText(UIFrame uiFrame, UIReportColumn uiColumn) {
            filterTf = new HTextField(uiFrame);
            this.uiColumn = uiColumn;
            attref = uiColumn.GetMOColumn().GetAttRef();
            Add(filterTf);
        }

        public HComponent GetUI() {
            return this;
        }

        public void UpdateSearchCriteria(MOSearchCriteria searchCriteria) {
            String searchValue = filterTf.GetText();
            if(StringUtils.IsNotEmpty(searchValue)) {
                if(MOColumn.FT_STARTSWITH.Equals(uiColumn.GetMOColumn().GetFilterType())) {
                    searchCriteria.AddStartsWithMatches(attref, searchValue);
                } else {
                    searchCriteria.AddContainsMatches(attref, searchValue, true);
                }
            }
        }

        public void SetFoundObjects(List<MODataObject> filteredMods) { Reset(); }

        public void Reset() {
            filterTf.SetText("");
        }

        public void SetFilterValues(String[][] filterValues) { }


    }


    public class PrintWindow : DIV, IHListener, IMainPanel {

        private UIFrame uiFrame;
        private HLink closeBt;
        private DIV buttonDiv;

        public PrintWindow(UIFrame uiFrame) {
            this.uiFrame = uiFrame;
            closeBt = new HLink(uiFrame, "Close Print View");
            buttonDiv = new DIV(UIStyles.GROUP, closeBt);
            //
            closeBt.AddHListener(this);
        }

        public void SetPrintPart(HComponent part) {
            this.Set(buttonDiv, new DIV(part));
            uiFrame.OpenDialogOnStack(this);
        }

        public void Arrived(HEvent he) {
            uiFrame.CloseDialogOnStack();
        }

        public HComponent GetView() {
            return this;
        }

        public void BeforeClose() {
        }

        public void BeforeShow() {
        }

    }

}