package org.remotequery;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Properties;
import java.util.Set;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.remotequery.RemoteQuery.Utils;

public class DefaultWebSecurity implements IWebSecurity {

	public Properties appProperties;
	public HttpServletRequest httpRequest;
	public HttpServletResponse httpResponse;

	@Override
	public void init(Properties appProperties, HttpServletRequest httpRequest, HttpServletResponse httpResponse) {
		this.appProperties = appProperties;
		this.httpRequest = httpRequest;
		this.httpResponse = httpResponse;
	}

	@Override
	public String userId() {
		return "ANONYMOUS";
	}

	@Override
	public Set<String> roles() {
		return new HashSet<String>();
	}

	@SuppressWarnings("unchecked")
	@Override
	public Map<String, String> parameters() {
		Map<String, String> resMap = new HashMap<String, String>();
		Map<String, String[]> allMap = httpRequest.getParameterMap();
		for (String key : allMap.keySet()) {
			String[] strArr = (String[]) allMap.get(key);
			if (strArr.length > 0) {
				resMap.put(key, Utils.joinTokens(strArr));
			} else {
				resMap.put(key, strArr[0]);
			}
		}
		return resMap;
	}

	@Override
	public String service() {
		String serviceString = null;
		String requestUri = httpRequest.getRequestURI();
		String[] parts = requestUri.split("/");
		if (parts.length > 0) {
			serviceString = parts[parts.length - 1];
		}
		return serviceString;
	}

}
