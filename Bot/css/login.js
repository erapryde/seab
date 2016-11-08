var g_key = "", n = "";
var nounce="";
var c = "";
function deriveKey(a, b) {
	a = new PBKDF2(a, b, 100, 16);
	function d(e) {
	}
	function c(e) {
		g_key = hexToByteArray(e);
	}
	a.deriveKey(d, c);
}

function fidLogin(a) {
	if(a==true)
	{
		g_key = "";
	}
	//var b = document.login.userId.value, d = document.login.password.value, c = "";
	var b = $("#username").val(), d = $("#password").val();
	document.submitForm.rp.value = d;
	if (b == "") {
		//alert("User Id is empty.");
		var c = "Error:User Id is empty.";
		displayMessage(c);
		return;
	} else if (d == "") {
		//alert("Password is empty.");
		var c = "Error:Password is empty.";
		displayMessage(c);
		return;
	} else if (b!=""){
		b=b.trim();
	} else {
		toggleSpinner("spinner", 1);
	}

	if (a && !g_key) {
		 var obj = new Object();
		 obj.loginId = b;
		 var jsonString= JSON.stringify(obj);
		   
		 c = sendRequest(jsonString);
		 var nounceHox="";
		 var nounceResult ="";
		 try {
			 c = $.parseJSON(c);
		 } catch ( e ) {
			 displayMessage("Error:User session expired.");
			 return;
		 }
		 if(c.status!=0){
			 displayMessage("Error:The userid or password you entered is incorrect.");
			 return;
		 }
		 $.each(c.data, function(key, val) {
				if (key=='nounceResult'){
					nounceResult=val;
				}
				if (key=='nonceHex'){
					nounceHox=val;
				}
			});
		 c= nounceResult;
		 nounce= nounceHox;
		if (validateID(b, "User Id")) {
			if (validatePIN(d)) {
				if (document.getElementById("captchaImage") != null) {
					if (handleCaptcha()) {
						//c = sendRequest("u=" + b + "&captcha=" + document.login.captchaResponse.value);
					} else {
						toggleSpinner("spinner", 0);						
						return false;
					}
				} else {
					//c = sendRequest("u=" + b);
					if (c.indexOf("Error:") != -1) {
						displayMessage(c);
						toggleSpinner("spinner", 0);
						return false;
					}
				}
				if (c.indexOf("Error:") != -1) {
					toggleSpinner("spinner", 0);
			        var captchaURL = $("#captchaImage").attr("src") + "?" + new Date().getTime();
			        $("#captchaImage").attr("src", captchaURL);
					displayMessage(c);
					return false;
				} else if (c.indexOf("http") != -1)
					window.location = c;
				else {
					a = c.substring(0, 32);

					a = hexToByteArray(a);

					b = c.substring(32, 64);

					n = hexToByteArray(b);
					deriveKey(d, a);
					setTimeout("fidLogin(false)", 200);
				}
			} else {
				toggleSpinner("spinner", 0);
			}
		} else {
			toggleSpinner("spinner", 0);
		}
	} else if (!a && !g_key) {
		setTimeout("fidLogin(false)", 200);
	} else {
		
		$("#password").val("");

		d = hex_hmac_sha1(g_key, n);

		var mod = $("#mod").val();//document.login.mod.value;
		var exp = $("#exp").val();//document.login.exp.value;

		var rsa = new RSAKey();
		rsa.setPublic(mod, exp);

		var f = rsa.encrypt(d);

		if (!a && g_key) {
			document.submitForm.u.value = b;
			document.submitForm.f.value = f;
			document.submitForm.n.value = nounce;
			//document.submitForm.pv.value = "no";
			//c = sendRequest("u="+b+"&f="+f);
			document.submitForm.pv.value = "yes";
			/*if (c.indexOf("Error:") != -1) {
				displayMessage(c);
				toggleSpinner("spinner", 0);
				return false;
			}*/
			document.submitForm.submit();
			toggleSpinner("spinner", 0);
		}
	}
}

function refreshCaptcha() {
	$("#refreshCaptcha").click(function() {
    	$("#captchaImage").fadeOut(500, function() {
        	var captchaURL = $("#captchaImage").attr("src") + "?" + new Date().getTime();
        	$("#captchaImage").attr("src", captchaURL);
      	});
	});
}

function validatePIN(a) {
    if (a == "") {
    	alert("Please enter password");
    	return false;
    }
    a = trim(a);
    a = a.toUpperCase();
    var b = 8;
    if (!checkLength(a, b, 24)) {
        //alert("Please enter valid a password (Minimum 8 and maximum 24 characters)");
        var c = "Please enter valid a password (Minimum 8 and maximum 24 characters)";
    	displayMessage(c);
        return false;
    }
    if (!checkFormat(a)) {
    	//alert("Please enter password");
    	 var c = "Please enter password";
    	 displayMessage(c);
        return false;
    }
    return true;
}
function handleCaptcha() {
	var captvalue= trim(document.login.captchaResponse.value);
	
	if (captvalue == "") {
		alert("Please enter the Captcha displayed");
		return false;
	}
	return true;
}