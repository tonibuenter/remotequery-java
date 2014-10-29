package org.remotequery;

import java.io.InputStream;

public interface IUploadFileHandler {
	String processFile(String filename, InputStream stream);
}