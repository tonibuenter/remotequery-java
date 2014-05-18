//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.JGround.MOM;
using Org.JGround.Util;


namespace Org.JGround.HWT.MOM {

    //public class UISearchPanel : DIV, IHListener,
    //     IMainPanel {
    //    //
    //    // CLASS LEVEL
    //    //
    //    private static readonly Logger logger = Logger.GetLogger(typeof(UISearchPanel));
    //    public static int PAGE_SIZE = 50;
    //    //
    //    // OBJECT LEVEL
    //    //
    //    private UIFrame uiFrame;
    //    private H1 titleH1;
    //    private HTextField queryTf;
    //    //
    //    private List<HCheckBox> moClassIncludeCBs;
    //    private HCheckBox includeDeletedCB;
    //    private HCheckBox restrictToBeApprovedCB;
    //    private HCheckBox includeAllCB;
    //    //
    //    private HLink searchBt;
    //    private HLink resetBt;
    //    //
    //    //
    //    private DIV resDiv;
    //    private MOSearchCriteria currentSearchCriteria;

    //    private UIDataSelectionList resultList;
    //    private SimplePagingList<MODataObject> pagingList = new SimplePagingList<MODataObject>();
    //    private UIPagingLinks pagingLinks;

    //    public UISearchPanel(UIFrame uiFrame) {
    //        this.uiFrame = uiFrame;
    //        titleH1 = new H1("Search");
    //        TABLE table = new TABLE(0, 4, 4);
    //        moClassIncludeCBs = new List<HCheckBox>();
    //        foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
    //            if(moClass.IsTopLevel()) {
    //                HCheckBox checkBox = new HCheckBox(uiFrame);
    //                checkBox.AddServersideObject(moClass);
    //                moClassIncludeCBs.Add(checkBox);
    //            }
    //        }
    //        //
    //        queryTf = new HTextField(uiFrame, 64);
    //        //
    //        includeDeletedCB = new HCheckBox(uiFrame);
    //        restrictToBeApprovedCB = new HCheckBox(uiFrame);
    //        includeAllCB = new HCheckBox(uiFrame);
    //        //
    //        searchBt = new HLink(uiFrame, "Search");
    //        resetBt = new HLink(uiFrame, "Reset");
    //        resultList = new UIDataSelectionList(uiFrame);
    //        pagingLinks = new UIPagingLinks(uiFrame, this);
    //        //

    //        //
    //        // LAYOUT
    //        //
    //        TD td1 = null;
    //        TD td2 = null;
    //        //
    //        Add(titleH1);
    //        Add(table);
    //        //
    //        table.Add(new TR(
    //                td1 = new TD(new HLabel("Search Text "),
    //                td2 = new TD(queryTf))));
    //        td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
    //        td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
    //        //
    //        table.Add(new TR(
    //                td1 = new TD(new HLabel("Include All"),
    //                td2 = new TD(includeAllCB))));
    //        td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
    //        td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
    //        //
    //        foreach(HCheckBox cb in moClassIncludeCBs) {
    //            table.Add(new TR(
    //                     td1 = new TD(new HLabel(((MOClass)cb.GetServerSideObject()).GetName()),
    //                     td2 = new TD(cb))));
    //            td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
    //            td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
    //        }
    //        //
    //        table.Add(new TR(
    //                td1 = new TD(new HLabel("Include Deleted"),
    //                td2 = new TD(includeDeletedCB))));
    //        td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
    //        td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
    //        //
    //        table.Add(new TR(
    //                td1 = new TD(new HLabel("To Be Approved"),
    //                td2 = new TD(restrictToBeApprovedCB))));
    //        td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
    //        td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);


    //        table.Add(new TR(td1 = new TD(2, new DIV(UIStyles.GROUP, searchBt, resetBt))));
    //        td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);

