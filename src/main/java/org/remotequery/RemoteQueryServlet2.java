package org.remotequery;

import static org.remotequery.z_RemoteQuery.ENCODING;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import javax.servlet.ServletConfig;
import javax.servlet.ServletContext;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.apache.commons.io.IOUtils;
import org.remotequery.z_RemoteQuery.JsonUtils;
import org.remotequery.z_RemoteQuery.ProcessLog;
import org.remotequery.z_RemoteQuery.Request;
import org.remotequery.z_RemoteQuery.Result;
import org.remotequery.z_RemoteQuery.Utils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * @author tonibuenter
 */
public class RemoteQueryServlet2 extends HttpServlet {
	/**
	   * 
	   */
	private static final long serialVersionUID = 1L;

	//
	private static final Logger logger = LoggerFactory.getLogger(RemoteQueryServlet2.class);

	private final Object lock = new Object();
	private final Properties appProperties = new Properties();

	private IWebRequestFactory webRequestFactory;

	@Override
	public void init(ServletConfig config) throws ServletException {
		super.init(config);

		logger.info("RemoteQueryServlet2 starting ...");

		ServletContext sc = config.getServletContext();
		//
		// APP PROPERTIES
		//

		String appPropertiesFile = System.getProperty("REMOTEQUERY_APP_PROPERTIES_FILE");
		if (!Utils.isBlank(appPropertiesFile)) {
			System.out.println("FOUND REMOTEQUERY_APP_PROPERTIES_FILE from system properties : " + appPropertiesFile);
		}
		if (Utils.isBlank(appPropertiesFile)) {
			appPropertiesFile = sc.getInitParameter("appPropertiesFile");
		}
		if (Utils.isBlank(appPropertiesFile)) {
			logger.error(
					"No value for appPropertiesFile defined in system properties and servlet context init parameters. RemoteQueryServlet will not continue.");
			return;
		}
		InputStream in = null;
		try {
			in = new FileInputStream(new File(appPropertiesFile));
			appProperties.load(in);
		} catch (Exception e) {
			logger.error("Can not read appProperties file: " + appPropertiesFile
					+ ". RemoteQueryServlet will not continue.");
			return;
		} finally {
			IOUtils.closeQuietly(in);
		}

		//
		//
		//

		//

		logger.info("RemoteQueryServlet2 started");
	}

	private IWebRequestFactory webRequestFactory() {
		if (this.webRequestFactory == null) {
			synchronized (lock) {
				if (this.webRequestFactory == null) {
					IWebRequestFactory tmp = null;
					String className = appProperties.getProperty("RemoteQueryServlet.webRequestFactory");
					if (!Utils.isBlank(className)) {
						try {
							tmp = (IWebRequestFactory) Class.forName(className).newInstance();
							tmp.init(appProperties);
							this.webRequestFactory = tmp;
						} catch (Exception e) {
							logger.error("Can not create IWebRequestFactory. "
									+ "Please check the app property RemoteQueryServlet.webRequestFactory", e);
						}
					}
				}
			}
		}
		return this.webRequestFactory;
	}

	@Override
	public void doPost(HttpServletRequest httpRequest, HttpServletResponse response)
			throws ServletException, IOException {
		logger.debug("redirect " + this.getServletName() + " .doPost");
		doGet(httpRequest, response);
	}

	@Override
	public void doGet(final HttpServletRequest httpRequest, HttpServletResponse httpResponse)
			throws ServletException, IOException {

		logger.debug("start " + this.getServletName() + " .doGet");

		String callback = httpRequest.getParameter("callback");

		try {

			IWebRequestFactory wrf = webRequestFactory();

			List<Request> requests = wrf.prepare(httpRequest, httpResponse);

			if (requests == null) {
				return;
			}

			List<Result> results = new ArrayList<Result>();
			for (Request request : requests) {
				Result result = request.run();
				if (result != null) {
					// prevent circular references
					result.subResult = null;
					results.add(result);
				}
			}

			//
			// Writing result to HttpResponse
			//

			if (results.size() > 0) {
				String jsonString = null;
				// prevent circular references
				if (requests.size() == 1) {
					jsonString = JsonUtils.toJson(results.get(0));
				} else {
					jsonString = JsonUtils.toJson(results);
				}
				returnAsJsonString(callback, jsonString, httpResponse);
			} else {
				returnAsJsonString(callback, JsonUtils.toJson("empty"), httpResponse);
			}
		} catch (IllegalStateException ie) {
			logger.warn(ie.getMessage());
		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		} finally {
			ProcessLog.RemoveCurrent();
		}
	}

	public static void returnAsJsonString(String callback, String s, HttpServletResponse response) {
		try {

			byte[] document = s.getBytes(ENCODING);
			response.setContentType("application/json");
			response.setContentLength(document.length);
			response.setCharacterEncoding(ENCODING);

			OutputStream out = response.getOutputStream();

			if (Utils.isBlank(callback)) {
				out.write(document);
			} else {
				out.write((callback + "(").getBytes());
				out.write(document);
				out.write(")".getBytes());
			}
			out.flush();
		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		}
	}

}
