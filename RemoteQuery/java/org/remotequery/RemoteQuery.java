package org.remotequery;

import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.Reader;
import java.io.Serializable;
import java.io.StringReader;
import java.io.StringWriter;
import java.io.Writer;
import java.lang.reflect.Array;
import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
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

import javax.sql.DataSource;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

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
 */
public class RemoteQuery {

	private static Logger rqLogger = LoggerFactory.getLogger(RemoteQuery.class);
	private static Logger rqMainLogger = rqLogger;
	private static Logger rqDebugLogger = LoggerFactory
	    .getLogger(RemoteQuery.class.getName() + ".sql");
	private static Logger sqlLogger = LoggerFactory.getLogger(RemoteQuery.class
	    .getName() + ".sql");

	public static Map<String, IProcessFactory> ScriptQueries = new HashMap<String, RemoteQuery.IProcessFactory>();

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

	public static final String ANONYMOUS = "ANONYMOUS";

	public static String COL_STATEMENTS = "STATEMENTS";
	public static String COL_ROLES = "ROLES";
	public static String COL_SERVICE_ID = "SERVICE_ID";
	/**
	 * Version 2.0
	 */
	public static String COL_DATASOURCE = "DATASOURCE";

	public static class MLT {
		// code indication
		public static final String java = "java";
		public static final String sql = "sql";
		// combination
		public static final String parameters = "parameters";
		public static final String serviceId = "serviceId";
		public static final String include = "include";
		// parameter / result statements
		public static final String set = "set";
		public static final String set_if_empty = "set-if-empty";
		public static final String copy_over = "copy-over";
		public static final String set_if_null = "set-if-null";
		public static final String set_null = "set-null";
		//
		public static final String debugOn = "debugOn";
		public static final String debugOff = "debugOff";
		//
		public static final String tx_begin = "tx-begin";
		public static final String tx_commit = "tx-commit";
	}

	public static class Params {
		public static String serviceId = "serviceId";
		public static String statements = "statements";
		public static String roles = "roles";
	}

	public static class DBColumns {
		public static String serviceId = "serviceId";
		public static String statements = "statements";
		public static String roles = "roles";
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
		LevelConstantNames.put("INITIAL", LevelConstants.INITIAL);
		LevelConstantNames.put("REQUEST", LevelConstants.REQUEST);
		LevelConstantNames.put("HEADER", LevelConstants.HEADER);
		LevelConstantNames.put("INTER_REQUEST", LevelConstants.INTER_REQUEST);
		LevelConstantNames.put("SESSION", LevelConstants.SESSION);
		LevelConstantNames.put("APPLICATION", LevelConstants.APPLICATION);
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
	 */
	public static class DataSources {

		private static DataSources instance;

		public static DataSources getInstance() {
			return instance == null ? instance = new DataSources() : instance;
		}

		private final Map<String, DataSource> dss = new HashMap<String, DataSource>();

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

	public static interface IProcessFactory extends Serializable {

		IProcess create(Request request, ServiceEntry serviceEntry, Result result,
		    String statements);

	}

	public static interface IProcess extends Serializable {

		Result process();

	}

	//
	//
	// PROCESSING JSON STATEMENTS
	//
	//

	public static class JsonAnswer {
		public Result result;
		boolean returnFlag;

		public JsonAnswer(Result result) {
			this.result = result;
		}

		public JsonAnswer() {
		}

		String getSingleValue() {
			if (result != null) {
				return result.getSingleValue();
			}
			return null;
		}

	}

	public interface IJsonProcessor {
		JsonAnswer run(Object statement);
	}

	@SuppressWarnings({ "rawtypes", "unchecked" })
	public static class JsonProcessor {

		public static Map<String, Class<? extends IJsonProcessor>> commands = new HashMap<String, Class<? extends IJsonProcessor>>();
		static {
			commands.put("add-role", AddRoleProcessor.class);
			commands.put("switch", SwitchProcessor.class);
			commands.put("userMessage", UserMessage.class);
			commands.put("foreach", ForeachProcessor.class);
			commands.put("union", UnionProcessor.class);
			commands.put("join", JoinProcessor.class);
		}

		String statements;
		ServiceEntry serviceEntry;
		Request request;
		JsonAnswer answer;
		final ProcessLog pLog;
		List<Result> results;

		public JsonProcessor(Request request, ServiceEntry serviceEntry,
		    Result result, String statements) {
			super();
			this.serviceEntry = serviceEntry;
			this.statements = statements;
			this.request = request;
			pLog = ProcessLog.Current();
			results = new ArrayList<Result>();
		}

		public Result process() {
			pLog.warn(request.getServiceId() + " -> JsonProcessor still in BETA !!!",
			    rqLogger);

			answer = new JsonAnswer();
			List statementList = JsonUtils.toObject(statements, List.class);
			JsonAnswer aResult = processStatementList(statementList);
			if (aResult.result == null) {
				aResult.result = new Result(pLog);
			} else {
				aResult.result.setProcessInfo(pLog);
			}
			return aResult.result;
		}

		public JsonAnswer processStatementList(List statements) {
			JsonAnswer a = new JsonAnswer();
			for (Object statement : statements) {
				a = processStatement(statement);
				if (answer.returnFlag) {
					return a;
				}
			}
			return a;
		}

		public JsonAnswer processStatement(Object statement) {
			JsonAnswer a = null;

			if (statement == null) {
				a = new JsonAnswer();
			}
			if (statement instanceof String) {
				a = (new StringProcessor(this).run(statement));
			} else if (statement instanceof Map) {
				a = processStatementMap((Map) statement);
			} else if (statement instanceof List) {
				a = processStatementList((List) statement);
			}
			return a;
		}

		public Result processServiceQuery(String statement) {
			Result prevPagingResult = answer != null ? answer.result : null;
			try {
				return MainQuery.processStatementList(request, serviceEntry,
				    prevPagingResult, statement);
			} catch (Exception e) {
				pLog.error("tried to run " + statement, rqLogger);
			}
			return null;
		}

		public JsonAnswer seriousErrorAnswer() {
			return new JsonAnswer();
		}

		public JsonAnswer seriousErrorAnswer(String message) {
			JsonAnswer ja = new JsonAnswer();
			ProcessLog.Current().error(message, rqLogger);
			return ja;
		}

		public JsonAnswer processStatementMap(Map<String, Object> statement) {
			try {
				for (Entry<String, Class<? extends IJsonProcessor>> e : commands
				    .entrySet()) {
					String name = e.getKey();
					Class<? extends IJsonProcessor> claxx = commands.get(name);
					if (statement.containsKey(name)) {
						Constructor<? extends IJsonProcessor> constructor = claxx
						    .getDeclaredConstructor(JsonProcessor.class);
						IJsonProcessor p = constructor.newInstance(this);
						return p.run(statement);
					}
				}
			} catch (Exception e) {
				rqLogger.error(e.toString());
			}
			pLog.warn("Can not process map statements (keys: " + statement.keySet()
			    + ")!", rqLogger);
			return new JsonAnswer();

		}

		//
		//
		// RQ JSON PROCESSORS (FEATURES)
		//
		//

		//
		//
		// RQ JSON PROCESSOR SWITCH
		//
		//

		public static class AddRoleProcessor implements IJsonProcessor {
			private final JsonProcessor context;

			public AddRoleProcessor(JsonProcessor context) {
				this.context = context;
			}

			public JsonAnswer run(Object o) {
				Map<String, String> map = (Map) o;
				String role = (String) map.get("add-role");
				context.request.addRole(role);
				return new JsonAnswer();
			}
		}

		//
		//
		// RQ JSON PROCESSOR SWITCH
		//
		//

		public static class SwitchProcessor implements IJsonProcessor {
			private final JsonProcessor context;