    //        Add(new DIV(UIStyles.GROUP2, resDiv = new DIV()));
    //        //
    //        uiFrame.SetMainPanel(this);
    //        //
    //        // EVENTS
    //        //
    //        InitSubmitEventSource(uiFrame);
    //        searchBt.AddHListener(this);
    //        resetBt.AddHListener(this);
    //        //this.AddSubmitListener(this);
    //    }

    //    public void Arrived(HEvent ae) {
    //        if(ae.GetSource() == searchBt) {
    //            DoSearch(CreateSearchCriteriaFromGUI(0));
    //            uiFrame.SetMainPanel(this);
    //        } else if(ae.GetSource() == this.resetBt) {
    //            resultList.RemoveAllObjects();
    //            //
    //            queryTf.SetText("");
    //            //
    //            foreach(HCheckBox cb in moClassIncludeCBs) {
    //                cb.SetChecked(false);
    //            }
    //            //
    //            includeAllCB.SetChecked(false);
    //            includeDeletedCB.SetChecked(false);
    //            restrictToBeApprovedCB.SetChecked(false);
    //            resDiv.RemoveAll();
    //            currentSearchCriteria = null;
    //            uiFrame.SetMainPanel(this);
    //        } else if(ae.getEventType() == HEvent.SUBMIT_EVENT) {
    //            DoSearch(CreateSearchCriteriaFromGUI(0));
    //            logger.Debug("SUBMIT EVENT EXECUTED : ");
    //        } else if(ae.GetSource() == pagingLinks) {
    //            DoSearch(CreateSearchCriteriaFromGUI(pagingLinks.GetNewPagingStartIndex()));
    //        }

    //    }

    //    public void DoSearch() {
    //        if(currentSearchCriteria != null) {
    //            logger.Debug(currentSearchCriteria);
    //            List<MODataObject> result = MODataObject.Search(currentSearchCriteria);
    //            logger.Debug("result list: " + result.Count());
    //            resultList.SetObjects(result);
    //            pagingList.SetObjects(result, currentSearchCriteria.GetStartPagingIndex(), currentSearchCriteria.GetPageSize());
    //            pagingLinks.SetObjects(pagingList);
    //            resDiv.Set(new H3("Found " + result.Count() + " results"), resultList, pagingLinks);

    //        } else {
    //            logger.Warn("no search critera object!");
    //            resDiv.Set(new H3("No search criteria!"));
    //        }
    //    }

    //    public void DoSearch(MOSearchCriteria searchCriteria) {
    //        SetSearchCriteria(searchCriteria);
    //        DoSearch();
    //    }



    //    private MOSearchCriteria CreateSearchCriteriaFromGUI(int startIndex) {
    //        MOSearchCriteria sc = new MOSearchCriteria();
    //        sc.SetStartPagingIndex(startIndex);
    //        sc.SetPageSize(PAGE_SIZE);
    //        sc.SetTitle("Search");
    //        //
    //        String searchString = this.queryTf.GetText().Trim();
    //        sc.FillInSearchString(searchString);
    //        logger.Debug("query: " + queryTf.GetText());
    //        // DATA STATES
    //        sc.AddIncludedDataStates(DataState.APPROVED, DataState.STORED);
    //        //
    //        if(includeDeletedCB.IsChecked()) {
    //            sc.AddIncludedDataStates(DataState.DELETED, DataState.DELETED_UNAPPROVED);
    //        }
    //        if(restrictToBeApprovedCB.IsChecked()) {
    //            sc.SetIncludedDataStates(DataState.DELETED_UNAPPROVED, DataState.STORED);
    //            includeDeletedCB.SetChecked(false);
    //        }

    //        foreach(HCheckBox cb in this.moClassIncludeCBs) {
    //            if(cb.IsChecked()) {
    //                sc.AddIncludedMoids(((MOClass)cb.GetServerSideObject()).GetMoid());
    //            }
    //        }
    //        if(includeAllCB.IsChecked()) {
    //            foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
    //                if(moClass.IsTopLevel()) {
    //                    sc.AddIncludedMoids(moClass.GetMoid());
    //                }
    //            }
    //        }
    //        return sc;
    //    }

