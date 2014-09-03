package remotequery.examples;

import java.io.File;
import java.io.FilenameFilter;
import java.io.IOException;
import java.sql.Connection;
import java.util.Arrays;
import java.util.logging.Logger;

import javax.servlet.ServletContext;
import javax.servlet.ServletContextEvent;
import javax.servlet.ServletContextListener;
import javax.sql.DataSource;

import org.apache.commons.io.FileUtils;
import org.hsqldb.jdbc.JDBCPool;
import org.remotequery.RemoteQuery;
import org.remotequery.RemoteQuery.BuiltIns;
import org.remotequery.RemoteQuery.DataSourceEntry;
import org.remotequery.RemoteQuery.IServiceRepository;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.ServiceRepository;
import org.remotequery.RemoteQuery.Utils;

import com.mysql.jdbc.jdbc2.optional.MysqlConnectionPoolDataSource;

public class AppInitListener1 implements ServletContextListener {

	private DataSource dataSource = null;

	public static boolean USE_JSON_SERVICE_REPOSITORY = false;
	public static boolean USE_HSQLDB = true;
	public static String accessServiceId = "WebAuthentication";
	public static String accessServiceClass = "remotequery.examples.WebAuthentication";

	@SuppressWarnings("unused")
	private static final long serialVersionUID = 1L;

	private static Logger logger = Logger.getLogger(AppInitListener1.class
	    .getName());

	static {
		System.out.println("loading " + AppInitListener1.class.getCanonicalName());
	}

	private Closeable closeable;

	public void contextInitialized(ServletContextEvent event) {
		//
		//
		//

		ServletContext sc = event.getServletContext();
		Connection connection = null;
		try {
			//
			// webInfPath
			//
			String webInfPath = sc.getRealPath("/WEB-INF");
			logger.info("webInfPath:" + webInfPath);

			// init RemoteQuery Parts

			//
			// AppInit-A :: Init default DataSource
			//
			String createRemoteQueryServiceTableSql = null;

			String dbUrl = sc.getInitParameter("dbUrl");
			dbUrl = Utils.isEmpty(dbUrl) ? "jdbc:hsqldb:file:~/RemoteQueryDB;shutdown=true"
			    : dbUrl;
			logger.info("Try to create connection to " + dbUrl);
			if (USE_HSQLDB) {
				createRemoteQueryServiceTableSql = "create table REMOTE_QUERY_SERVICE ("
				    + "SERVICE_ID varchar(512), "
				    + "STATEMENTS varchar(4096), "
				    + "ROLES varchar(1024), "
				    + "DATASOURCE varchar(1024), "
				    + " PRIMARY KEY(SERVICE_ID) " + ")";
				dataSource = new JDBCPool();
				((JDBCPool) dataSource).setDatabase(dbUrl);
				((JDBCPool) dataSource).setUser("SA");
				((JDBCPool) dataSource).setPassword("SA");
				closeable = new Closeable() {
					public void close() throws Exception {
						((JDBCPool) dataSource).close(2);
					}
				};
			} else {
				createRemoteQueryServiceTableSql = "create table REMOTE_QUERY_SERVICE ("
				    + "SERVICE_ID varchar(512), "
				    + "STATEMENTS text, "
				    + "ROLES varchar(1024), "
				    + "DATASOURCE varchar(1024), "
				    + " PRIMARY KEY(SERVICE_ID) " + ")";
				dataSource = new MysqlConnectionPoolDataSource();
				((MysqlConnectionPoolDataSource) dataSource).setUrl(dbUrl);
				((MysqlConnectionPoolDataSource) dataSource).setUser("...");
				((MysqlConnectionPoolDataSource) dataSource).setPassword("...");
				closeable = new Closeable() {
					public void close() throws Exception {
						((MysqlConnectionPoolDataSource) dataSource).getPooledConnection()
						    .close();

					}
				};
			}

			// getting a connection initialisation of the services
			connection = dataSource.getConnection();

			// create and set as default dataSource
			new DataSourceEntry(dataSource);

			//
			// AppInit-B :: Init default ServiceRepository
			//

			IServiceRepository repo = null;

			// create and set as default serviceRepository
			if (USE_JSON_SERVICE_REPOSITORY) {
				// create a JSON ServiceRepository
				repo = new ServiceRepository("[]");
			} else {
				// create a ServiceRepository in the Database

				String dropRemoteQueryServiceTableSql = "drop table REMOTE_QUERY_SERVICE";
				Utils.runQuery(connection, dropRemoteQueryServiceTableSql);
				Utils.runQuery(connection, createRemoteQueryServiceTableSql);
				repo = new ServiceRepository(dataSource, "REMOTE_QUERY_SERVICE");
			}

			//
			// AppInit-D :: Addl Built-In RegisterService service
			//

			//

			ServiceEntry se = new ServiceEntry("RegisterService", "java:"
			    + BuiltIns.RegisterService.class.getName(), "");

			repo.add(se);
			se = new ServiceEntry("ListServices", "java:"
			    + BuiltIns.ListServices.class.getName(), "");

			//

			repo.add(se);
			//
			// AppInit-E :: Init DB and RemoteQueries (only for Development!)
			//

			// create ServiceEntry for the create statements
			String createAddressQuery = "create table ADDRESS (" + "FI"
			    + "RST_NAME varchar(1024), " + "LAST_NAME varchar(1024), "
			    + "STREET varchar(1024), " + "CITY varchar(1024), "
			    + "ZIP varchar(1024), " + "COUNTRY varchar(1024))";
			logger.info("Try to create ADDRESS table ...");
			Utils.runQuery(connection, createAddressQuery);

			//
			//
			//

		} catch (Exception e) {
			logger.severe(e.getLocalizedMessage());
		} finally {
			Utils.closeQuietly(connection);
		}
		logger.info(this.getClass().getName() + " contextInitialized done.");
	}

	public void contextDestroyed(ServletContextEvent sce) {
		try {
			if (closeable != null) {
				closeable.close();
			}
		} catch (Exception e) {
			logger.severe("closeable close: " + e.getMessage());
		}
		logger.info(this.getClass().getName() + " contextDestroyed done.");
	}

	private void loadRqSqlFiles(File rqSqlFIle) throws IOException {
		//
		// *.rq.sql
		//

		File[] sq_sqlFiles = rqSqlFIle.listFiles(new FilenameFilter() {
			@Override
			public boolean accept(File arg0, String arg1) {
				String filename = arg1.toLowerCase();
				return filename.endsWith("rq.sql");
			}
		});

		Arrays.sort(sq_sqlFiles);

		for (File sqlFile : sq_sqlFiles) {
			String rqStatements = FileUtils.readFileToString(sqlFile);
			RemoteQuery.Utils.processRqQueryText(rqStatements);
		}
	}
}

interface Closeable {
	void close() throws Exception;
}
