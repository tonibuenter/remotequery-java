//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;

//
namespace Org.JGround.Util {

    public static class Locker {
        public static readonly Object MUTEX = new Object();
    }

    public class Logger {

        private static Object mutex2 = new Object();

        public enum LogLevels { DEBUG = 1, INFO = 2, WARN = 3, ERROR = 4, FATAL = 5 };

        public static LogLevels ToLevel(String level) {
            return (LogLevels)Enum.Parse(typeof(LogLevels), level.ToUpper());
        }

        public static LogLevels GLOBAL_LOG_LEVEL = LogLevels.DEBUG;

        private static Dictionary<String, LogLevels> levels_intern = new Dictionary<string, LogLevels>();
        private static Dictionary<String, LogLevels> levels = new Dictionary<string, LogLevels>();

        public static void LOG_LEVEL(LogLevels logLevel, Type type) {
            LOG_LEVEL(logLevel, type.GetClassName());
        }

        public static void LOG_LEVEL(LogLevels logLevel, String name) {
            Dictionary<String, LogLevels> levels_tmp;
            lock(mutex2) {
                levels_intern.Put(name, logLevel);
                levels_tmp = new Dictionary<string, LogLevels>(levels_intern);
            }
            levels = levels_tmp;
        }

        public static String LOG_DIR { set; get; }

        private static TextWriter tw;
        public static TextWriter GetWriter() {
            if(tw == null) {
                TextWriter s = new StreamWriter(GetCurrentLogFile());
                tw = StreamWriter.Synchronized(s);
            }
            return tw;
        }

        public static Logger GetLogger(String claxx) {
            return new Logger(claxx);
        }

        public static Logger GetLogger(Type claxx) {
            return new Logger(claxx.GetClassName());
        }

        private String loggerName;

        private Logger(String name) {
            loggerName = name;
        }

        public void Debug(params Object[] objs) {
            // lock (mutex)
            WriteLog(LogLevels.DEBUG, objs);
        }

        public void Warn(params Object[] objs) {
            // lock (mutex)
            WriteLog(LogLevels.WARN, objs);
        }

        public void Info(params Object[] objs) {
            // lock (mutex)
            WriteLog(LogLevels.INFO, objs);
        }

        public void Error(params Object[] objs) {
            //  lock (mutex)
            WriteLog(LogLevels.ERROR, objs);
        }

        public void Fatal(params Object[] objs) {
            //   lock (mutex)
            WriteLog(LogLevels.FATAL, objs);
        }

        public bool IsDebug() {
            return GLOBAL_LOG_LEVEL == LogLevels.DEBUG;
        }

        private delegate void log(object o);



        private void WriteLog(LogLevels level, Object[] objs) {
            TextWriter w = null;
            log[] lws = null;
            try {
                LogLevels ll = levels.Get(this.loggerName);
                if(ll == 0) {
                    ll = GLOBAL_LOG_LEVEL;
                }
                if(level - ll >= 0) {
                    lock(mutex2) {
                        if(LOG_DIR != null) {
                            w = File.AppendText(GetCurrentLogFile());
                            //  w = GetWriter();
                            lws = new log[] { Console.Write, w.Write };
                        } else {
                            lws = new log[] { Console.Write };
                        }
                        WriteStart(level, lws);
                        foreach(Object obj in objs) {
                            Write(obj, lws);
                        }
                        WriteEnd(lws);
                    }
                }
            }
            finally {
                if(w != null) {
                    w.Close();
                }
            }
        }

        private void Write(Object obj, log[] lw) {
            Console.Write(obj.ToString() + "; ");
            foreach(log w in lw) {
                w((obj != null ? obj.ToString() : "null") + ";");
            }
        }

        private void WriteStart(LogLevels level, log[] lw) {
            foreach(log w in lw) {
                w(level + " : " + loggerName + " : " + String.Format("{0:HH:mm:ss:ffff}", DateTime.Now) + " : ");
            }
        }

        private void WriteEnd(log[] lw) {
            foreach(log w in lw) {
                w(Environment.NewLine);
            }
        }

