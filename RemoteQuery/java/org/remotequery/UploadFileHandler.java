package org.remotequery;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.util.TreeMap;

import javax.servlet.http.HttpServletRequest;

import org.remotequery.RemoteQueryServlet.IUploadFileHandler;
import org.remotequery.RemoteQueryServlet.WebConstants;

import com.google.gson.internal.Streams;


public class UploadFileHandler {
	public  void processingMultipart(HttpServletRequest httpRequest, IUploadFileHandler uploadFileHandler,
      ) throws Exception
{

  boolean isMultipart = ServletFileUpload.isMultipartContent(httpRequest);
  MyRequestParameters myParameters = new MyRequestParameters();
  if (isMultipart)
  {
      //
      // multi part processing
      //
      TreeMap<String, String> files = new TreeMap<String, String>();

      // Create a new file upload handler
      ServletFileUpload upload = new ServletFileUpload();

      // Parse the request
      FileItemIterator iter = upload.getItemIterator(httpRequest);
      while (iter.hasNext())
      {
          FileItemStream item = iter.next();
          String name = item.getFieldName();
          InputStream stream = item.openStream();
          if (item.isFormField())
          {
              if (name.startsWith(IWebRequest.DATAURL_PREFIX))
              {
                  //
                  // check for dataUrl, see also : http://en.wikipedia.org/wiki/Data_URI_scheme
                  //
                  String fileName = name.substring(IWebRequest.DATAURL_PREFIX.length());
                  String value = Streams.asString(stream, AppConstants.encoding);
                  byte[] docu = StringUtils.dataUrl2Binary(value);
                  InputStream stream2 = new ByteArrayInputStream(docu);
                  String fileIdentificator = uploadFileHandler.processFile(fileName, stream2);
                  files.put(name, fileIdentificator);
                  IOUtils.closeQuietly(stream2);
              }
              else
              {
                  String value = Streams.asString(stream, AppConstants.encoding);

                  logger.debug("Form field " + name + " with value " + value + " detected.");
                  if (UUtils.rnn(value).length() > WebConstants.MAX_FIELD_LENGTH)
                  {
                      logger.warn("Field value for " + name + " is to long. It is removed!");
                  }
                  else
                  {
                      myParameters.add(name, value);
                  }
              }
          }
          else if (ap.canUploadFiles())
          {
              String fileName = FilenameUtils.getName(item.getName());
              if (StringUtils.isNotBlank(fileName))
              {
                  String fileIdentificator = uploadFileHandler.processFile(fileName, stream);
                  files.put(name, fileIdentificator);
              }
          }
      }
  }
}

}