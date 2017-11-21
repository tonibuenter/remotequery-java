package org.remotequery.web;


import java.io.IOException;
import java.io.OutputStream;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.servlet.ServletConfig;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.apache.commons.lang3.StringUtils;
import org.remotequery.RemoteQuery;
import org.remotequery.RemoteQuery.JsonUtils;
import org.remotequery.RemoteQuery.ProcessLog;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.Utils;
import org.remotequery.tests.TestCentral;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * The RemoteQueryWeb is probably the most simple way using RemoteQuery with
 * Java web server technology.
 * 
 * If the web server provides proper user authentication RemoteQueryWeb is also
 * secured.
 * 
 * For role detection it is assumed the a service Role.select is available.
 * 
 */
public class RemoteQueryWeb extends HttpServlet {
	/**
	   * 
	   */
	private static final long serialVersionUID = 1L;

	/**
	 * Class containing constants for the web programming with RemoteQuery.
	 * 
	 * @author tonibuenter
	 */

	//
	private static final Logger logger = LoggerFactory.getLogger(RemoteQueryWeb.class);

	@Override
	public void init(ServletConfig config) throws ServletException {
		super.init(config);
		logger.info("RemoteQueryWeb starting using TestCentral for RemoteQuery initialisation ...");
		try {
			TestCentral.init();
		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		}
		logger.info("RemoteQueryWeb started");
	}

	@Override
	public void doPost(HttpServletRequest httpRequest, HttpServletResponse response)
			throws ServletException, IOException {
		doGet(httpRequest, response);
	}

	@SuppressWarnings("unchecked")
	@Override
	public void doGet(final HttpServletRequest httpRequest, HttpServletResponse httpResponse)
			throws ServletException, IOException {

		String serviceId = null;
		Map<String, String> parameters = new HashMap<String, String>();
		String userId = null;
		List<String> roles = null;

		Request request = new Request();
		Result result = null;

		//
		// Detecting userId, defaults to ANONYMOUS
		//

		userId = httpRequest.getUserPrincipal() != null ? httpRequest.getUserPrincipal().getName()
				: RemoteQuery.ANONYMOUS;

		String callback = httpRequest.getParameter("callback");

		//
		// Detecting serviceId
		//

		String requestUri = httpRequest.getRequestURI();
		String[] parts = requestUri.split("/");
		if (parts.length > 0) {
			serviceId = parts[parts.length - 1];
		}
		logger.debug("serviceId: " + serviceId);
		if (Utils.isBlank(serviceId)) {
			httpResponse.sendError(401);
			return;
		}

		//
		// Detecting parameters
		//

		for (Object key : httpRequest.getParameterMap().keySet()) {
			String[] ps = (String[]) httpRequest.getParameterMap().get(key);
			parameters.put((String) key, StringUtils.join(ps));
		}

		try {

			//
			// Read roles for userId from session of from service 'Role.select'
			//

			request.setUserId(userId);

			roles = (List<String>) httpRequest.getSession().getAttribute("roles");
			if (roles == null) {
				result = request.setServiceId("Role.select").put("userId", userId).run();

				roles = result.getColumn("role");
				httpRequest.getSession().setAttribute("roles", roles);
			}

			//
			// Run the main request
			//

			result = request.setServiceId(serviceId).put(parameters).setRoles(roles).run();

			//
			// Write back the result object as JSON
			//

			if (result != null) {
				String s = JsonUtils.toJson(result);
				returnAsJsonString(callback, s, httpResponse);
			} else {
				returnAsJsonString(callback, JsonUtils.toJson("empty"), httpResponse);
			}
		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		} finally {
			ProcessLog.RemoveCurrent();
		}
	}

	public static void returnAsJsonString(String callback, String s, HttpServletResponse response) {
		try {
			byte[] document = s.getBytes("utf-8");
			response.setContentType("application/json");
			response.setContentLength(document.length);
			response.setCharacterEncoding("utf-8");

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
