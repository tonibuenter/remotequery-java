//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using Org.JGround.HWT.MOM;
using Org.JGround.Util;

namespace Org.JGround.HWT.Components {

    public interface IMainPanel {
        void BeforeShow();
        void BeforeClose();
        HComponent GetView();
    }


    public class HListSelectionPanel<E> : HContainer, IHListener {

        class HTextLinkElementList : IHListener {



            private class TableRow : TR {
                private HLink hlink;
                private HTextLinkElementList textLinkElementList;
                internal TableRow(HFrame fr, String text, String subId, HTextLinkElementList textLinkElementList) {
                    this.textLinkElementList = textLinkElementList;
                    htext = new HText();
                    hlink = new HLink(fr, HStyles.DELETE_ICON);
                    Add(new TD(new DIV(htext, HStyles.S100)));
                    Add(new TD(new DIV(hlink, HStyles.S100)));
                    SetText(text);
                    SetSubId(subId);
                    hlink.AddHListener(textLinkElementList);
                }


                public void SetSubId(String subId) {
                    hlink.SetSubId(subId);
                }

                public HLink GetLink() {
                    return hlink;
                }
            }

            private List<TableRow> rows = new List<TableRow>();
            private TR controls;
            private List<E> objects = new List<E>();
            private TABLE table = new TABLE();
            private HListSelectionPanel<E> selectionPanel;
            private HFrame fr;

            public HTextLinkElementList(HFrame fr, HListSelectionPanel<E> selectionPanel) {
                this.fr = fr;
                this.selectionPanel = selectionPanel;
            }

            public void SetList(List<E> list) {
                Clear();
                AddAll(list);
                UpdateTable();
            }

            public List<E> GetList() {
                return objects;
            }

            public void AddAll(List<E> list) {
                foreach (E obj in list) {
                    Add(obj);
                }
            }

            public void Add(E obj) {
                if (objects.Contains(obj)) {
                    return;
                }
                int currentSize = objects.Count;
                if (currentSize == rows.Count) {
                    TableRow row = new TableRow(fr, obj.ToString(), currentSize.ToString(), this);
                    rows.Add(row);
                } else {
                    TableRow text = rows.Get(currentSize);
                    text.SetText(obj.ToString());
                }
                objects.Add(obj);
                UpdateTable();
            }

            public void Remove(int index) {
                rows.RemoveAt(index);
                // rows.Add();
                ReindexLink();
                objects.RemoveAt(index);
                UpdateTable();
            }

            private void ReindexLink() {
                int index = 0;
                foreach (TableRow row in rows) {
                    row.SetSubId(index.ToString());
                    index++;
                }
            }

            public void Clear() {
                objects.Clear();
            }

            public int Count() {
                return objects.Count;
            }

            internal void UpdateTable() {
                table.RemoveAll();
                for (int i = 0; i < Count(); i++) {
                    table.Add(rows.Get(i));
                }
                if (controls == null) {
                    controls = new TR(new TD(selectionPanel.selectHList), new TD(selectionPanel.addLink));
                }
                table.Add(controls);
            }

            public void Arrived(HEvent he) {
                HLink link = (HLink)he.GetSource();
                int delInt = Int32.Parse(link.GetSubId());
                this.Remove(delInt);
                UpdateTable();
            }

            public HComponent GetView() {
                return table;
            }
        }

        //
        private HTextLinkElementList textLinkElementList;
        //
        internal List<E> selectModel;
        internal HList<E> selectHList;
        internal HLink addLink;

        //
        //
        //

        public HListSelectionPanel(HFrame hframe)
            : this(hframe, null) {
        }

        public HListSelectionPanel(HFrame hframe, List<E> selectList) {
            //
            selectModel = new List<E>();
            selectHList = new HList<E>(hframe, selectModel, false, 1);
            SetElementList(selectList);
            addLink = new HLink(hframe, "Add");
            //
            textLinkElementList = new HTextLinkElementList(hframe, this);
            //
            DIV mainDIV = new DIV();
            mainDIV.SetStyleClass(UIStyles.GROUP);
            mainDIV.Add(textLinkElementList.GetView());
            Add(mainDIV);
            textLinkElementList.UpdateTable();
            //
            addLink.AddHListener(this);
        }

        public void SetElementList(List<E> list) {
            selectModel.Clear();
            if (ICollectionUtils.IsEmpty(list)) {
                return;
            }
            selectModel.AddRange(list);
        }

        public void Arrived(HEvent he) {
            if (he.GetSource() == addLink) {
                IList<E> selectSet = selectHList.GetSelected();
                if (selectSet.Count > 0) {
                    foreach (E e in selectSet) {
                        textLinkElementList.Add(e);
                    }
                }
            }
        }

        public void SetSelectedElementList(List<E> list) {
            textLinkElementList.SetList(list);
        }

        public List<E> GetSelectedElementList() {
            return textLinkElementList.GetList();
        }

    }

    public class UIInfoDialog : IMainPanel, IHListener {

        private UIFrame uiFrame;
        private HText confirmTx;
        private DIV dispDIV;
        private HLink okBt;
        private DIV div;

        public UIInfoDialog(UIFrame uiFrame) {
            this.uiFrame = uiFrame;
           
            TABLE table = new TABLE();
            div = new DIV(UIStyles.CONFIRMATION_DIALOG, table);
            okBt = new HLink(uiFrame, "OK");
            confirmTx = new HText();
            table.Add(new TR(new TD(new DIV(UIStyles.CONFIRMATION_DIALOG_TEXT, confirmTx))));
            table.Add(new TR(new TD(dispDIV = new DIV(default(string), UIStyles.CONFIRMATION_DIALOG_CONTEXT))));
            TD td;
            table.Add(new TR(td = new TD(new DIV(HStyles.HLINK, okBt))));
            td.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.CENTER);
            //
            okBt.AddHListener(this);
        }

        public void SetConfirmContent(String confirmText, HComponent content) {
            confirmTx.SetText(confirmText);
            dispDIV.Set(content);
        }

        public void Arrived(HEvent he) {
            //uiFrame.SetPreviousMainPanel();
            uiFrame.ShowHomePanel();
        }

        public void BeforeShow() { }

        public void BeforeClose() { }

        public HComponent GetView() {
            return div;
        }

    }

}