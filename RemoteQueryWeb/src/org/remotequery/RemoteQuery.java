package org.remotequery;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Reader;
import java.io.Serializable;
import java.io.StringWriter;
import java.io.Writer;
import java.lang.reflect.Array;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.lang.reflect.Type;
import java.sql.Clob;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.SQLException;
import java.sql.Statement;
import java.sql.Timestamp;
import java.sql.Types;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Calendar;
import java.util.Collection;
import java.util.Date;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;
import java.util.TreeMap;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.sql.DataSource;

import org.remotequery.RemoteQueryServlet.WebConstants;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonPrimitive;
import com.google.gson.reflect.TypeToken;

/**
 * TODOs set-0, set-1, ... set-initial, set-parameter:, set-session, ...
 * set-application, ...
 * 
 * @author tonibuenter
 * 
 */
public class RemoteQuery {

	/**
	 * "DEFAULT_DATASOURCE" is the default data source name.
	 */
	public static String DEFAULT_DATASOURCE_NAME = "DEFAULT_DATASOURCE";

	/**
	 * UTF-8 is the default encoding.
	 */
	public static String ENCODING = "UTF-8";

	public static int MAX_RECURSION = 20;

	public static final char DEFAULT_DEL = ',';
	public static final char DEFAULT_ESC = '\\';

	public static class MLT {
		// code indication
		public static final String java = "java";
		public static final String sql = "sql";
		// combination
		public static final String serviceId = "serviceId";
		public static final String include = "include";
		// parameter manipulation
		public static final String set = "set";
		public static final String set_if_empty = "set-if-empty";
		public static final String set_if_null = "set-if-null";
		public static final String set_null = "set-null";
		//
		public static final String tx_begin = "tx-begin";
		public static final String tx_commit = "tx-commit";
	}

	public static class Params {
		public static String serviceId = "serviceId";
		public static String statements = "statements";
		public static String accessRoles = "accessRoles";
	}

	public static class DBColumns {
		public static String serviceId = "serviceId";
		public static String statements = "statements";
		public static String accessRoles = "accessRoles";
	}

	public static class LevelConstants {
		public static final int INITIAL = 0;
		public static final int REQUEST = 10;
		public static final int HEADER = 20;
		public static final int INTER_REQUEST = 30;
		public static final int SESSION = 40;
		public static final int APPLICATION = 50;
	}

	private static Map<String, Integer> LevelConstantNames = new HashMap<String, Integer>();
	static {
		LevelConstantNames.put("INITIAL", new Integer(LevelConstants.INITIAL));
		LevelConstantNames.put("REQUEST", new Integer(LevelConstants.REQUEST));
		LevelConstantNames.put("HEADER", new Integer(LevelConstants.HEADER));
		LevelConstantNames.put("INTER_REQUEST", new Integer(
		    LevelConstants.INTER_REQUEST));
		LevelConstantNames.put("SESSION", new Integer(LevelConstants.SESSION));
		LevelConstantNames.put("APPLICATION", new Integer(
		    LevelConstants.APPLICATION));
	}
	//
	//
	//

	public static char STATEMENT_DELIMITER = ';';
	public static char STATEMENT_ESCAPE = '\\';

	/**
	 * The DataSourceEntry class main responsibility is to provide a data source
	 * object.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class DataSourceEntry {

		/**
		 * Provide a data source object for the default data source name.
		 * 
		 * @param ds
		 */
		public DataSourceEntry(DataSource ds) {
			DataSources.getInstance().put(ds);
		}

		/**
		 * Provide a data source object for the dataSourceName.
		 * 
		 * @param ds
		 * @param dataSourceName
		 */

