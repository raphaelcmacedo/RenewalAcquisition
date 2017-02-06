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
        success: function (object) {
           
            if (object.Success) {
             
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
