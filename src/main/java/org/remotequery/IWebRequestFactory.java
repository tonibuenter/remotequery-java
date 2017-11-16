package org.remotequery;

import java.util.List;
import java.util.Properties;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.remotequery.z_RemoteQuery.Request;

public interface IWebRequestFactory {

	public void init(Properties appProperties);

	public List<Request> prepare(HttpServletRequest httpRequest, HttpServletResponse httpResponse);

}
