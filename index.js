var entitiesEndpointUrl = "http://dam-ent-20151113.azurewebsites.net/digitalassets/entities/7fccfc4a5a10457f89d0ce5ca33c0f58/Files";

function S4() {
    return (((1+Math.random())*0x10000)|0).toString(16).substring(1); 
}
 
$(document).ready(function() {
	$("#upload").on("click", function() {
		var keywords = $("#keywords").val();
		var fileInput = $("#picker")[0];
		if (fileInput.files.length < 1) {
			alert("Did not select any files");
		}
		var fileName = fileInput.files[0].name;
		var fileGuid = (S4() + S4() + "-" + S4() + "-4" + S4().substr(0,3) + "-" + S4() + "-" + S4() + S4() + S4()).toLowerCase();

		var data = { 
				"Id": fileGuid,
				"Name": fileName,
				"ContentUrl":"http://damtest1.blob.core.windows.net/public/" + fileName.split(".")[0],
				"ParentFolderId":"00000000-0000-0000-0000-000000000000",
				"Keywords": keywords.split(";")
  		};
  		$.post(entitiesEndpointUrl, data);
  		/*
  		$.ajax({
		    type: 'POST',
		    url: entitiesEndpointUrl,
		    data: data,
		    dataType: 'json',
		    success: function(responseData, textStatus, jqXHR) {
		        alert("POST successful");
		    },
		    error: function (responseData, textStatus, errorThrown) {
		        alert('POST failed.');
		    }
		});
*/
	});
});