		public DataSourceEntry(DataSource ds, String dataSourceName) {
			DataSources.getInstance().put(dataSourceName, ds);
		}

	}

	/**
	 * Singleton for keeping the data source objects.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class DataSources {

		private static DataSources instance;

		public static DataSources getInstance() {
			return instance == null ? instance = new DataSources() : instance;
		}

		private Map<String, DataSource> dss = new HashMap<String, DataSource>();

		public DataSource get() {
			return dss.get(DEFAULT_DATASOURCE_NAME);
		}

		public DataSource get(String dataSourceName) {
			if (dataSourceName == null) {
				dataSourceName = DEFAULT_DATASOURCE_NAME;
			}
			return dss.get(dataSourceName);
		}

		public void put(String dataSourceName, DataSource ds) {
			dss.put(dataSourceName, ds);
		}

		public void put(DataSource ds) {
			dss.put(DEFAULT_DATASOURCE_NAME, ds);
		}

	}

	public static interface IQuery extends Serializable {

		Result run(Request request);

	}

	/**
	 * Interface for service repositories. Currently we have a JSON base and a SQL
	 * base service repository.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static interface IServiceRepository {

		ServiceEntry get(String serviceId) throws Exception;

		void add(ServiceEntry serviceEntry);

	}

	/**
	 * MainQuery is the main class the provides the processing of a RemoteQuery
	 * request. It takes care of the service statement parsing and processing.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class MainQuery implements IQuery {

		// TODO idea :: how to create your own plugins like 'my-sql-extension:select
		// %SELECTLIST% where %WHERE% clause'

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		private static final Logger logger = Logger.getLogger(MainQuery.class
		    .getName());

		private Map<String, Serializable> processStore = new HashMap<String, Serializable>();

		public void put(String key, Serializable value) {
			processStore.put(key, value);
		}

		public Serializable get(String key) {
			return processStore.get(key);
		}

		public Result run(RemoteQuery.Request request) {
			Result result = null;
			ProcessLog log = ProcessLog.Current();
			log.incrRecursion();
			String serviceId = request.getServiceId();
			String userId = request.getUserId();
			if (Utils.isEmpty(userId)) {
				log.warn(
				    "Request object has no userId set. Process continues with userId="
				        + WebConstants.ANONYMOUS, logger);
				request.setUserId(WebConstants.ANONYMOUS);
				userId = request.getUserId();
			}
			// TODO better in the process object ?
			try {
				//
				ServiceEntry serviceEntry = ServiceRepository.getInstance().get(
				    serviceId);
				if (serviceEntry == null) {
					log.error("No ServiceEntry found for " + serviceId, logger);
					return new Result(log);
				}
				//
				// CHECK ACCESS
				//
				boolean hasAccess = false;
				Set<String> accessRoles = serviceEntry.getAccessRole();
				if (Utils.isEmpty(accessRoles)) {
					hasAccess = true;
				} else {
					for (String accessRole : accessRoles) {
						if (request.getRoles().contains(accessRole)) {
							hasAccess = true;
							break;
						}
					}
				}
				if (hasAccess) {
					log.system("Access to " + serviceId + " for " + userId + " : ok",
					    logger);
				} else {
					log.warn("No access to " + serviceId + " for " + userId + " (roles: "
					    + accessRoles + ")", logger);
					return new Result(log);
				}
				//
				// START PROCESSING STATEMENTS
				//

				log.system("ServiceEntry found for userId=" + userId + " is : "
				    + serviceEntry, logger);

				String statements = serviceEntry.getStatements();

				String[] statementList = Utils.tokenize(statements,
				    STATEMENT_DELIMITER, STATEMENT_ESCAPE);

				// parameterSupport = ParameterSupport.begin(con, sqRequest, sqEntry);
				for (String statement : statementList) {
					Result result2 = processSingleStatement(request, serviceEntry,
					    statement);
					if (result2 != null) {
						if (result != null) {
							result2.setSubResult(result);
						}
						result = result2;
					}

				}

			} catch (Exception e) {
				log.error(e, logger);
			} finally {
				// ParameterSupport.release(parameterSupport);
			}
			if (result == null) {
				// TODO Default result object ?
			} else {
				result.setUserId(userId);
			}
			return result;
		}

		private Result processSingleStatement(Request request,
		    ServiceEntry serviceEntry, String serviceStmt) {
			ProcessLog log = ProcessLog.Current();
			try {
				Result result = null;

				log.incrRecursion();
				int recursion = log.getRecursionValue();
				if (recursion > MAX_RECURSION) {
					log.error("Recursion limit reached " + MAX_RECURSION
					    + ". Stop processing.", logger);
					return null;
				}

				serviceStmt = serviceStmt.trim();
				if (Utils.isEmpty(serviceStmt)) {
					log.warn("Empty serviceStmt -> no processing.", logger);
					return null;
				}

				String[] p = Utils.tokenize(serviceStmt, ':', STATEMENT_ESCAPE);

				// unexpected parsing result
				if (p == null || p.length == 0) {
					log.warn("Unexpected serviceStmt : " + serviceStmt + ". Skipping!",
					    logger);
					return null;
				}
				//
				// plain SQL case
				//
				if (p.length == 1 || !startsWithMLT(serviceStmt)) {
					result = processSql(serviceEntry, serviceStmt, request);
					return result;
				}
				String cmd = p[0];
				String stmt = p[1];
				//
				// include
				//
				if (cmd.equals(MLT.include)) {
					ServiceEntry se2 = ServiceRepository.getInstance().get(stmt);
					if (se2 == null) {
						log.warn("Tried to include " + stmt + ". Skipping.", logger);
						return null;
					}
					String includeServiceStmt = se2.getStatements();
					result = processSingleStatement(request, serviceEntry,
					    includeServiceStmt);
					return result;
				}
				//
				// java:
				//
				if (cmd.equals(MLT.java)) {
					result = processJava(stmt, request);
					return result;
				}
				//
				// sql:
				//
				if (cmd.equals(MLT.sql)) {
					result = processSql(serviceEntry, stmt, request);
					return result;
				}
				//
				// set, ... request parameter assignements
				//
				if (cmd.startsWith(MLT.set)) {
					applySetCommand(request, cmd, stmt);
					return null;
				}
				if (cmd.equals(MLT.serviceId)) {
					String parentServiceId = serviceEntry.getServiceId();
					String subServiceId = stmt;
					request.setServiceId(subServiceId);
					result = run(request);
					request.setServiceId(parentServiceId);
					return result;
				}
			} catch (Exception e) {
				log.error(
				    "ServiceStmt for " + serviceStmt + " failed. Skip execution.",
				    logger);

			} finally {
				log.decrRecursion();
			}
			return null;

		}

		public static boolean startsWithMLT(String serviceStmt) {
			if (serviceStmt.startsWith(MLT.include)) {
				return true;
			}
			if (serviceStmt.startsWith(MLT.java)) {
				return true;
			}
			if (serviceStmt.startsWith(MLT.serviceId)) {
				return true;
			}
			if (serviceStmt.startsWith(MLT.sql)) {
				return true;
			}
			if (serviceStmt.startsWith(MLT.set)) {
				return true;
			}
			if (serviceStmt.startsWith(MLT.tx_begin)) {
				return true;
			}
			if (serviceStmt.startsWith(MLT.tx_commit)) {
				return true;
			}
			return false;
		}

		public static void applySetCommand(Request request, String cmd, String stmt) {
			if (cmd.startsWith(MLT.set_null)) {
				TreeMap<Integer, Map<String, String>> ptm = request
				    .getParametersTreeMap();
				String[] keys = Utils.tokenize(stmt, ',');
				for (String key : keys) {

					for (Entry<Integer, Map<String, String>> e : ptm.entrySet()) {
						Map<String, String> m = e.getValue();
						if (m != null) {
							m.remove(key);
						}
					}
				}
				return;

			}
			String ls = "";
			if (cmd.startsWith(MLT.set_if_empty)) {
				ls = cmd.substring(MLT.set_if_empty.length());
			} else if (cmd.startsWith(MLT.set_if_null)) {
				ls = cmd.substring(MLT.set_if_null.length());
			} else {
				ls = cmd.substring(MLT.set.length());
			}
			Integer level = parseLevel(ls);
			if (level == null) {
				logger.severe("Syntax error int set clause: " + cmd + " (stmt : "
				    + stmt + "). Skipping.");
				return;
			}

			String[] pairs = stmt.split(",");
			for (String pair : pairs) {
				String[] nv = pair.split("=");
				String n = nv[0];
				String v = nv.length > 1 ? nv[1] : null;
				String oldValue = request.getValue(n);
				if (oldValue == null && cmd.startsWith(MLT.set_if_null)) {
					request.put(level, n, v);
				} else if ((oldValue == null || oldValue.trim().length() == 0)
				    && cmd.startsWith(MLT.set_if_empty)) {
					request.put(level, n, v);
				} else {
					request.put(level, n, v);
				}
			}
		}

		/**
		 * Parses the input string and return
		 * 
		 * @param ls
		 * @return
		 */
		public static Integer parseLevel(String ls) {
			try {
				ls = ls.trim();
				if (ls.length() == 0) {
					return 0;
				}

				if (ls.startsWith("-")) {
					ls = ls.substring(1);
				}
				Integer l = LevelConstantNames.get(ls.toUpperCase());
				if (l != null) {
					return l;
				}
				return Integer.parseInt(ls);
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return null;
		}

		private Result processJava(String className, Request request)
		    throws InstantiationException, IllegalAccessException,
		    ClassNotFoundException {
			Result result = null;

			Object serviceObject = Class.forName(className).newInstance();
			if (serviceObject instanceof IQuery) {
				IQuery service = (IQuery) serviceObject;
				result = service.run(request);
			} else {
				ProcessLog.Current().error(
				    "Class " + className + " is not an instance of "
				        + IQuery.class.getName(), logger);
			}

			return result;
		}

		public Result processSql(ServiceEntry serviceEntry, String sql,
		    Request request) {

			Result result = null;
			// context
			ProcessLog log = ProcessLog.Current();

			PreparedStatement ps = null;
			ResultSet rs = null;

			QueryAndParams qap = null;

			log.system("sql before conversion: " + sql);

			qap = convertQuery(sql);
			sql = qap.questionMarkQuery;
			log.system("sql after conversion: " + sql);

			//
			// PREPARE SERVICE_STMT
			//

			List<Object> paramObjects = new ArrayList<Object>();

			for (String attributeName : qap.parameters) {
				String attributeValue = request.getValue(attributeName);
				if (attributeValue == null) {
					log.error("No value provided for parameter name:" + attributeName
					    + " (serviceId:" + request.getServiceId() + ")", logger);
				}
				paramObjects.add(attributeValue);
			}
			//
			// DEFAULT PARAMETER
			//

			//
			// FINALIZE SERVICE_STMT
			//

			DataSource ds = DataSources.getInstance().get(
			    serviceEntry.getDatasourceName());
			if (ds == null) {
				log.error("No DataSource found ServiceEntry.getDatasourceName="
				    + serviceEntry.getDatasourceName() + "!", logger);
			} else {
				Connection con = null;
				try {
					con = ds.getConnection();
					ps = con.prepareStatement(sql);
					int index = 0;
					for (Object v : paramObjects) {
						ps.setObject(++index, v);
					}
					boolean hasResultSet = ps.execute();
					if (hasResultSet) {
						rs = ps.getResultSet();
						result = Utils.buildResult(0, -1, rs);
						result.setName(serviceEntry.getServiceId());
					} else {
						log.system("ServiceEntry : " + serviceEntry.getServiceId()
						    + " 	rowsAffected:" + ps.getUpdateCount());
					}
				}

				catch (SQLException e) {
					String warnMsg = "Warning for service " + serviceEntry.getServiceId()
					    + " (parameters:" + qap.parameters + ") execption msg: "
					    + e.getMessage();
					log.warn(warnMsg);
					logger.warning(warnMsg);
				}

				catch (Exception e) {
					String errorMsg = "Error for service " + serviceEntry.getServiceId()
					    + " (parameters:" + qap.parameters + ") execption msg: "
					    + e.getMessage();
					log.error(errorMsg, logger);
					log.error(qap == null ? "-no qap-" : "parameterNameQuery="
					    + qap.parameterNameQuery + ", questionMarkQuery="
					    + qap.questionMarkQuery + ", parameters=" + qap.parameters,
					    logger);
				} finally {
					Utils.closeQuietly(rs);
					Utils.closeQuietly(ps);
					Utils.closeQuietly(con);
				}
			}
			return result;
		}

		//
		// NEW
		//

		static class QueryAndParams {
			String parameterNameQuery;
			String questionMarkQuery;
			List<String> parameters = new ArrayList<String>();

			@Override
			public String toString() {
				return "QueryAndParams [parameterNameQuery=" + parameterNameQuery
				    + ", questionMarkQuery=" + questionMarkQuery + ", parameters="
				    + parameters + "]";
			}

		}

		static QueryAndParams convertQuery(String query) {

			StringBuffer buf = new StringBuffer(query.length());

			boolean started = false;
			boolean prot = false;
			List<String> parameters = new ArrayList<String>();
			StringBuffer currentParam = new StringBuffer();
			for (char c : query.toCharArray()) {

				if (prot) {
					//
				} else {

					if (started) {
						if (Character.isJavaIdentifierPart(c)) {
							currentParam.append((char) c);
							continue;
						} else {
							started = false;
							parameters.add(currentParam.toString());
							currentParam = new StringBuffer();
							// buf.append(c);
							// continue;
						}
					}
					if (!started && c == ':') {
						started = true;
						buf.append('?');
						continue;
					}
				}

				if (c == '\'') {
					prot = !prot;
				}

				buf.append(c);
			}

			// end processing
			if (started) {
				parameters.add(currentParam.toString());
			}

			//
			QueryAndParams qap = new QueryAndParams();
			qap.parameterNameQuery = query;
			qap.questionMarkQuery = buf.toString();
			qap.parameters = parameters;
			return qap;
		}

	}

	//
	//
	// BuildIns ...
	//
	//

	/**
	 * BuildIns is a collection of Java IQueries which solve rather universal
	 * tasks such as : register a service,
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class BuiltIns {

		public static class RegisterService implements IQuery {

			/**
			 * 
			 */
			private static final long serialVersionUID = 1L;

			@Override
			public Result run(Request request) {
				ProcessLog pLog = ProcessLog.Current();
				String serviceEntriesJson = request.getValue("serviceEntries");
				if (!Utils.isBlank(serviceEntriesJson)) {
					List<ServiceEntry> serviceEntries = JsonUtils.toList(
					    serviceEntriesJson, ServiceEntry.class);
					IServiceRepository sr = ServiceRepository.getInstance();
					for (ServiceEntry serviceEntry : serviceEntries) {
						sr.add(serviceEntry);
					}
				} else {
					String serviceId = request.getValue(Params.serviceId);
					String statements = request.getValue(Params.statements);
					String accessRoles = request.getValue(Params.accessRoles);
					IServiceRepository sr = ServiceRepository.getInstance();
					sr.add(new ServiceEntry(serviceId, statements, accessRoles));
				}
				return new Result(pLog);
			}

		}
	}

	/**
	 * ProcessLog class is a collector of log information specific to a single
	 * request resolution.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class ProcessLog implements Serializable {

		public static final int USER_OK_CODE = 10;
		public static final int USER_WARNING_CODE = 20;
		public static final int USER_ERROR_CODE = 30;
		public static final int OK_CODE = 1000;
		public static final int WARNING_CODE = 2000;
		public static final int ERROR_CODE = 3000;
		public static final int SYSTEM_CODE = 4000;

		public static final ThreadLocal<ProcessLog> TL = new ThreadLocal<ProcessLog>() {
			@Override
			protected ProcessLog initialValue() {
				return new ProcessLog();
			}
		};

		public static class LogEntry implements Serializable, Comparable<LogEntry> {

			private static final long serialVersionUID = 1L;

			private String msg;
			private int msgCode;
			private long time;

			private String state;

			public LogEntry(int msgCode, String msg, String state, long time,
			    String... data) {
				super();
				this.msg = msg;
				this.msgCode = msgCode;
				this.state = state;
				this.time = time;
			}

			public String getMsg() {
				return msg;
			}

			public String getState() {
				return this.state == null ? ProcessLog.OK : this.state;
			}

			public String getTime() {
				return Utils.toIsoDateTime(time);
			}

			public long getTimeMillis() {
				return time;
			}

			public int compareTo(LogEntry o) {
				int i = this.prio(state) - prio(o.state);
				if (i == 0) {
					i = this.time == o.time ? 0 : (this.time < o.time ? 1 : -1);
				}
				return i;
			}

			private int prio(String state) {
				if (Error.equals(state)) {
					return 1;
				}
				if (Warning.equals(state)) {
					return 2;
				}
				if (OK.equals(state)) {
					return 3;
				}
				if (System.equals(state)) {
					return 4;
				}
				return 5;
			}

			@Override
			public String toString() {
				return Utils.toIsoDateTime(time) + ":" + msg + "(" + state + ":"
				    + msgCode + ")";
			}

			@Override
			public int hashCode() {
				final int prime = 31;
				int result = 1;
				result = prime * result + ((msg == null) ? 0 : msg.hashCode());
				result = prime * result + ((state == null) ? 0 : state.hashCode());
				result = prime * result + (int) (time ^ (time >>> 32));
				return result;
			}

			@Override
			public boolean equals(Object obj) {
				if (this == obj)
					return true;
				if (obj == null)
					return false;
				if (getClass() != obj.getClass())
					return false;
				LogEntry other = (LogEntry) obj;
				if (msg == null) {
					if (other.msg != null)
						return false;
				} else if (!msg.equals(other.msg))
					return false;
				if (state == null) {
					if (other.state != null)
						return false;
				} else if (!state.equals(other.state))
					return false;
				if (time != other.time)
					return false;
				return true;
			}

			public int getMsgCode() {
				return msgCode;
			}

			public void setMsgCode(int msgCode) {
				this.msgCode = msgCode;
			}

		}

		/**
	     * 
	     */

		public static final String Warning = "Warning";
		public static final String Error = "Error";
		public static final String OK = "OK";
		public static final String System = "System";
		private static final long serialVersionUID = 1L;
		private static final String FILE_ID = "_FILE_ID_";
		private transient boolean silently = false;
		transient private long betweenTime = java.lang.System.currentTimeMillis();

		//
		// ProcessLog Object Level
		//

		private String state = OK;
		private List<LogEntry> logEntries = new ArrayList<LogEntry>();
		private Map<String, String> attributes = new HashMap<String, String>();

		private Long lastUsedTime;
		private String userId;
		private String name;
		private int recursion = 0;

		public ProcessLog() {
		}

		public void incrRecursion() {
			recursion++;
		}

		public void decrRecursion() {
			recursion--;
		}

		public int getRecursionValue() {
			return recursion;
		}

		public void infoUser(String line) {
			add(USER_OK_CODE, line, OK);
		}

		public void warnUser(String line, Logger logger) {
			add(USER_WARNING_CODE, line, Warning);
			_writeLog(line, Level.WARNING, logger);
		}

		public void errorUser(String line, Logger... logger) {
			add(USER_ERROR_CODE, line, Error);
			_writeLog(line, Level.SEVERE, logger);
		}

		public void error(String line, Logger... logger) {
			this.state = Error;
			add(ERROR_CODE, line, Error);
			_writeLog(line, Level.SEVERE, logger);
		}

		public void error(Throwable t, Logger... logger) {
			error(t.getMessage(), t, logger);
		}

		public void error(String line, Throwable t, Logger... logger) {
			this.state = Error;
			add(ERROR_CODE, line, Error);
			_writeLog(line + "\n" + Utils.getStackTrace(t), Level.SEVERE, logger);
		}

		public void error(int msgCode, String line, Logger... logger) {
			this.state = Error;
			add(msgCode, line, Error);
			_writeLog(line, Level.SEVERE, logger);
		}

		public void warn(String line, Logger... logger) {
			add(WARNING_CODE, line, Warning);
			_writeLog(line, Level.WARNING, logger);
		}

		public void system(String line, Logger... logger) {
			long now = java.lang.System.currentTimeMillis();
			long timeGap = now - this.betweenTime;
			this.betweenTime = now;
			add(SYSTEM_CODE, line + " [time gap : " + (timeGap / 1000) + "s]", System);
			_writeLog(line, Level.INFO, logger);
		}

		public void system(Throwable t, Logger... logger) {
			String line = Utils.getStackTrace(t);
			system(line, logger);
		}

		public void _writeLog(String line, Level level, Logger... loggers) {
			for (Logger logger : loggers)
				if (level == Level.WARNING) {
					logger.warning(line);
				} else if (level == Level.SEVERE) {
					logger.severe(line);
				} else {
					logger.info(line);
				}
		}

		private void add(int msgCode, String line, String level) {
			if (silently) {
				return;
			}
			//
			// Making sure that a ProcessLog Object does not use the same time
			// value twice.
			//
			if (lastUsedTime == null || lastUsedTime == 0) {
				lastUsedTime = java.lang.System.currentTimeMillis();
			}
			long time = java.lang.System.currentTimeMillis();
			if (time <= lastUsedTime) {
				time = lastUsedTime + 1;
			}
			lastUsedTime = time;
			logEntries.add(new LogEntry(msgCode, line, level, time));
		}

		public List<LogEntry> getLines() {
			return logEntries;
		}

		public boolean isOk() {
			return OK.equals(this.state);
		}

		public boolean isError() {
			return Error.equals(this.state);
		}

		public String getState() {
			return state;
		}

		public void setState(String state) {
			this.state = state;
		}

		public void setAttribute(String key, String value) {
			this.attributes.put(key, value);
		}

		public Map<String, String> getAttributes() {
			return attributes;
		}

		public String getAttribute(String key) {
			return this.attributes.get(key);
		}

		public void setUserId(String userId) {
			this.userId = userId;
		}

		public String getUserId() {
			return this.userId;
		}

		public void setSilently(boolean silently) {
			this.silently = silently;
		}

		public boolean isSilently() {
			return silently;
		}

		public void setName(String name) {
			this.name = name;
		}

		public String getName() {
			return this.name;
		}

		public String getId() {
			return this.attributes.get(FILE_ID);
		}

		public void setId(String id) {
			this.attributes.put(FILE_ID, id);
		}

		public static ProcessLog Current() {
			return TL.get();
		}

		public static void RemoveCurrent() {
			TL.remove();
		}

		public static ProcessLog newOnThread(String userId) {
			ProcessLog rl = new ProcessLog();
			TL.set(rl);
			return TL.get();
		}

	}

	/**
	 * <h2>Request for service process</h2>
	 * 
	 * A request object is similar to a HTTP request. Both have parameters string
	 * and file base. Parameter exist on different scopes or as we call them here
	 * 'levels'.
	 * 
	 * <h3>Parameters Levels</h3>
	 * 
	 * <h4>Initial Parameters</h4>
	 * 
	 * These are parameters set during the creation of the request, usually the
	 * can not be overriden later (during processing of the request)
	 * 
	 * <h4>Request Parameters</h4>
	 * 
	 * Request parameters such as Http Request
	 * 
	 * 
	 * Parameters <h4>Inter Request</h4>
	 * 
	 * A set of parameters for inter request communication.
	 * 
	 * <h4>Session Parameters</h4>
	 * 
	 * Session parameters are similar to request attributes Session : parameters
	 * from HttpSession Application : parameters from the ServletContext often
	 * called Application Context
	 * 
	 * $Parameter: the $Paramters like $USERID, $USERTID (an optional technical
	 * user id), $SERVICEID, $SESSIONID, $C
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class Request implements Serializable {

		/**
	 * 
	 */
		private static final long serialVersionUID = 1L;

		private String serviceId;
		private String userId;
		private Set<String> roles = new HashSet<String>();
		// private IRoleProvider roleProvider;

		private TreeMap<Integer, Map<String, String>> parametersTreeMap = new TreeMap<Integer, Map<String, String>>();

		private int defaultLevel = 10;

		private transient Map<String, Object> transientAttributes = new HashMap<String, Object>();

		@SuppressWarnings("unchecked")
		private Map<String, Serializable>[] fileList = new Map[4];
		{
			for (int i = 0; i < 4; i++)
				fileList[i] = new HashMap<String, Serializable>();
		}

		public Request() {
		}

		public Request(int defaultLevel) {
			this.defaultLevel = defaultLevel;
		}

		public String getServiceId() {
			return serviceId;
		}

		public void setServiceId(String serviceId) {
			this.serviceId = serviceId;
		}

		public Map<String, String> getParametersMinLevel(int level) {
			Set<Integer> keys = parametersTreeMap.keySet();
			for (Integer key : keys) {
				if (key == level) {
					return parametersTreeMap.get(key);
				}
			}
			return null;
		}

		public Map<String, String> getParameters(int level) {
			Map<String, String> map = parametersTreeMap.get(new Integer(level));
			if (map == null) {
				map = new HashMap<String, String>();
				parametersTreeMap.put(new Integer(level), map);
			}
			return map;
		}

		public String getValue(int level, String key) {
			Map<String, String> parameters = getParameters(level);
			if (parameters != null) {
				return parameters.get(key);
			}
			return null;
		}

		public String getValue(String name) {
			Set<Integer> keys = parametersTreeMap.keySet();
			for (Integer key : keys) {
				Map<String, String> parameters = getParameters(key);
				String s = parameters.get(name);
				if (s != null) {
					return s;
				}
			}
			return null;
		}

		// TODO junit
		public Map<String, String> getMapSnapshot() {
			Map<String, String> map = new HashMap<String, String>();
			Set<Integer> keys = parametersTreeMap.keySet();

			for (Integer key : keys) {
				Map<String, String> parameters = getParameters(key);
				for (Entry<String, String> e : parameters.entrySet()) {
					if (!map.containsKey(key)) {
						map.put(e.getKey(), e.getValue());
					}
				}
			}
			return map;
		}

		public String put(int level, String key, String value) {
			Map<String, String> parameters = getParameters(level);
			if (parameters == null) {
				parameters = new HashMap<String, String>();
				parametersTreeMap.put(level, parameters);
			}
			return parameters.put(key, value);
		}

		public String put(String key, String value) {
			return put(defaultLevel, key, value);
		}

		// public IRoleProvider getRoleProvider() {
		// return roleProvider;
		// }
		//
		// public void setRoleProvider(IRoleProvider roleProvider) {
		// this.roleProvider = roleProvider;
		// }

		public void addRole(String role) {
			this.roles.add(role);
		}

		// public boolean isInRole(String role) {
		// if (roles.contains(role)) {
		// return true;
		// }
		// if (roleProvider != null && roleProvider.isInRole(role)) {
		// return true;
		// }
		// return false;
		// }

		public String getUserId() {
			return userId;
		}

		public void setUserId(String userId) {
			this.userId = userId;
		}

		public Set<String> getRoles() {
			return roles;
		}

		public void setRoles(Set<String> roles) {
			this.roles = roles;
		}

		public void setTransientAttribute(String name, Object value) {
			this.transientAttributes.put(name, value);

		}

		public Object getTransientAttribute(String name) {
			return this.transientAttributes.get(name);
		}

		public TreeMap<Integer, Map<String, String>> getParametersTreeMap() {
			return parametersTreeMap;
		}

		public void setParametersTreeMap(
		    TreeMap<Integer, Map<String, String>> parametersTreeMap) {
			this.parametersTreeMap = parametersTreeMap;
		}

	}

	public static class Result {

		private static final Logger logger = Logger.getLogger(Result.class
		    .getName());

		public static Map<String, String> asPropertyMap(Object object) {
			Map<String, String> map = new HashMap<String, String>();
			Method[] methods = object.getClass().getMethods();
			for (Method method : methods) {
				String name = Utils.getStringGetterMethodName(method);
				if (!Utils.isBlank(name)) {
					try {
						Object o = method.invoke(object, (Object[]) null);
						String value = o == null ? null : o.toString();
						if (!Utils.isBlank(value)) {
							map.put(name, value);
						}
					} catch (Exception e) {
						logger.warning(e.getMessage());
					}
				}
			}
			return map;
		}

		public static Result asPagingResult(Object object) {
			Map<String, String> pm = asPropertyMap(object);
			String[] header = new String[pm.size()];
			String[] row = new String[pm.size()];
			int i = 0;
			for (String key : pm.keySet()) {
				header[i] = key;
				row[i] = pm.get(key);
				i++;
			}
			Result pr = new Result(header);
			pr.addRow(row);
			return pr;
		}

		//
		// Object Level
		//
		private String name = "";
		private String userId = "-";
		private int size = 0;
		private int from = 0;
		private int totalCount = 0;

		private List<List<String>> table = new ArrayList<List<String>>();
		private List<String> header = new ArrayList<String>();
		private String exception = null;
		private ProcessLog processLog = null;
		private Result subResult = null;

		public static boolean USE_CAMEL_CASE_FOR_RESULT_HEADER = true;

		public Result(String... header) {
			this.header = Arrays.asList(header);
		}

		public Result(int from, int totalCount, List<String> header,
		    List<List<String>> table) {
			super();
			this.from = from;
			this.totalCount = totalCount;
			this.header = header;
			this.table = table;
			update();
		}

		public Result(ProcessLog processLog) {
			super();
			this.processLog = processLog;
			update();
		}

		public void update() {
			size = this.table.size();
			totalCount = Math.max(from + size, totalCount);
		}


		public void addRow(String... row) {
			table.add(Arrays.asList(row));
			update();
		}

		public String getName() {
			return name;
		}

		public String getException() {
			return exception;
		}

		public Map<String, String> getFirstRowAsMap() {
			Map<String, String> map = new HashMap<String, String>();
			if (table != null && table.size() > 0) {
				List<String> row = table.get(0);
				for (int i = 0; i < header.size(); i++) {
					map.put(header.get(i), row.get(i));
				}

			}
			return map;
		}

		public String getSingleValue() {
			if (!Utils.isEmpty(table) && !Utils.isEmpty(table.get(0))) {
				return table.get(0).get(0);
			}
			return null;
		}

		public String getValue(int rowIndex, String head) {
			int index = getColIndex(head);
			return (index > -1 && rowIndex < table.size()) ? table.get(rowIndex).get(
			    index) : null;
		}

		public int getColIndex(String head) {
			for (int i = 0; i < header.size(); i++) {
				if (head.equalsIgnoreCase(header.get(i))) {
					return i;
				}
			}
			return -1;
		}

		public Map<String, Map<String, String>> toMap(String keyHead) {
			Map<String, Map<String, String>> map = new HashMap<String, Map<String, String>>();
			int index = getColIndex(keyHead);

			for (int i = 0; i < table.size(); i++) {
				List<String> row = table.get(i);
				Map<String, String> subMap = new HashMap<String, String>();
				String key = row.get(index);
				map.put(key, subMap);
				for (int j = 0; j < row.size(); j++) {
					subMap.put(this.header.get(j), row.get(j));
				}
			}
			return map;
		}

		public Map<String, List<String>> toMap(int... indexes) {
			Map<String, List<String>> map = new HashMap<String, List<String>>();
			for (int j = 0; j < table.size(); j++) {
				List<String> value = table.get(j);
				String key = createMapKey(value, indexes);
				map.put(key, value);
			}
			return map;
		}

		public Map<String, String> toTwoColumnMap(String keyHeader,
		    String valueHeader) {
			Map<String, String> map = new HashMap<String, String>();
			int keyIndex = getColIndex(keyHeader);
			int valueIndex = getColIndex(valueHeader);
			for (int j = 0; j < table.size(); j++) {
				List<String> row = table.get(j);
				String key = row.get(keyIndex);
				String value = row.get(valueIndex);
				map.put(key, value);
			}
			return map;
		}

		public Map<String, List<List<String>>> toMultiMap(int... indexes) {
			Map<String, List<List<String>>> map = new HashMap<String, List<List<String>>>();
			for (int j = 0; j < table.size(); j++) {
				List<String> value = table.get(j);
				String key = createMapKey(value, indexes);
				List<List<String>> rows = map.get(key);
				if (rows == null) {
					rows = new ArrayList<List<String>>();
					map.put(key, rows);
				}
				rows.add(value);
			}
			return map;
		}

		public static String createMapKey(List<String> row, int... indexes) {
			String key = "";
			for (int i = 0; i < indexes.length; i++) {
				if (i == 0) {
					key = row.get(indexes[i]);
				} else {
					key = key + "-" + row.get(indexes[i]);
				}
			}
			return key;
		}

		public void setException(String exception) {
			this.exception = exception;
		}

		public Result(Exception e) {
			this.exception = e.getMessage();
		}

		public String getUserId() {
			return userId;
		}

		public int getSize() {
			return size;
		}

		public List<List<String>> getTable() {
			return table;
		}

		public List<String> getHeader() {
			return header;
		}

		public int getTotalCount() {
			return totalCount;
		}

		public int getFrom() {
			return from;
		}

		public void setFrom(int from) {
			this.from = from;
		}

		public void setUserId(String userId) {
			this.userId = userId;
		}

		@Override
		public String toString() {
			String tableString = "[";
			for (int i = 0; i < table.size(); i++) {
				if (i != 0) {
					tableString += ", ";
				}
				List<String> row = table.get(i);
				tableString += row;
			}
			tableString += "]";
			return "Result [name=" + name + ", userId=" + userId + ", size=" + size
			    + ", from=" + from + ", totalCount=" + totalCount + ", table="
			    + tableString + ", header=" + header + ", exception=" + exception
			    + "]";
		}

		public String getSingleValue(String head) {
			int index = getColIndex(head);
			return (index > -1 && table.size() > 0) ? table.get(0).get(index) : null;
		}

		/**
		 * Creates a list of objects. The data is filled by applying corresponding
		 * set methods. E.g. for Columnt FIRST_NAME it will try to call
		 * setFIRST_NAME(String) or setFirstName(String)
		 * 
		 * @param <E>
		 * @param claxx
		 * @return
		 * @throws Exception
		 */
		public <E> List<E> asList(Class<E> claxx) {
			List<E> list = new ArrayList<E>();
			try {
				if (table.size() == 0) {
					return list;
				}

				for (int i = 0; i < table.size(); i++) {
					E e = claxx.newInstance();
					list.add(e);
				}

				int colIndex = 0;
				for (String head : header) {
					if (Utils.isBlank(head)) {
						head = "" + colIndex;
					}
					String[] res = Utils.createSetGetNames(head);
					Method m = null;
					for (String setGetName : res) {
						try {
							m = claxx.getMethod("set" + setGetName, String.class);
						} catch (Exception e) {
							logger.info("Did not find method: " + setGetName + ": ->" + e);
						}
						if (m != null) {
							break;
						}
					}
					if (m != null) {
						int rowIndex = 0;
						for (E e : list) {
							m.invoke(e, table.get(rowIndex).get(colIndex));
							rowIndex++;
						}
					}
					colIndex++;
				}
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return list;
		}

		public <E> Map<String, E> asMap(String keyProperty, Class<E> claxx) {
			Map<String, E> map = new HashMap<String, E>();
			try {
				List<E> list = this.asList(claxx);
				map = Utils.asMap(keyProperty, list);
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return map;
		}

		public <E> Map<String, List<E>> asMapList(String property, Class<E> claxx) {
			Map<String, List<E>> map = new HashMap<String, List<E>>();
			try {
				List<E> list = this.asList(claxx);
				String methodName = "get" + property.substring(0, 1).toUpperCase()
				    + property.substring(1);
				Method m = claxx.getMethod(methodName);
				for (E e : list) {
					String key = m.invoke(e).toString();
					List<E> listElement = map.get(key);
					if (listElement == null) {
						listElement = new ArrayList<E>();
						map.put(key, listElement);
					}
					listElement.add(e);
				}
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return map;
		}

		public void setName(String name) {
			this.name = name;
		}

		public ProcessLog getProcessLog() {
			return processLog;
		}

		public void setProcessLog(ProcessLog processLog) {
			this.processLog = processLog;
		}

		public static Result createSingleValue(String header, String value) {
			Result r = new Result(header);
			r.addRow(value);
			return r;
		}

		public Result getSubResult() {
			return subResult;
		}

		public void setSubResult(Result subResult) {
			this.subResult = subResult;
		}

	}

	/**
	 * ServiceEntry class holds all information a service such as serviceId,
	 * statements, data source name and access.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class ServiceEntry implements Serializable {

		public static final String SYSTEM_ROLENAME = "SYSTEM";

		/**
     * 
     */
		private static final long serialVersionUID = 1L;

		private String serviceId;
		private String statements;
		private Set<String> accessRoles;

		private String datasourceName = DEFAULT_DATASOURCE_NAME;

		public ServiceEntry(String serviceId, String statements, String accessRoles) {
			super();
			this.serviceId = serviceId;
			this.statements = statements;
			if (!Utils.isEmpty(accessRoles)) {
				String[] r = accessRoles.split(",");
				this.accessRoles = new HashSet<String>(Arrays.asList(r));
			}
		}

		public ServiceEntry(String serviceId, String statements,
		    Set<String> accessRoles) {
			super();
			this.serviceId = serviceId;
			this.statements = statements;
			this.accessRoles = accessRoles;
		}

		public String getServiceId() {
			return serviceId;
		}

		public String getStatements() {
			return statements;
		}

		public Set<String> getAccessRole() {
			return this.accessRoles;
		}

		public String getDatasourceName() {
			return datasourceName;
		}

		public void setDatasourceName(String datasourceName) {
			this.datasourceName = datasourceName;
		}

		@Override
		public String toString() {
			return "ServiceEntry [serviceId=" + serviceId + ", statements="
			    + statements + ", accessRoles=" + accessRoles + ", datasourceName="
			    + datasourceName + "]";
		}

		@Override
		public int hashCode() {
			final int prime = 31;
			int result = 1;
			result = prime * result
			    + ((serviceId == null) ? 0 : serviceId.hashCode());
			return result;
		}

		@Override
		public boolean equals(Object obj) {
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (getClass() != obj.getClass())
				return false;
			ServiceEntry other = (ServiceEntry) obj;
			if (serviceId == null) {
				if (other.serviceId != null)
					return false;
			} else if (!serviceId.equals(other.serviceId))
				return false;
			return true;
		}

	}

	public static class ServiceRepository implements IServiceRepository {

		private static final Logger logger = Logger
		    .getLogger(ServiceRepository.class.getName());

		private static ServiceRepository instance;

		public static ServiceRepository getInstance() {
			if (instance == null) {
				throw new RuntimeException(ServiceRepository.class.getName()
				    + " not yet created. Please create an instance first!");
			}
			return instance;
		}

		public static void closeAll() {
			if (instance != null) {
				instance = null;
			}
		}

		private ServiceRepositorySql sql;
		private ServiceRepositoryJson json;

		/**
		 * Creates a ServiceRepository and assign the static instance with it.
		 * 
		 * @param ds
		 *          datasource for ServiceEntries.
		 * @param tableName
		 *          table name (including possible schema prefix)
		 */
		public ServiceRepository(DataSource ds, String tableName) {
			logger.fine("try to create ServiceRepositorySql");
			sql = new ServiceRepositorySql(ds, tableName);
			instance = this;
		}

		/**
		 * Creates a JsonServiceRepository and assign the static instance with it.
		 * 
		 * @param jsonString
		 */
		public ServiceRepository(String jsonString) {
			logger.info("Try to create ServiceRepositoryJson ...");
			json = new ServiceRepositoryJson(jsonString);
			instance = this;
		}

		@Override
		public ServiceEntry get(String serviceId) throws SQLException, Exception {
			ServiceEntry se = null;
			if (sql != null) {
				se = sql.get(serviceId);
			} else if (json != null) {
				se = json.get(serviceId);
			} else {
				throw new RuntimeException(
				    "No service repository initialized. Currently 'sql' and 'json' service repositories are available.");
			}
			return se;
		}

		@Override
		public void add(ServiceEntry serviceEntry) {
			if (sql != null) {
				sql.add(serviceEntry);
			} else if (json != null) {
				json.add(serviceEntry);
			} else {
				throw new RuntimeException(
				    "No service repository initialized. Currently 'sql' and 'json' service repositories are available.");
			}
		}
	}

	public static class ServiceRepositoryJson implements IServiceRepository {

		public static String COL_SERVICE_STATEMENT = "SERVICE_STATEMENT";
		public static String COL_ACCESS_ROLES = "ACCESS_ROLES";
		public static String COL_SERVICE_ID = "SERVICE_ID";
		/**
		 * Version 2.0
		 */
		public static String COL_DATASOURCE_NAME = "DATASOURCE_NAME";

		private static final Logger logger = Logger
		    .getLogger(ServiceRepositoryJson.class.getName());

		private static ServiceRepositoryJson instance;

		public static ServiceRepositoryJson getInstance() {
			if (instance == null) {
				throw new RuntimeException(ServiceRepositoryJson.class.getName()
				    + " not yet created. Please create an instance first!");
			}
			return instance;
		}

		private List<ServiceEntry> entries;

		/**
		 * Creating a ServiceRepositoryJson object
		 * 
		 * @param string
		 *          file path or json string
		 * @param charsetName
		 *          ENCODING for reading the file
		 */
		@SuppressWarnings("unchecked")
		public ServiceRepositoryJson(String string, String charsetName) {
			File f = new File(string);
			String jsonString;
			if (f.exists()) {
				jsonString = Utils.readFileToString(f, charsetName);
			} else {
				jsonString = string;
			}
			if (Utils.isBlank(jsonString)) {
				entries = new ArrayList<ServiceEntry>();
			}
			Type tt = new TypeToken<List<ServiceEntry>>() {
			}.getType();
			entries = (List<ServiceEntry>) JsonUtils.toObject(jsonString, tt);
			logger.fine("found " + entries.size() + " service entries.");
		}

		/**
		 * Creating a ServiceRepositoryJson object
		 * 
		 * @param string
		 *          file path or json string
		 */
		public ServiceRepositoryJson(String string) {
			this(string, null);
		}

		public ServiceEntry get(String serviceId) throws SQLException {
			for (ServiceEntry sid : entries) {
				if (sid.getServiceId().equals(serviceId)) {
					return sid;
				}
			}
			return null;
		}

		@Override
		public void add(ServiceEntry serviceEntry) {
			ProcessLog pLog = ProcessLog.Current();
			if (entries.contains(serviceEntry)) {
				pLog.warn("ServiceEntry " + serviceEntry.getServiceId()
				    + " will be replaced by new ServiceEntry.", logger);
				entries.remove(serviceEntry);
			}
			entries.add(serviceEntry);
		}
	}

	public static class ServiceRepositorySql implements IServiceRepository {

		public static String COL_SERVICE_STATEMENT = "SERVICE_STATEMENT";
		public static String COL_ACCESS_ROLES = "ACCESS_ROLES";
		public static String COL_SERVICE_ID = "SERVICE_ID";
		/**
		 * Version 2.0
		 */
		public static String COL_DATASOURCE_NAME = "DATASOURCE_NAME";

		private static final Logger logger = Logger
		    .getLogger(ServiceRepositorySql.class.getName());

		private DataSource ds;
		private String tableName;
		private String selectQuery;

		public ServiceRepositorySql(DataSource ds, String tableName) {
			this.ds = ds;
			if (tableName.split(" ").length > 1) {
				selectQuery = tableName;
			} else {
				this.tableName = tableName;
			}
		}

		public ServiceEntry get(String serviceId) throws SQLException {
			Connection con = null;
			PreparedStatement ps = null;
			ResultSet rs = null;
			ServiceEntry se = null;
			try {
				con = getConnection();
				String sql = selectQuery != null ? selectQuery : "select "
				    + COL_SERVICE_STATEMENT + ", " + COL_ACCESS_ROLES + " from "
				    + tableName + " where " + COL_SERVICE_ID + " = ?";
				ps = con.prepareStatement(sql);
				ps.setString(1, serviceId);

				rs = ps.executeQuery();
				if (rs.next()) {
					String statements = rs.getString(COL_SERVICE_STATEMENT);
					String accessRoles = rs.getString(COL_ACCESS_ROLES);
					se = new RemoteQuery.ServiceEntry(serviceId, statements, accessRoles);
					logger.info("Found " + se);
					return se;
				}
			} finally {
				Utils.closeQuietly(rs);
				Utils.closeQuietly(ps);
				returnConnection(con);
			}
			return null;
		}

		private Connection getConnection() throws SQLException {
			if (ds == null) {
				throw new RuntimeException(ServiceRepositorySql.class.getName()
				    + " DataSource is null. Please provide a DataSource!");
			}
			return ds.getConnection();
		}

		private void returnConnection(Connection con) throws SQLException {
			if (ds != null) {
				con.close();
			}
		}

		@Override
		public void add(ServiceEntry serviceEntry) {
			// TODO Auto-generated method stub

		}
	}

	static class StringTokenizer2 {

		private int index = 0;
		private String[] tokens = null;
		private StringBuffer buf;
		private boolean ignoreWhiteSpace = true;

		public StringTokenizer2(String string, char del, char esc) {
			this(string, del, esc, true);
		}

		public StringTokenizer2(String string, char del, char esc,
		    boolean ignoreWhiteSpace) {
			this.ignoreWhiteSpace = ignoreWhiteSpace;
			// first we count the tokens
			int count = 1;
			boolean inescape = false;
			char c;
			buf = new StringBuffer();
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					count++;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					continue;
				}
				inescape = false;
			}
			tokens = new String[count];

			// now we collect the characters and create all tokens
			int k = 0;
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					tokens[k] = buf.toString();
					buf.delete(0, buf.length());
					k++;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					continue;
				}
				buf.append(c);
				inescape = false;
			}
			tokens[k] = buf.toString();
		}

		public boolean hasMoreTokens() {
			return index < tokens.length;
		}

		public String nextToken() {
			String token = tokens[index];
			index++;
			return ignoreWhiteSpace ? token.trim() : token;
		}

		public int countTokens() {
			return tokens.length;
		}

		public String[] getAllTokens() {
			return tokens;
		}

		/**
		 * Static convenience method for converting a string directly into an array
		 * of String by using the delimiter and escape character as specified.
		 */
		public static String[] toTokens(String line, char delim, char escape) {
			StringTokenizer2 tokenizer = new StringTokenizer2(line, delim, escape);
			return tokenizer.getAllTokens();
		}

		/**
		 * Create a string with the delimiter an escape character as specified.
		 */
		public static String toString(String[] tokens, char delim, char escape) {

			String token = null;
			int i, j;
			char c;
			StringBuffer buff = new StringBuffer();

			for (i = 0; i < tokens.length; i++) {
				token = tokens[i];
				for (j = 0; j < token.length(); j++) {
					c = token.charAt(j);
					if (c == escape || c == delim) {
						buff.append(escape);
					}
					buff.append(c);
				}
				buff.append(delim);
			}
			if (buff.length() > 0)
				buff.setLength(buff.length() - 1);
			return buff.toString();
		}

	}

	public static class Utils {

		private static final Logger logger = Logger
		    .getLogger(Utils.class.getName());

		public static final String ISO_DATE_PATTERN_yyyy_MM_dd = "yyyy-MM-dd";
		public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm = "yyyy-MM-dd HH:mm";
		public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss = "yyyy-MM-dd HH:mm:ss SSS";
		public static final String ISO_DATE_PATTERN_HH_mm = "HH:mm";

		public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss__SSS = "yyyy-MM-dd HH:mm:ss SSS";

		public static ThreadLocal<SimpleDateFormat> IsoDateTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd);
			}
		};

		public static ThreadLocal<SimpleDateFormat> IsoDateTimeTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm);
			}
		};

		public static ThreadLocal<SimpleDateFormat> IsoDateTimeFullTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss);
			}
		};

		public static ThreadLocal<SimpleDateFormat> IsoTimeTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_HH_mm);
			}
		};

		public static final String nowIsoDateTimeFull() {
			return IsoDateTimeFullTL.get().format(new Date());
		}

		public static final String nowIsoDateTime() {
			return IsoDateTimeTL.get().format(new Date());
		}

		public static final String nowIsoTime() {
			return IsoTimeTL.get().format(new Date());
		}

		public static final String nowIsoDate() {
			return IsoDateTL.get().format(new Date());
		}

		public static Date parseDate(String date) throws ParseException {
			return IsoDateTL.get().parse(date);
		}

		public static String toIsoDate(long timeMillis) {
			return IsoDateTL.get().format(new Date(timeMillis));
		}

		public static String toIsoDate(Date time) {
			return IsoDateTL.get().format(time);
		}

		public static Date parseTime(String date) throws ParseException {
			return IsoTimeTL.get().parse(date);
		}

		public static Date parseDateTime(String date) throws ParseException {
			return IsoDateTimeTL.get().parse(date);
		}

		public static String formatToDateTime(Date time) {
			return IsoDateTimeTL.get().format(time);
		}

		public static String formatToDateTime(long time) {
			return formatToDateTime(new Date(time));
		}

		public static String formatToDate(long time) {
			return toIsoDate(new Date(time));
		}

		public static String formatToTime(Date time) {
			return IsoTimeTL.get().format(time);
		}

		public static String formatToTime(long time) {
			return formatToTime(new Date(time));
		}

		public static Date toDate(String isoDateString) throws ParseException {
			return IsoDateTL.get().parse(isoDateString);
		}

		public static Date tryToDate(String isoDateString) {
			try {
				return toDate(isoDateString);
			} catch (Exception e) {
				return null;
			}
		}

		public static boolean isDate(String isoDateString) {
			Date d = null;
			try {
				d = IsoDateTL.get().parse(isoDateString);
			} catch (Exception e) {
			}
			return d != null;
		}

		//
		// public static long toTimeInMillis(String isoDateTimeString) {
		// try {
		// if (isoDateTimeString.length() == ISO_DATE_PATTERN_yyyy_MM_dd.length()) {
		// return toDateInMillis(isoDateTimeString);
		// }
		// return IsoDateTimeTL.get().parse(isoDateTimeString).getTime();
		// } catch (ParseException e) {
		// logger.severe(Utils.getStacktrace(e));
		// }
		// return 0;
		// }

		public static String toIsoDateTime(Date date) {
			return IsoDateTimeTL.get().format(date);
		}

		public static String toIsoDateTime(long date) {
			return IsoDateTimeTL.get().format(new Date(date));
		}

		public static int getCurrentHour() {
			Calendar c = Calendar.getInstance();
			c.setTime(new Date());
			return c.get(Calendar.HOUR_OF_DAY);
		}

		public static int getMinuteOfTheDay() {
			Calendar c = Calendar.getInstance();
			c.setTime(new Date());
			return 60 * c.get(Calendar.HOUR_OF_DAY) + c.get(Calendar.MINUTE);
		}

		public static long toDateInMillis(String isoDateString) {
			try {
				return IsoDateTL.get().parse(isoDateString).getTime();
			} catch (ParseException e) {
				logger.severe(getStackTrace(e));
			}
			return 0;
		}

		public static long toTimeInMillis(String isoDateTimeString) {
			long res = 0;
			try {
				res = IsoDateTimeFullTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			try {
				res = IsoDateTimeSecTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			try {
				res = IsoDateTimeTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			try {
				res = IsoDateTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			logger.severe("Parse error for " + isoDateTimeString + ". Return 0.");
			return res;
		}

		public static String toIsoDateTimeSec(long timeMillis) {
			return toIsoDateTimeSec(new Date(timeMillis));
		}

		public static String toIsoDateTimeSec(Date date) {
			return IsoDateTimeSecTL.get().format(date);
		}

		public static ThreadLocal<SimpleDateFormat> IsoDateTimeSecTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss);
			}
		};

		public static String nowIsoDateTimeSec() {
			return toIsoDateTimeSec(new Date());
		}

		public static String getStackTrace(Throwable t) {
			StringBuffer buf = new StringBuffer();
			if (t != null) {
				buf.append(" <");
				buf.append(t.toString());
				buf.append(">");

				java.io.StringWriter sw = new java.io.StringWriter(1024);
				java.io.PrintWriter pw = new java.io.PrintWriter(sw);
				t.printStackTrace(pw);
				pw.close();
				buf.append(sw.toString());
			}
			return buf.toString();
		}

		//
		// STRING UTILS
		//

		public static String rne(String string, String defaultContentType) {
			if (isEmpty(string)) {
				return defaultContentType;
			} else {
				return string;
			}
		}

		public static String rnn(Object o) {
			return o == null ? "" : o.toString();
		}

		public static boolean isEmpty(String str) {
			return str == null || str.length() == 0;
		}

		public static boolean isBlank(String str) {
			int strLen;
			if (str == null || (strLen = str.length()) == 0) {
				return true;
			}
			for (int i = 0; i < strLen; i++) {
				if ((Character.isWhitespace(str.charAt(i)) == false)) {
					return false;
				}
			}
			return true;
		}

		public static String[] createSetGetNames(String head) {
			String normalCase = head.substring(0, 1).toUpperCase()
			    + head.substring(1);
			String camelCase = camelCase(head);
			camelCase = camelCase.substring(0, 1).toUpperCase()
			    + camelCase.substring(1);
			String[] res = { normalCase, camelCase };
			return res;
		}

		public static String camelCase(String columnName) {
			StringBuilder res = new StringBuilder();
			boolean upper = false;
			for (int i = 0; i < columnName.length(); i++) {
				char c = columnName.charAt(i);
				if (c == '_' || !Character.isLetterOrDigit(c)) {
					upper = true;
					continue;
				}
				if (upper) {
					res.append(Character.toUpperCase(c));
					upper = false;
				} else {
					res.append(Character.toLowerCase(c));
				}
			}
			return res.toString();
		}

		public static String[] tokenize(String string) {
			return tokenize(string, DEFAULT_DEL, DEFAULT_ESC);
		}

		public static String[] tokenize(String string, char del) {
			return tokenize(string, del, DEFAULT_ESC);
		}

		public static String joinTokens(Collection<String> list) {
			// TODO apply del and esc !!!
			String res = "";
			int i = 0;
			for (String s : list) {
				s = escape(s, DEFAULT_DEL, DEFAULT_ESC);
				if (i == 0) {
					res = s;
				} else {
					res = res + DEFAULT_DEL + s;
				}
				i++;
			}
			return res;
		}

		public static String joinTokens(String[] list) {
			return joinTokens(Arrays.asList(list));
		}

		public static String escape(String in, char del, char esc) {
			char[] chars = in.toCharArray();
			String res = "";
			for (int i = 0; i < chars.length; i++) {
				char c = chars[i];
				if (c == del || c == esc) {
					res += esc;
				}
				res += c;
			}
			return res;
		}

		public static String[] tokenize(String string, char del, char esc) {
			if (isEmpty(string)) {
				return new String[0];
			}
			// first we count the tokens
			int count = 1;
			boolean inescape = false;
			char c, pc = 0;
			StringBuffer buf = new StringBuffer();
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					count++;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					continue;
				}
				inescape = false;
			}
			String[] tokens = new String[count];

			// now we collect the characters and create all tokens
			int k = 0;
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					tokens[k] = buf.toString();
					buf.delete(0, buf.length());
					k++;
					pc = c;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					pc = c;
					continue;
				}
				//
				// append
				//
				if (c != del && pc == esc) {
					buf.append(pc);
				}
				buf.append(c);
				pc = c;
				inescape = false;
			}
			tokens[k] = buf.toString();
			return tokens;
		}

		public static Set<String> asSet(String... values) {
			Set<String> set = new HashSet<String>();
			if (values != null) {
				for (int i = 0; i < values.length; i++) {
					set.add(values[i]);
				}
			}
			return set;
		}

		public static Set<String> asSet(String string, char del, char esc) {
			return asSet(tokenize(string, del, esc));
		}

		public static <E> List<E> asList(E... elements) {
			ArrayList<E> list = new ArrayList<E>(1);
			for (E e : elements) {
				list.add(e);
			}
			return list;
		}

		//
		// Collection Utils
		//

		//
		// IO UTILS
		//

		public static void copy(Reader in, Writer out) throws IOException {
			int c;
			while ((c = in.read()) != -1) {
				out.write(c);
			}
		}

		public static void closeQuietly(Writer w) {
			try {
				w.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static void closeQuietly(Reader r) {
			try {
				r.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static String readFileToString(File file, String charsetName) {
			BufferedReader r = null;
			charsetName = charsetName == null ? ENCODING : charsetName;
			StringBuffer buf = new StringBuffer((int) file.length());
			try {
				r = new BufferedReader(new InputStreamReader(new FileInputStream(file),
				    charsetName));
				int c = 0;
				while ((c = r.read()) != -1) {
					buf.append(c);
				}
			} catch (Exception e) {
				logger.severe(getStackTrace(e));
			} finally {
				closeQuietly(r);
			}
			return buf.toString();
		}

		//
		//
		// DB UTILS
		//
		//

		public static void closeQuietly(ResultSet rs) {
			try {
				rs.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static void closeQuietly(Statement stmt) {
			try {
				stmt.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static void commitSilently(Connection connection) {
			try {
				connection.commit();
			} catch (Exception e) {
			}
		}

		public static void closeQuietly(Connection connection) {
			try {
				connection.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static Object runQuery(Connection connection, String sqlStatement,
		    Object... parameters) {
			PreparedStatement ps = null;
			ResultSet rs = null;
			try {
				ps = connection.prepareStatement(sqlStatement);
				for (int i = 0; i < parameters.length; i++) {
					ps.setObject(i + 1, parameters[i]);
				}

				boolean hasResultSet = ps.execute();
				if (hasResultSet) {
					rs = ps.getResultSet();
					return rs;
				} else {
					return new Integer(ps.getUpdateCount());
				}
			} catch (Throwable t) {
				logger.warning(t.getMessage());
			} finally {
				Utils.closeQuietly(ps);
			}
			return new Integer(-1);
		}

		public static String sqlValueToString(Object value, int sqlType)
		    throws IllegalArgumentException, SecurityException,
		    IllegalAccessException, InvocationTargetException,
		    NoSuchMethodException {

			switch (sqlType) {
			case Types.CHAR:
			case Types.VARCHAR:
				return value == null ? "" : value.toString();// _NULL_ :
				// String.valueOf(value);
			default:
			}
			String strValue = value == null ? "" : value.toString();
			if (isBlank(strValue)) {
				return "";
			}
			if (value.getClass().getName().toString()
			    .startsWith("oracle.sql.TIMESTAMP")) {
				value = value.getClass().getMethod("toJdbc").invoke(value);
			}
			if (value instanceof Timestamp) {
				Timestamp t = (Timestamp) value;
				return toIsoDateTime(t.getTime());
			}
			if (value instanceof Date) {
				Date t = (Date) value;
				return toIsoDate(t.getTime());
			}
			return strValue;
		}

		public static String blobToString(Clob blob) {
			try {
				Writer out = new StringWriter();
				Reader in = blob.getCharacterStream();
				copy(in, out);
				out.flush();
				closeQuietly(out);
				return out.toString();
			} catch (Exception e) {
				logger.severe(getStackTrace(e));
			}
			return "";
		}

		public static Result buildResult(int start, int max, ResultSet rs) {
			Result result = null;
			try {

				//
				ResultSetMetaData md = rs.getMetaData();

				int colums = rs.getMetaData().getColumnCount();
				String[] header = new String[colums];
				int[] sqlTypes = new int[colums];
				header = new String[colums];
				for (int i = 0; i < colums; i++) {
					header[i] = md.getColumnName(i + 1);
					if (Result.USE_CAMEL_CASE_FOR_RESULT_HEADER) {
						header[i] = camelCase(header[i]);
					}
					sqlTypes[i] = md.getColumnType(i + 1);
				}
				//
				int counter = 0;
				result = new Result(header);
				result.from = -1;
				String[] row = null;
				while (rs.next()) {

					if (counter >= start && ((counter < start + max) || max == -1)) {
						if (result.from == -1) {
							result.from = counter;
						}

						row = new String[colums];

						for (int i = 0; i < colums; i++) {
							if (sqlTypes[i] == Types.BLOB) {
								logger
								    .warning("BLOB type not supported! Returning empty string!");
								row[i] = "";
							} else if (sqlTypes[i] == Types.CLOB) {
								Clob clob = (Clob) rs.getObject(i + 1);
								row[i] = blobToString(clob);
							} else {
								Object sqlValue = rs.getObject(i + 1);
								row[i] = sqlValueToString(sqlValue, sqlTypes[i]);
							}
						}
						result.addRow(row);
					} else {
						//
					}
					counter++;
				}
				result.totalCount = counter;

			} catch (Exception e) {
				result = new Result(e);
			} finally {

			}
			return result;
		}

		//
		// COLLECTION UTILS
		//

		public static <E> boolean isEmpty(Collection<E> c) {
			return c == null || c.size() == 0;
		}

		//
		// OBJECT UTILS
		//

		public static Map<String, String> asMap(Object obj) throws Exception {
			Map<String, String> values = new HashMap<String, String>();
			Method[] methods = obj.getClass().getMethods();

			for (Method method : methods) {
				if (Modifier.isStatic(method.getModifiers())) {
					continue;
				}
				if (method.getParameterTypes().length == 0
				    && method.getName().startsWith("get")
				    && method.getName().length() > 3
				    && method.getReturnType().equals(String.class)) {
					String key = method.getName().substring(3, 4).toLowerCase()
					    + method.getName().substring(4);
					Object r = method.invoke(obj, (Object[]) null);
					String value = rnn(r);
					values.put(key, value);
				}
			}
			return values;
		}

		public static <E> E newObject(Map<String, String> values, Class<E> claxx)
		    throws Exception {
			E e;
			e = claxx.newInstance();
			int colIndex = 0;
			for (String propertyName : values.keySet()) {
				if (isBlank(propertyName)) {
					propertyName = "" + colIndex;
				}
				String[] res = createSetGetNames(propertyName);
				Method m = null;
				for (String setGetName : res) {
					try {
						m = claxx.getMethod("set" + setGetName, String.class);
					} catch (Exception e1) {
					}
					if (m != null) {
						break;
					}
				}
				if (m != null) {
					try {
						m.invoke(e, values.get(propertyName));
					} catch (Exception e1) {
					}
				}
				colIndex++;
			}
			return e;
		}

		public static String getStringGetterMethodName(Method method) {

			if (method.getParameterTypes().length != 0) {
				return null;
			}
			if (String.class.equals(method.getReturnType()) == false) {
				return null;
			}
			String methodName = method.getName();

			if (!methodName.startsWith("get") || !(methodName.length() > 3)
			    || !Character.isUpperCase(methodName.charAt(3))) {
				return null;
			}
			return Character.toLowerCase(methodName.charAt(3))
			    + methodName.substring(4);

		}

		@SuppressWarnings("unchecked")
		public static <E> Map<String, E> asMap(String keyProperty, List<E> list) {
			if (isEmpty(list)) {
				return new HashMap<String, E>();
			}
			Class<E> claxx = (Class<E>) list.get(0).getClass();
			Map<String, E> map = new HashMap<String, E>();
			try {
				String methodName = "get" + keyProperty.substring(0, 1).toUpperCase()
				    + keyProperty.substring(1);
				Method m = claxx.getMethod(methodName);
				for (E e : list) {
					map.put(m.invoke(e).toString(), e);
				}
			} catch (Exception e) {
				logger.severe(getStackTrace(e));
			}
			return map;
		}

	}

	public static class JsonUtils {

		private static final Logger logger = Logger.getLogger(JsonUtils.class
		    .getName());

		public static String toJson(Object object) {
			Gson gson = new Gson();
			String jsonStr = gson.toJson(object);
			return jsonStr;
		}

		public static <T> T fromJson(String jsonStr, Class<T> classOfT) {
			Gson gson = new Gson();
			return gson.fromJson(jsonStr, classOfT);
		}

		public static String toJson(String name, String value) {
			Gson gson = new Gson();
			JsonObject jo = new JsonObject();
			jo.addProperty(name, value);

			return gson.toJson(jo);
		}

		public static String toJson(String name, Object value) {
			if (value instanceof String) {
				return toJson(name, (String) value);
			}
			Gson gson = new Gson();
			JsonObject o = new JsonObject();
			JsonElement e = gson.toJsonTree(value);
			o.add(name, e);
			return gson.toJson(o);
		}

		public static String toJson(JsonObject jsonObject) {
			return jsonObject.toString();
		}

		public static JsonObject toJsonObject(String s) {
			JsonParser jp = new JsonParser();
			JsonObject jo = jp.parse(s).getAsJsonObject();
			return jo;
		}

		public static String exception(String message) {
			return toJson("exception", message);
		}

		public static String message(String message) {
			return toJson("message", message);
		}

		public static Map<String, String> toStringMap(String jsonString) {
			JsonParser p = new JsonParser();
			JsonObject jsonObject = (JsonObject) p.parse(jsonString);
			Set<Map.Entry<String, JsonElement>> jsonElements = jsonObject.entrySet();
			Map<String, String> map = new HashMap<String, String>();
			for (Map.Entry<String, JsonElement> entry : jsonElements) {
				String key = entry.getKey();
				JsonElement je = entry.getValue();
				if (je.isJsonPrimitive()) {
					JsonPrimitive jp = (JsonPrimitive) je;
					if (jp.isBoolean() || jp.isNumber() || jp.isString()) {
						map.put(key, jp.getAsString());
					}
				}
			}
			return map;
		}

		public static String jsonNoop() {
			return toJson("NOOP");
		}

		public static class Parameters extends HashMap<String, String> {

			/**
	         * 
	         */
			private static final long serialVersionUID = 1L;

		}

		public static String jsonException(String message) {
			return toJson("exception", message);
		}

		public static String jsonMessage(String message) {
			return toJson("message", message);
		}

		public static <E> E toObject(String json, Class<E> claxx) {
			return new Gson().fromJson(json, claxx);
		}

		public static <E> E toObjectSilently(String json, Class<E> claxx) {
			try {
				return new Gson().fromJson(json, claxx);
			} catch (Exception e) {
			}
			return null;
		}

		//
		// LIST MAP start
		//
		private static Object _toListMapE(JsonElement je) {
			if (je.isJsonNull()) {
				return null;
			}
			if (je.isJsonPrimitive()) {
				return _toListMapP(je.getAsJsonPrimitive());
			}
			if (je.isJsonArray()) {
				return _toListMapA(je.getAsJsonArray());
			}
			if (je.isJsonObject()) {
				return _toListMapO(je.getAsJsonObject());
			}
			return null;
		}

		private static String _toListMapP(JsonPrimitive jp) {
			return jp.getAsString();
		}

		private static Object _toListMapA(JsonArray ja) {
			List<Object> list = new ArrayList<Object>(ja.size());
			for (int i = 0; i < ja.size(); i++) {
				JsonElement je = ja.get(i);
				Object v = _toListMapE(je);
				if (v != null) {
					list.add(v);
				}
			}
			return list;
		}

		private static Object _toListMapO(JsonObject jo) {
			Set<Map.Entry<String, JsonElement>> jes = jo.entrySet();
			Map<String, Object> map = new HashMap<String, Object>();
			for (Map.Entry<String, JsonElement> entry : jes) {
				String key = entry.getKey();
				Object v = _toListMapE(entry.getValue());
				if (v != null) {
					map.put(key, v);
				}
			}
			return map;
		}

		public static Object toListMap(String jsonString) {
			JsonParser p = new JsonParser();
			JsonElement je = (JsonObject) p.parse(jsonString);
			return _toListMapE(je);
		}

		public static Object toObject(String json, Type type) {
			try {
				return new Gson().fromJson(json, type);
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return null;
		}

		//
		// LIST MAP end
		//

		@SuppressWarnings("unchecked")
		public static <E> E[] toArray(String jsonString, Class<E> claxx) {

			JsonParser p = new JsonParser();
			E[] array = null;
			Gson g = new Gson();
			JsonElement je = (JsonElement) p.parse(jsonString);
			if (je.isJsonArray()) {
				JsonArray a = je.getAsJsonArray();
				array = (E[]) Array.newInstance(claxx, a.size());
				int index = 0;
				for (JsonElement e : a) {
					array[index] = g.fromJson(e, claxx);
					index++;
				}
			}
			return array;

		}

		public static <E> List<E> toList(String jsonString, Class<E> claxx) {
			return Utils.asList(toArray(jsonString, claxx));
		}
	}

	public static class ObjectStore<E> {
		private static final Logger logger = Logger.getLogger(ObjectStore.class
		    .getName());

		private Class<E> resultClass;
		private Set<String> roles;
		private String userId;

		public ObjectStore(Class<E> resultClass, String userId, Set<String> roles) {

			this.resultClass = resultClass;
			// this.basicParams = basicParams;
			this.userId = userId;
			if (roles != null) {
				this.roles = roles;
			} else {
				this.roles = new HashSet<String>();
			}
		}

		public E newInstance(Map<String, String> params) {
			try {
				return Utils.newObject(params, this.resultClass);
			} catch (Exception e) {
				logger.severe(e.getMessage());
				return null;
			}
		}

		public E newInstance(Request request) {
			return newInstance(request.getMapSnapshot());
		}

		private Result _process(String serviceId,
		    TreeMap<Integer, Map<String, String>> params) {
			Request request = new Request();
			request.setUserId(userId);
			request.setRoles(roles);
			request.setServiceId(serviceId);
			if (params != null) {
				request.setParametersTreeMap(params);
			}

			MainQuery mq = new MainQuery();
			return mq.run(request);
		}

		public List<E> search(String serviceId,
		    TreeMap<Integer, Map<String, String>> params) {
			Result pr = _process(serviceId, params);
			List<E> result = null;
			if (pr != null) {
				result = pr.asList(resultClass);
			}
			return result;
		}

		public List<E> search(String serviceId, Map<String, String> params) {
			return search(serviceId, wrap(params));
		}

		public List<E> search(String serviceId, E object) throws Exception {
			if (object == null) {
				return search(serviceId);
			}
			Map<String, String> params = Utils.asMap(object);

			return search(serviceId, wrap(params));
		}

		TreeMap<Integer, Map<String, String>> wrap(Map<String, String> params) {
			TreeMap<Integer, Map<String, String>> tm = new TreeMap<Integer, Map<String, String>>();
			tm.put(0, params);
			return tm;
		}

		public List<E> search(String serviceId) {
			return search(serviceId, (TreeMap<Integer, Map<String, String>>) null);
		}

		public E get(String serviceId, TreeMap<Integer, Map<String, String>> tm) {
			List<E> result = this.search(serviceId, tm);
			return !Utils.isEmpty(result) ? result.get(0) : null;
		}

		public E get(String serviceId, Map<String, String> parameters) {
			return get(serviceId, wrap(parameters));
		}

		public E get(String serviceId, E object) throws Exception {
			List<E> result = this.search(serviceId, object);
			return !Utils.isEmpty(result) ? result.get(0) : null;
		}

		public E get(String serviceId) {
			return get(serviceId, new HashMap<String, String>());
		}

		public Result update(String serviceId, Map<String, String> parameters) {
			return _process(serviceId, wrap(parameters));
		}

		public Result update(String serviceId, E object) throws Exception {
			Map<String, String> params = Utils.asMap(object);

			return _process(serviceId, wrap(params));
		}

		// public void update(String serviceId, E object,
		// Map<String, String> additionalParams) throws Exception {
		// DataService ds = DataService.getInstance();
		// ds.process(serviceId,
		// applyBasicParams(ObjectUtils.asMap(object), additionalParams));
		// }

		public Result update(String serviceId) {
			return _process(serviceId, null);
		}

		public Set<String> getRoles() {
			return roles;
		}

		public void setRoles(Set<String> roles) {
			this.roles = roles;
		}

		// public static Map<String, String> applyBasicParams(
		// Map<String, String> params, Map<String, String> basicParams) {
		// if (MapUtils.isNotEmpty(basicParams)) {
		// for (String key : basicParams.keySet()) {
		// params.put(key, basicParams.get(key));
		// }
		// }
		// return params;
		//
		// }

	}

	// public class ObjectUtils extends org.apache.commons.lang.ObjectUtils {
	//
	// private static Log logger = LogFactory.getLog(ObjectUtils.class);
	//
	// public static boolean areNotEquals(Object o1, Object o2) {
	// return !areEquals(o1, o2);
	// }
	//
	// public static boolean areEquals(Object o1, Object o2) {
	// return o1 == o2 || (o1 != null && o1.equals(o2));
	// }
	//
	// public static Object toObject(byte[] objectBytes) throws Exception {
	// ByteArrayInputStream bin = new ByteArrayInputStream(objectBytes);
	// ObjectInputStream oin = new ObjectInputStream(bin);
	// Object object = oin.readObject();
	// oin.close();
	// return object;
	// }
	//
	// public static byte[] toByteArray(Object object) throws Exception {
	// ByteArrayOutputStream bout = new ByteArrayOutputStream();
	// ObjectOutputStream oout = new ObjectOutputStream(bout);
	// oout.writeObject(object);
	// oout.close();
	// return bout.toByteArray();
	// }
	//
	// @SuppressWarnings("resource")
	// public static Object readObjectFromFile(File file)
	// throws FileNotFoundException, IOException, ClassNotFoundException {
	// ObjectInputStream oin = null;
	// FileInputStream in = new FileInputStream(file);
	// oin = new ObjectInputStream(in);
	// Object o = oin.readObject();
	// return o;
	// }
	//
	// public static Object readObjectFromFile(String fileName)
	// throws FileNotFoundException, IOException, ClassNotFoundException {
	// return readObjectFromFile(new File(fileName));
	// }
	//
	// public static void writeObjectToFile(String fileName, Serializable object)
	// throws FileNotFoundException, IOException, ClassNotFoundException {
	// writeObjectToFile(new File(fileName), object);
	// }
	//
	// @SuppressWarnings("resource")
	// public static void writeObjectToFile(File file, Serializable object)
	// throws FileNotFoundException, IOException {
	// ObjectOutputStream oout = null;
	// FileOutputStream out = new FileOutputStream(file);
	// oout = new ObjectOutputStream(out);
	// oout.writeObject(object);
	// }
	//
	// public static boolean isNull(Object o) {
	// return o == null;
	// }
	//
	// public static boolean isNotNull(Object o) {
	// return !isNull(o);
	// }
	//
	// public static Object toExpectedType(String value, Class<?> expectedType)
	// throws ParseException {
	// if (expectedType.equals(Date.class)) {
	// Date date = null;
	// date = DateUtils.toDate(value);
	// return date;
	// }
	// if (expectedType.equals(Double.class)) {
	// return new Double(value);
	// }
	// if (expectedType.equals(Long.class)) {
	// return new Long(value);
	// }
	// if (expectedType.equals(Integer.class)) {
	// return new Integer(value);
	// }
	// return value;
	// }
	//
	// public static String getClassName(Object object) {
	// if (object == null) {
	// return "null";
	// } else {
	// return object.getClass().getSimpleName();
	// }
	// }
	//
	// public static String toString2(Object object) {
	// return ReflectionToStringBuilder.toString(object,
	// ToStringStyle.NO_FIELD_NAMES_STYLE, false);
	// }
	//

	//
	//
	// @SuppressWarnings("unchecked")
	// public static <E> Map<String, E> asMap(String keyProperty, List<E> list) {
	// if (CollectionUtils.isEmpty(list)) {
	// return MapUtils.EMPTY_MAP;
	// }
	// Class<E> claxx = (Class<E>) list.get(0).getClass();
	// Map<String, E> map = new HashMap<String, E>();
	// try {
	// String methodName = "get" + keyProperty.substring(0, 1).toUpperCase()
	// + keyProperty.substring(1);
	// Method m = claxx.getMethod(methodName);
	// for (E e : list) {
	// map.put(m.invoke(e).toString(), e);
	// }
	// } catch (Exception e) {
	// logger.error(e, e);
	// }
	// return map;
	// }
	//
	// }

	public static void shutdown() {
		Utils.IsoDateTimeSecTL.remove();
		Utils.IsoDateTimeTL.remove();
		Utils.IsoDateTL.remove();
		Utils.IsoTimeTL.remove();
		Utils.IsoDateTimeFullTL.remove();
	}

}