			public SwitchProcessor(JsonProcessor context) {
				this.context = context;
			}

			public JsonAnswer run(Object o) {
				try {
					Map statement = (Map) o;
					Object switchStatement = statement.get("switch");
					if (switchStatement == null) {
						return context
						    .seriousErrorAnswer("Serious Error in *switch* statement : statement is null!");
					}
					JsonAnswer janswer = context.processStatement(switchStatement);
					String value = janswer.result != null ? janswer.result
					    .getSingleValue() : null;
					String caseKey = "case:" + value;
					Object caseStatement = statement.get(caseKey);
					Object nullStatement = statement.get("null");
					Object defaultStatement = statement.get("default");
					if (caseStatement != null && value != null) {
						return context.processStatement(caseStatement);
					} else if (nullStatement != null && value == null) {
						return context.processStatement(nullStatement);
					} else if (defaultStatement != null) {
						return context.processStatement(defaultStatement);
					}
				} catch (Exception e) {
					context.pLog.error("Serious Error in *switch* statement : " + e,
					    rqLogger);
				}
				return context.seriousErrorAnswer();
			}
		}

		//
		//
		// RQ JSON PROCESSOR FOREACH -begin-
		//
		//

		public static class ForeachProcessor implements IJsonProcessor {
			private final JsonProcessor context;

			public ForeachProcessor(JsonProcessor context) {
				this.context = context;

			}

			public JsonAnswer run(Object o) {
				JsonAnswer lastAnswer = new JsonAnswer();
				try {
					Map statement = (Map) o;
					Map foreachStatement = (Map) statement.get("foreach");
					Object parametersStatement = foreachStatement.get("parameters");
					Object indexName = foreachStatement.get("index-name");
					indexName = indexName == null ? "index" : indexName;
					Object doStatement = foreachStatement.get("do");
					if (parametersStatement == null) {
						context.pLog.warn("Missing parameters for foreach statement");
						return new JsonAnswer();
					}
					JsonAnswer ca = context.processStatement(parametersStatement);
					if (ca.result != null) {
						for (int index = 0; index < ca.result.size; index++) {
							Map<String, String> paramters = ca.result.getRowAsMap(index);
							for (String key : paramters.keySet()) {
								context.request.put(key, paramters.get(key));
							}
							context.request.put("" + indexName, "" + index);
							JsonAnswer doAnswer = context.processStatement(doStatement);
							lastAnswer = doAnswer;
							// doAnswer.childOf(context.answer)
						}
					}
					return lastAnswer;
				} catch (Exception e) {
					context.pLog.error("Serious Error in *switch* statement ", rqLogger);
				}
				return context.seriousErrorAnswer();
			}
		}

		//
		//
		// RQ JSON PROCESSOR UNION -begin-
		//
		//

		public static class UnionProcessor implements IJsonProcessor {
			private final JsonProcessor context;

			public UnionProcessor(JsonProcessor context) {
				this.context = context;
			}

			public JsonAnswer run(Object o) {
				JsonAnswer uAnswer = null;
				try {
					Map statement = (Map) o;
					List unionStatementList = (List) statement.get("union");
					for (int i = 0; i < unionStatementList.size(); i++) {
						JsonAnswer ca = context.processStatement(unionStatementList.get(i));
						if (uAnswer == null) {
							uAnswer = ca;
						} else {
							uAnswer.result.rowsAffected += ca.result.rowsAffected;
							// NOT IMPLEMENTED
							uAnswer.result.append(ca.result);
						}
					}
					ProcessLog.Current().warn("not yet implemented");
					return uAnswer;
				} catch (Exception e) {
					context.pLog.error("Serious Error in *union* statement ", rqLogger);
				}
				return context.seriousErrorAnswer();
			}
		}

		//
		//
		// RQ JSON PROCESSOR JOIN -begin-
		//
		//

		public static class JoinProcessor implements IJsonProcessor {
			private final JsonProcessor context;

			public JoinProcessor(JsonProcessor context) {
				this.context = context;
			}

			public JsonAnswer run(Object o) {
				JsonAnswer uAnswer = null;
				Result result = null;
				try {
					Map statement = (Map) o;
					List joinStatementList = (List) statement.get("join");
					String joinKey = (String) statement.get("join-key");
					String joinColumn = (String) statement.get("join-column");
					for (int i = 0; i < joinStatementList.size(); i++) {
						JsonAnswer ca = context.processStatement(joinStatementList.get(i));
						if (uAnswer == null) {
							uAnswer = ca;
							result = ca.result;
						} else {
							uAnswer.result.rowsAffected += ca.result.rowsAffected;
							// NOT IMPLEMENTED
							result.join(joinKey, joinColumn, uAnswer.result);
						}
					}
					ProcessLog.Current().warn("not yet implemented");
					return uAnswer;
				} catch (Exception e) {
					context.pLog.error("Serious Error in *join* statement ", rqLogger);
				}
				return context.seriousErrorAnswer();
			}
		}

		//
		//
		// RQ JSON PROCESSOR USER MESSAGE -begin-
		//
		//

		public static class UserMessage implements IJsonProcessor {
			private final JsonProcessor context;

			public UserMessage(JsonProcessor context) {
				this.context = context;
			}

			public JsonAnswer run(Object o) {
				Map statement = (Map) o;
				String s = "" + Utils.firstValue(statement);
				int index = s.indexOf(":");
				if (index == -1) {
					context.pLog.user(ProcessLog.OK, s);
				} else {
					try {
						String level = s.substring(0, index);
						String message = s.substring(index + 1);
						context.pLog.user(level.trim(), message.trim());
					} catch (Exception e) {
						context.pLog.error("Can not process userMessage : " + s);
					}
				}
				return new JsonAnswer();
			}

		}

		//
		//
		// RQ JSON PROCESSOR STRING -begin-
		//
		//

		public static class StringProcessor implements IJsonProcessor {
			private final JsonProcessor parent;

			public StringProcessor(JsonProcessor parent) {
				this.parent = parent;
			}

			public JsonAnswer run(Object o) {
				String statement = (String) o;
				return new JsonAnswer(parent.processServiceQuery(statement));
			}
		}

	}

	//
	//
	// END OF JSON STATEMENT PROCESSING
	//
	//

	/**
	 * This class provides a convenient access to the ServiceEntry and the
	 * connection defined by the ServieEntrys data source name. The connection is
	 * automatically returned to the DataSource after the run mehod. The class can
	 * be used for any IQuery implementation.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static abstract class AbstractQuery implements IQuery {

		private static final long serialVersionUID = 1L;
		protected Connection connection;
		protected ServiceEntry serviceEntry;
		protected Map<String, String> initParameters = new HashMap<String, String>();

		public Connection getConnection() {
			return connection;
		}

		public void setConnection(Connection connection) {
			this.connection = connection;
		}

		public ServiceEntry getServiceEntry() {
			return serviceEntry;
		}

		public void setServiceEntry(ServiceEntry serviceEntry) {
			this.serviceEntry = serviceEntry;
		}

		public void setInitParameters(String s) {

			String[] ps1 = Utils.tokenize(s, ':');
			for (int i = 0; i < ps1.length; i++) {
				String[] ps2 = Utils.tokenize(ps1[i], '=');
				if (ps2.length == 1) {
					initParameters.put(ps2[0], "");
				}
				if (ps2.length == 2) {
					initParameters.put(ps2[0], ps2[1]);
				}
			}
		}

	}

	/**
	 * Interface for service repositories. Currently we have a JSON base and a SQL
	 * base service repository.
	 * 
	 * @author tonibuenter
	 */
	public static interface IServiceRepository {

		ServiceEntry get(String serviceId) throws Exception;

		List<ServiceEntry> list() throws Exception;

		void add(ServiceEntry serviceEntry) throws Exception;

	}

	public static interface IResultListener {

		public void start();

