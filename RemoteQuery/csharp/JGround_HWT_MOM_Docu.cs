//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Org.JGround.Imaging;
using Org.JGround.MOM;
using Org.JGround.Util;
using System.Collections.Generic;
using Org.JGround.Web;

namespace Org.JGround.HWT.MOM.Docu
{


    public class UIAttributeControlDocu : UIAttributeControlBase, IHListener
    {

        public static readonly int MEDIUM_HEIGHT = 200;
        public static readonly int SMALL_HEIGHT = 40;

        public static String CreateResizedImagePath(String originalImageFile, int resizeConstant)
        {
            return
            Path.Combine(Path.GetDirectoryName(originalImageFile), resizeConstant + Path.GetExtension(originalImageFile));
        }

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewControlFactory));

        public static UIAttributeControlDocu Create(IUIViewPanel viewPanel, MOControl moControl)
        {
            return new UIAttributeControlDocu(viewPanel, moControl);
        }

        private MOAttribute moAttribute;
        private HLink uploadBt;
        private HTag docuLink;

        private HFileInput fileInput;
        private DIV editDIV;
        private String fileName;

        private UIAttributeControlDocu(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl)
        {
            this.moAttribute = moControl.GetMOAttribute();
            //
            uploadBt = new HLink(viewPanel.GetUIFrame(), "Upload");
            docuLink = new HTag(HDTD.Element.A);
            docuLink.SetAttribute(HDTD.AttName.TARGET, HDTD.AttValue._BLANK);

            fileInput = new HFileInput(viewPanel.GetUIFrame());
            //Add(editDIV = new DIV(fileInput, new DIV(uploadBt, HStyles.HLINK)));
            Add(editDIV = new DIV(fileInput, new DIV(uploadBt)));
            //
            uploadBt.AddHListener(this);
        }

        public override void Set(params String[] values)
        {
            if (ArrayUtils.IsNotEmpty(values) && StringUtils.IsNotEmpty(values[0]))
            {
                fileName = values[0];
                //String fileNameEncoded = uiFrame.GetHttpServerUtility().UrlEncode(fileName);
                String ext = Path.GetExtension(fileName);
                String linkValue = DocuServlet.CreateDocuLink(GetDataContext().GetOid(), viewPanel.GetUIFrame().GetHttpServerUtility().UrlEncode(StringUtils.toLetters(fileName)));
                //linkValue = uiFrame.GetHttpServerUtility().UrlPathEncode(linkValue);
                docuLink.SetAttribute(HDTD.AttName.HREF, linkValue);
                docuLink.RemoveAll();
                //
                // make sure the document can be downloaded from.
                //
                uiFrame.GetHWTEvent().GetSession().Add(DocuServlet.SESSION_ID_PREFIX + GetDataContext().GetOid(), "R");
                if ("pictureUpload".Equals(moView.GetWidgetHint()))
                {
                    String mediumPictureLinkValue = DocuServlet.CreateDocuLink(
                            GetDataContext().GetOid(), MEDIUM_HEIGHT + Path.GetExtension(fileName).ToLower());
                    IMG img = new IMG(mediumPictureLinkValue);
                    docuLink.Add(img);
                }
                else
                {
                    docuLink.SetText(fileName);
                }
                editDIV.Set(docuLink);
            }
            else
            {
                Clear();
            }
        }

        public override String[] Get()
        {
            return StringUtils.IsEmpty(fileName) ? new String[] { } : new String[] { fileName };
        }

        public override void Clear()
        {
            fileName = null;
            editDIV.Set(fileInput, uploadBt);
        }

        public void Arrived(HEvent he)
        {
            if (he.GetSource() == uploadBt)
            {
                String uploadedFile = this.fileInput.GetFileName();
                String tmpSavedFileName = this.fileInput.GetTempSavedFileName();
                if (StringUtils.IsNotEmpty(uploadedFile))
                {
                    long oid = GetDataContext().GetOid();
                    fileName = Path.GetFileName(uploadedFile);
                    String saveName = DocuServlet.CreateUploadFileNameLowerCase(DocuServlet.DOCU_STORE_DIR, oid, fileName);
                    String dir = Path.GetDirectoryName(saveName);
                    IOUtils.EmptyDirectory(dir, "*");
                    Directory.CreateDirectory(dir);
                    logger.Debug("tmpSavedFileName", tmpSavedFileName, "saveName", saveName, "fileName", fileName, "oid", oid);
                    try
                    {
                        File.Copy(tmpSavedFileName, saveName);
                        File.Delete(tmpSavedFileName);
                        //
                        // CREATE RESIZED IMAGES
                        //
                        if (ImagingUtils.IsExtensionSupported(saveName))
                        {
                            ImagingUtils.ResizeToHeight(saveName, MEDIUM_HEIGHT, CreateResizedImagePath(saveName, MEDIUM_HEIGHT));
                            ImagingUtils.ResizeToHeight(saveName, SMALL_HEIGHT, CreateResizedImagePath(saveName, SMALL_HEIGHT));
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("could not save or delete file: ", e);
                    }
                    Set(Path.GetFileName(saveName));
                }
            }

        }
    }


    public class UIAttributeControlUploadifyDocu : UIAttributeControlBase, INewFileListener, IHListener
    {



        private static Logger logger = Logger.GetLogger(typeof(UIAttributeControlUploadifyDocu));

        public static UIAttributeControlUploadifyDocu Create(IUIViewPanel viewPanel, MOControl moControl)
        {
            return new UIAttributeControlUploadifyDocu(viewPanel, moControl);
        }

        private bool useTrash = true;
        private MOAttribute moAttribute;


        private HUploadifyInput fileInput;
        private TABLE fileTable;

        private UIAttributeControlUploadifyDocu(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl)
        {
            this.moAttribute = moControl.GetMOAttribute();
            //
            fileInput = new HUploadifyInput(viewPanel.GetUIFrame());
            fileInput.InitSubmitEventSource(viewPanel.GetUIFrame());
            fileInput.SetINewFileListener(this);
            fileInput.SetTitle("Warning: Add and deletes of files are done without save of data object!");
            //
            fileTable = new TABLE();
            //
            Add(new DIV(fileInput), new DIV(fileTable));
        }

        public override void Set(params String[] values)
        {
            fileTable.RemoveAll();
            long oid = GetDataContext().GetOid();
            String directory = GetDirectory();
            if (Directory.Exists(directory))
            {
                String[] fileNames = Directory.GetFiles(directory);
                Array.Sort(fileNames);
                foreach (String filePath in fileNames)
                {
                    String fileName = Path.GetFileName(filePath);
                    FileInfo fi = new FileInfo(filePath);
                    DateTime dt = fi.LastWriteTime;
                    String timeString = DateTimeUtils.FormatDateTime(dt);

                    String linkValue = DocuServlet.CreateDocuLink(oid, viewPanel.GetUIFrame().GetHttpServerUtility().UrlEncode(fileName));
                    HTag fileLink = new HTag(HDTD.Element.A);
                    fileLink.SetAttribute(HDTD.AttName.TARGET, HDTD.AttValue._BLANK);
                    fileLink.SetAttribute(HDTD.AttName.HREF, linkValue);
                    fileLink.SetCss("margin-right: 4px; display: block");
                    fileLink.SetText(fileName);
                    fileLink.SetCss("font-size: 12px");
                    //
                    HLink deleteLink = new HLink(viewPanel.GetUIFrame(), "delete");
                    deleteLink.SetCss("margin: 2px; display: block");
                    deleteLink.SetSubId(fileName);
                    deleteLink.SetAttribute(HDTD.AttName.TITLE, "Achtung, die Datei wird unwiderruflich gelöscht!");
                    deleteLink.AddHListener(this);
                    //
                    // make sure the document can be downloaded from.
                    //
                    DIV timeDiv = new DIV(timeString);
                    timeDiv.SetCss("padding-left:4px;padding-right:4px;font-size:11px;vertical-align: bottom;");
                    DIV fileLinkDiv = new DIV(fileLink);
                    fileLinkDiv.SetCss("padding-left:4px;padding-right:4px;vertical-align: bottom;");
                    fileTable.Add(new TR(new TD(timeDiv), new TD(fileLinkDiv), new TD(deleteLink)));
                    uiFrame.GetHWTEvent().GetSession().Add(DocuServlet.SESSION_ID_PREFIX + GetDataContext().GetOid(), "R");
                }
            }
            if (fileTable.Count() == 0)
            {
                fileTable.Add(new TR(new TD("-no files-")));
            }
        }

        private String GetDirectory()
        {
            long oid = GetDataContext().GetOid();
            return DocuServlet.CreateUploadDirectory(DocuServlet.DOCU_STORE_DIR, oid);
        }

        public override String[] Get()
        {
            return new String[0];
        }

        public override void Clear()
        {
            fileTable.RemoveAll();
        }

        public void Arrived(UploadifyFile file)
        {
            String uploadedFile = file.fileName;
            String tmpSavedFileName = file.tempSavedFileName;
            if (StringUtils.IsNotEmpty(uploadedFile))
            {
                long oid = GetDataContext().GetOid();
                String directory = GetDirectory();
                String fileName = Path.GetFileName(uploadedFile);
                String saveName = Path.Combine(directory, fileName);
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    if (File.Exists(saveName))
                    {
                        File.Delete(saveName);
                    }
                    logger.Debug("tmpSavedFileName", tmpSavedFileName, "saveName", saveName, "fileName", fileName, "oid", oid);
                    File.Copy(tmpSavedFileName, saveName);
                    File.Delete(tmpSavedFileName);
                }
                catch (Exception e)
                {
                    logger.Warn("could not save or delete file: ", e);
                }
            }
            Set(null);
        }

        public void Arrived(HEvent he)
        {
            String fileName = he.GetSubEventId();
            String directory = GetDirectory();
            String p = Path.Combine(directory, fileName);
            if (useTrash)
            {
                String directoryTrash = Path.Combine(directory, "trash");
                Directory.CreateDirectory(directoryTrash);
                String pTrash = Path.Combine(directoryTrash, fileName);
                if (File.Exists(pTrash))
                {
                    File.Delete(pTrash);
                }
                File.Move(p, pTrash);
            }
            else
            {
                File.Delete(p);
            }


            //
            Set(null);
        }


    }




    public class UIAttributeViewDocu : UIAttributeViewBase
    {



        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewControlFactory));

        internal static UIAttributeViewDocu Create(IUIViewPanel viewPanel, MOView moControl)
        {
            return new UIAttributeViewDocu(viewPanel, moControl);
        }

        private MOAttribute moAttribute;
        private HTag docuLink;

        // HListSelectionPanel<MODataObject>

        private UIAttributeViewDocu(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl)
        {
            this.moAttribute = moControl.GetMOAttribute();
            //
            docuLink = new HTag(HDTD.Element.A);
            docuLink.SetAttribute(HDTD.AttName.TARGET, HDTD.AttValue._BLANK);
            //Add(new DIV(docuLink, HStyles.HLINK, UIStyles.NOWRAP));
            Add(new DIV(docuLink, UIStyles.MO_NOWRAP));
            //
        }

        public override void Set(params String[] values)
        {
            if (ArrayUtils.IsNotEmpty(values) && StringUtils.IsNotEmpty(values[0]))
            {
                String fileName = values[0];
                //String fileNameEncoded = uiFrame.GetHttpServerUtility().UrlEncode(fileName);
                String ext = Path.GetExtension(fileName);
                //old String linkValue = DocuServlet.CreateDocuLink(GetDataContext().GetOid(), uiFrame.GetHttpServerUtility().UrlEncode(fileName));
                String linkValue = DocuServlet.CreateDocuLink(GetDataContext().GetOid(), uiFrame.GetHttpServerUtility().UrlEncode(StringUtils.toLetters(fileName)));
                //linkValue = uiFrame.GetHttpServerUtility().UrlPathEncode(linkValue);
                docuLink.SetAttribute(HDTD.AttName.HREF, linkValue);
                docuLink.RemoveAll();
                //
                // make sure the document can be downloaded from.
                //
                uiFrame.GetHWTEvent().GetSession().Add(DocuServlet.SESSION_ID_PREFIX + GetDataContext().GetOid(), "R");
                //
                if ("picture".Equals(moView.GetWidgetHint()) && ImagingUtils.IsExtensionSupported(fileName))
                {
                    String mediumPictureLinkValue = DocuServlet.CreateDocuLink(
                            GetDataContext().GetOid(), UIAttributeControlDocu.SMALL_HEIGHT + Path.GetExtension(fileName).ToLower());
                    IMG img = new IMG(mediumPictureLinkValue);
                    docuLink.Add(img);
                }
                else
                {
                    docuLink.SetText(fileName);
                }
            }
            else
            {
                Clear();
            }
        }

        public override void Clear()
        {
            docuLink.RemoveAll();
        }

    }



    public class UIAttributeViewUploadifyDocu : UIAttributeViewBase
    {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewUploadifyDocu));

        internal static UIAttributeViewUploadifyDocu Create(IUIViewPanel viewPanel, MOView moControl)
        {
            return new UIAttributeViewUploadifyDocu(viewPanel, moControl);
        }

        private MOAttribute moAttribute;

        private TABLE fileTable;
        // HListSelectionPanel<MODataObject>

        private UIAttributeViewUploadifyDocu(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl)
        {
            this.moAttribute = moControl.GetMOAttribute();
            //

            Add(fileTable = new TABLE());
        }

        public override void Set(params String[] values)
        {
            fileTable.RemoveAll();
            try
            {
                long oid = GetDataContext().GetOid();
                String directory = DocuServlet.CreateUploadDirectory(DocuServlet.DOCU_STORE_DIR, oid);
                if (Directory.Exists(directory))
                {
                    String[] fileNames = Directory.GetFiles(directory);
                    Array.Sort(fileNames);
                    foreach (String filePath in fileNames)
                    {
                        FileInfo fi = new FileInfo(filePath);
                        DateTime dt = fi.LastWriteTime;
                        String timeString = DateTimeUtils.FormatDateTime(dt);

                        String fileName = Path.GetFileName(filePath);
                        String linkValue = DocuServlet.CreateDocuLink(oid, viewPanel.GetUIFrame().GetHttpServerUtility().UrlEncode(fileName));
                        HTag fileLink = new HTag(HDTD.Element.A);
                        fileLink.SetAttribute(HDTD.AttName.TARGET, HDTD.AttValue._BLANK);
                        //linkValue = uiFrame.GetHttpServerUtility().UrlPathEncode(linkValue);
                        fileLink.SetAttribute(HDTD.AttName.HREF, linkValue);
                        fileLink.SetText(fileName);


                        DIV timeDiv = new DIV(timeString);
                        timeDiv.SetCss("padding-left:4px;padding-right:4px;font-size:11px;vertical-align: bottom;");
                        DIV fileLinkDiv = new DIV(fileLink);
                        fileLinkDiv.SetCss("padding-left:4px;padding-right:4px;vertical-align: bottom;");
                        fileTable.Add(new TR(new TD(timeDiv), new TD(fileLinkDiv)));


                        //
                        // make sure the document can be downloaded from.
                        //
                        uiFrame.GetHWTEvent().GetSession().Add(DocuServlet.SESSION_ID_PREFIX + GetDataContext().GetOid(), "R");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Problem with reading uploadify files...", e);
            }

        }

        public override void Clear()
        {
            fileTable.RemoveAll();
        }

    }





    public class DocuServlet : IHttpHandler, IRequiresSessionState
    {

        //
        // CLASS LEVEL
        //

        private static Logger logger = Logger.GetLogger(typeof(DocuServlet));
        public static readonly int BUFFERSIZE = 2000;

        public static readonly String SESSION_ID_PREFIX = "_DocuServlet_";
        public static readonly String DOCU_URI = "docu";
        public static String DOCU_STORE_DIR { get; set; }
        public static String REPORT_STORE_DIR { get; set; }

        public static String REPORT_URI = "report";

        public static String CreateDocuLink(long oid, String fileNameEncoded)
        {
            return "../" + DocuServlet.DOCU_URI + "/" + oid + "/" + fileNameEncoded;
        }

        public static String CreateReportLink(String fileName)
        {
            return "../" + DocuServlet.DOCU_URI + "/" + REPORT_URI + "/" + fileName;
        }

        public static String CreateUploadFileNameLowerCase(String fileStoreDirectory, long oid, String fileName)
        {
            String p = CreateUploadDirectory(fileStoreDirectory, oid);
            p = Path.Combine(p, fileName.ToLower());
            return p;
        }

        public static String CreateUploadDirectory(String fileStoreDirectory, long oid)
        {
            String p = fileStoreDirectory;
            p = Path.Combine(p, CreateSubDirName(oid));
            p = Path.Combine(p, oid.ToString());
            return p;
        }

        public static String CreateUploadFileName(String fileStoreDirectory, long oid, String fileName)
        {
            String p = CreateUploadDirectory(fileStoreDirectory, oid);
            p = Path.Combine(p, fileName);
            return p;
        }

        public static String FindUploadFileName(String fileStoreDirectory, long oid, String findFileName)
        {
            String p = CreateUploadDirectory(fileStoreDirectory, oid);

            try
            {
                String[] files = Directory.GetFiles(p);
                foreach (String file in files)
                {
                    if (file.EndsWith(findFileName))
                    {
                        return file;
                    }
                }
                return Directory.GetFiles(p)[0];
            }
            catch (Exception e)
            {
                logger.Error(e);
                return null;
            }
        }



        public static String FindReportFileName(String userName)
        {
            userName = userName.Replace("\\", "_");
            userName = userName.Replace("/", "_");
            return Path.Combine(REPORT_STORE_DIR, userName);
        }

        public static String CreateSubDirName(long oid)
        {
            long f = oid / 1000;
            f = f % 1000;
            return "docs" + f.ToString();
        }

        //
        // OBJECT LEVEL
        //

        public void ProcessRequest(HttpContext context)
        {
            String r = context.Request.Url.AbsoluteUri;
            HttpRequest request = context.Request;
            logger.Debug("context.Request.Url.AbsoluteUri", r);
            String fileName = null;
            String extension = null;
            String[] parts = r.Split('/');
            if (parts.Length > 2)
            {
                String docuid = parts[parts.Length - 2];
                fileName = parts[parts.Length - 1];
                if (docuid.Equals(REPORT_URI))
                {
                    logger.Debug("docuid: " + REPORT_URI);
                    try
                    {
                        context.Session[SESSION_ID_PREFIX].Equals(fileName);
                    }
                    catch (Exception)
                    {
                        context.Response.Output.WriteLine("<html><body>Sorry, no access to the requested report.</body></html>");
                        return;
                    }
                    logger.Debug("report request : " + fileName);
                    fileName = FindReportFileName(fileName);
                    extension = ".xls";
                }
                else
                {
                    logger.Debug("docuid: ", docuid);

                    //
                    // Check Access Rights
                    //
                    try
                    {
                        context.Session[SESSION_ID_PREFIX + docuid].Equals("R");
                    }
                    catch (Exception)
                    {
                        context.Response.Output.WriteLine("<html><body>Sorry, no access to the requested document.</body></html>");
                        return;
                    }
                    // 
                    // Currently the real filename is not used because of problematic URL encodings.
                    //
                    logger.Debug("dummyFileName: ", fileName);
                    fileName = FindUploadFileName(DOCU_STORE_DIR, Int64.Parse(docuid), context.Server.UrlDecode(fileName));
                    fileName = fileName == null ? Path.Combine(request.PhysicalApplicationPath, "img/noimage.png") : fileName;
                    extension = Path.GetExtension(fileName).ToLower();

                }
                // 
                // Setting Content Type
                //
                if (extension.Equals(".pdf"))
                {
                    context.Response.ContentType = "application/pdf";
                }
                else if (extension.Equals(".xls"))
                {
                    context.Response.ContentType = "application/excel";
                }
                else if (extension.Equals(".jpg"))
                {
                    context.Response.ContentType = "image/jpeg";
                }
                else if (extension.Equals(".gif"))
                {
                    context.Response.ContentType = "image/gif";
                }
                else if (extension.Equals(".png"))
                {
                    context.Response.ContentType = "image/png";
                }
                else
                {
                    context.Response.ContentType = "application/bin";
                }
                logger.Debug("file: " + fileName);
                //
                // Writing file
                //
                int counter = 0;
                byte[] buffer = new byte[BUFFERSIZE];
                using (FileStream fs = File.OpenRead(fileName))
                {
                    Stream os = context.Response.OutputStream;
                    while ((counter = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        os.Write(buffer, 0, counter);
                    }
                    os.Flush();
                }

            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

    }

}