        public static String GetCurrentLogFile() {
            return Path.Combine(LOG_DIR, String.Format("{0:yyyy-MM-dd}", DateTime.Now)) + "-log.txt";
        }


    }


    public static class TypeExtensions {

        public static String GetClassName(this Type type) {
            return String.Intern(type.Namespace + "." + type.Name);
        }

    }


    public class IEnumerableUtils {


        public static String Join<E>(IEnumerable<E> enumerable, String separator) {
            if(enumerable == null) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            foreach(E e in enumerable) {
                if(counter > 0) {
                    sb.Append(separator);
                }
                sb.Append(e);
                counter++;
            }
            return sb.ToString();
        }

        public static E First<E>(IEnumerable<E> enumerable) {
            if(enumerable == null) {
                return default(E);
            }
            foreach(E e in enumerable) {
                return e;
            }
            return default(E);
        }

        public static String Join<E>(IEnumerable<E> enumerable) {
            return Join(enumerable, ",");
        }



    }
    public class ICollectionUtils {

        //public static bool IsNotEmpty(ICollection collection) {
        //    return !IsEmpty(collection);
        //}

        public static bool IsNotEmpty<E>(ICollection<E> collection) {
            return !IsEmpty(collection);
        }

        public static bool IsEmpty<E>(ICollection<E> collection) {
            return collection == null || collection.Count == 0;
        }


        public static String Join<E>(ICollection<E> collection, String separator) {
            if(ICollectionUtils.IsEmpty(collection)) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            foreach(E e in collection) {
                if(counter > 0) {
                    sb.Append(separator);
                }
                sb.Append(e);
                counter++;
            }
            return sb.ToString();
        }

        public static String Join<E>(ICollection<E> collection) {
            return Join(collection, ",");
        }

    }

    public static class ICollectionExtensions {

        public static E Get<E>(this ICollection<E> collection, int index) {
            int i = 0;
            foreach(E obj in collection) {
                if(i == index) {
                    return obj;
                }
                i++;
            }
            throw new IndexOutOfRangeException("last index " + i + "  you tried " + index);
        }

        public static List<E> ToList<E>(this ICollection<E> collection) {
            List<E> res = new List<E>();
            res.AddRange(collection);
            return res;
        }




    }


    public class ListUtils : ICollectionUtils {

        public static List<E> EmptyList<E>() {
            return new List<E>();
        }

        public static List<E> UnifyUnique<E>(params List<E>[] lists) {
            List<E> res = new List<E>();
            foreach(List<E> list in lists) {
                if(ICollectionUtils.IsNotEmpty(list)) {
                    foreach(E el in list) {
                        res.AddUnique(el);
                    }
                }
            }
            return res;
        }

        public static List<E> UnifyUnique<E>(List<E> list1, E[] list2) {
            List<E> res = new List<E>();
            if(ICollectionUtils.IsNotEmpty(list1)) {
                foreach(E el in list1) {
                    res.AddUnique(el);
                }
            }
            if(ArrayUtils.IsNotEmpty(list2)) {
                foreach(E el in list2) {
                    res.AddUnique(el);
                }
            }
            return res;
        }

        public static List<E> RNN<E>(List<E> list) {
            if(list == null) {
                return EmptyList<E>();
            } else {
                return list;
            }
        }
    }

    public static class ListExtensions {

        public static bool AddUnique<T>(this List<T> list, T obj) {
            if(obj == null) {
                return false;
            }
            if(list.Contains(obj)) {
                return false;
            } else {
                list.Add(obj);
                return true;
            }
        }

        public static void AddUnique<T>(this List<T> list, IList<T> objs) {
            foreach(T obj in objs) {
                list.AddUnique(obj);
            }
        }

        public static E Get<E>(this List<E> list, int index) {
            return list[index];
        }

        public static E GetLast<E>(this List<E> list) {
            if(list.Count > 0) {
                return list[list.Count - 1];
            }
            return default(E);
        }

