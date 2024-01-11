
var dataTable;
$(document).ready(function () {
    loadDataItems();
});


//function loadDataTable() {
//    dataTable = $('#tblData').DataTable({
//        "ajax": {
//            "url": "/Admin/ItemTransfer/GetProduct"
//        },
//        "columns": [
//            { "data": "name", "width": "30%" },    
//            { "data": "description", "width": "40%" },
//            { "data": "category", "width": "20%" },
//            { "data": "room", "width": "20%" },
           
//            {
//                "data": "id",
//                "render": function (data) {
//                    return `
//                            <div class="text-center">
//                                 <a href="/Admin/ItemTransfer/GetItem/${data}" class="btn btn-success text-white" style="cursor:pointer">
//                                    <i class="fas fa-check"></i> 
//                                </a>    
                              
//                            </div>
//                           `;
//                }, "width": "10%"
//            }
//        ], 
//        "createdRow": function (row, data, index) {
//            $(row).attr('onclick', 'select_row(this)');
//            $(row).attr('data-dismiss', 'modal');
//            $('#ItemsNameVal').val('tdsssss');
//            //$('td', row).eq(0).attr('style', 'display:none;');
//            //$('td', row).eq(2).attr('style', 'display:none;');
//        }
//    });
//}

function loadDataItems() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/ItemTransfer/GetAllItems"
        },
        "columns": [
            { "data": "name", "width": "30%" },
            { "data": "category", "width": "30%" },
            { "data": "room", "width": "10%" },
            { "data": "location", "width": "30%" },
            { "data": "id", "width": "10%" },
            { "data": "roomid", "width": "10%" },
            { "data": "name_of_room_and_location_", "width": "20%" }

        ],
        "createdRow": function (row, data, index) {
            $(row).attr('onclick', 'select_row(this)');
            $(row).attr('data-bs-dismiss', 'modal');
            $('td', row).eq(4).attr('style', 'display:none;');
            $('td', row).eq(5).attr('style', 'display:none;');
            $('td', row).eq(6).attr('style', 'display:none;');
        },
    });
}

function select_row(obj) {
    var itemid = $('td', obj).eq(4).html().trim();
    var itemname = $('td', obj).eq(0).html().trim();   
    var roodname = $('td', obj).eq(2).html().trim();
    var roodid = $('td', obj).eq(5).html().trim();
    var locationname = $('td', obj).eq(6).html().trim();

    $('#ItemsName').val(itemname);
    $('#LocationPrevious').val(locationname);
    $('#roomId').val(roodid);
    $('#roomName').val(roodname);
}

function Delete(url) {
    swal({
        title: "Apakah Anda yakin ingin Menghapus?",
        text: "Anda tidak akan dapat memulihkan data!",
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