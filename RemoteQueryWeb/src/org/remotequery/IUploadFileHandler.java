package org.remotequery;

import java.io.InputStream;

import org.remotequery.RemoteQueryServlet.RequestData;

public interface IUploadFileHandler {

	void processFile(String filename, InputStream stream, RequestData requestData);

	void done(RequestData requestData);

}