		public void done();

		public void setName(String name);

		public void setFrom(int from);

		public void setHeader(String[] header);

		public void addRow(String[] row);

		public void setTotalCount(int totalCount);

		public void setException(Exception e);

		public void setRowsAffected(int rowsAffected);

	}

	/**
	 * Convenience method for running a main query.
	 * 
	 * @author tonibuenter
	 */
	public static Result runMain(Request request) {
		MainQuery mq = new MainQuery();
		return mq.run(request);
	}

	/**
	 * MainQuery is the main class the provides the processing of a RemoteQuery
	 * request. It takes care of the service statement parsing and processing.
	 * 
	 * @author tonibuenter
	 */
	public static class MainQuery implements IQuery {

		// TODO idea :: how to create your own plugins like 'my-sql-extension:select
		// %SELECTLIST% where %WHERE% clause'

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		private final Map<String, Serializable> processStore = new HashMap<String, Serializable>();

		public void put(String key, Serializable value) {
			processStore.put(key, value);
		}

		public Serializable get(String key) {
			return processStore.get(key);
		}

		public Result run(Request request) {
			Result result = null;
			ProcessLog log = ProcessLog.Current();

			String serviceId = request.getServiceId();
			String userId = request.getUserId();
			if (Utils.isEmpty(userId)) {
				log.warn(
				    "Request object has no userId set. Process continues with userId="
				        + ANONYMOUS, rqLogger);
				request.setUserId(ANONYMOUS);
				userId = request.getUserId();
			}
			// TODO better in the process object ?
			log.incrRecursion();
			try {
				//

				ServiceEntry serviceEntry = ServiceRepository.getInstance().get(
				    serviceId);
				log.serviceEntry_Start(serviceEntry);
				if (serviceEntry == null) {
					log.error("No ServiceEntry found for " + serviceId, rqLogger);
					return new Result(log);
				}
				//
				// CHECK ACCESS
				//
				boolean hasAccess = false;
				Set<String> roles = serviceEntry.getRoleSet();
				if (Utils.isEmpty(roles)) {
					hasAccess = true;
				} else {
					for (String role : roles) {
						if (request.getRoles().contains(role)) {
							hasAccess = true;
							break;
						}
					}
				}
				if (hasAccess) {
					log.system("Access to " + serviceId + " for " + userId + " : ok",
					    rqLogger);
				} else {
					log.warn(
					    "No access to " + serviceId + " for " + userId
					        + " (service roles: " + roles + ", request roles: "
					        + request.getRoles() + ")", rqLogger);
					return new Result(log);
				}
				//
				// START PROCESSING STATEMENTS
				//

				log.system("ServiceEntry found for userId=" + userId + " is : "
				    + serviceEntry, rqLogger);

				String statements = serviceEntry.getStatements();
				statements = Utils.trim(statements);
				log.system("Statements (trimmed) " + statements, rqLogger);
				//
				// REGISTERED SCRIPTING
				//
				for (Entry<String, IProcessFactory> entry : ScriptQueries.entrySet()) {
					if (statements.startsWith(entry.getKey())) {
						IProcessFactory fac = entry.getValue();
						try {
							IProcess process = fac.create(request, serviceEntry, result,
							    statements);
							return process.process();
						} catch (Exception e) {
							log.error(e, rqLogger);
						} finally {
							//
						}
						return null;
					}
				}
				//
				// JSON PROCESSOR
				//
				if (statements.startsWith("[") && statements.endsWith("]")) {
					JsonProcessor jp = new JsonProcessor(request, serviceEntry, result,
					    statements);
					result = jp.process();
				} else {
					result = processStatementList(request, serviceEntry, result,
					    statements);
				}

			} catch (Exception e) {
				log.error(e, rqLogger);
			} finally {
				// ParameterSupport.release(parameterSupport);
				log.decrRecursion();
				log.serviceEntry_End();
			}
			if (result == null) {
				// TODO Default result object ?
			} else {
				result.setUserId(userId);
			}
			return result;
		}

		public static Result processStatementList(Request request,
		    ServiceEntry serviceEntry, Result result, String statements) {

			String[] statementList = Utils.tokenize(statements, STATEMENT_DELIMITER,
			    STATEMENT_ESCAPE);
			for (String statement : statementList) {
				Result result2 = processSingleStatement(request, result, serviceEntry,
				    statement);
				if (result2 != null) {
					if (result != null) {
						result2.setSubResult(result);
					}
					result = result2;
				}
			}
			return result;
		}

