document.getElementById('fake-file-button-browse').addEventListener('click', function () {
    document.getElementById('files-input-upload').click();    
});

document.getElementById('fake-file-button-upload').addEventListener('click', function () {
    var formData = new FormData();
    formData.append("FileUpload", document.getElementById("files-input-upload").files[0]);
    $.ajax({
        url: '/Renewal/CheckFile/',
        type: 'POST',
        data: formData,
        dataType: 'json',
        contentType: false,
        processData: false,
        success: function (data) {
           
            if (data.Success || (!data.Success && data.MessageType === "Warning")) {
                var $tbody = document.getElementById("sampleBody"),
                    $tr, $td, arr;
                $tbody.innerHTML = "";

                if (!data.Success) {
                    document.getElementById("warning").style.display = "inline";
                    document.getElementById("warningMessage").textContent = data.Message;
                    document.getElementById("async").value = true;
                }

                for (var i = 0, len = data.Samples.length; i < len; i++) {
                    $tr = document.createElement("TR");
                    arr = data.Samples[i].split(',');
                    for (var j = 0, len2 = arr.length; j < len2; j++) {
                        $td = document.createElement("TD");
                        $td.textContent = arr[j];
                        $tr.appendChild($td);
                    }
                    $tbody.appendChild($tr);
                }

                $("#myModal").on('shown.bs.modal', function () {
                    //action
                });

                // show the modal onload
                $("#myModal").modal({
                    show: true
                });
             
            } else {
                document.getElementById("AlertaMensagem").textContent = object.Message;
                document.getElementById('alerta').className = "oaerror danger in";
            }

        }
    });
});

document.getElementById('confirmBtn').addEventListener('click', function () {
    var formData = new FormData();
    formData.append("FileUpload", document.getElementById("files-input-upload").files[0]);
    formData.append("Email", document.getElementById("exampleInputEmail2").value);
    formData.append("Async", document.getElementById("async").value);

    $.ajax({
        url: '/Renewal/ReadExcel/',
        type: 'POST',
        data: formData,
        dataType: 'json',
        contentType: false,
        processData: false,
        success: function (data) {

            if (data.Success || (!data.Success && data.MessageType === "Warning")) {
               

            } else {
                document.getElementById("AlertaMensagem").textContent = object.Message;
                document.getElementById('alerta').className = "oaerror danger in";
            }

        }
    });
});

document.getElementById('files-input-upload').addEventListener('change', function () {
    document.getElementById('fake-file-input-name').value = this.value;

    document.getElementById('fake-file-button-upload').removeAttribute('disabled');    
});



document.getElementById('remover-alerta').addEventListener('click', function () {
    document.getElementById('alerta').className = "oaerror danger out";
});
document.getElementById('adicionar-alerta').addEventListener('click', function () {
    document.getElementById('alerta').className = "oaerror danger in";
});
