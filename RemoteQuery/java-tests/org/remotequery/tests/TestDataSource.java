package org.remotequery.tests;

import java.io.PrintWriter;
import java.sql.Array;
import java.sql.Blob;
import java.sql.CallableStatement;
import java.sql.Clob;
import java.sql.Connection;
import java.sql.DatabaseMetaData;
import java.sql.NClob;
import java.sql.PreparedStatement;
import java.sql.SQLClientInfoException;
import java.sql.SQLException;
import java.sql.SQLFeatureNotSupportedException;
import java.sql.SQLWarning;
import java.sql.SQLXML;
import java.sql.Savepoint;
import java.sql.Statement;
import java.sql.Struct;
import java.util.Map;
import java.util.Properties;
import java.util.concurrent.Executor;
import java.util.logging.Logger;

import javax.sql.DataSource;

public class TestDataSource implements DataSource {

	MyWrappedConnection wrappedConnection;

	TestDataSource(Connection con) {
		this.wrappedConnection = new MyWrappedConnection(con);
	}

	@Override
	public PrintWriter getLogWriter() throws SQLException {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void setLogWriter(PrintWriter out) throws SQLException {
		// TODO Auto-generated method stub

	}

	@Override
	public void setLoginTimeout(int seconds) throws SQLException {
		// TODO Auto-generated method stub

	}

	@Override
	public int getLoginTimeout() throws SQLException {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public <T> T unwrap(Class<T> iface) throws SQLException {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public boolean isWrapperFor(Class<?> iface) throws SQLException {
		// TODO Auto-generated method stub
		return false;
	}

	@Override
	public Connection getConnection() throws SQLException {
		// TODO Auto-generated method stub
		return wrappedConnection;
	}

	@Override
	public Connection getConnection(String username, String password)
	    throws SQLException {
		return wrappedConnection;
	}

	@Override
  public Logger getParentLogger() throws SQLFeatureNotSupportedException {
	  // TODO Auto-generated method stub
	  return null;
  }

}

class MyWrappedConnection implements Connection {

	Connection c;

	MyWrappedConnection(Connection c) {
		this.c = c;
	}

	public <T> T unwrap(Class<T> iface) throws SQLException {
		return c.unwrap(iface);
	}

	public boolean isWrapperFor(Class<?> iface) throws SQLException {
		return c.isWrapperFor(iface);
	}

	public Statement createStatement() throws SQLException {
		return c.createStatement();
	}

	public PreparedStatement prepareStatement(String sql) throws SQLException {
		return c.prepareStatement(sql);
	}

	public CallableStatement prepareCall(String sql) throws SQLException {
		return c.prepareCall(sql);
	}

	public String nativeSQL(String sql) throws SQLException {
		return c.nativeSQL(sql);
	}

	public void setAutoCommit(boolean autoCommit) throws SQLException {
		c.setAutoCommit(autoCommit);
	}

	public boolean getAutoCommit() throws SQLException {
		return c.getAutoCommit();
	}

	public void commit() throws SQLException {
		c.commit();
	}

	public void rollback() throws SQLException {
		c.rollback();
	}

	public void close() throws SQLException {
		// c.close();
	}

	public boolean isClosed() throws SQLException {
		return c.isClosed();
	}

	public DatabaseMetaData getMetaData() throws SQLException {
		return c.getMetaData();
	}

	public void setReadOnly(boolean readOnly) throws SQLException {
		c.setReadOnly(readOnly);
	}

	public boolean isReadOnly() throws SQLException {
		return c.isReadOnly();
	}

	public void setCatalog(String catalog) throws SQLException {
		c.setCatalog(catalog);
	}

	public String getCatalog() throws SQLException {
		return c.getCatalog();
	}

	public void setTransactionIsolation(int level) throws SQLException {
		c.setTransactionIsolation(level);
	}

	public int getTransactionIsolation() throws SQLException {
		return c.getTransactionIsolation();
	}

	public SQLWarning getWarnings() throws SQLException {
		return c.getWarnings();
	}

	public void clearWarnings() throws SQLException {
		c.clearWarnings();
	}

	public Statement createStatement(int resultSetType, int resultSetConcurrency)
	    throws SQLException {
		return c.createStatement(resultSetType, resultSetConcurrency);
	}

	public PreparedStatement prepareStatement(String sql, int resultSetType,
	    int resultSetConcurrency) throws SQLException {
		return c.prepareStatement(sql, resultSetType, resultSetConcurrency);
	}

	public CallableStatement prepareCall(String sql, int resultSetType,
	    int resultSetConcurrency) throws SQLException {
		return c.prepareCall(sql, resultSetType, resultSetConcurrency);
	}

	public Map<String, Class<?>> getTypeMap() throws SQLException {
		return c.getTypeMap();
	}

	public void setTypeMap(Map<String, Class<?>> map) throws SQLException {
		c.setTypeMap(map);
	}

	public void setHoldability(int holdability) throws SQLException {
		c.setHoldability(holdability);
	}

	public int getHoldability() throws SQLException {
		return c.getHoldability();
	}

	public Savepoint setSavepoint() throws SQLException {
		return c.setSavepoint();
	}

	public Savepoint setSavepoint(String name) throws SQLException {
		return c.setSavepoint(name);
	}

	public void rollback(Savepoint savepoint) throws SQLException {
		c.rollback(savepoint);
	}

	public void releaseSavepoint(Savepoint savepoint) throws SQLException {
		c.releaseSavepoint(savepoint);
	}

	public Statement createStatement(int resultSetType, int resultSetConcurrency,
	    int resultSetHoldability) throws SQLException {
		return c.createStatement(resultSetType, resultSetConcurrency,
		    resultSetHoldability);
	}

	public PreparedStatement prepareStatement(String sql, int resultSetType,
	    int resultSetConcurrency, int resultSetHoldability) throws SQLException {
		return c.prepareStatement(sql, resultSetType, resultSetConcurrency,
		    resultSetHoldability);
	}

	public CallableStatement prepareCall(String sql, int resultSetType,
	    int resultSetConcurrency, int resultSetHoldability) throws SQLException {
		return c.prepareCall(sql, resultSetType, resultSetConcurrency,
		    resultSetHoldability);
	}

	public PreparedStatement prepareStatement(String sql, int autoGeneratedKeys)
	    throws SQLException {
		return c.prepareStatement(sql, autoGeneratedKeys);
	}

	public PreparedStatement prepareStatement(String sql, int[] columnIndexes)
	    throws SQLException {
		return c.prepareStatement(sql, columnIndexes);
	}

	public PreparedStatement prepareStatement(String sql, String[] columnNames)
	    throws SQLException {
		return c.prepareStatement(sql, columnNames);
	}

	public Clob createClob() throws SQLException {
		return c.createClob();
	}

	public Blob createBlob() throws SQLException {
		return c.createBlob();
	}

	public NClob createNClob() throws SQLException {
		return c.createNClob();
	}

	public SQLXML createSQLXML() throws SQLException {
		return c.createSQLXML();
	}

	public boolean isValid(int timeout) throws SQLException {
		return c.isValid(timeout);
	}

	public void setClientInfo(String name, String value)
	    throws SQLClientInfoException {
		c.setClientInfo(name, value);
	}

	public void setClientInfo(Properties properties)
	    throws SQLClientInfoException {
		c.setClientInfo(properties);
	}

	public String getClientInfo(String name) throws SQLException {
		return c.getClientInfo(name);
	}

	public Properties getClientInfo() throws SQLException {
		return c.getClientInfo();
	}

	public Array createArrayOf(String typeName, Object[] elements)
	    throws SQLException {
		return c.createArrayOf(typeName, elements);
	}

	public Struct createStruct(String typeName, Object[] attributes)
	    throws SQLException {
		return c.createStruct(typeName, attributes);
	}

	@Override
  public void setSchema(String schema) throws SQLException {
	  // TODO Auto-generated method stub
	  
  }

	@Override
  public String getSchema() throws SQLException {
	  // TODO Auto-generated method stub
	  return null;
  }

	@Override
  public void abort(Executor executor) throws SQLException {
	  // TODO Auto-generated method stub
	  
  }

	@Override
  public void setNetworkTimeout(Executor executor, int milliseconds)
      throws SQLException {
	  // TODO Auto-generated method stub
	  
  }

	@Override
  public int getNetworkTimeout() throws SQLException {
	  // TODO Auto-generated method stub
	  return 0;
  }
}