        public static List<E> Subtract<E>(this List<E> list, List<E> subtractList) {
            List<E> res = new List<E>();
            foreach(E el in list) {
                if(!subtractList.Contains(el)) {
                    res.Add(el);
                }
            }
            return res;
        }

        public static List<E> Subtract<E>(this List<E> list, E[] subtractList) {
            List<E> res = new List<E>();
            foreach(E el in list) {
                if(!ArrayUtils.Contains(subtractList, el)) {
                    res.Add(el);
                }
            }
            return res;
        }
    }




    public class StringUtils {

        public static bool IsNotEmpty(String str) {
            return str != null && str.Length > 0;
        }

        public static bool IsNotBlank(String s) {
            return IsNotEmpty(s) && IsNotEmpty(s.Trim());
        }


        public static bool IsEmpty(String str) {
            return str == null || str.Length == 0;
        }

        public static bool IsBlank(string s) {
            return s == null || s.Trim().Length == 0;
        }

        public static String RNN(String str) {
            return str == null ? "" : str;
        }
        public static String RNA(String str) {
            return str == null ? "n/a" : str;
        }

        public static String toLetters(String s) {
            if(StringUtils.IsNotBlank(s)) {
                StringBuilder sb = new StringBuilder();
                foreach(char c in s) {
                    if(Char.IsLetterOrDigit(c) || c == '.') {
                        sb.Append(c);
                    } else {
                        sb.Append("_");
                    }
                }
                return sb.ToString();
            }
            return s;
        }