		public static Result processSingleStatement(Request request,
		    Result currentResult, ServiceEntry serviceEntry, String serviceStmt) {
			ProcessLog log = ProcessLog.Current();
			try {
				Result result = null;

				log.incrRecursion();
				int recursion = log.getRecursionValue();
				if (recursion > MAX_RECURSION) {
					log.error("Recursion limit reached " + MAX_RECURSION
					    + ". Stop processing.", rqLogger);
					return currentResult;
				}

				serviceStmt = serviceStmt.trim();
				if (Utils.isEmpty(serviceStmt)) {
					log.warn("Empty serviceStmt -> no processing.", rqLogger);
					return currentResult;
				}

				String[] pair = Utils.parseCommandValue(serviceStmt);

				String cmd = pair[0].trim();
				String stmt = pair[1].trim();

				//
				// plain SQL fall back case
				//
				if (!startsWithMLT(serviceStmt)) {
					cmd = MLT.sql;
					stmt = serviceStmt;
				}
				//
				// include
				//
				if (cmd.equals(MLT.include)) {
					ServiceEntry se2 = ServiceRepository.getInstance().get(stmt);
					if (se2 == null) {
						log.warn("Tried to include " + stmt + ". Skipping.", rqLogger);
						return currentResult;
					}
					String includeServiceStmt = se2.getStatements();
					result = processSingleStatement(request, result, serviceEntry,
					    includeServiceStmt);
					return result;
				}
				//
				// java:
				//
				if (cmd.equals(MLT.java) || "class".equals(cmd)) {
					result = processJava(stmt, serviceStmt, request, serviceEntry);
					return result;
				}
				//
				// sql:
				//
				if (cmd.equals(MLT.sql)) {
					Result r = new Result();
					processSql(serviceEntry, stmt, request, r);
					return r;
				}
				//
				// set, ... request parameter assignements
				//
				if (cmd.startsWith(MLT.set)) {
					applySetCommand(request, cmd, stmt);
					return currentResult;
				}
				//
				// copy-over
				//
				if (cmd.startsWith(MLT.copy_over)) {
					String[] s = stmt.split("=");
					if (s.length > 1) {
						request.put(s[0], request.getValue(pair[1]));
					}
					return currentResult;
				}
				//
				// debug
				//
				if (cmd.startsWith(MLT.debugOn)) {
					rqLogger = rqDebugLogger;
					return currentResult;
				}
				if (cmd.startsWith(MLT.debugOff)) {
					rqLogger = rqMainLogger;
					return currentResult;
				}
				//
				// parameters
				//
				if (cmd.equals(MLT.parameters)) {
					Result r = new Result();
					processSql(serviceEntry, stmt, request, r);
					if (r.size > 0) {
						Map<String, String> paramters = r.getRowAsMap(0);
						for (String key : paramters.keySet()) {
							request.put(key, paramters.get(key));
						}
					}
					return currentResult;
				}
				//
				// serviceId
				//
				if (cmd.equals(MLT.serviceId)) {
					String parentServiceId = serviceEntry.getServiceId();
					String subServiceId = stmt;
					request.setServiceId(subServiceId);
					MainQuery mq = new MainQuery();
					result = mq.run(request);
					request.setServiceId(parentServiceId);
					return result;
				}
			} catch (Exception e) {
				log.error(
				    "ServiceStmt for " + serviceStmt + " failed. Exception: "
				        + e.getMessage(), rqLogger);

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
			if (serviceStmt.startsWith(MLT.parameters)) {
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
			if (serviceStmt.startsWith(MLT.debugOn)) {
				return true;
			}
			if (serviceStmt.startsWith(MLT.debugOff)) {
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

			String[] nv = Utils.tokenize(stmt, '=');
			String n = nv[0];
			String v = nv.length > 1 ? nv[1] : null;
			n = Utils.trim(n);
			v = Utils.trim(v);
			String requestValue = request.getValue(n);
			// set_if_null
			if (cmd.equals(MLT.set_if_null)) {
				if (requestValue == null) {
					request.put(n, v);
				}
			}
			// set_if_empty
			else if (cmd.equals(MLT.set_if_empty)) {
				if (Utils.isEmpty(requestValue)) {
					request.put(n, v);
				}
			}
			// set
			else {
				request.put(n, v);
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
				rqLogger.error(Utils.getStackTrace(e));
			}
			return null;
		}

		private static Result processJava(String className, String classParameters,
		    Request request, ServiceEntry serviceEntry)
		    throws InstantiationException, IllegalAccessException,
		    ClassNotFoundException, SQLException {
			Result result = null;
			Connection connection = null;
			ProcessLog pLog = ProcessLog.Current();
			try {
				rqLogger.debug("Process Java :: start with " + className);
				Object serviceObject = Class.forName(className).newInstance();

				if (!(serviceObject instanceof IQuery)) {
					ProcessLog.Current().error(
					    "Class " + className + " is not an instance of "
					        + IQuery.class.getName(), rqLogger);
					return null;
				}

				IQuery service = (IQuery) serviceObject;
				if (service instanceof AbstractQuery) {
					AbstractQuery aqService = (AbstractQuery) service;
					//
					aqService.setServiceEntry(serviceEntry);
					//
					DataSource ds = DataSources.getInstance().get(
					    serviceEntry.getDatasourceName());
					if (ds != null) {
						connection = ds.getConnection();
						aqService.setConnection(connection);
					}
					//
					aqService.setInitParameters(classParameters);
				}
				result = service.run(request);
			} catch (Exception e) {
				pLog.error(e, rqLogger);
			} finally {
				Utils.closeQuietly(connection);
			}

			return result;
		}

		public static void processSql(ServiceEntry serviceEntry, String sql,
		    Request request, IResultListener irl) {
			ProcessLog log = ProcessLog.Current();

			DataSource ds = DataSources.getInstance().get(
			    serviceEntry.getDatasourceName());
			if (ds == null) {
				Exception e = new Exception(
				    "No DataSource found ServiceEntry.getDatasourceName="
				        + serviceEntry.getDatasourceName() + "!");
				log.error(e.getMessage(), rqLogger);
				irl.setException(e);
				return;
			}
			processSql(ds, sql, request, irl);
		}

		public static void processSql(DataSource ds, String sql, Request request,
		    IResultListener irl) {

			Connection con = null;
			try {
				con = ds.getConnection();
				processSql(con, sql, request, irl);
			} catch (Exception e) {
				// TODO: handle exception
			} finally {
				Utils.closeQuietly(con);
			}
		}

		public static void processSql(Connection con, String sql, Request request,
		    IResultListener irl) {
			String serviceId = request.getServiceId();
			serviceId = Utils.isBlank(serviceId) ? "-serviceId-" : serviceId;
			ProcessLog pLog = ProcessLog.Current();

			PreparedStatement ps = null;
			ResultSet rs = null;

			QueryAndParams qap = null;

			pLog.system("sql before conversion: " + sql);
			sqlLogger.debug("**************************************");
			sqlLogger.debug("Start sql (in service : " + serviceId + ")\n" + sql);

			qap = convertQuery(sql);
			sql = qap.questionMarkQuery;
			pLog.system("sql after conversion: " + sql);

			//
			// PREPARE SERVICE_STMT
			//

			List<Object> paramObjects = new ArrayList<Object>();

			for (String attributeName : qap.parameters) {
				String attributeValue = request.getValue(attributeName);
				if (attributeValue == null) {
					pLog.warn("processSql:No value provided for parameter name:"
					    + attributeName + " (serviceId:" + request.getServiceId()
					    + "). Will use empty string.", rqLogger);
					paramObjects.add("");
					sqlLogger.debug("sql-param " + attributeName + " : ");
				} else {
					paramObjects.add(attributeValue);
					sqlLogger
					    .debug("sql-param " + attributeName + " : " + attributeValue);
				}
			}
			//
			// DEFAULT PARAMETER
			//

			//
			// FINALIZE SERVICE_STMT
			//

			try {
				ps = con.prepareStatement(sql);
				int index = 0;
				for (Object v : paramObjects) {
					ps.setObject(++index, v);
					if (rqLogger.isDebugEnabled()) {
						// rqLogger.debug("index: " + index + " value: " + v);
					}
				}
				boolean hasResultSet = ps.execute();
				if (hasResultSet) {
					rs = ps.getResultSet();
					Utils.buildResult(0, -1, rs, irl);
					irl.setName(serviceId);
				} else {
					int rowsAffected = ps.getUpdateCount();
					pLog.system("ServiceEntry : " + serviceId + "; rowsAffected : "
					    + rowsAffected);
					irl.setRowsAffected(rowsAffected);
					sqlLogger.debug("sql-rows-affected : " + rowsAffected);
				}
			}

			catch (SQLException e) {
				String warnMsg = "Warning for " + serviceId + " (parameters:"
				    + qap.parameters + ") execption msg: " + e.getMessage();
				pLog.warn(warnMsg, rqLogger);
			}

			catch (Exception e) {
				String errorMsg = "Error for " + serviceId + " (parameters:"
				    + qap.parameters + ") execption msg: " + e.getMessage();
				pLog.error(errorMsg, rqLogger);
				pLog.error(qap == null ? "-no qap-" : "parameterNameQuery="
				    + qap.parameterNameQuery + ", questionMarkQuery="
				    + qap.questionMarkQuery + ", parameters=" + qap.parameters,
				    rqLogger);
			} finally {
				Utils.closeQuietly(rs);
				Utils.closeQuietly(ps);
			}

			sqlLogger.debug("End sql ");
			sqlLogger.debug("**************************************");

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
							currentParam.append(c);
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
				IServiceRepository sr = ServiceRepository.getInstance();
				try {
					if (!Utils.isBlank(serviceEntriesJson)) {
						List<ServiceEntry> serviceEntries = JsonUtils.toList(
						    serviceEntriesJson, ServiceEntry.class);
						for (ServiceEntry serviceEntry : serviceEntries) {
							sr.add(serviceEntry);
						}
					} else {
						String serviceId = request.getValue(Params.serviceId);
						String statements = request.getValue(Params.statements);
						String roles = request.getValue(Params.roles);
						sr.add(new ServiceEntry(serviceId, statements, roles));
					}
				} catch (Exception e) {
					pLog.error(e, rqLogger);
				}
				return new Result(pLog);
			}

		}

		public static class ListServices implements IQuery {

			/** 
			 * 
			 * 
			*/
			private static final long serialVersionUID = 1L;

			@Override
			public Result run(Request request) {

				ProcessLog pLog = ProcessLog.Current();
				IServiceRepository sr = ServiceRepository.getInstance();
				Result r = new Result(COL_SERVICE_ID, COL_STATEMENTS, COL_ROLES,
				    COL_DATASOURCE);
				try {
					List<ServiceEntry> list = sr.list();
					for (ServiceEntry se : list) {
						r.addRowVar(se.getServiceId(), se.getStatements(),
						    Utils.joinTokens(se.getRoleSet()), se.getDatasourceName());
					}
				} catch (Exception e) {
					pLog.error(e, rqLogger);
				}
				r.setProcessInfo(pLog);
				return r;
			}
		}

		public static class MultiService implements IQuery {

			/** 
			 * 
			 * 
			*/
			private static final long serialVersionUID = 1L;

			@SuppressWarnings("rawtypes")
			@Override
			public Result run(Request request) {
				Request requestC = null;
				Result resultC = null;
				ProcessLog pLog = null;
				ProcessLog mainPlog = ProcessLog.Current();
				Result mainResult = new Result("MultiResult");
				mainResult.setProcessInfo(mainPlog);
				String requestArray = request.getValue("requestArray");
				List requestList = JsonUtils.toObject(requestArray, List.class);
				MainQuery mainQuery = new MainQuery();
				for (int i = 0; i < requestList.size(); i++) {
					pLog = ProcessLog.Current(new ProcessLog(request.getUserId()));
					requestC = request.deepCopy();
					requestC.remove("requestArray");
					Object r = requestList.get(i);
					if (r instanceof Map) {
						Map rm = (Map) r;
						String serviceId = rm.get("serviceId") + "";
						if (Utils.isBlank(serviceId)) {
							pLog.error("no serviceId in request", rqLogger);
						} else {
							requestC.setServiceId(serviceId);
							Object p = rm.get("parameters");
							if (p instanceof Map) {
								Map pm = (Map) p;
								for (Object o : pm.entrySet()) {
									Entry e = (Entry) o;
									requestC.put(e.getKey() + "", e.getValue() + "");
								}
							} else {
								pLog.warn("No parameter in request ", rqLogger);
							}
							resultC = mainQuery.run(requestC);
							resultC.setProcessInfo(pLog);
							String rs = JsonUtils.toJson(resultC);
							mainResult.addRowVar(rs);
						}

					} else {
						mainPlog.error("Request " + i + " is not an object : " + r);
					}
				}
				return mainResult;
			}
		}
	}

	/**
	 * ProcessLog class is a collector of log information specific to a single
	 * request resolution.
	 * 
	 * @author tonibuenter
	 */
	public static class ProcessLog implements Serializable {

		public static final int USER_OK_CODE = 10;
		public static final int USER_WARNING_CODE = 20;
		public static final int USER_ERROR_CODE = 30;
		public static final int OK_CODE = 1000;
		public static final int WARNING_CODE = 2000;
		public static final int ERROR_CODE = 3000;
		public static final int SYSTEM_CODE = 4000;

		public static final String Warning = "Warning";
		public static final String Error = "Error";
		public static final String OK = "OK";
		public static final String System = "System";
		private static final long serialVersionUID = 1L;
		private static final String FILE_ID = "_FILE_ID_";

		public static final Map<String, Integer> USER_CODE_MAP = new HashMap<String, Integer>();
		static {
			USER_CODE_MAP.put(OK, USER_OK_CODE);
			USER_CODE_MAP.put(Warning, USER_WARNING_CODE);
			USER_CODE_MAP.put(Error, USER_ERROR_CODE);
		}

		public static final ThreadLocal<ProcessLog> TL = new ThreadLocal<ProcessLog>() {
			@Override
			protected ProcessLog initialValue() {
				return new ProcessLog();
			}
		};

		public static class LogEntry implements Serializable, Comparable<LogEntry> {

			private static final long serialVersionUID = 1L;

			private final String msg;
			private int msgCode;
			private final long time;

			private final String state;

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
				if (this == obj) {
					return true;
				}
				if (obj == null) {
					return false;
				}
				if (getClass() != obj.getClass()) {
					return false;
				}
				LogEntry other = (LogEntry) obj;
				if (msg == null) {
					if (other.msg != null) {
						return false;
					}
				} else if (!msg.equals(other.msg)) {
					return false;
				}
				if (state == null) {
					if (other.state != null) {
						return false;
					}
				} else if (!state.equals(other.state)) {
					return false;
				}
				if (time != other.time) {
					return false;
				}
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

		//
		// ProcessLog Object Level
		//

		private transient boolean silently = false;
		transient private long betweenTime = java.lang.System.currentTimeMillis();

		private String state = OK;
		private final List<LogEntry> processLines = new ArrayList<LogEntry>();
		private final Map<String, String> attributes = new HashMap<String, String>();

		private Long lastUsedTime;
		private String userId;
		private String name;
		private int recursion = 0;

		public ProcessLog() {
		}

		public ProcessLog(String userId) {
			this.userId = userId;
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

		public void user(String level, String line) {
			Integer code = USER_CODE_MAP.get(level);
			code = code == null ? USER_OK_CODE : code;
			add(code, line, level);
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
			for (Logger logger : loggers) {
				if (level == Level.WARNING) {
					logger.warn(line);
				} else if (level == Level.SEVERE) {
					logger.error(line);
				} else {
					logger.info(line);
				}
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
			processLines.add(new LogEntry(msgCode, line, level, time));
		}

		public List<LogEntry> getLines() {
			return processLines;
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

		public static ProcessLog Current(ProcessLog pLog) {
			TL.set(pLog);
			return TL.get();
		}

		public static void RemoveCurrent() {
			TL.remove();
		}

		//
		// public static ProcessLog newOnThread(String userId) {
		// ProcessLog rl = new ProcessLog();
		// TL.set(rl);
		// return TL.get();
		// }

		//
		// DEBUG CONTEXT -start-
		//

		public DebugContext debugContext;
		public transient DebugContext current;

		public static class DebugContext {
			public static int SE = 0;

			public int type;
			public String name;
			public List<String[]> contents;
			public Map<String, String> parameters;
			public List<DebugContext> children = new ArrayList<DebugContext>();
			public transient DebugContext parent;

			public DebugContext(int type) {
				this.type = type;
			}

		}

		public void serviceEntry_Start(ServiceEntry se) {
			if (current == null) {
				return;
			}
			DebugContext dc = new DebugContext(DebugContext.SE);
			dc.parent = this.current;
			dc.name = se.getServiceId();
			this.current = dc;
		}

		public void serviceEntry_End() {
			if (current == null) {
				return;
			}
			this.current = this.current.parent;
		}

		//
		// DEBUG CONTEXT -end-
		//

	}

	/**
	 * <h2>Request for service process</h2> A request object is similar to a HTTP
	 * request. Both have parameters string and file base. Parameter exist on
	 * different scopes or as we call them here 'levels'. <h3>Parameters Levels</h3>
	 * <h4>Initial Parameters</h4> These are parameters set during the creation of
	 * the request, usually the can not be overriden later (during processing of
	 * the request) <h4>Request Parameters</h4> Request parameters such as Http
	 * Request Parameters <h4>Inter Request</h4> A set of parameters for inter
	 * request communication. <h4>Session Parameters</h4> Session parameters are
	 * similar to request attributes Session : parameters from HttpSession
	 * Application : parameters from the ServletContext often called Application
	 * Context $Parameter: the $Paramters like $USERID, $USERTID (an optional
	 * technical user id), $SERVICEID, $SESSIONID, $C
	 * 
	 * @author tonibuenter
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

		private int defaultLevel = 10;
		private TreeMap<Integer, Map<String, String>> parametersTreeMap = new TreeMap<Integer, Map<String, String>>();

		private Map<String, String> fileInfo;

		private transient Map<String, Object> transientAttributes = new HashMap<String, Object>();

		@SuppressWarnings("unchecked")
		private final Map<String, Serializable>[] fileList = new Map[4];
		{
			for (int i = 0; i < 4; i++) {
				fileList[i] = new HashMap<String, Serializable>();
			}
		}

		public Request() {
		}

		public Request deepCopy() {
			Request copy = (Request) Utils.deepClone(this);
			copy.transientAttributes = transientAttributes;
			return copy;
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

		public void setParameters(Map<String, String> parameters) {
			this.put(parameters);
		}

		public Map<String, String> getParameters(int level) {
			Map<String, String> map = parametersTreeMap.get(level);
			if (map == null) {
				map = new HashMap<String, String>();
				parametersTreeMap.put(level, map);
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

		public String getValue(String name, String defaultValue) {
			String r = getValue(name);
			return r == null ? defaultValue : r;
		}

		// TODO junit
		public Map<String, String> getParameterSnapshhot() {
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

		public void remove(String key) {
			Collection<Map<String, String>> maps = parametersTreeMap.values();
			for (Map<String, String> map : maps) {
				map.remove(key);
			}
		}

		public String put(String key, String value) {
			return put(defaultLevel, key, value);
		}

		public void put(Map<String, String> map) {
			put(defaultLevel, map);
		}

		public void put(int level, Map<String, String> map) {
			for (Entry<String, String> e : map.entrySet()) {
				put(level, e.getKey(), e.getValue());
			}
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

		public Map<String, String> getFileInfo() {
			return fileInfo;
		}

		public void setFileInfo(Map<String, String> fileInfo) {
			this.fileInfo = fileInfo;
		}

	}

	public static class ResultUtils {
		@SuppressWarnings("rawtypes")
		public static Result asResult(Object obj) {
			Map<String, String> pm = null;
			Result result = null;
			String[] header = null;
			String[] row = null;
			Collection coll = null;
			int i = 0;
			if (obj instanceof Collection) {
				coll = (Collection) obj;
			} else {
				coll = Utils.asList(obj);
			}

			boolean first = true;
			for (Object o : coll) {
				pm = Utils.asPropertyMap(o);
				if (first) {
					header = new String[pm.size()];
					i = 0;
					for (String key : pm.keySet()) {
						header[i] = key;
						i++;
					}
					result = new Result(header);
					first = false;
				}
				row = new String[pm.size()];
				i = 0;
				for (String key : pm.keySet()) {
					row[i] = pm.get(key);
					i++;
				}
				result.addRow(row);
			}
			return result;
		}
	}

	public static class Result implements IResultListener {

		private static final Logger logger = LoggerFactory.getLogger(Result.class);

		//
		// Object Level
		//
		private String name = "";
		private String userId = "-";
		private int size = 0;
		private int from = 0;
		private int totalCount = 0;
		private int rowsAffected = 0;
		private boolean hasMore = false;

		private List<List<String>> table = new ArrayList<List<String>>();
		private List<String> header = new ArrayList<String>();
		private String exception = null;
		private ProcessLog processInfo = null;
		public Result subResult = null;

		public static boolean USE_CAMEL_CASE_FOR_RESULT_HEADER = true;

		public Result(String... header) {
			this.header = Arrays.asList(header);
		}

		public void append(Result result) {
			// TODO Auto-generated method stub

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
			this.processInfo = processLog;
			update();
		}

		public void update() {
			size = this.table.size();
			totalCount = Math.max(from + size, totalCount);
		}

		@Override
		public void addRow(String[] row) {
			table.add(Arrays.asList(row));
			update();
		}

		public void addRow(List<String> row) {
			table.add(row);
			update();
		}

		public void addRowVar(String... row) {
			addRow(row);
		}

		public String getName() {
			return name;
		}

		public String getException() {
			return exception;
		}

		public Map<String, String> getRowAsMap(int index) {
			Map<String, String> map = new HashMap<String, String>();
			if (table != null && table.size() > index) {
				List<String> row = table.get(index);
				for (int i = 0; i < header.size(); i++) {
					map.put(header.get(i), row.get(i));
				}
			}
			return map;
		}

		public Map<String, String> getFirstRowAsMap() {
			return getRowAsMap(0);
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

		public boolean getHasMore() {
			return this.hasMore;
		}

		public int getRowsAffected() {
			return this.rowsAffected;
		}

		public int getFrom() {
			return from;
		}

		public void setUserId(String userId) {
			this.userId = userId;
		}

		@Override
		public String toString() {
			String tableString = "[";
			for (int i = 0; i < table.size(); i++) {
				if (i != 0) {
					tableString += "\n";
				}
				List<String> row = table.get(i);
				tableString += row;
			}
			tableString += "]";
			return "Result [name=" + name + ", userId=" + userId + ", size=" + size
			    + "\nfrom=" + from + ", totalCount=" + totalCount + "\n" + header
			    + "\n" + tableString + ",\nexception=" + exception + "]";
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
					Field f = null;
					for (String setGetName : res) {
						try {
							m = claxx.getMethod("set" + setGetName, String.class);
						} catch (Exception e) {
							// logger.debug("Did not find method: " + setGetName + ": ->" +
							// e);
						}
						if (m != null) {
							break;
						}
						// try field
						try {
							String fieldName = setGetName.substring(0, 1).toLowerCase()
							    + setGetName.substring(1);
							f = claxx.getField(fieldName);
						} catch (Exception e) {
							logger.info("Did not find field: " + setGetName + ": ->" + e);
						}
						if (f != null) {
							break;
						}
					}
					if (m != null) {
						int rowIndex = 0;
						for (E e : list) {
							m.invoke(e, table.get(rowIndex).get(colIndex));
							rowIndex++;
						}
					} else if (f != null) {
						int rowIndex = 0;
						for (E e : list) {
							f.set(e, table.get(rowIndex).get(colIndex));
							rowIndex++;
						}
					}
					colIndex++;
				}
			} catch (Exception e) {
				logger.error(Utils.getStackTrace(e));
			}
			return list;
		}

		public <E> Map<String, E> asMap(String keyProperty, Class<E> claxx) {
			Map<String, E> map = new HashMap<String, E>();
			try {
				List<E> list = this.asList(claxx);
				map = Utils.asMap(keyProperty, list);
			} catch (Exception e) {
				logger.error(e.getMessage(), e);
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
				logger.error(e.getMessage(), e);
			}
			return map;
		}

		public ProcessLog getProcessInfo() {
			return processInfo;
		}

		public void setProcessInfo(ProcessLog processLog) {
			this.processInfo = processLog;
		}

		public static Result createSingleValue(String header, String value) {
			Result r = new Result(header);
			r.addRowVar(value);
			return r;
		}

		public Result getSubResult() {
			return subResult;
		}

		public void setSubResult(Result subResult) {
			this.subResult = subResult;
		}

		public void join(String joinKey, String joinCol, Result joinResult) {
			int joinKeyIndex = joinResult.getHeaderIndex(joinCol);

			Map<String, Integer> keyIndexMap = getKeyIndexMap(joinCol);
			Map<String, List<List<String>>> joinSubtables = new HashMap<String, List<List<String>>>();
			for (List<String> joinRow : joinResult.table) {
				String key = joinRow.get(joinKeyIndex);
				if (keyIndexMap.containsKey(key)) {
					List<List<String>> list = joinSubtables.get(key);
					if (list == null) {
						list = new ArrayList<List<String>>();
						joinSubtables.put(key, list);
					}
					list.add(joinRow);
				}

			}

			this.header.add(joinCol);

			for (Entry<String, List<List<String>>> e : joinSubtables.entrySet()) {
				String key = e.getKey();
				List<List<String>> r = e.getValue();
				Result tmpResult = new Result(0, r.size(), joinResult.header, r);
				String s = JsonUtils.toJson(tmpResult);
				List<String> row = this.table.get(keyIndexMap.get(key));
				row.add(s);
			}

		}

		public Map<String, Integer> getKeyIndexMap(String colName) {
			Map<String, Integer> map = new HashMap<String, Integer>();
			Set<Object> processCheckSet = new HashSet<Object>();
			int keyIndex = getHeaderIndex(colName);
			for (int i = 0; i < this.table.size(); i++) {
				List<String> row = this.table.get(i);
				String key = row.get(keyIndex);
				if (processCheckSet.contains(key)) {
					continue;
				}
				processCheckSet.add(key);
				map.put(key, i);
			}
			return map;
		}

		public int getHeaderIndex(String colName) {
			for (int i = 0; i < header.size(); i++) {
				if (header.get(i).equals(colName)) {
					return i;
				}
			}
			return -1;
		}

		public static Result union(Result p1, Result p2) {
			if (p1 == null) {
				return p2;
			}
			if (p2 == null) {
				return p1;
			}
			if (p2.table != null) {
				for (List<String> row : p2.table) {
					p1.table.add(row);
				}
			}
			return p1;
		}

		@Override
		public void start() {
			// TODO Auto-generated method stub

		}

		@Override
		public void done() {
			// TODO Auto-generated method stub

		}

		@Override
		public void setName(String name) {
			this.name = name;
		}

		@Override
		public void setFrom(int from) {
			this.from = from;
		}

		@Override
		public void setHeader(String[] header) {
			this.header = Arrays.asList(header);
			;
		}

		@Override
		public void setTotalCount(int totalCount) {
			this.totalCount = totalCount;
		}

		@Override
		public void setException(Exception e) {
			this.exception = e.getMessage();
		}

		@Override
		public void setRowsAffected(int rowsAffected) {
			this.rowsAffected = rowsAffected;
		}

	}

	/**
	 * ServiceEntry class holds all information a service such as serviceId,
	 * statements, data source name and access.
	 * 
	 * @author tonibuenter
	 */
	public static class ServiceEntry implements Serializable {

		public static final String SYSTEM_ROLENAME = "SYSTEM";

		/**
     * 
     */
		private static final long serialVersionUID = 1L;

		private final String serviceId;
		private final String statements;
		private Set<String> roles;

		private String datasourceName = DEFAULT_DATASOURCE_NAME;

		public ServiceEntry(String serviceId, String statements, String roles) {
			super();
			this.serviceId = serviceId;
			this.statements = statements;
			if (!Utils.isEmpty(roles)) {
				String[] r = roles.split(",");
				this.roles = new HashSet<String>(Arrays.asList(r));
			}
		}

		public ServiceEntry(String serviceId, String statements, Set<String> roles) {
			super();
			this.serviceId = serviceId;
			this.statements = statements;
			this.roles = roles;
		}

		public String getServiceId() {
			return serviceId;
		}

		public String getStatements() {
			return statements;
		}

		public Set<String> getRoleSet() {
			return this.roles;
		}

		public String getRoles() {
			return Utils.joinTokens(roles);
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
			    + statements + ", roles=" + roles + ", datasourceName="
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
			if (this == obj) {
				return true;
			}
			if (obj == null) {
				return false;
			}
			if (getClass() != obj.getClass()) {
				return false;
			}
			ServiceEntry other = (ServiceEntry) obj;
			if (serviceId == null) {
				if (other.serviceId != null) {
					return false;
				}
			} else if (!serviceId.equals(other.serviceId)) {
				return false;
			}
			return true;
		}

	}

	public static class ServiceRepository implements IServiceRepository {

		private static final Logger logger = LoggerFactory
		    .getLogger(ServiceRepository.class);

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

		private IServiceRepository sr() {
			if (sql != null) {
				return sql;
			} else if (json != null) {
				return json;
			} else {
				throw new RuntimeException(
				    "No service repository initialized. Currently 'sql' and 'json' service repositories are available.");
			}

		}

		/**
		 * Creates a ServiceRepository and assign the static instance with it.
		 * 
		 * @param ds
		 *          datasource for ServiceEntries.
		 * @param tableName
		 *          table name (including possible schema prefix)
		 */
		public ServiceRepository(DataSource ds, String tableName) {
			logger.debug("try to create ServiceRepositorySql");
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
			IServiceRepository sr = sr();
			return sr.get(serviceId);
		}

		@Override
		public void add(ServiceEntry serviceEntry) throws Exception {
			IServiceRepository sr = sr();
			sr.add(serviceEntry);
		}

		@Override
		public List<ServiceEntry> list() throws Exception {
			return sr().list();
		}
	}

	public static class ServiceRepositoryJson implements IServiceRepository {

		private static final Logger logger = LoggerFactory
		    .getLogger(ServiceRepositoryJson.class);

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
			logger.debug("found " + entries.size() + " service entries.");
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

		@Override
		public List<ServiceEntry> list() {
			return this.entries;
		}
	}

	public static class ServiceRepositorySql implements IServiceRepository {

		private static final Logger logger = LoggerFactory
		    .getLogger(ServiceRepositorySql.class.getName());

		private final DataSource ds;
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
			String sql = selectQuery != null ? selectQuery : "select "
			    + COL_SERVICE_ID + ", " + COL_STATEMENTS + ", " + COL_ROLES
			    + " from " + tableName + " where " + COL_SERVICE_ID + " = ?";
			List<ServiceEntry> r = _get(sql, serviceId);
			return Utils.isEmpty(r) ? null : r.get(0);
		}

		@Override
		public List<ServiceEntry> list() throws Exception {
			String sql = "select " + COL_SERVICE_ID + ", " + COL_STATEMENTS + ", "
			    + COL_ROLES + ", " + COL_DATASOURCE + " from " + tableName;
			List<ServiceEntry> r = _get(sql);
			return r;
		}

		private List<ServiceEntry> _get(String sql, String... parameters)
		    throws SQLException {
			Connection con = null;
			PreparedStatement ps = null;
			ResultSet rs = null;
			ServiceEntry se = null;
			List<ServiceEntry> result = new ArrayList<ServiceEntry>();
			try {
				con = getConnection();

				ps = con.prepareStatement(sql);
				for (int i = 0; i < parameters.length; i++) {
					ps.setString(i + 1, parameters[i]);
				}

				rs = ps.executeQuery();
				while (rs.next()) {
					String serviceId = rs.getString(COL_SERVICE_ID);
					String statements = rs.getString(COL_STATEMENTS);
					String roles = rs.getString(COL_ROLES);
					se = new RemoteQuery.ServiceEntry(serviceId, statements, roles);
					logger.info("Found " + se);
					result.add(se);
				}
			} finally {
				Utils.closeQuietly(rs);
				Utils.closeQuietly(ps);
				returnConnection(con);
			}
			return result;
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
		public void add(ServiceEntry se) throws Exception {
			Connection con = null;
			try {
				String sql = "insert into " + tableName + " (" + COL_SERVICE_ID + ", "
				    + COL_STATEMENTS + ", " + COL_ROLES + ", " + COL_DATASOURCE
				    + ") values (?,?,?,?)";
				con = getConnection();
				Utils.runQuery(con, sql, se.getServiceId(), se.getStatements(),
				    se.getRoles(), se.getDatasourceName());
			} finally {
				returnConnection(con);
			}

		}

	}

	static class StringTokenizer2 {

		private int index = 0;
		private String[] tokens = null;
		private final StringBuffer buf;
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
			if (buff.length() > 0) {
				buff.setLength(buff.length() - 1);
			}
			return buff.toString();
		}

	}

	public static class Utils {

		private static final Logger logger = LoggerFactory.getLogger(Utils.class);

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

		public static String[] parseCommandValue(String statement) {
			String[] pair = { "", "" };
			int indexOf = statement.indexOf(':');
			if (indexOf == -1) {
				pair[0] = statement;
			} else {
				pair[0] = statement.substring(0, indexOf);
				pair[1] = statement.substring(indexOf + 1);
			}
			return pair;
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
				logger.error(e.getMessage(), e);
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
			logger.error("Parse error for " + isoDateTimeString + ". Return 0.");
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

		public static String trim(String str) {
			if (str == null) {
				return str;
			}
			return str.trim();
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
			return joinTokens(list, DEFAULT_DEL, DEFAULT_ESC);
		}

		public static String joinTokens(Collection<String> list, char del, char esc) {
			if (isEmpty(list)) {
				return "";
			}
			String res = "";
			int i = 0;
			for (String s : list) {
				s = escape(s, del, esc);
				if (i == 0) {
					res = s;
				} else {
					res = res + del + s;
				}
				i++;
			}
			return res;
		}

		public static String joinTokens(String[] arr) {
			return joinTokens(arr, 0, arr.length, DEFAULT_DEL, DEFAULT_ESC);
		}

		public static String joinTokens(String[] arr, int start, char del, char esc) {
			return joinTokens(arr, start, arr.length, del, esc);
		}

		public static String joinTokens(String[] arr, int start, int end, char del,
		    char esc) {
			List<String> list = new ArrayList<String>(end - start);
			for (int i = start; i < arr.length && i < end; i++) {
				list.add(arr[i]);
			}
			return joinTokens(list, del, esc);
		}

		public static String escape(String in, char del, char esc) {
			char[] chars = in.toCharArray();
			String res = "";
			for (char c : chars) {
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
				for (String value : values) {
					set.add(value);
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
				logger.error(e.getMessage(), e);
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
				logger.warn(t.getMessage());
			} finally {
				Utils.closeQuietly(ps);
			}
			return new Integer(-1);
		}

		// TODO not yet done
		public static void processRqQueryText(String rqServices) {

			Map<String, String> parameters = new HashMap<String, String>();
			String query = "";
			BufferedReader in = new BufferedReader(new StringReader(rqServices));
			String line = null;
			boolean startParameter = false;
			boolean startQuery = false;
			try {
				while ((line = in.readLine()) != null) {
					line = line.trim();
					if (line.length() == 0) {
						continue;
					}

					// comment
					if (line.startsWith("--")) {
						if (!startParameter) {
							//
							// execute collected
							//
							if (startQuery) {
								parameters.put("SERVICE_STMT", query);
								// saveServicequery(parameters);
								startQuery = false;

							}
							startParameter = true;

							parameters = new HashMap<String, String>();
						}
						// processParameter(parameters, line.substring(2));
						continue;
					}
					startParameter = false;
					if (!startQuery) {
						startQuery = true;
						query = line + '\n';
					} else {
						query += line + '\n';
					}

				}
				if (startQuery) {
					parameters.put("SERVICE_STMT", query);
					// saveServicequery(parameters);
					startQuery = false;

				}
			} catch (IOException e) {
				logger.error(e.getMessage());
			}

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
				logger.error(e.getMessage(), e);
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
								logger.warn("BLOB type not supported! Returning empty string!");
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

		public static void buildResult(int start, int max, ResultSet rs,
		    IResultListener irl) {
			boolean fromDone = false;
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
				irl.setHeader(header);
				sqlLogger.debug("sql-result-header " + Arrays.toString(header));
				irl.setFrom(-1);
				String[] row = null;
				while (rs.next()) {

					if (counter >= start && ((counter < start + max) || max == -1)) {
						if (fromDone) {
							irl.setFrom(counter);
							fromDone = true;
						}

						row = new String[colums];

						for (int i = 0; i < colums; i++) {
							if (sqlTypes[i] == Types.BLOB) {
								logger.warn("BLOB type not supported! Returning empty string!");
								row[i] = "";
							} else if (sqlTypes[i] == Types.CLOB) {
								Clob clob = (Clob) rs.getObject(i + 1);
								row[i] = blobToString(clob);
							} else {
								Object sqlValue = rs.getObject(i + 1);
								row[i] = sqlValueToString(sqlValue, sqlTypes[i]);
							}
						}
						irl.addRow(row);
						sqlLogger.debug("sql-result-row " + counter + " : "
						    + Arrays.toString(row));
					} else {
						//
					}
					counter++;
				}
				if (fromDone == false) {
					irl.setFrom(-1);
				}
				irl.setTotalCount(counter);

			} catch (Exception e) {
				irl.setException(e);
			} finally {

			}
		}

		//
		// COLLECTION UTILS
		//

		public static <E> boolean isEmpty(Collection<E> c) {
			return c == null || c.size() == 0;
		}

		//
		// MAP UTILS
		//

		public static <E, F> boolean isEmpty(Map<E, F> c) {
			return c == null || c.size() == 0;
		}

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

		public static <K, V> V firstValue(Map<K, V> map) {
			if (map == null) {
				return null;
			}
			for (Entry<K, V> e : map.entrySet()) {
				return e.getValue();
			}
			return null;
		}

		//
		// OBJECT UTILS
		//

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

		public static Serializable deepClone(Serializable s) {
			if (s == null) {
				return s;
			}
			Serializable object = null;
			try {
				ByteArrayOutputStream bos = new ByteArrayOutputStream();
				ObjectOutputStream oos = new ObjectOutputStream(bos);
				oos.writeObject(s);
				oos.flush();
				oos.close();
				bos.close();
				byte[] byteData = bos.toByteArray();

				ByteArrayInputStream bais = new ByteArrayInputStream(byteData);
				ObjectInputStream bis = new ObjectInputStream(bais);
				object = (Serializable) bis.readObject();
				bis.close();
				bais.close();
			} catch (Exception e1) {
				rqLogger.error(e1 + "");
			}
			return object;
		}

		public static String getStringGetterMethodName(Method method) {

			if (method.getParameterTypes().length != 0) {
				return null;
			}
			// if (String.class.equals(method.getReturnType()) == false) {
			// return null;
			// }
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
				logger.error(e.getMessage(), e);
			}
			return map;
		}

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
						logger.warn(e.getMessage());
					}
				}
			}
			return map;
		}

	}

	public static class JsonUtils {

		private static final Logger logger = LoggerFactory
		    .getLogger(JsonUtils.class);

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
			try {
				return new Gson().fromJson(json, claxx);
			} catch (Exception e) {
				logger.warn(
				    "Could not convert json string to object. Class:"
				        + claxx.getSimpleName() + ", json string:" + json, e);
			}
			return null;
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
			JsonElement je = p.parse(jsonString);
			return _toListMapE(je);
		}

		public static Object toObject(String json, Type type) {
			try {
				return new Gson().fromJson(json, type);
			} catch (Exception e) {
				logger.error(e.getMessage(), e);
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
			JsonElement je = p.parse(jsonString);
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
		private static final Logger logger = LoggerFactory
		    .getLogger(ObjectStore.class);

		private final Class<E> resultClass;
		private Set<String> roles;
		private final String userId;

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

		public ObjectStore(Class<E> resultClass) {
			this(resultClass, RemoteQuery.ANONYMOUS, new HashSet<String>());
		}

		public E newInstance(Map<String, String> params) {
			try {
				return Utils.newObject(params, this.resultClass);
			} catch (Exception e) {
				logger.error(e.getMessage());
				return null;
			}
		}

		public E newInstance(Request request) {
			return newInstance(request.getParameterSnapshhot());
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

		public Map<String, String> getMap(String serviceId,
		    Map<String, String> parameters) throws Exception {
			Request request = new Request();
			request.setUserId(userId);
			request.setRoles(roles);
			request.setServiceId(serviceId);
			for (Entry<String, String> e : parameters.entrySet()) {
				request.put(e.getKey(), e.getValue());
			}

			MainQuery mq = new MainQuery();
			Result r = mq.run(request);
			if (r.size > 0) {
				return r.getRowAsMap(0);
			}
			return new HashMap<String, String>();
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

	}

	public static void shutdown() {
		Utils.IsoDateTimeSecTL.remove();
		Utils.IsoDateTimeTL.remove();
		Utils.IsoDateTL.remove();
		Utils.IsoTimeTL.remove();
		Utils.IsoDateTimeFullTL.remove();
	}

}
