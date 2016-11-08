function validateCPIN(a, b) {
	a = trim(a);
	b = trim(b);
	if (a.length != b.length) {
		$('#errorMsgApplication').html("The new password and confirm password do not match. Please try again.");
		$('#modalApplicationError').modal('show');
		return false;
	}
	for ( var d = 0; d < a.length; d++) {
		var c = a.charAt(d), e = b.charAt(d);
		if (c != e) {
			$('#errorMsgApplication').html("The new password and confirm password do not match. Please try again.");
			$('#modalApplicationError').modal('show');
			return false;
		}
	}
	return true;
}
function validatePIN(a, b) {
	if (a == "") {
		alert("Please enter " + b + " password");
		return false;
	}
	a = trim(a);
	a = a.toUpperCase();
	var b = 8;
	if (!checkLength(a, b, 24)) {
		$('#errorMsgApplication').html("Please enter valid " + b + " password. Minimum 8 and maximum 24 chars, at least 1 Upper Case and 1 Digit.");
		$('#modalApplicationError').modal('show');
		return false;
	}
    if (!checkFormat(a)) {
    	$('#errorMsgApplication').html("You have entered an incorrect " + b + " password. Please try again.");
		$('#modalApplicationError').modal('show');
    	return false;
   	}
   
	return true;
}
function validatePIN2(a, b) {
	b = trim(b);
	b = b.toUpperCase();
	var d = trim(a);
	a = a.toUpperCase();
	if (a.length != b.length)
		return true;
	d = false;
	for ( var c = 0; c < a.length; c++) {
		var e = a.charAt(c), f = b.charAt(c);
		if (e != f) {
			d = true;
			break;
		}
	}
	d || alert("The new password cannot be the same as the user id. Please try again.");
	return d;
}
function validatePINStren(a) {
	if (passwordStrength(a) < 3) {
		$('#errorMsgApplication').html("Your new password has to be at least 'Medium' strength.");
		$('#modalApplicationError').modal('show');
		return false;
	}

	return true;
}
function passwordStrength(a) {
	var b = 0;
	a.length > 8 && b++;
	a.match(/[a-z]/) && a.match(/[A-Z]/) && b++;
	a.match(/\d+/) && b++;
	a.match(/.[!,\",#,$,%,&,\',(,),*,+,\,,\-,.,\/,:,;,<,=,>,?,@,\[,\\,\],^,_,`,{,|,},~]/) && b++;
	a.length > 12 && b++;
	return b;
}
function passwordStrengthDesc(a) {
	var b = [];
	b[0] = "<small>Very Weak</small>";
	b[1] = "<small>Weak</small>";
	b[2] = "<small>Better</small>";
	b[3] = "<small>Medium</small>";
	b[4] = "<small>Strong</small>";
	b[5] = "<small>Strongest</small>";
	return b[a];
}
function checkPasswordStrength(a) {
	a = passwordStrength(a);
	document.getElementById("passwordDescription").innerHTML = passwordStrengthDesc(a);
	document.getElementById("passwordStrength").className = "strength" + a;
}