        public static String toNumbers(String s) {
            if(StringUtils.IsNotBlank(s)) {
                StringBuilder sb = new StringBuilder();
                foreach(char c in s) {
                    if(Char.IsDigit(c)) {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }
            return null;
        }

        public static String[] RemoveEmptyElements(String[] sarray) {
            List<String> list = new List<string>();
            if(sarray == null) {
                return default(String[]);
            }
            foreach(String s in sarray) {
                if(IsNotEmpty(s)) {
                    list.Add(s);
                }
            }
            return list.ToArray();
        }

        public static String CutAndEllipsis(String str, int maxLength) {
            int max2 = maxLength - 3;
            if(str.Length > max2) {
                str = str.Substring(0, max2) + "...";
            }
            return str;
        }



        internal static bool EqualsIgnoreCase(string s1, string s2) {
            if(s1 == s2) {
                return true;
            }
            if(s1 != null && s1 != null) {
                return s1.Equals(s2, StringComparison.CurrentCultureIgnoreCase);
            }
            return false;
        }
    }

    public static class ObjectUtils {

        public static String ToString(Object obj) {
            return obj == null ? "" : obj.ToString();
        }

        public static T[] ToArray<T>(params T[] objs) {
            return objs;
        }

        public static bool AreEquals(Object value, Object newValue) {
            if(value == newValue) {
                return true;
            }
            if(value == null || newValue == null) {
                return false;
            }
            return value.Equals(newValue);
        }

        public static bool AreNotEquals(Object value, Object newValue) {
            return !AreEquals(value, newValue);
        }

        public static Object Clone(Object obj) {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            binaryFormatter.Serialize(memoryStream, obj);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return binaryFormatter.Deserialize(memoryStream);
        }



    }

    public static class EnumUtils {

        //public static String[] ToStringArray(Enum[] enumArray) {
        //    String[] ar = new String[enumArray.Length];
        //    for (int i = 0; i < ar.Length; i++) {
        //        ar[i] = enumArray[i].ToString();
        //    }
        //    return ar;
        //}

        public static String[] ToStringArray(IList enumList) {
            String[] ar = new String[enumList.Count];
            for(int i = 0; i < ar.Length; i++) {
                ar[i] = enumList[i].ToString();
            }
            return ar;
        }
    }


    public static class ArrayUtils {

        public static E[] EmptyList<E>() {
            return new E[] { };
        }

        public static E[] Add<E>(E[] array, E element) {
            E[] array2 = new E[array.Length + 1];
            Array.Copy(array, array2, array.Length);
            array2[array.Length] = element;
            return array2;
        }

        public static E[] Add<E>(E[] array, params E[] elements) {
            E[] array2 = new E[array.Length + elements.Length];
            Array.Copy(array, array2, array.Length);
            Array.ConstrainedCopy(elements, 0, array2, array.Length, elements.Length);
            return array2;
        }

        public static E[] AddUnique<E>(E[] array, E element) {
            if(Contains(array, element)) {
                return array;
            }
            return Add(array, element);
        }

        public static E[] AddUnique<E>(E[] array, params E[] additionalsElements) {
            List<E> list = null;
            if(IsNotEmpty(array)) {
                list = new List<E>(array);
            } else {
                list = new List<E>();
            }
            foreach(E element in additionalsElements) {
                if(!list.Contains(element)) {
                    list.Add(element);
                }
            }
            return list.ToArray();
        }


        public static String Join<E>(E[] array, String separator) {
            if(IsEmpty(array)) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < array.Length; i++) {
                if(i > 0) {
                    sb.Append(separator);
                }
                sb.Append(array[i]);
            }
            return sb.ToString();
        }

        public static String Join(Object[] array) {
            return Join(array, ",");
        }

        public static bool IsEmpty<E>(E[] array) {
            if(array == null || array.Length == 0) {
                return true;
            } else {
                return false;
            }
        }

        public static bool IsNotEmpty<E>(E[] array) {
            return !IsEmpty(array);
        }

        public static bool Contains<E>(E[] array, E element) {
            if(array == null) {
                return false;
            }
            foreach(E o in array) {
                if(o.Equals(element)) {
                    return true;
                }
            }
            return false;
        }

        public static E[] RNN<E>(E[] array) {
            if(array == null) {
                return new E[] { };
            } else {
                return array;
            }
        }


        public static bool AreEquals(Object[] values, Object[] newValues) {
            if(values == newValues) {
                return true;
            }
            if(values == null || newValues == null) {
                return false;
            }
            if(values.Length != newValues.Length) {
                return false;
            }
            for(int i = 0; i < values.Length; i++) {
                if(!ObjectUtils.AreEquals(values[i], newValues[i])) {
                    return false;
                }
            }
            return true;
        }


        public static bool OneStartsWith(string[] values, string startsWithValue) {
            foreach(String value in values) {
                if(value == startsWithValue) {
                    return true;
                }
                if(value == null) {
                    continue;
                }
                if(value.StartsWith(startsWithValue)) {
                    return true;
                }
            }
            return false;
        }


        public static bool OneContains(string[] values, string containsValue, bool ignoreCase) {
            if(containsValue == null) {
                return false;
            }
            foreach(String value in values) {
                if(value == containsValue) {
                    return true;
                }
                if(value == null) {
                    continue;
                }
                if(ignoreCase) {
                    if(value.ToLower().IndexOf(containsValue.ToLower()) > -1) {
                        return true;
                    }
                } else {
                    if(value.IndexOf(containsValue) > -1) {
                        return true;
                    }
                }
            }
            return false;
        }


        public static Object First(Object[] values) {
            if(values == null || values.Length == 0) {
                return null;
            } else {
                return values[0];
            }
        }

        public static string[] Trim(String[] p) {
            List<String> list = new List<string>();
            if(IsNotEmpty(p)) {
                for(int i = 0; i < p.Length; i++) {
                    p[i] = p[i].Trim();
                    if(p[i].Length > 0) {
                        list.Add(p[i]);
                    }
                }
            }
            return list.ToArray();
        }

        public static int Length(Object[] p) {
            return IsNotEmpty(p) ? p.Length : 0;
        }



        public static E[] Subtract<E>(E[] minuend, E[] subtrahend) {
            List<E> difference = new List<E>();
            foreach(E el in subtrahend) {
                if(!ArrayUtils.Contains(minuend, el)) {
                    difference.Add(el);
                }
            }
            return difference.ToArray();
        }



        public static E[] Intersection<E>(E[] set1, E[] set2) {
            if(set1 == null || set2 == null) {
                return EmptyList<E>();
            }
            List<E> intersection = new List<E>();
            foreach(E el in set1) {
                if(ArrayUtils.Contains(set2, el)) {
                    intersection.Add(el);
                }
            }
            return intersection.ToArray();
        }

    }


