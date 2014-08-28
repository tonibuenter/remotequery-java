package remotequery.examples;

import java.sql.Connection;
import java.util.logging.Logger;

import javax.servlet.ServletContext;
import javax.servlet.ServletContextEvent;
import javax.servlet.ServletContextListener;
import javax.sql.DataSource;

import org.hsqldb.jdbc.JDBCPool;
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

	@SuppressWarnings("unused")
	private static final long serialVersionUID = 1L;

	private static Logger logger = Logger.getLogger(AppInitListener1.class
	    .getName());

	static {
		System.out.println("loading " + AppInitListener1.class.getCanonicalName());
	}

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

			String dbUrl = sc.getInitParameter("dbUrl");
			dbUrl = Utils.isEmpty(dbUrl) ? "jdbc:hsqldb:file:~/RemoteQueryDB;shutdown=true"
			    : dbUrl;
			logger.info("Try to create connection to " + dbUrl);
			if (USE_HSQLDB) {
				dataSource = new JDBCPool();
				((JDBCPool) dataSource).setDatabase(dbUrl);
				((JDBCPool) dataSource).setUser("SA");
				((JDBCPool) dataSource).setPassword("SA");
			} else {
				dataSource = new MysqlConnectionPoolDataSource();
				((MysqlConnectionPoolDataSource) dataSource).setDatabaseName(dbUrl);
				((MysqlConnectionPoolDataSource) dataSource).setUser("...");
				((MysqlConnectionPoolDataSource) dataSource).setPassword("...");
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
				String createRemoteQueryServiceTableSql = "create table REMOTE_QUERY_SERVICE ("
				    + "SERVICE_ID varchar(1024), "
				    + "STATEMENTS varchar(1024), "
				    + "ROLES varchar(1024), "
				    + "DATASOURCE varchar(1024), "
				    + " PRIMARY KEY(SERVICE_ID) " + ")";
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
			if (dataSource != null) {
				// depending on DataSource class : dataSource.close()
			}
		} catch (Exception e) {
			logger.severe("DataSource close: " + e.getMessage());
		}
		logger.info(this.getClass().getName() + " contextDestroyed done.");
	}
}
