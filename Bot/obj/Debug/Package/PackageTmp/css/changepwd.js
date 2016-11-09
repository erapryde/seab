var g_key = "", challenge = "";
function deriveKey(a, b) {
	a = new PBKDF2(a, b, 100, 16);
	function d(e) {
	}
	function c(e) {
		g_key = hexToByteArray(e);
	}
	a.deriveKey(d, c);
}

function changep2(a, a1) {
	var b = document.changepwd.userId.value;
	var d = document.changepwd.currentPassword.value;//old
	var c = document.changepwd.newPassword.value;//new 
	var e = document.changepwd.confirmPassword.value;//new confirm
	var g = document.changepwd.salt.value;
	if(d==""){
		//alert("Please enter old password");
		$('#errorMsgApplication').html("Please enter old password");
		$('#modalApplicationError').modal('show');
		return;
	}else if(c==""){
		//alert("Please enter password");
		$('#errorMsgApplication').html("Please enter password");
		$('#modalApplicationError').modal('show');
		return;
	}else if(e==""){
		$('#errorMsgApplication').html("Please enter confirm password");
		$('#modalApplicationError').modal('show');
		return;
	}
	
	if (a && a1 && !g_key) {
		if (validateID(b, "User Id"))
			if (validatePIN(d, "old"))
				if (validatePIN(c, ""))
					if (validatePIN(e, "confirm"))
						if (validateCPIN(c, e))
							if (validatePIN2(b, c)) {
								if(validatePINStren(c)){
								// e = document.changepwd.salt.value;
								// e = hexToByteArray(e);

								//console.log("Using new password salt: " + h);

								//h = hexToByteArray(h); // to change from h to g
								g = hexToByteArray(g);

								// Derive PBKDF2 hash of new password
								// deriveKey(b, e);
								//deriveKey(c, h); // to change from h to g => SALT NEVER CHANGE
								deriveKey(c, g); 
								
								setTimeout("changep2(false,true)", 100);
								}
							}
	} else if (!a && a1 && !g_key)
		{
		setTimeout("changep2(false, true)", 100);
		}
	else if (!a && a1 && g_key) {
		// b = byteArrayToHex(aesEncrypt(d, g_key));
		// c = byteArrayToHex(aesEncrypt(c, g_key));
		// e = byteArrayToHex(aesEncrypt(e, g_key));
		// Store PBKDF2 hash of new pwd temp in form first

		var gkeyHex = binb2hex(str2binb(g_key));
		document.changepwd.newPassword.value = gkeyHex;
		document.changepwd.confirmPassword.value = gkeyHex;

		g_key = false;
		// Derive PBKDF2 hash of current pwd

		//console.log("Using old password salt: " + g);

		g = hexToByteArray(g);
		deriveKey(d, g);
		setTimeout("changep2(false,false)", 100);

	} else if (!a && !a1 && !g_key) {
		// Wait for PDKDF2 of current pwd to complete
		setTimeout("changep2(false,false)", 100);
	} else {
		// Get HMAC hash of current pwd PBKDF2 hash
		var nonce = document.changepwd.nonce.value;
		nonce = hexToByteArray(nonce);

		//console.log("Current pwd hash before hmac: " + g_key);

		var k = hex_hmac_sha1(g_key, nonce);

		var mod = document.changepwd.modulus.value;
		var exp = document.changepwd.exponent.value;

		var rsa = new RSAKey();
		rsa.setPublic(mod, exp);

		//console.log("Current pwd hash hmac before enc: " + k);

		// RSA encrypt current pwd
		var f = rsa.encrypt(k);

		//console.log("Current pwd hash hmac after enc: " + f);

		// RSA encrypt new pwd
		var h = document.changepwd.newPassword.value;

		//console.log("New pwd hash before enc: " + h);

		var j = rsa.encrypt(h);

		//console.log("New pwd hash after enc: " + j);

		document.changepwd.currentPassword.value = f;
		document.changepwd.newPassword.value = j;
		document.changepwd.confirmPassword.value = j;
		document.changepwd.modulus.value = "";
		document.changepwd.exponent.value = "";		
		document.changepwd.submit();
	}
}