    //
    // Collection Helpers
    //

    public static class DictionaryExtensions {

        public static void Put<K, V>(this Dictionary<K, V> dic, K key, V value, bool overwrite) {
            if(dic.ContainsKey(key)) {
                if(overwrite == false) {
                    return;
                }
                dic.Remove(key);
            }
            dic.Add(key, value);
        }

        public static void Put<K, V>(this Dictionary<K, V> dic, K key, V value) {
            Put(dic, key, value, true);
        }

        public static V Get<K, V>(this Dictionary<K, V> dic, K key) {
            if(dic.ContainsKey(key)) {
                return dic[key];
            }
            return default(V);
        }

    }



    public static class XmlElementHelper {

        private static Logger logger = Logger.GetLogger(typeof(XmlElementHelper));

        public static String GetTextByTagName(this XmlElement xml, String tagName) {
            if(xml.GetElementsByTagName(tagName).Count > 1) {
                logger.Warn("found more than one element for '" + xml.InnerText + "'");
            }
            foreach(XmlElement element in xml.GetElementsByTagName(tagName)) {
                return element.InnerText;
            }
            return null;
        }

        public static XmlElement GetFirstChild(this XmlElement element, String tagName) {
            XmlNodeList list = element.GetElementsByTagName(tagName);
            if(list == null || list.Count == 0) {
                return null;
            } else {
                return (XmlElement)list[0];
            }
        }

        public static List<XmlElement> GetChildren(this XmlElement element, params String[] names) {
            List<XmlElement> res = new List<XmlElement>();
            foreach(Object o in element) {
                if(o is XmlElement) {
                    XmlElement child = (XmlElement)o;
                    if(ArrayUtils.IsNotEmpty(names)) {
                        if(ArrayUtils.Contains(names, child.Name)) {
                            res.Add(child);
                        }
                    } else {
                        res.Add(child);
                    }
                }
            }
            return res;
        }

        public static XmlElement GetFirstChild(this XmlElement element) {
            XmlNodeList list = element.ChildNodes;
            foreach(XmlNode node in list) {
                if(node.GetType() == typeof(XmlElement)) {
                    return (XmlElement)node;
                }
            }
            return null;
        }

        public static String GetAttributeValue(this XmlElement element, String attributeName) {
            XmlAttribute att = element.Attributes[attributeName];
            if(att != null) {
                return att.Value;
            } else {
                return null;
            }
        }

        public static String GetAttributeValueIgnoreCase(this XmlElement element, String attributeName) {
            attributeName = attributeName.ToLower();
            foreach(XmlAttribute attribute in element.Attributes) {
                if(attribute.Name.ToLower().Equals(attributeName)) {
                    return attribute.Value;
                }
            }
            return null;
        }
    }

    public static class DateTimeUtils {

        public static long OneDayInMillis = 24 * 60 * 60 * 1000;

        public static DateTime Create(long ticks) {
            return new DateTime(ticks);
        }

        public static DateTime Parse(String s) {
            return DateTime.Parse(s);
        }

        public static bool TryParse(String s) {
            DateTime result;
            return DateTime.TryParse(s, out result);
        }


        public static String FormatDateTime(long ticks) {
            return String.Format("{0:yyyy-MM-dd HH:mm}", new DateTime(ticks));
        }

        public static String FormatDateTime(DateTime dateTime) {
            return String.Format("{0:yyyy-MM-dd HH:mm}", dateTime);
        }

        public static String FormatDate(long ticks) {
            return String.Format("{0:yyyy-MM-dd}", new DateTime(ticks));
        }

        public static String FormatDate(DateTime dateTime) {
            return String.Format("{0:yyyy-MM-dd}", dateTime);
        }

        public static long TicksToMillis(long ticks) {
            return ticks / 10000;
        }

        public static long MillisToTicks(long millis) {
            return millis * 10000;
        }

