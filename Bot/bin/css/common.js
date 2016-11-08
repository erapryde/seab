function checkLength(a, b, d) {
	if (a.length >= b && a.length <= d)
		return true;
	return false;
}
function trim(a) {
	return a.replace(/^\s+|\s+$/g, "");
}
function ltrim(a) {
	return a.replace(/^\s+/, "");
}
function rtrim(a) {
	return a.replace(/\s+$/, "");
}
function validateID(a, b) {
	a = trim(a);
	if (a == "") {
		alert(b + " is empty.");
		return false;
	}
	return true;
}
function checkFormat(a) {
	for ( var b = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~", d, c = 0; c < a.length; c++) {
		d = a.charAt(c);
		d = b.indexOf(d);
		if (d < 0)
			return false;
	}
	return true;
}
function showDiv(a, b) {
	if (document.getElementById) {
		a = document.getElementById(a);
		//a.style.display = b ? "inline" : "none";
	}
}
function loadSpinner(a) {
	if (document.images) {
		preload_image_object = new Image;
		preload_image_object.src = a;
	}
}
function toggleSpinner(a, b) {
	return showDiv(a, b);
}
function displayMessage(a) {
	//alert(a);
	var splitString= a.split(":");
	var errorMsg = splitString[1] != null?splitString[1]:"";
	//alert(document.getElementById("statusbar"));
	if(document.getElementById("statusbar")!=null){
		document.getElementById("statusbar").style.display="block";
		document.getElementById("statusbar").innerHTML = errorMsg;
	}
	return false;
}
function handleKeyPress(a, b) {
	document.getElementById("statusbar").style.display="none";
	kc = a.keyCode ? a.keyCode : a.which;
	sk = a.shiftKey ? a.shiftKey : kc == 16 ? true : false;
	if (kc >= 65 && kc <= 90 && !sk || kc >= 97 && kc <= 122 && sk)
		document.getElementById("divCapsLock" + b).style.display = "block";
	else
		document.getElementById("divCapsLock" + b).style.display = "none";
	if (kc == 13) {
		login(true);
		return false;
	} else
		return true;
}