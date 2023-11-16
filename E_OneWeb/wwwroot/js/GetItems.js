
var dataTable;
$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/ItemTransfer/GetProduct"
        },
        "columns": [
            { "data": "name", "width": "20%" },    
            { "data": "description", "width": "30%" },
            { "data": "category", "width": "20%" },
            { "data": "room", "width": "20%" },
           
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                 <a href="/Admin/ItemTransfer/GetItem/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-solid fa-check"></i> 
                                </a>    
                              
                            </div>
                           `;
                }, "width": "40%"
            }
        ],
        //let getStudies = (url, id) => {
        //    $("#formModal").find("tbody").html('');
        //    $.each(data, (index, val) => {
        //        let tr = `<tr class="table-light"><td class="text-start">${val.name}</td></tr>`;
        //        $("#formModal").find("tbody").append(tr);
        //    });
        //    $("#formModal").modal('show');
        "createdRow": function (row, data, index) {
            $(row).attr('onclick', 'select_row(this)');
            $(row).attr('data-dismiss', 'modal');
            $('#ItemsNameVal').val('tdsssss');
            //$('td', row).eq(0).attr('style', 'display:none;');
            //$('td', row).eq(2).attr('style', 'display:none;');
        }
    });
}

//function LoadPolicy() {
//    try {
//        var urlx = "/Admin/ItemTransfer/GetProduct";

//        $("#tblData").DataTable({
//            "processing": true, // for show progress bar
//            "serverSide": true, // for process server side
//            "filter": true, // this is for disable filter (search box)
//            "ajax": {
//                url: urlx,
//                type: "POST",
//                datatype: "json",
//                data: {
//            /*        idpartner: $("#SearchIdPartner").val()*/
//                }

//                //,
//                //success: function (result) {
//                //    console.log('asdf');
//                //    console.log(result);
//                //}
//            },
//            "columns": [
//                { "data": "name", "width": "30%" },           
//                { "data": "category", "width": "20%" },
//                { "data": "room", "width": "20%" }
//            ],
//            "createdRow": function (row, data, index) {
//                $(row).attr('onclick', 'select_row(this)');
//                $(row).attr('data-dismiss', 'modal');
//                //$('td', row).eq(0).attr('style', 'display:none;');
//                //$('td', row).eq(2).attr('style', 'display:none;');
              

//            },

//        });
//    } catch (e) {
//        alert(e);
//        console.log(e);
//    }
//}
function select_row(obj) {
/*    var idpolicy = $('td', obj).eq(0).html().trim();*/
  /*  var policyNo = $('td', obj).eq(1).html().trim();*/
/*    $('#SearchIdPolicyRev').val(idpolicy);*/
    $('#ItemsNameVal').val(obj);
}

function Delete(url) {
    swal({
        title: "Are you sure you want to Delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}