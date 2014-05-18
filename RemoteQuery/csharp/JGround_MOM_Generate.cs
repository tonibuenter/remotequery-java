//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using Org.JGround.Util;


namespace Org.JGround.MOM.Generate {


    public static class DefGenerator {
        private static Logger logger = Logger.GetLogger(typeof(DefGenerator));

        public static void ProcessSanityCheck() {
            foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
                moClass.ProcessSanityCheck();
            }
        }

        public static void ProcessDEFClassGeneration(String classOutputDirectory, String classFilename, String nameSpace) {
            StreamWriter w = null;
            using(w = new StreamWriter(File.Open(Path.Combine(classOutputDirectory, classFilename), FileMode.Create))) {
                w.WriteLine("// Date and time of generation : " + DateTime.Now);
                w.WriteLine("using System;");
                w.WriteLine();
                w.WriteLine("namespace " + nameSpace + " {");
                w.WriteLine();
                w.WriteLine("    public static class DEF {");
                w.WriteLine();

                foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
                    String moid = moClass.GetMoid();
                    int index = moid.LastIndexOf('.');
                    if(index < 0) {
                        logger.Warn("no prefix - no class is generated");
                    } else {
                        //String nameSpace = moid.Substring(0, index);
                        String name = moid.Substring(index + 1);
                        w.WriteLine();
                        // w.WriteLine("        public static class " + moClass.GetName().Replace(" ", "_") + " {");
                        w.WriteLine("        public static class " + name + " {");
                        w.WriteLine();
                        WriteStaticReadonlyAttribute(w, MO.AttName.moid, moClass.GetMoid());
                        WriteStaticReadonlyAttribute(w, "MO_NAME", moClass.GetName());

                        foreach(MOAttribute moAttribute in moClass.GetAllMOAttributes()) {
                            WriteStaticReadonlyAttribute(w, moAttribute.GetName().Replace(" ", "_"), moAttribute.GetName());
                        }
                        w.WriteLine();
                        w.WriteLine("        }");
                        w.WriteLine();
                    }
                }

                // 
                // CODE TABLE NAMES
                //
                List<String> codeTableNames = new List<string>();
                w.WriteLine("        public static class CODETABLE {");
                w.WriteLine();

                foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
                    foreach(MOAttribute moAttribute in moClass.GetAllMOAttributes()) {
                        MOType t = moAttribute.GetMOType().GetBaseType();
                        if(t is MOTypeCodeTable) {
                            MOTypeCodeTable ct = (MOTypeCodeTable)t;
                            codeTableNames.AddUnique(ct.GetCodeTableName());
                        }
                    }
                }

                foreach(String name in codeTableNames) {
                    WriteStaticReadonlyAttribute(w, name.ToUpper(), name);
                }
                w.WriteLine("        }");
                w.WriteLine("    }");

                w.WriteLine();
                w.WriteLine("}");

            }

        }

        private static void WriteStaticReadonlyAttribute(StreamWriter w, String name, String value) {
            w.WriteLine("            public static readonly String " + name + " =\"" + value + "\";");
        }



    }


    public static class CodeTableGenerator {
        public static void Run(String codetableName, String inputFile, String outputDir) {
            StreamWriter w = null;
            using(StreamReader r = new StreamReader(inputFile))
            using(w = new StreamWriter(File.Open(Path.Combine(outputDir, "codelist-" + codetableName + ".xml"), FileMode.Create))) {
                String line = null;
                w.WriteLine("<codetable name=\"" + codetableName + "\">");
                while((line = r.ReadLine()) != null) {
                    String[] v = line.Split('=');
                    if(v.Length == 2 && !v[0].Trim().ToLower().Equals("list")) {

                        w.WriteLine("<element><code>" + v[0].Trim() + "</code><name>" + v[1].Trim() + "</name><description /></element>");
                    }
                }
                w.WriteLine("</codetable>");
            }
        }
    }

    //public static class DEF {

    //        public static class Designer {
    //            public static readonly String moid = "ch.vitra.vips.Designer";
    //            public static readonly String firstName = "firstName";
    //            public static readonly String lastName = "lastName";
}