        public static long NowInMillis() {
            return TicksToMillis(DateTime.Now.Ticks);
        }


        private static LocalDataStoreSlot timeSlot = Thread.GetNamedDataSlot("_time_snap_slot");

        public static long GetTimeSlotFromThread(String key) {
            Dictionary<String, long> times = (Dictionary<String, long>)Thread.GetData(timeSlot);
            long ticks = DateTime.Now.Ticks;
            if(times != null && times.ContainsKey(key)) {
                ticks = times.Get(key);
            }
            return ticks;
        }

        public static void SetTimeSlotToThread(String key, long nowTicks) {
            Dictionary<String, long> times = (Dictionary<String, long>)Thread.GetData(timeSlot);
            if(times == null) {
                times = new Dictionary<String, long>();
                Thread.SetData(timeSlot, times);
            }
            times.Put(key, nowTicks);
        }

        public static String PerformanceLoggerName = "performace.Logger";
        private static Logger perfLogger = Logger.GetLogger(PerformanceLoggerName);

        public static void StartTime(String key) {
            SetTimeSlotToThread(key, DateTime.Now.Ticks);
        }

        public static long LogTime(String key) {
            long now = GetTimeSlotFromThread(key);
            long diff = TicksToMillis(DateTime.Now.Ticks - now);
            perfLogger.Info(key, "milliseconds", diff);
            return diff;
        }


        public static String tryIsoDate(String value) {
            if(StringUtils.IsNotBlank(value)) {
                DateTime dt;
                if(DateTime.TryParse(value, out dt)) {
                    return String.Format("{0:yyyy-MM-dd}", dt);
                }
            }
            return "";
        }
    }

    public class TimeTracker {

        private double startTime = System.DateTime.Now.Ticks;

        public void PrintConsole() {
            double currentTime = System.DateTime.Now.Ticks;
            Console.WriteLine("TT sec: " + GetDelayAndReset());
            startTime = currentTime;
        }

        public double GetDelayAndReset() {
            double currentTime = System.DateTime.Now.Ticks;
            double delay = (currentTime - startTime) / TimeSpan.TicksPerSecond;
            startTime = currentTime;
            return delay;
        }

    }



    public static class Assert {

        private static Logger logger = Logger.GetLogger(typeof(Assert));

        public static void True(bool v, Object info) {
            logger.Debug("True: ", v, info);
            if(!v) {
                throw new Exception("True expected: " + info);
            }
        }

        public static void Equal(Object o1, Object o2) {
            logger.Debug("Equal: ", o1 ?? "null", o2 ?? "null");
            if(o1 == o2) {
                return;
            }
            if(!o1.Equals(o2)) {
                throw new Exception("EQUAL EXPECTED: " + o1 + " : " + o2);
            }
        }

        public static void NotEqual(Object o1, Object o2) {
            logger.Debug("NotEqual: ", o1, o2);
            if(o1.Equals(o2)) {
                throw new Exception("NOT EQUAL EXPECTED: " + o1 + " : " + o2);
            }
        }
    }

    public static class StreamWriterExtensions {

        public static void WriteExcel(this StreamWriter w, String data) {
            foreach(char c in data) {
                if(c == '\t' || c == '\n' || c == '\r') {
                    w.Write(' ');
                } else {
                    w.Write(c);
                }
            }
        }
        public static void WriteExcelPercentage(this StreamWriter w, String data) {
            if(StringUtils.IsNotBlank(data)) {
                w.WriteExcel(data + "%");
            }
        }
    }


    public static class PathUtils {

        public static String CreateSaveFileName(String fileName) {
            fileName = fileName.Replace(@"\", "_");
            fileName = fileName.Replace(@"/", "_");
            return fileName;
        }
    }

    public static class IOUtils {

        public static void EmptyDirectory(String dir, String searchPattern) {
            if(Directory.Exists(dir)) {
                String[] files = Directory.GetFiles(dir, searchPattern);
                foreach(String file in files) {
                    File.Delete(file);
                }
                String[] directories = Directory.GetDirectories(dir, searchPattern);
                foreach(String directory in directories) {
                    Directory.Delete(directory);
                }
            }
        }

    }



