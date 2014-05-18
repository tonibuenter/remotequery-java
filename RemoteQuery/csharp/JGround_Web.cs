//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using Org.JGround.Util;

namespace Org.JGround.Web {


    public class UploadifyFile
    {
        public String fileName;
        public String tempSavedFileName;
        public String remoteName;
        public String remoteType;

    }

    public class WebUtils {

        public static readonly String MULTIPART_FORM_DATA = "multipart/form-data";

        public static void WriteHttpContextProperties(TextWriter w, HttpContext context) {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            TimeSpan maxAge = TimeSpan.FromHours(2);
            DateTime expiresDate = DateTime.Now + maxAge;
            response.Cache.SetMaxAge(maxAge);
            response.Cache.SetExpires(expiresDate);
            context.Response.ContentType = "text/html";
            context.Response.Write("<html><body><h1>HttpTest</h1>");
            //
            // CONFIGURATION 
            //
            W.WTag(w, "h2", "Configuration");
            W.WStartTag(w, "table", new String[] { "border", "1" });
            W.WStartTag(w, "tr");
            W.WStartTag(w, "td");
            W.WTag(w, "b", "WebConfigurationManager");
            W.WEndTag(w, "td");
            W.WStartTag(w, "td");
            W.WStartTag(w, "pre");
            foreach(var obj in WebConfigurationManager.AppSettings) {
                w.WriteLine(obj);
            }
            W.WEndTag(w, "pre");
            W.WEndTag(w, "td");
            W.WEndTag(w, "tr");
            W.WEndTag(w, "table");
            //
            // REQUEST TESTS
            //
            W.WTag(w, "h2", "Request stuff");
            W.WStartTag(w, "table", new String[] { "border", "1" });
            WriteRow(w, "Current Time (Server)", "" + DateTime.Now.ToLongTimeString());
            WriteRow(w, "request.AcceptTypes ", ArrayUtils.Join(request.AcceptTypes, ", "));
            WriteRow(w, "request.AnonymousID ", request.AnonymousID);
            WriteRow(w, "request.ApplicationPath ", request.ApplicationPath);
            WriteRow(w, "request.AppRelativeCurrentExecutionFilePath ", request.AppRelativeCurrentExecutionFilePath);
            WriteRow(w, "request.Browser ", request.Browser);
            WriteRow(w, "request.ContentEncoding ", request.ContentEncoding);
            WriteRow(w, "request.ContentLength ", request.ContentLength);
            WriteRow(w, "request.ContentType ", request.ContentType);
            WriteRow(w, "request.CurrentExecutionFilePath ", request.CurrentExecutionFilePath);
            WriteRow(w, "request.FilePath ", request.FilePath);
            WriteRow(w, "request.Headers ", request.Headers);
            WriteRow(w, "request.HttpMethod ", request.HttpMethod);
            WriteRow(w, "request.IsAuthenticated ", request.IsAuthenticated);
            WriteRow(w, "request.IsLocal ", request.IsLocal);
            WriteRow(w, "request.IsSecureConnection ", request.IsSecureConnection);
            WriteRow(w, "request.Path ", request.Path);
            WriteRow(w, "request.PathInfo ", request.PathInfo);
            WriteRow(w, "request.PhysicalApplicationPath ", request.PhysicalApplicationPath);
            WriteRow(w, "request.PhysicalPath ", request.PhysicalPath);
            WriteRow(w, "request.QueryString ", request.QueryString);
            WriteRow(w, "request.RawUrl ", request.RawUrl);
            WriteRow(w, "request.RequestType ", request.RequestType);
            WriteRow(w, "request.ServerVariables ", request.ServerVariables);
            WriteRow(w, "request.TotalBytes ", request.TotalBytes);
            WriteRow(w, "request.Url ", request.Url);
            WriteRow(w, "request.UrlReferrer ", request.UrlReferrer);
            WriteRow(w, "request.UserAgent ", request.UserAgent);
            WriteRow(w, "request.UserHostAddress ", request.UserHostAddress);
            WriteRow(w, "request.UserHostName ", request.UserHostName);
            WriteRow(w, "request.UserLanguages ", ArrayUtils.Join(request.UserLanguages, ", "));
            W.WEndTag(w, "table");
            //
            // SESSION TESTS
            //
            W.WTag(w, "h2", "Session stuff");
            HttpSessionState session = context.Session;
            if(session["scounter"] != null) {
                int sc = (int)session["scounter"];
                sc++;
                session["scounter"] = sc;
            } else {
                session["scounter"] = 1;
            }
            W.WTag(w, "div", "session counter : " + session["scounter"]);
            String sessionKey = "__VIPS_SESS__";
            if(session[sessionKey] == null) {
                W.WTag(w, "div", "session is new!");
                session[sessionKey] = "VIPSSESSION";
            } else {
                W.WTag(w, "div", "session id: " + session.SessionID);
            }
            //
            //
            //
            W.WTag(w, "div", "user identity name : " + HttpContext.Current.User.Identity.Name.ToString());
            W.WTag(w, "div", "user is in Role CO : " + HttpContext.Current.User.IsInRole("CO"));
            //
            // FILE UPLOAD PROCESSING
            //
            HttpFileCollection files = context.Request.Files;
            if(files != null && files.Count > 0) {
                W.WTag(w, "h2", "File upload");
                W.WStartTag(w, "pre");
                for(int i = 0; i < files.Count; i++) {
                    if(files[i].FileName != String.Empty) {
                        w.WriteLine(files[i]);
                    }
                }
                W.WEndTag(w, "pre");
            } else {
                W.WTag(w, "div", "No file uploaded (files: " + (files == null ? -1 : files.Count) + ")");
            }
            //
            // Application Properties
            //
            W.WTag(w, "h1", "Application Properties");
            String path = context.Server.MapPath("~/App_Data/" + "application.properties");
            FileInfo file = new FileInfo(path);
            if(file.Exists) {
                String[] lines = File.ReadAllLines(path);
                foreach(String line in lines) {
                    W.WTag(w, "pre", line);
                }
            } else {
                W.WTag(w, "div", "Application Properties");
            }
            context.Response.Write("</body></html>");
        }