    //    private void RefreshSearch() {
    //        if(currentSearchCriteria != null) {
    //            DoSearch();
    //        }
    //    }

    //    //
    //    // IMainPanel
    //    //

    //    public void BeforeClose() {

    //    }

    //    public void BeforeShow() {
    //        uiFrame.SetFocusElement(this.queryTf);
    //        RefreshSearch();
    //    }

    //    public HComponent GetView() {
    //        return this;
    //    }

    //    public override void PrintTo(System.IO.TextWriter pr) {

    //        base.PrintTo(pr);
    //    }


    //    public void SetSearchCriteria(MOSearchCriteria searchCriteria) {
    //        this.titleH1.SetText(searchCriteria.GetTitle());
    //        this.currentSearchCriteria = searchCriteria;

    //        foreach(HCheckBox cb in this.moClassIncludeCBs) {
    //            if(currentSearchCriteria.IsMoidIncluded(((MOClass)cb.GetServerSideObject()).GetMoid())) {
    //                cb.SetChecked(true);
    //            }
    //        }
    //        this.queryTf.SetText(searchCriteria.GetSearchString());
    //    }
    //}


    public class UIDataObjectEditCommandRenderer : HContainer {

        public static readonly String EDIT = "Edit";
        public static readonly String VIEW = "View";

        protected UIDataObjectCellsRenderer dataObjectCellRenderer;

        private HLink editBt;
        private HLink viewBt;
        private DIV commandDiv;
        private TD[] cells;
        private int columnCount;
        private bool viewOnly;

        public UIDataObjectEditCommandRenderer(UIFrame uiFrame, IHListener listener) : this(uiFrame, listener, false) { }

        public UIDataObjectEditCommandRenderer(UIFrame uiFrame, IHListener listener, bool viewOnly) {
            this.dataObjectCellRenderer = new UIDataObjectCellsRenderer(uiFrame);
            this.viewOnly = viewOnly;
            this.columnCount = dataObjectCellRenderer.ColumnCount() + 1;
            cells = new TD[columnCount];

            TD commandTD = new TD(commandDiv = new DIV());
            commandDiv.SetStyleClass(HStyles.S100);
            cells[columnCount - 1] = commandTD;

            editBt = new HLink(uiFrame, EDIT);
            viewBt = new HLink(uiFrame, VIEW);
            //
            editBt.AddHListener(listener);
            viewBt.AddHListener(listener);
        }

        public HLink GetEditButton() {
            return editBt;
        }

        public HLink GetViewButton() {
            return viewBt;
        }

        public override int Count() {
            return columnCount;
        }


        public TD[] GetCells(MODataObject obj) {
            TD[] ocells = this.dataObjectCellRenderer.GetCells(obj);
            for(int i = 0; i < ocells.Length; i++) {
                cells[i] = ocells[i];
            }
            //
            commandDiv.RemoveAll();
            UIWorkaround.AddNBSPAsFirstElement(commandDiv);
            if(!viewOnly && MOAccess.GetInstance().CanWrite(obj)) {
                editBt.SetSubId(obj.GetOid().ToString());
                commandDiv.Add(editBt);
            }
            //if(MOAccess.GetInstance().CanRead(obj)) {
            //    viewBt.SetSubId(obj.GetOid().ToString());
            //    commandDiv.Add(viewBt);
            //}
            return cells;
        }
    }


    //    public TD[] GetCells(MODataObject obj) {

