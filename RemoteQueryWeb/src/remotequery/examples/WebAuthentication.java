package remotequery.examples;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import java.util.logging.Logger;

import org.remotequery.RemoteQuery.IQuery;
import org.remotequery.RemoteQuery.LevelConstants;
import org.remotequery.RemoteQuery.ProcessLog;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.Utils;

public class WebAuthentication implements IQuery {

	private static Logger logger = Logger.getLogger(WebAuthentication.class
	    .getName());
	public static Map<String, Long> sessions = new HashMap<String, Long>();

	private static final long serialVersionUID = 1L;

	@Override
	public Result run(Request request) {

		String sessionId = request.getValue("sessionId");
		String userId = request.getValue("userId");
		String secretWord = request.getValue("secretWord");

		Long sessionTime = sessions.get(sessionId);
		boolean isSessionOk = sessionTime != null
		    && (System.currentTimeMillis() - sessionTime) < (1000 * 60 * 60);
		if (isSessionOk) {
			return sessionOkResult(request, sessionId);
		} else {
			// try login
			if (!Utils.isBlank(userId) && !Utils.isBlank(secretWord)) {
				if (!Utils.isBlank(sessionId = createSessionId(userId, secretWord))) {
					return sessionOkResult(request, sessionId);
				}
			}
		}
		return authFailedResult("no session, no (userId, secretWord)");
	}

	private Result sessionOkResult(Request request, String sessionId) {
		sessions.put(sessionId, System.currentTimeMillis());
		request.getParameters(LevelConstants.SESSION).put("sessionId", sessionId);
		// processing roles
		Set<String> roles = new HashSet<String>();
		roles.add("USER");
		request.setRoles(roles);
		// result
		Result r = new Result("sessionId");
		r.addRow(sessionId);
		return r;
	}

	private String createSessionId(String userId, String secretWord) {
		String sessionId = userId.equals(secretWord) ? "Session-" + Math.random()
		    : null;
		if (!Utils.isBlank(sessionId)) {

			return sessionId;
		}
		return null;
	}

	private Result authFailedResult(String message) {
		ProcessLog processLog = ProcessLog.Current();
		processLog.warn("No session found", logger);
		Result result = new Result(processLog);
		result.setException(message);
		return result;
	}
}