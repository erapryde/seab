var g_xmlhttp;
try {
	g_xmlhttp = new XMLHttpRequest;
} catch (e$$3) {
	try {
		g_xmlhttp = new ActiveXObject("Msxml2.XMLHTTP");
	} catch (e$$4) {
		try {
			g_xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
		} catch (e$$5) {
		}
	}
}
g_xmlhttp || displayMessage("Cannot obtain XMLHttpRequest object!");
function sendRequest(a) {
	g_xmlhttp.open("POST", "auth/preAuthenticate", false);
	g_xmlhttp.setRequestHeader("Content-Type", "text/xml");
	g_xmlhttp.send(a);
	
	return a = g_xmlhttp.responseText;
}