    //        TD[] ocells = this.dataObjectCellRenderer.GetCells(obj);
    //        for(int i = 0; i < ocells.Length; i++) {
    //            cells[i] = ocells[i];
    //        }
    //        //
    //        commandDiv.RemoveAll();
    //        UIWorkaround.AddNBSPAsFirstElement(commandDiv);
    //        if(!viewOnly && MOAccess.GetInstance().CanWrite(obj)) {
    //            editBt.SetSubId(obj.GetOid().ToString());
    //            commandDiv.Add(editBt);
    //        }
    //        if(MOAccess.GetInstance().CanRead(obj)) {

    //            viewBt.SetSubId(obj.GetOid().ToString());
    //            commandDiv.Add(viewBt);
    //        }
    //        return cells;
    //    }
    //}

    ///
    //
    public class UIDataSelectionList : TABLE, IHListener {
        //
        // CLASS LEVEL
        //
        private static readonly Logger logger = Logger.GetLogger(typeof(UIDataSelectionList));
        //
        // OBJECT LEVEL
        //
        private UIFrame uiFrame;
        private UIDataObjectEditCommandRenderer renderer;
        private HLink editBt;
        private HLink viewBt;
        private List<MODataObject> objects = new List<MODataObject>();
        //
        private TR tr = new TR();

        public UIDataSelectionList(UIFrame uiFrame) : this(uiFrame, false) { }

        public UIDataSelectionList(UIFrame uiFrame, bool viewOnly) {
            this.uiFrame = uiFrame;
            renderer = new UIDataObjectEditCommandRenderer(uiFrame, this, viewOnly);
            editBt = renderer.GetEditButton();
            viewBt = renderer.GetViewButton();
        }

        public override int Count() {
            return objects.Count();
        }

        public override HComponent Get(int index) {
            return tr.Set(renderer.GetCells(objects[index]));
        }

        public void Arrived(HEvent ae) {
            if(ae.GetSource() == editBt) {
                MODataObject mod = MODataObject.GetById(editBt.GetSubId());
                UIEditWindow editWindow = uiFrame.GetEditWindow(mod.GetMoid());
                editWindow.SetData(mod);
                uiFrame.SetMainPanel(editWindow);
            }
            if(ae.GetSource() == viewBt) {
                MODataObject mod = MODataObject.GetById(viewBt.GetSubId());
                UIViewWindow viewWindow = uiFrame.GetViewWindow(mod.GetMoid());
                //viewWindow.SetData(mod);
                //uiFrame.SetMainPanel(viewPanel);
                uiFrame.OpenDialogOnStack(new UIViewWindowDelegator(viewWindow, mod));
            }
        }

        public void SetObjects(IEnumerable<MODataObject> moDataObjects) {
            objects.Clear();
            objects.AddRange(moDataObjects);
        }

        public void AddObject(MODataObject moDataObject) {
            objects.Add(moDataObject);
        }

        public List<MODataObject> GetObjects() {
            return objects;
        }

        public void RemoveAllObjects() {
            objects.Clear();
        }
    }

    public class UIPagingLinks : DIV, IHListener {

        private HLink link;
        private SPAN nonLink;
        private int maxFound;
        private int pageSize;
        private int prev;
        private int next;
        private int start;
        private int maxNav = 5;
        private List<int> pageIndexes = new List<int>();
        private IHListener pagingListener;
        private int newPagingStartIndex;

        public UIPagingLinks(UIFrame uiFrame, IHListener pagingListener) {
            //
            this.SetStyleClass(UIStyles.GROUP);
            this.pagingListener = pagingListener;
            link = new HLink(uiFrame);
            nonLink = new SPAN("", UIStyles.NOLINK);
            //
            link.AddHListener(this);

        }