    public class StringTokenizer2 {

        private int index = 0;
        private String[] tokens = null;
        private StringBuilder buf;
        private bool ignoreWhiteSpace = true;

        public StringTokenizer2(String str, char del, char esc)
            : this(str, del, esc, true) { }

        public StringTokenizer2(
            String str,
            char del,
            char esc,
            bool ignoreWhiteSpace) {
            this.ignoreWhiteSpace = ignoreWhiteSpace;
            // first we count the tokens
            int count = 1;
            bool inescape = false;
            char c;
            buf = new StringBuilder();
            for(int i = 0; i < str.Length; i++) {
                c = str[i];
                if(c == del && !inescape) {
                    count++;
                    continue;
                }
                if(c == esc && !inescape) {
                    inescape = true;
                    continue;
                }
                inescape = false;
            }
            tokens = new String[count];

            // now we collect the characters and create all tokens
            int k = 0;
            for(int i = 0; i < str.Length; i++) {
                c = str[i];
                if(c == del && !inescape) {
                    tokens[k] = buf.ToString();
                    buf.Remove(0, buf.Length);
                    k++;
                    continue;
                }
                if(c == esc && !inescape) {
                    inescape = true;
                    continue;
                }
                // TOKENIZER BUG
                if(inescape && c == 'n') {
                    buf.Append('\n');
                } else if(inescape && c == 't') {
                    buf.Append('\t');
                } else {
                    buf.Append(c);
                }
                // buf.Append(c);
                // TOKENIZER BUG
                inescape = false;
            }
            tokens[k] = buf.ToString();
        }

        public bool HasMoreTokens() {
            return index < tokens.Length;
        }

        public String NextToken() {
            String token = tokens[index];
            index++;
            return ignoreWhiteSpace ? token.Trim() : token;
        }

        public int CountTokens() {
            return tokens.Length;
        }

        public String[] GetAllTokens() {
            return tokens;
        }

        /** 
       * Static convenience method for 
       * converting a string directly into an array of 
       * String by using the delimiter and escape character as specified.
       */
        public static String[] ToTokens(String line, char delim, char escape) {
            StringTokenizer2 tokenizer = new StringTokenizer2(line, delim, escape);
            return tokenizer.GetAllTokens();
        }

        /**
         * Create a string with the delimiter an escape character as specified.
         */
        public static String ToString(String[] tokens, char delim, char escape) {

            String token = null;
            int i, j;
            char c;
            StringBuilder buff = new StringBuilder();

            for(i = 0; i < tokens.Length; i++) {
                token = tokens[i];
                for(j = 0; j < token.Length; j++) {
                    c = token[j];
                    if(c == escape || c == delim) {
                        buff.Append(escape);
                    }
                    buff.Append(c);
                }
                buff.Append(delim);
            }
            if(buff.Length > 0)
                buff.Remove(buff.Length - 1, 1);
            return buff.ToString();
        }

    }



    public class ToLineString {

        public static readonly char DELIM = '|';
        public static readonly char ESCAPE = '\\';

        private char delim;
        private char escape;
        private StringBuilder acc = new StringBuilder();

        public ToLineString(char delim, char escape) {
            this.delim = delim;
            this.escape = escape;
        }

        public ToLineString() :
            this(DELIM, ESCAPE) {
        }

        public void Add(String s) {
            for(int i = 0; i < s.Length; i++) {
                char c = s[i];
                if(c == delim) {
                    acc.Append(escape);
                    acc.Append(c);
                } else if(c == escape) {
                    acc.Append(escape);
                    acc.Append(c);
                } else
                    // TOKENIZER BUG
                    if(c == '\n') {
                        acc.Append(ESCAPE);
                        acc.Append("n");
                    } else if(c == '\t') {
                        acc.Append(ESCAPE);
                        acc.Append("t");
                        //} else if (c == '\n') {
                        //    acc.Append("\\\\n");
                        //} else if (c == '\t') {
                        //    acc.Append("\\\\t");
                        //    TOKENIZER BUG
                    } else if(Char.IsWhiteSpace(c) && c != ' ') {
                        // do nothing
                    } else {
                        acc.Append(c);
                    }
            }
            acc.Append(delim);
        }

