# webserver-printing

This is a fork of [drualcman's Local Web Server for Printing](https://github.com/drualcman/webserver-printing). This fork contains a few changes:

- Printer names that are network paths no longer invalidate the JSON.
- Added exception catching for various errors that may occur to someone using this program for the first time (such as needing to run as admin or having the port blocked by another process).

# Summary

This is a Local Web Server that can access any printer devices on the host computer and print to them via a HTTP request. This can open up the ability to make asynchronous print requests through JavaScript, cURL or any other tool capable of sending HTTP requests. This tool is good when you need to print from your web app directly to a printer available to the server.

## How to use
### From JAVASCRIPT or HTML FORM can send request to this server. Can use post, get or mix the 2 options on the request call like.
* ARGUMENTS (all optional)
* PROTOCOL: HTTTP or HTTPS
* SERVER: LOCALHOST
* PORT NUMBER: port number to use, default 8888, can send more than one

### PROPERTIES
* PRINTER = Printer Name to use, this is REQUIRED
* URL = Url to request a file to print, PDF, or any other format, but not HTML page.
* FILE = Full path about some LOCAL FILE on the machine. This file must be exist on the local computer request to print, not in a server
* COUNT = Number of copies for the document. Default always 1

### FUNCTIONS
printerlist = Get the printers installed on the computer

## Examples
```
//post data
var data = new FormData();
data.append("printer", "[printer name]");
data.append("url", "[url with a document to print]");           //if not url, send path
data.append("file", "[exact path with the file to print]");     //if not path, send url
data.append("count", "[number of copioes]");                    //optional. Default 1
```

### Get data
* Get request with ```http://localhost:8888/printerlist```
* Get request with ``` http://localhost:8888?printer=[printer name]&url[url file to print]```
* Get request with ```http://localhost:8888?printer=[printer name]&url[url file to print]&count=3```
* Get request with ```http://localhost:8888?printer=[printer name]&file[full path file to print]```
* Get request with ```http://localhost:8888?printer=[printer name]&file[full path file to print]&count=3```

### Post request with url http://localhost:8888 and the form data

```
//post data
var data = new FormData();
data.append("printer", "[printer name]");
data.append("url", "[url with a document to print]");           //if url not send path
```

You can combine post data and get data. Property only can send once or in get variables or in post variables.

### Contributions From
RAW PRINT are used on this project from https://github.com/frogmorecs/RawPrint but with a small changes. Thanks to the owner