        public void SetObjects(IPagingList<MODataObject> resultList) {

            maxFound = resultList.GetMaxFound();
            start = resultList.GetStartIndex();
            pageSize = resultList.GetPageSize();
            //
            pageIndexes.Clear();
            //prev
            int nrOfPrev = start / pageSize;
            nrOfPrev = Math.Min(nrOfPrev, maxNav);
            //
            int nrOfNext = (maxFound - start) / pageSize;
            nrOfNext = maxFound % pageSize == 0 ? nrOfNext - 1 : nrOfNext;
            nrOfNext = nrOfNext > 0 ? nrOfNext : 0;
            nrOfNext = Math.Min(nrOfNext, maxNav);
            //
            if(nrOfPrev > 0) {
                prev = start - pageSize;
                pageIndexes.Add(prev);
                for(int i = nrOfPrev; i > 0; i--) {
                    pageIndexes.Add(start - pageSize * i);
                }
            }
            if(maxFound > pageSize) {
                pageIndexes.Add(start);
            }
            if(nrOfNext > 0) {
                for(int i = 1; i <= nrOfNext; i++) {
                    pageIndexes.Add(start + pageSize * i);
                }
                next = start + pageSize;
                pageIndexes.Add(next);
            }
            if(pageIndexes.Count == 0) {
                this.SetAttribute(HDTD.AttName.STYLE, "display: none");
            } else {
                this.RemoveAttribute(HDTD.AttName.STYLE);
            }
        }


        public override int Count() {
            return pageIndexes.Count;
        }

        public override HComponent Get(int index) {
            int pageStart = pageIndexes.Get(index);
            int page = pageStart / pageSize + 1;
            //
            link.SetText(page.ToString());
            link.SetSubId(pageStart.ToString());
            //
            if(pageStart == start) {
                // current
                nonLink.SetText(page.ToString());
                return nonLink;
            }
            if(pageStart == prev && index == 0) {
                // previous
                link.SetText("prev");
            }
            if(pageStart == next && index == pageIndexes.Count - 1) {
                // next
                link.SetText("next");
            }
            return link;
        }
        public void Arrived(HEvent he) {
            newPagingStartIndex = Int32.Parse(link.GetSubId());
            this.pagingListener.Arrived(new HEvent(this));
        }

        public int GetNewPagingStartIndex() {
            return this.newPagingStartIndex;
        }

        public int GetPageSize() {
            return this.pageSize;
        }

        //public void SetPageSize(int pageSize) {
        //    this.pageSize = pageSize;
        //}
    }


    public interface IPagingList<E> {
        int GetMaxFound();
        int GetStartIndex();
        IEnumerable<E> GetPage();
        int GetPageSize();
        Exception GetException();
        long GetProcessingTime();
        E Get(int index);
        int Count();
    }

    public static class IPagingListUtils {

        public static String GetResultsLabel<E>(IPagingList<E> list) {
            StringBuilder sb = new StringBuilder();
            if(list.Count() > 0) {
                sb.Append("Results : ");
                sb.Append(list.GetStartIndex() + 1);
                sb.Append(" - ");
                sb.Append(list.GetStartIndex() + list.Count());
                sb.Append(" of ");
                sb.Append(list.GetMaxFound());
            } else {
                sb.Append("No Results");
            }
            return sb.ToString();
        }

    }

    public class SimplePagingList<E> : IPagingList<E> {

        private List<E> list;
        private int startIndex;
        private int pageSize;

        public SimplePagingList(List<E> list) {
            SetObjects(list, 0, 100);
        }

        public SimplePagingList() { }

        public void SetObjects(List<E> list, int startIndex, int pageSize) {
            this.list = list;
            this.startIndex = startIndex;
            this.pageSize = pageSize;
        }

        public int GetMaxFound() {
            return list.Count();
        }

        public int GetStartIndex() {
            return startIndex;
        }

        public Exception GetException() {
            return null;
        }

        public long GetProcessingTime() {
            return -1;
        }

        public E Get(int index) {
            return list.Get(startIndex + index);
        }

        public int Count() {
            return Math.Min(list.Count - startIndex, pageSize);
        }

        public IEnumerable<E> GetPage() {
            for(int i = 0; i < Count(); i++) {
                yield return Get(i);
            }
        }

        public int GetPageSize() {
            return this.pageSize;
        }

    }
}