        public void Add(long value) {
            acc.Append(value);
            acc.Append(delim);
        }

        public void Add(int value) {
            acc.Append(value);
            acc.Append(delim);
        }

        public void Add(bool value) {
            acc.Append(value);
            acc.Append(delim);
        }

        public void Add(List<long> longList) {
            Add(ArrayUtils.Join(longList.ToArray(), " "));
        }

        public void Clear() {
            acc.Remove(0, acc.Length);
        }

        public override String ToString() {
            return acc.ToString();
        }

        public void WriteLine(StreamWriter writer) {
            int len = acc.Length > 0 ? acc.Length - 1 : acc.Length;
            for(int i = 0; i < len; i++) {
                writer.Write(acc[i]);
            }
            writer.WriteLine();
        }
    }


    public class FromLineString {

        private static Logger logger = Logger.GetLogger(typeof(FromLineString));

        public static readonly char DELIM = '|';
        public static readonly char ESCAPE = '\\';

        private String[] tokens;
        private int index = 0;

        public FromLineString(String input, char delim, char escape) {
            StringTokenizer2 tokenizer = new StringTokenizer2(input, delim, escape);
            tokens = tokenizer.GetAllTokens();
        }

        public FromLineString(String input) : this(input, DELIM, ESCAPE) { }

        public String GetString() {
            String token = tokens[index++];
            if(logger.IsDebug()) {
                logger.Debug("index:" + (index - 1) + " token:" + token);
            }
            // TOKENIZER BUG
            // return Line2text(token);
            return token;
            // TOKENIZER BUG

        }

        public long GetLong() {
            String token = tokens[index++];
            if(logger.IsDebug()) {
                logger.Debug("index:" + (index - 1) + " token:" + token);
            }
            return Int64.Parse(token);
        }

        public int GetInt() {
            String tmp = tokens[index++];
            logger.Debug("index:" + (index - 1) + " token:" + tmp);
            return Int32.Parse(tmp);
        }

        public bool GetBoolean() {
            String token = tokens[index++];
            if(logger.IsDebug()) {
                logger.Debug("index:" + (index - 1) + " token:" + token);
            }
            return token.Equals(true);
        }

        public int GetSize() {
            return tokens.Length;
        }

        public List<long> GetLongList() {
            String token = tokens[index++];
            if(logger.IsDebug()) {
                logger.Debug("index:" + (index - 1) + " token:" + token);
            }
            List<long> res = new List<long>();
            foreach(String s in token.Split(' ')) {
                res.Add(Int64.Parse(s));
            }
            return res;
        }
        // TOKENIZER BUG
        //public static String Line2text(String str) {
        //    String s = str.Replace("\\n", "\n");
        //    return s.Replace("\\t", "\t");
        //}
        // TOKENIZER BUG

        public bool HasMoreTokens() {
            return index < tokens.Length;
        }

        public int Count() {
            return tokens.Length;
        }
    }


    public class RatingList<E> {

        private class Slot<F> {
            internal int rating;
            internal F element;
            internal Slot(int rating, F element) {
                this.rating = rating;
                this.element = element;
            }
        }

        private List<Slot<E>> slots = new List<Slot<E>>();

        public void Insert(int rating, E element) {
            bool done = false;
            Slot<E> newSlot = new Slot<E>(rating, element);
            for(int i = 0; i < slots.Count; i++) {
                Slot<E> s = slots[i];
                if(s.rating <= rating) {
                    slots.Insert(i, newSlot);
                    done = true;
                    break;
                }
            }
            if(done == false) {
                slots.Add(newSlot);
            }
        }

        public List<E> ToList() {
            List<E> res = new List<E>();
            foreach(Slot<E> s in slots) {
                res.Add(s.element);
            }
            return res;
        }


    }

}