function changep(a, a1) {
	var b = document.changepwd.userId.value;
	var d = document.changepwd.currentPassword.value;
	var c = document.changepwd.newPassword.value;
	var e = document.changepwd.confirmNewPassword.value;
	var g = document.changepwd.salt.value;

	if (a && a1 && !g_key) {
		if (validateID(b, "User Id"))
			if (validatePIN(d, "old"))
				if (validatePIN(c, ""))
					if (validatePIN(e, "confirm"))
						if (validateCPIN(c, e))
							if (validatePIN2(b, c)) {
								e = sendRequest("getsalt");
								console.log("salt returned from server: " + e);
								if (e.indexOf("Error") != -1)
									displayMessage(e);
								else {
									// document.changepwd.newSalt.value = e;
									// e =
									// document.changepwd.nonce.value.substring(
									// 0, 32);

									e = hexToByteArray(e);

									// Derive PBKDF2 hash of new password
									// deriveKey(b, e);
									deriveKey(c, e);

									setTimeout("changep(false,true)", 100);
								}
							}
	} else if (!a && a1 && !g_key)
		// Wait for PDKDF2 of new pwd to complete
		setTimeout("changep(false,true)", 100);
	else if (!a && a1 && g_key) {
		// b = byteArrayToHex(aesEncrypt(d, g_key));
		// c = byteArrayToHex(aesEncrypt(c, g_key));
		// e = byteArrayToHex(aesEncrypt(e, g_key));

		// Store PBKDF2 hash of new pwd temp in form first

		var gkeyHex = binb2hex(str2binb(g_key));

		document.changepwd.newPassword.value = gkeyHex;
		document.changepwd.confirmNewPassword.value = gkeyHex;

		g_key = false;

		// Derive PBKDF2 hash of current pwd
		g = hexToByteArray(g);
		deriveKey(d, g);

		setTimeout("changep(false,false)", 100);

	} else if (!a && !a1 && !g_key) {
		// Wait for PDKDF2 of current pwd to complete
		setTimeout("changep(false,false)", 100);
	} else {

		// Get HMAC hash of current pwd PBKDF2 hash
		var nonce = document.changepwd.nonce.value;
		nonce = hexToByteArray(nonce);
		var k = hex_hmac_sha1(g_key, nonce);

		var mod = document.changepwd.modulus.value;
		var exp = document.changepwd.exponent.value;

		var rsa = new RSAKey();
		rsa.setPublic(mod, exp);

		//console.log("Current pwd hash hmac before enc: " + k);

		// RSA encrypt current pwd
		var f = rsa.encrypt(k);

		//console.log("Current pwd hash hmac after enc: " + f);

		// RSA encrypt new pwd
		var h = document.changepwd.newPassword.value;

		//console.log("New pwd hash before enc: " + h);

		var j = rsa.encrypt(h);

		//console.log("New pwd hash after enc: " + j);

		document.changepwd.currentPassword.value = f;
		document.changepwd.newPassword.value = j;
		document.changepwd.confirmNewPassword.value = j;
		document.changepwd.submit();
	}
}