        private static void WriteRow(TextWriter w, String label, object value) {
            W.WStartTag(w, "tr");
            //
            W.WStartTag(w, "td");
            W.WTag(w, "b", label);
            W.WEndTag(w, "td");
            //
            W.WStartTag(w, "td");
            W.WTag(w, "code", value == null ? "-" : value.ToString());
            W.WEndTag(w, "td");
            //
            W.WEndTag(w, "tr");
        }

    }


    public class W {
        public W() { }

        public static void WriteHello(TextWriter writer) {
            writer.Write("Hello World");
        }

        public static void WTag(TextWriter w, String tagName, String text) {
            WStartTag(w, tagName);
            w.Write(text);
            WEndTag(w, tagName);
            w.WriteLine();
        }

        public static void WTag(TextWriter w, String p) {
            WTag(w, p, "");
        }

        public static void WStartTag(TextWriter writer, String tagName) {
            writer.Write("<");
            writer.Write(tagName);
            writer.Write(">");
        }

        public static void WEndTag(TextWriter writer, String tagName) {
            writer.Write("</");
            writer.Write(tagName);
            writer.Write(">");
        }

        public static void WStartTag(TextWriter w, String tagName, String[] attributes) {
            w.Write('<');
            w.Write(tagName);
            w.Write(' ');
            for(int i = 1; i < attributes.Length; i += 2) {
                w.Write(attributes[i - 1]);
                w.Write("=\"");
                w.Write(attributes[i]);
                w.Write("\" ");
            }
            w.Write('>');
        }

        public static void WTag(TextWriter w, String tagName, String[] attributes) {
            WTag(w, tagName, attributes, null);
        }

        public static void WTag(TextWriter w, String tagName, String[] attributes, String text) {
            WStartTag(w, tagName, attributes);
            w.Write(text == null ? "" : text);
            WEndTag(w, tagName);
        }
    }


    public class MailService {

        private static readonly Logger logger = Logger.GetLogger(typeof(MailService));
        private static MailService instance;

        public static MailService GetInstance() {
            return instance == null ? instance = new MailService() : instance;
        }

        // eg @"~\web.config"
        public static string WEB_CONFIG_PATH { get; set; }



        private List<_Mail> mailsToSend = new List<_Mail>();

        private class _Mail {
            public IEnumerable<String> tos;
            public string subject;
            public string text;
            public bool isBodyHtml;
            public _Mail(IEnumerable<String> tos, string subject, string text, bool isBodyHtml) {
                this.tos = tos;
                this.subject = subject;
                this.text = text;
                this.isBodyHtml = isBodyHtml;
            }
        }


        private MailService() { }

        //private bool SendHtmlMail(IEnumerable<String> tos, string subject, string htmlText) {
        //    return SendMail(tos, subject, htmlText, true);
        //}


        //private bool SendTextMail(IEnumerable<String> tos, string subject, string text) {
        //    return SendMail(tos, subject, text, false);
        //}

        public bool AddMail(IEnumerable<String> tos, string subject, string text, bool isHtmlBody) {
            lock(mailsToSend) {
                mailsToSend.Add(new _Mail(tos, subject, text, isHtmlBody));
                return true;
            }
        }

        public bool SendAllMails() {
            lock(mailsToSend) {
                foreach(_Mail mail in mailsToSend) {
                    try {
                        SendMail(mail.tos, mail.subject, mail.text, mail.isBodyHtml);
                    }
                    catch(Exception e) {
                        logger.Error(e, e);
                    }
                }
                mailsToSend.Clear();
            }
            return true;
        }


        public bool SendMail(IEnumerable<String> tos, string subject, string text, bool isBodyHtml) {
            try {
                Configuration config = WebConfigurationManager.OpenWebConfiguration(WEB_CONFIG_PATH);
                MailSettingsSectionGroup settings =
                        (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(settings.Smtp.From);
                foreach(string to in tos) {
                    msg.To.Add(new MailAddress(to));
                }
                msg.Subject = subject;
                msg.Body = text;
                msg.IsBodyHtml = isBodyHtml;
                msg.Priority = MailPriority.High;
                SmtpClient c = new SmtpClient(settings.Smtp.Network.Host);
                if(StringUtils.IsNotEmpty(settings.Smtp.Network.UserName)) {
                    NetworkCredential SMTPUserInfo = new NetworkCredential(
                        settings.Smtp.Network.UserName,
                        settings.Smtp.Network.Password);
                    c.Credentials = SMTPUserInfo;
                }
                c.Send(msg);
                return true;
            }
            catch(Exception e) {
                logger.Warn("Could not send Mail", e);
                return false;
            }
        }


    }




}