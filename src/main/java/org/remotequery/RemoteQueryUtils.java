package org.remotequery;

import static org.apache.commons.lang3.StringUtils.trim;

import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.HashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.apache.commons.lang3.StringUtils;
import org.apache.commons.lang3.SystemUtils;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class RemoteQueryUtils {

	private static Logger logger = LoggerFactory.getLogger(RemoteQueryUtils.class);

	/**
	 * @param con
	 * @param rqSqlText
	 * @param saveServiceId
	 * @param source
	 */
	public static void processRqSqlText(String rqSqlText, String saveServiceId, String source) {
		int counter = 0;
		Map<String, String> parameters = new HashMap<String, String>();
		String statements = "";
		try {

			String[] lines = StringUtils.split(rqSqlText, SystemUtils.LINE_SEPARATOR);
			boolean inComment = false;
			boolean inStatement = false;

			for (String line2 : lines) {
				String line = StringUtils.trim(line2);
				if (StringUtils.isBlank(line)) {
					continue;
				}
				// comment
				if (line.startsWith("--")) {
					if (!inComment) {
						//
						// execute collected
						//
						if (inStatement) {
							saveRQService(saveServiceId, parameters, statements, source);
							statements = "";
							parameters = new HashMap<String, String>();
							inStatement = false;
							counter++;
						}
					}
					inComment = true;
					processParameter(parameters, line.substring(2));
					continue;
				}
				inComment = false;
				inStatement = true;
				statements += line2 + '\n';
			}
			if (inStatement) {
				saveRQService(saveServiceId, parameters, statements, source);
				counter++;
			}

		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		} finally {

		}
		logger.info(source + " : " + counter + " sq sql statements done.");

	}

	/**
	 * @param saveServiceId
	 * @param parameters
	 * @param statements
	 * @param source
	 * @return
	 */
	public static Result saveRQService(String saveServiceId, Map<String, String> parameters, String statements,
	    String source) {
		parameters.put("source", source);
		parameters.put("statements", statements);
		return new Request().setServiceId(saveServiceId).addRole("SYSTEM").put(parameters).run();
	}

	/**
	 * 
	 * @param parameters
	 * @param line
	 */
	public static void processParameter(Map<String, String> parameters, String line) {
		String[] p = StringUtils.split(line, "=");
		if (p.length > 1) {
			// String name = Utils.camelCase(trim(p[0]));
			String name = trim(p[0]);
			String value = trim(p[1]);
			parameters.put(name, value);
		}
	}

	/**
	 * @param con
	 * @param statements
	 * @param source
	 * @return
	 */
	public static int processSqlText(Connection con, String statements, String source) {
		int counter = 0;
		Statement stmt = null;
		String[] lines = StringUtils.split(statements, SystemUtils.LINE_SEPARATOR);
		String sqlStatement = "";
		for (int i = 0; i < lines.length; i++) {
			String origLine = lines[i];
			String line = StringUtils.trim(lines[i]);
			// comment
			if (line.startsWith("--") || StringUtils.isEmpty(line)) {
				continue;
			}
			// sqlStatement end
			if (line.endsWith(";")) {
				// sqlStatement += " " + line.substring(0, line.length() - 1);
				sqlStatement += line.substring(0, line.length() - 1) + SystemUtils.LINE_SEPARATOR;
				try {
					stmt = con.createStatement();
					stmt.execute(sqlStatement);
					int updateCount = stmt.getUpdateCount();
					logger.info("Update count is: " + updateCount + " on :" + sqlStatement);
					counter++;
				} catch (Exception e) {
					logger.warn(source + ":" + i + ": " + e.getMessage());
				} finally {
					try {
						stmt.close();
					} catch (SQLException e) {
						logger.warn("On stmt.close:" + i + ": " + e.getMessage());
					}
				}

				sqlStatement = "";
				continue;
			}
			// else
			// sqlStatement += " " + line;
			sqlStatement += origLine + SystemUtils.LINE_SEPARATOR;
		}
		logger.info(source + " : " + counter + " sql statements done.");
		return counter;
	}

	public static String texting(String templateString, Map<String, String> map) {

		if (map == null) {
			return templateString;
		}

		Pattern ptn = Pattern.compile("\\:\\w+");

		Matcher matcher = ptn.matcher(templateString);
		StringBuffer stringBuffer = new StringBuffer();
		while (matcher.find()) {
			String s = matcher.group();
			String key = s.substring(1);
			if (map.containsKey(key)) {
				matcher.appendReplacement(stringBuffer, map.get(key));
			} else {
				matcher.appendReplacement(stringBuffer, s);
			}
		}
		return stringBuffer.toString();

	}

}