function resetp(a) {
	var b = document.rpform.userId.value, d = document.rpform.password.value, c = document.rpform.confirmPassword.value;
	var w = document.rpform.question1.value;
	var x = document.rpform.answer1.value;
	var y = document.rpform.question2.value;
	var z = document.rpform.answer2.value;
	
	if(w==""){
		$('#errorMsgApplication').html("Please select your first security question");
		$('#modalApplicationError').modal('show');
		return;
	} else if(x==""){
		$('#errorMsgApplication').html("Please answer your first security answer");
		$('#modalApplicationError').modal('show');
		return;
	} else if(y==""){
		$('#errorMsgApplication').html("Please select your second security question");
		$('#modalApplicationError').modal('show');
		return;
	} else if(z==""){
		$('#errorMsgApplication').html("Please answer your second security answer");
		$('#modalApplicationError').modal('show');
		return;
	}
	
	if(w==y) {
		$('#errorMsgApplication').html("Please select different security questions");
		$('#modalApplicationError').modal('show');
		return;
	}
	
	//alert("Value of a: " + a);
	if (a && !g_key) {
		if (validatePIN(d, ""))
			if (validatePIN(c, "confirm"))
				if (validateCPIN(d, c))
					if (validatePIN2(b, d)) {
						if(validatePINStren(d)){
							c = document.rpform.salt.value;
	
							 //console.log("Using salt: " + c);
	
							c = hexToByteArray(c);
	
							// deriveKey(b, c);
	
							// Derive PBKDF2 hash of new password
							deriveKey(d, c);
							setTimeout("resetp(false)", 100);
						}
					}
	} else if (!a && !g_key)
		setTimeout("resetp(false)", 100);
	else {

		var mod = document.rpform.modulus.value;
		var exp = document.rpform.exponent.value;

		var gkeyHex = binb2hex(str2binb(g_key));

		 //console.log("G_Key has value: " + gkeyHex);

		 //console.log("Before enc pwd hash:" + gkeyHex);

		var rsa = new RSAKey();
		rsa.setPublic(mod, exp);
		var f = rsa.encrypt(gkeyHex);

		 //console.log("Enc pwd hex:" + f);

		// b = byteArrayToHex(aesEncrypt(d, g_key));
		// c = byteArrayToHex(aesEncrypt(c, g_key));

		// document.rpform.password.value = b;
		// document.rpform.confirmPassword.value = c;

		document.rpform.password.value = f;
		document.rpform.confirmPassword.value = f;
		document.rpform.modulus.value = "";
		document.rpform.exponent.value = "";
		document.rpform.submit();
	}
}

function setp(a) {
	var b = document.rpform.userId.value, d = document.rpform.password.value, c = document.rpform.confirmPassword.value;
	
	//alert("Value of a: " + a);
	if (a && !g_key) {
		if (validatePIN(d, ""))
			if (validatePIN(c, "confirm"))
				if (validateCPIN(d, c))
					if (validatePIN2(b, d)) {
						if(validatePINStren(d)){
							c = document.rpform.salt.value;
	
							 //console.log("Using salt: " + c);
	
							c = hexToByteArray(c);
	
							// deriveKey(b, c);
	
							// Derive PBKDF2 hash of new password
							deriveKey(d, c);
							setTimeout("setp(false)", 100);
						}
					}
	} else if (!a && !g_key)
		setTimeout("setp(false)", 100);
	else {

		var mod = document.rpform.modulus.value;
		var exp = document.rpform.exponent.value;

		var gkeyHex = binb2hex(str2binb(g_key));

		 //console.log("G_Key has value: " + gkeyHex);

		 //console.log("Before enc pwd hash:" + gkeyHex);

		var rsa = new RSAKey();
		rsa.setPublic(mod, exp);
		var f = rsa.encrypt(gkeyHex);

		 //console.log("Enc pwd hex:" + f);

		// b = byteArrayToHex(aesEncrypt(d, g_key));
		// c = byteArrayToHex(aesEncrypt(c, g_key));

		// document.rpform.password.value = b;
		// document.rpform.confirmPassword.value = c;

		document.rpform.password.value = f;
		document.rpform.confirmPassword.value = f;
		document.rpform.modulus.value = "";
		document.rpform.exponent.value = "";
		document.rpform.submit();
	}
}

function formatAns(form,obj, answerField) {
	
	var hashed = formatSecAns(obj.value);

	var mod = form.modulus.value;
	var exp = form.exponent.value;
	var rsa = new RSAKey();
	rsa.setPublic(mod, exp);

	document.getElementById(answerField).value = rsa.encrypt(hashed);
	
	
}
function formatSecAns(a) {
	return hex_sha1(a.toLowerCase().replace(/\s+/g, ""));
}

/*$(document).ready(function() {
	if(!($('#validateForm[id$=".errors"]').is(':visible'))){
		$('#no').attr('checked', true);
	}
	
	if($("#yes").is(":checked")){
		$('#rpform').show();
		$('#validateForm').hide();
	}
	else if($("#no").is(":checked")){
		$('#rpform').hide();
		$('#validateForm').show();
	} 
	$('[name="checkSetPwd"]').click(function(){
		if($("#yes").is(":checked")){
			$('#rpform').show();
			$('#validateForm').hide();
		}
		else if($("#no").is(":checked")){
			$('#rpform').hide();
			$('#validateForm').show();
		}
	});
	
	
	
});*/

function checkForChecked(){
	if($('#checkSetPwd').attr('checked')){
		$('#rpform').hide();
		$('#validateForm').show();
	}
	else{
		$('#rpform').show();
		$('#validateForm').hide();
	} 
}

function comparePwd(a, a1) {
	var b = document.validateForm.userId.value;
	var d = document.validateForm.currentPassword.value;//old
	//var c = document.changepwd.newPassword.value;//new 
	//var e = document.changepwd.confirmPassword.value;//new confirm
	var g = document.validateForm.salt.value;
	
	if(d==""){
		$('#errorMsgApplication').html("Please enter current password");
		$('#modalApplicationError').modal('show');
		return;
	}
	/*else if(c==""){
		alert("Please enter password");
		return;
	}else if(e==""){
		alert("Please enter confirm password");
		return;
	}*/
	
	if (a && a1 && !g_key) {
		if (validateID(b, "User Id"))
			if (validatePIN(d, "old")){
						g = hexToByteArray(g);
	
									// Derive PBKDF2 hash of new password
									// deriveKey(b, e);
									//deriveKey(c, h); // to change from h to g => SALT NEVER CHANGE
									//deriveKey(c, g); 
									
									setTimeout("comparePwd(false,true)", 100);
									}
								//}
							//}
	} else if (!a && a1 && !g_key) {
		setTimeout("comparePwd(false, true)", 100);
	} else if (!a && a1 && g_key) {
		// b = byteArrayToHex(aesEncrypt(d, g_key));
		// c = byteArrayToHex(aesEncrypt(c, g_key));
		// e = byteArrayToHex(aesEncrypt(e, g_key));
		// Store PBKDF2 hash of new pwd temp in form first

		//var gkeyHex = binb2hex(str2binb(g_key));
		//document.changepwd.newPassword.value = gkeyHex;
		//document.changepwd.confirmPassword.value = gkeyHex;

		g_key = false;
		// Derive PBKDF2 hash of current pwd

		//console.log("Using old password salt: " + g);

		g = hexToByteArray(g);
		deriveKey(d, g);
		setTimeout("comparePwd(false,false)", 100);

	} else if (!a && !a1 && !g_key) {
		// Wait for PDKDF2 of current pwd to complete
		setTimeout("comparePwd(false,false)", 100);
	} else {
		// Get HMAC hash of current pwd PBKDF2 hash
		//alert();
		var nonce = document.validateForm.nonce.value;
		nonce = hexToByteArray(nonce);

		var k = hex_hmac_sha1(g_key, nonce);

		var mod = document.validateForm.modulus.value;
		var exp = document.validateForm.exponent.value;

		var rsa = new RSAKey();
		rsa.setPublic(mod, exp);

		// RSA encrypt current pwd
		var f = rsa.encrypt(k);

		//console.log("Current pwd hash hmac after enc: " + f);

		// RSA encrypt new pwd
		//var h = document.changepwd.newPassword.value;

		//console.log("New pwd hash before enc: " + h);

		var j = rsa.encrypt(h);

		//console.log("New pwd hash after enc: " + j);

		document.validateForm.currentPassword.value = f;
		document.validateForm.modulus.value = "";
		document.validateForm.exponent.value = "";		
		var frm = $('<form>').attr({action:'validateOldPassword.action', method:'POST'}).appendTo($('body'));
		$('<input>').attr({'type':'hidden', 'value':'', 'name':'modulus'}).appendTo(frm);
		$('<input>').attr({'type':'hidden', 'value':'', 'name':'exponent'}).appendTo(frm);
		$('<input>').attr({'type':'hidden', 'value':nonce, 'name':'nonce'}).appendTo(frm);
		$('<input>').attr({'type':'hidden', 'value':b, 'name':'userId'}).appendTo(frm);
		$('<input>').attr({'type':'hidden', 'value':f, 'name':'currentPassword'}).appendTo(frm);
		frm.submit();
	}
}
